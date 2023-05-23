using practice.Models;

namespace practice.Repository
{
    public interface IItemRepository
    {
        public Task<int> CountAsync();

        public List<Item> GetAll();

        public Task<List<Item>> GetAllAsync();

        public Task<Item?> GetByNameAsync(string name);

        public Task<List<Item>> GetFewAsync(int load);

        public Task<Brand?> GetBrandByNameAsync(string name);
    }
}