using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcExampleP33.Models;
using MvcExampleP33.Models.Dto;
using MvcExampleP33.Models.Forms;

namespace MvcExampleP33.Controllers;

public class UserController(
    UserManager<User> userManager, 
    RoleManager<IdentityRole<int>> roleManager
    ) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = (
            await userManager
            .Users
            .ToListAsync())
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Roles = userManager.GetRolesAsync(u).Result.ToList()
            });

        return View(users);
    }

    public async Task<IActionResult> Roles(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        
        if (user is null)
        {
            return NotFound();
        }
        
        ViewData["User"] = user;
        ViewData["AllRoles"] = await roleManager.Roles.Select(r => r.Name).ToListAsync();
        var userRoles = await userManager.GetRolesAsync(user);
        return View(userRoles);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateRole([FromBody] UserRoleUpdateForm form)
    {
        var user = await userManager.FindByIdAsync(form.UserId.ToString());
        if (user is null)
        {
            return NotFound(); // 404
        }
        if (form.IsSelected)
        {
            await userManager.AddToRoleAsync(user, form.Role);
        }
        else
        {
            await userManager.RemoveFromRoleAsync(user, form.Role);
        }

        return Ok(); // 200
    }
}
