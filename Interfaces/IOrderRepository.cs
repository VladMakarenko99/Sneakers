using Sneakers.Models;

namespace Sneakers.Interfaces
{
    public interface IOrderRepository
    {
        public Task Add(Order order);

        public Order? GetById(string id);
    }
}