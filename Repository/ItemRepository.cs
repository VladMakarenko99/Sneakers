using practice.Data;
using practice.Models;
using Microsoft.EntityFrameworkCore;

namespace practice.Repository
{
    public class ItemRepository : IItemRepository
    {
        private AppDbContext _context;
        public ItemRepository(AppDbContext _context) => this._context = _context;
        
        public async Task<int> CountAsync()
        {
            return await _context.Items.CountAsync();
        }

        public List<Item> GetAll()
        {
            return _context.Items.ToList();
        }

        public async Task<List<Item>> GetAllAsync()
        {
            return await _context.Items.ToListAsync();
        }

        public async Task<Brand?> GetBrandByNameAsync(string name)
        {
            System.Console.WriteLine(name);
            return await _context.Brands.FirstOrDefaultAsync(x => x.Name == name.Replace("-", " "));
        }

        public async Task<Item?> GetByNameAsync(string name)
        {
            return await _context.Items.FirstOrDefaultAsync(i => i.Name == name);
        }

        public async Task<List<Item>> GetFewAsync(int load)
        {
            return await _context.Items.Take(load).ToListAsync();
        }
    }
}