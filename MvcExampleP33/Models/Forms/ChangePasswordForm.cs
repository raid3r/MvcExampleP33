using System.ComponentModel.DataAnnotations;

namespace MvcExampleP33.Models.Forms;

public class ChangePasswordForm
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }
}
