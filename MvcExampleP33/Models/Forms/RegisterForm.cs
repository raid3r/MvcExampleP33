using System.ComponentModel.DataAnnotations;

namespace MvcExampleP33.Models.Forms;

public class RegisterForm
{
    [Required]
    [Display(Name = "Email")]
    public string Login { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }
}
