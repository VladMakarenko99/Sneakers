using practice.Models;

namespace practice.Repository
{
    public interface IOrderRepository
    {
        public Task Add(Order order);
        public Order? GetById(string id);
    }
}