
using practice.Data;
using practice.Models;
using Microsoft.EntityFrameworkCore;

namespace practice.Repository;

public class UserRepository : IUserRepository
{
    private AppDbContext _context;
    public UserRepository(AppDbContext _context) => this._context = _context;

    public async Task AddAsync(User user)
    {
        await _context.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email!);
    }

    public async Task Remove(User user)
    {
        _context.Users.Remove(user!);
        await _context.SaveChangesAsync();
    }

    public async Task SetCartItems(User? user, string? cartItemsJson)
    {
        user!.CartItemsJson = cartItemsJson;
        await _context.SaveChangesAsync();
    }

    public async Task Update(User user)
    {
        _context.Update(user);
        await _context.SaveChangesAsync();
    }
}