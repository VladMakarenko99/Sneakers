using Sneakers.Models;

namespace Sneakers.Data;

public class AddProducts
{
    public static void Add(AppDbContext _context)
    {
        for (int i = 0; i < 3; i++)
        {
            _context.Items.Add(new Item
            {
                Price = i + new Random().Next(80, 200),
                Name = $"image{29 + i}",
                Description = "Description",
                Size = null,
                Image = "image.jpeg",
                BrandId = new Random().Next(1, 3),
                BestFor = "Running"
            }
            );
        }
        _context.SaveChanges();

    }

    public static void Update(AppDbContext _context)
    {
        var list = _context.Items.Skip(12).ToList();
        foreach (Item item in list)
        {
            item.BrandId = 4;
            _context.Update(item);
            _context.SaveChanges();
        }
    }

}
