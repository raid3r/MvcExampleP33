namespace MvcExampleP33.Models;

public class Category
{
    public int Id { get; set; }
    public string Title { get; set; }
    public virtual ICollection<Product> Products { get; set; } = [];

    public virtual ImageFile? Image { get; set; }
}
