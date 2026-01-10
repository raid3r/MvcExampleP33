using System.ComponentModel.DataAnnotations;

namespace MvcExampleP33.Models.Forms;

public class ProductForm
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Product Title")]
    public string Title { get; set; }
    [Required]
    [StringLength(500)]
    [Display(Name = "Product Description")]
    public string Description { get; set; }
    [Required]
    [Display(Name = "Product Price")]
    public decimal Price { get; set; }
    [Required]
    [Display(Name = "Select category")]
    public int CategoryId { get; set; }

    public IFormFile? Image { get; set; }
}
