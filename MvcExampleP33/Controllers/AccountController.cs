using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcExampleP33.Models;
using MvcExampleP33.Models.Forms;
using System.Security.Claims;

namespace MvcExampleP33.Controllers;

public class AccountController(UserManager<User> userManager) : Controller
{
    // /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginForm());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var user = await userManager.FindByEmailAsync(form.Login);
        if (user is null)
        {
            ModelState.AddModelError(nameof(LoginForm.Password), "Invalid login or password");
            return View(form);
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, form.Password);
        if (!isPasswordValid)
        {
            ModelState.AddModelError(nameof(LoginForm.Password), "Invalid login or password");
            return View(form);
        }

        await SignInUserAsync(user);

        return RedirectToAction("Index", "Home");
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


    // /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterForm());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        if (form.Password != form.ConfirmPassword)
        {
            ModelState.AddModelError(nameof(RegisterForm.ConfirmPassword), "Passwords do not match");
            return View(form);
        }

        // Create new user
        var user = new User
        {
            FullName = form.FullName,
            UserName = form.Login,
            Email = form.Login,
        };
        var result = await userManager.CreateAsync(user, form.Password);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(RegisterForm.Password), result.Errors.First().Description);
            return View(form);
        }

        await userManager.AddToRoleAsync(user, RoleConstants.User);

        await SignInUserAsync(user);
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> AccessDenied()
    {
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return RedirectToAction("Index", "Home");
    }
}
