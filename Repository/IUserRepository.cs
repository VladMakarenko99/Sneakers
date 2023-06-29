using practice.Models;

namespace practice.Repository
{
    public interface IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email);

        public Task SetCartItems(User? user, string? cartItemsJson);

        public Task AddAsync(User user);

        public Task Remove(User user);

        public Task Update(User user);

        public Task UploadPhotoAsync(User user, IFormFile profilePhoto);

        public Task<List<Order>> GetOrders (int userId);
    }
}