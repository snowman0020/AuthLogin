# Auth App — Clean Architecture

Full-stack authentication system with **Nuxt.js** frontend and **.NET 10 C#** backend.

## Architecture

```
Auth/
├── backend/                 # .NET 10 C# — Clean Architecture N-Tier
│   ├── AuthApi.sln
│   └── src/
│       ├── AuthApi.Domain/          # Entities + Repository/Service Interfaces
│       ├── AuthApi.Application/     # DTOs + Service Implementations (JWT, Auth, ApiKey)
│       ├── AuthApi.Infrastructure/  # SQLite + EF Core + Repository Implementations
│       └── AuthApi.API/             # Controllers + Swagger + Program.cs
└── frontend/                # Nuxt.js 3 — Login, Register, Dashboard
```

## Features

| Feature | Details |
|---|---|
| JWT Auth | Access token (15 min) + HS256 signing |
| Refresh Token | Secure rotation — old token revoked on refresh |
| API Key | Header `X-API-Key: ak_...` for server-to-server |
| Swagger | `/swagger` with Bearer + API Key auth dialogs |
| SQLite | Local file database via EF Core (`auth.db`) |
| CORS | Frontend at `http://localhost:3009` |

## Quick Start

### Backend
```bash
cd backend/src/AuthApi.API
# Set env vars or edit appsettings.Development.json:
# JWT_SECRET, FRONTEND_URL
dotnet run
# → http://localhost:3000
# → http://localhost:3000/swagger
```

SQLite database (`auth.db`) is created automatically on first startup.

### Frontend
```bash
cd frontend
npm install
npm run dev
# → http://localhost:3009
```

## Environment Variables

### Backend (`backend/.env`)
```env
JWT_SECRET=your-secret-key
PORT=3000
FRONTEND_URL=http://localhost:3009
```

### Frontend (`frontend/.env`)
```env
NUXT_PUBLIC_API_BASE=http://localhost:3000/api
```

## API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | — | Register new user |
| POST | `/api/auth/login` | — | Login → JWT + Refresh Token |
| POST | `/api/auth/refresh` | — | Refresh access token |
| POST | `/api/auth/logout` | JWT | Revoke refresh token |
| POST | `/api/auth/logout-all` | JWT | Revoke all sessions |
| GET | `/api/user/me` | JWT or API Key | Get profile |
| GET | `/api/user/admin` | JWT + Admin role | Admin only |
| POST | `/api/apikey` | JWT | Create API key |
| GET | `/api/apikey` | JWT | List API keys |
| DELETE | `/api/apikey/{id}` | JWT | Revoke API key |
| GET | `/api/auditlog` | JWT + Admin | All request logs |
| GET | `/api/auditlog/my` | JWT | My request logs |
