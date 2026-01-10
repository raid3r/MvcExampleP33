namespace MvcExampleP33.Models;

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public virtual Category Category { get; set; }

    public virtual ICollection<ImageFile> Images { get; set; } = [];
}
