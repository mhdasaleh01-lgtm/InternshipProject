using System.Security.Claims;
using InternshipProject.Data;
using InternshipProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace InternshipProject.Security;

public class SessionValidation(AppDbContext db) : CookieAuthenticationEvents
{
    public static ClaimsPrincipal CreatePrincipal(AppUser user) => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role.Name),
        new Claim("SecurityStamp", user.SecurityStamp)
    }, CookieAuthenticationDefaults.AuthenticationScheme));

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = int.TryParse(id, out var userId)
            ? await db.Users.AsNoTracking().Include(u => u.Role).SingleOrDefaultAsync(u => u.Id == userId)
            : null;

        if (user is null || context.Principal?.FindFirstValue("SecurityStamp") != user.SecurityStamp)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }

        // Read the current role every request so a demotion takes effect immediately.
        if (context.Principal?.FindFirstValue(ClaimTypes.Role) != user.Role.Name ||
            context.Principal?.Identity?.Name != user.Username)
        {
            context.ReplacePrincipal(CreatePrincipal(user));
            context.ShouldRenew = true;
        }
    }
}
