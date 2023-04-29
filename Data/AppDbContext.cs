using Microsoft.EntityFrameworkCore;
using practice.Models;

namespace practice.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    
    public DbSet<Item> Items { get; set; } = null!;
    
    public DbSet<Brand> Brands { get; set; } = null!;

    public DbSet<Order> Orders { get; set; } = null!;


    public AppDbContext(DbContextOptions<AppDbContext> option) : base(option) { }

    public AppDbContext()
    {
    }
}