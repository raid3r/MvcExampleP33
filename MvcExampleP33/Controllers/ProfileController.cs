using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcExampleP33.Models;
using MvcExampleP33.Models.Dto;
using MvcExampleP33.Models.Forms;
using MvcExampleP33.Services;
using System.Security.Claims;

namespace MvcExampleP33.Controllers;

[Authorize]
public class ProfileController(
    UserManager<User> userManager,
    FileStorageService fileStorageService,
    StoreContext context
    ) : Controller
{
    public async Task<User> GetCurrentUser()
    {
        var identityId = int.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        var user = await context
            .Users
            .Include(u => u.Avatar)
            .FirstAsync(u => u.Id == identityId);
        return user;
    }


    public async Task<IActionResult> Index()
    {
       var user = await GetCurrentUser();

        var userDto = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            AvatarSrc = user.Avatar?.Src,
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



    // {"fullname": "New full name"}
    [HttpPost]
    public async Task<JsonResult> UpdateFullName([FromBody] string fullName)
    {
        var user = await userManager.GetUserAsync(User);
        user.FullName = fullName;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            Response.StatusCode = 400;
            return Json(new { success = false, errors = result.Errors.Select(e => e.Description) });
        }

        await SignInUserAsync(user);

        return Json(new { success = true });
    }


    private async Task SignInUserAsync(User user)
    {
        var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        if (!string.IsNullOrEmpty(user.FullName))
        {
            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName));
        }

        if (user.Avatar != null)
        {
            identity.AddClaim(new Claim("avatar_src", user.Avatar.Src));
        }

        var userRoles = await userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var principal = new ClaimsPrincipal(identity);


        await HttpContext.SignInAsync(
            scheme: IdentityConstants.ApplicationScheme,
            principal: principal,
            properties: new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            }
            );
    }

    [HttpPost]
    public async Task<JsonResult> UploadAvatar(IFormFile avatar)
    {
        if (avatar == null || avatar.Length == 0)
        {
            Response.StatusCode = 400;
            return Json(new { success = false, error = "No file uploaded" });
        }
        
        var user = await GetCurrentUser();

        if (user.Avatar != null)
        {
            fileStorageService.DeleteFile(user.Avatar.FileName);
            context.Images.Remove(user.Avatar);
        }

        var savedFileName = await fileStorageService.SaveFileAsync(avatar);
        user.Avatar = new ImageFile
        {
            FileName = savedFileName,
        };

        await context.SaveChangesAsync();
        await SignInUserAsync(user);
        return Json(new { success = true, avatarSrc = savedFileName });
    }

}
