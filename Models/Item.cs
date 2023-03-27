namespace practice.Models;

public class Item
{
    public int Id { get; set; }

    public int Price { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? Size { get; set; }

    private string? _image;
    public string? Image
    {
        get => $"img/{_image}.jpeg";

        set => _image = value;
    }

    public int BrandId { get; set; }

    public Brand? Brand { get; set; }
}


