using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcExampleP33.Models;
using MvcExampleP33.Models.Dto;

namespace MvcExampleP33.Controllers;

public class UserController(UserManager<User> userManager) : Controller
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
}
