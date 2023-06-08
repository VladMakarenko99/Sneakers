
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

    public async Task SetOrder(User user, Guid id)
    {
        user.OrderId = id;
        _context.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task Update(User user)
    {
        _context.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task UploadPhotoAsync(User user, IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                user = await GetByEmailAsync(user.Email!) ?? new User();
                user.ProfilePhoto = memoryStream.ToArray();
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
        }

    }
}