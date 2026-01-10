using System.ComponentModel.DataAnnotations;

namespace MvcExampleP33.Models.Forms;

public class CategoryForm
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Category Title")]
    public string Title { get; set; }


    [Display(Name = "Category Image")]
    public IFormFile? Image { get; set; }
}
