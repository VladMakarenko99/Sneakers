using Sneakers.Interfaces;
using Sneakers.Data;
using Sneakers.Models;
using Microsoft.EntityFrameworkCore;

namespace Sneakers.Repository;

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

    public async Task<List<Order>> GetOrders(int userId)
    {
       var orders = await _context.Orders.Where(x => x.UserId == userId).ToListAsync();
       return orders;
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