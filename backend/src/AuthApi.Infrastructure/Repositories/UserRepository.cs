using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using AuthApi.Infrastructure.Data;
using MongoDB.Driver;

namespace AuthApi.Infrastructure.Repositories;

public class UserRepository(MongoDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(string id) =>
        await db.Users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByEmailAsync(string email) =>
        await db.Users.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task<User?> GetByUsernameAsync(string username) =>
        await db.Users.Find(u => u.Username == username).FirstOrDefaultAsync();

    public async Task<bool> ExistsByEmailAsync(string email) =>
        await db.Users.Find(u => u.Email == email).AnyAsync();

    public async Task<bool> ExistsByUsernameAsync(string username) =>
        await db.Users.Find(u => u.Username == username).AnyAsync();

    public async Task CreateAsync(User user) =>
        await db.Users.InsertOneAsync(user);

    public async Task UpdateAsync(User user) =>
        await db.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
}
