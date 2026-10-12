using InternshipProject.Data;
using InternshipProject.Models;
using InternshipProject.Security;
using InternshipProject.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipProject.Controllers;

public class AccountController(AppDbContext db, IPasswordHasher<AppUser> hasher) : Controller
{
    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await db.Users.Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Username == model.Username.Trim());
        var result = user is null ? PasswordVerificationResult.Failed :
            hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (user is null || result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, model.Password);
            await db.SaveChangesAsync();
        }
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            SessionValidation.CreatePrincipal(user));
        return Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl!) :
            RedirectToAction("Index", "Products");
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet, AllowAnonymous]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View();
    }
}
