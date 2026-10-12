using InternshipProject.Data;
using InternshipProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipProject.Controllers;

[Authorize(Roles = Role.Write + "," + Role.Read)]
public class RolesController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() =>
        View(await db.Roles.AsNoTracking().OrderBy(r => r.Id).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var role = await db.Roles.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id);
        return role is null ? NotFound() : View(role);
    }
}
