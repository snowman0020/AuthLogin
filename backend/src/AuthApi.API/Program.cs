using AuthApi.API.Authentication;
using AuthApi.Application.Services;
using AuthApi.Domain.Interfaces.Repositories;
using AuthApi.Domain.Interfaces.Services;
using AuthApi.Infrastructure.Data;
using AuthApi.Infrastructure.Repositories;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// ─── Load .env ────────────────────────────────────────────────────────────────
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".env");
if (File.Exists(envPath)) Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

// Inject env vars into config
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["MongoDB:Uri"]          = Environment.GetEnvironmentVariable("MONGODB_URI")          ?? builder.Configuration["MongoDB:Uri"],
    ["Jwt:Secret"]           = Environment.GetEnvironmentVariable("JWT_SECRET")           ?? builder.Configuration["Jwt:Secret"],
    ["Cors:AllowedOrigins"]  = Environment.GetEnvironmentVariable("FRONTEND_URL")         ?? builder.Configuration["Cors:AllowedOrigins"],
});

// ─── MongoDB + Infrastructure ─────────────────────────────────────────────────
builder.Services.AddSingleton<MongoDbContext>();

// ─── Repositories (Data Access Layer) ─────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

// ─── Services (Business Logic Layer) ──────────────────────────────────────────
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

// ─── Authentication ────────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        opt.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync("{\"message\":\"Unauthorized\"}");
            }
        };
    })
    .AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>(ApiKeyAuthHandler.SchemeName, _ => { });

builder.Services.AddAuthorization();

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opt =>
    opt.AddPolicy("Frontend", p =>
        p.WithOrigins(
            builder.Configuration["Cors:AllowedOrigins"]?.Split(",")
                ?? ["http://localhost:3009"])
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));

// ─── Controllers + Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1",
        Description = "Clean Architecture Auth API — JWT + Refresh Token + API Key + MongoDB"
    });

    // JWT Bearer
    opt.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT access token"
    });

    // API Key
    opt.AddSecurityDefinition(ApiKeyAuthHandler.SchemeName, new OpenApiSecurityScheme
    {
        Name = ApiKeyAuthHandler.HeaderName,
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "API key (prefix: ak_...)"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme } },
            []
        },
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = ApiKeyAuthHandler.SchemeName } },
            []
        }
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory,
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath)) opt.IncludeXmlComments(xmlPath);
});

// ─── Build + Pipeline ──────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
        opt.RoutePrefix = "swagger";
    });
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
