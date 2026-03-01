using AuthApi.Domain.Entities;
using AuthApi.Domain.Interfaces.Repositories;
using AuthApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(string id) =>
        await db.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> ExistsByEmailAsync(string email) =>
        await db.Users.AnyAsync(u => u.Email == email);

    public async Task<bool> ExistsByUsernameAsync(string username) =>
        await db.Users.AnyAsync(u => u.Username == username);

    public async Task CreateAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }
}
