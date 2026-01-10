namespace MvcExampleP33.Models;

public class ImageFile
{
    public int Id { get; set; }
    public string FileName { get; set; }

    public virtual ICollection<Product> Products { get; set; } = [];

    public string Src { get => "/uploads/images/" + FileName[0] + "/" + FileName[1] + "/" + FileName; }
}
