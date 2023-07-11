namespace Sneakers.Models;

public class Brand
{
    public int Id { get; set; }
    
    public string? Name { get; set; }
    
    public List<Item>? Items { get; set; }
}