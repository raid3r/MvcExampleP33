using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcExampleP33.Models;
using MvcExampleP33.Models.Dto;
using MvcExampleP33.Models.Forms;

namespace MvcExampleP33.Controllers;

[Authorize]
public class ProfileController(
    UserManager<User> userManager
    ) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Roles = (await userManager.GetRolesAsync(user)).ToList()
        };

        return View(userDto);
    }

    public async Task<IActionResult> ResetPasswordForm()
    {
        return PartialView(new ChangePasswordForm());
    }

    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordForm form)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return PartialView("ResetPasswordForm", form);
        }
        if (form.Password != form.ConfirmPassword)
        {
            ModelState.AddModelError(nameof(ChangePasswordForm.ConfirmPassword), "Passwords do not match");
            Response.StatusCode = 400;
            return PartialView("ResetPasswordForm", form);
        }

        var user = await userManager.GetUserAsync(User);
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, form.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(nameof(ChangePasswordForm.Password), error.Description);
            }
            Response.StatusCode = 400;
            return PartialView("ResetPasswordForm", form);
        }
        return PartialView("ResetPasswordSuccess");
    }
}
