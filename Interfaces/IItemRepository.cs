using Sneakers.Models;

namespace Sneakers.Interfaces
{
    public interface IItemRepository
    {
        public Task<int> CountAsync();

        public Task<List<Item>> GetAllAsync();

        public Task<Item?> GetByNameAsync(string name);

        public Task<List<Item>> GetFewAsync(int load);

        public Task<Brand?> GetBrandByNameAsync(string name);
    }
}