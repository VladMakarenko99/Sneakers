namespace practice.Models;

public class Item
{
    public int Id { get; set; }

    public int Price { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? Size { get; set; }

    public string? Image { get; set; }

    public int BrandId { get; set; }

    public Brand? Brand { get; set; }

    public string? BestFor { get; set; }
}


