using Microsoft.AspNetCore.Identity;

namespace MvcExampleP33.Models;

public class User: IdentityUser<int>
{
    public string? FullName { get; set; }

    public virtual ImageFile? Avatar { get; set; }
}
