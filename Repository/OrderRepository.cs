using practice.Data;
using practice.Models;

namespace practice.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private AppDbContext _context;
        public OrderRepository(AppDbContext _context) => this._context = _context;

        public async Task Add(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public Order? GetById(string id) => _context.Orders.FirstOrDefault(x => x.Id == new Guid(id));
    }
}