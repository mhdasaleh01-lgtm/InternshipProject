using InternshipProject.Data;
using InternshipProject.Models;
using InternshipProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InternshipProject.Controllers;

[Authorize(Roles = Role.Write)]
public class UsersController(AppDbContext db, IPasswordHasher<AppUser> hasher) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() =>
        View(await db.Users.AsNoTracking().Include(u => u.Role).OrderBy(u => u.Username).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await db.Users.AsNoTracking().Include(u => u.Role).SingleOrDefaultAsync(u => u.Id == id);
        return user is null ? NotFound() : View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Create() => View(await PopulateRoles(new UserFormViewModel()));

    [HttpPost]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), "Password is required.");
        await ValidateForm(model);
        if (!ModelState.IsValid) return View(await PopulateRoles(model));
        var user = new AppUser { Username = model.Username, FullName = model.FullName.Trim(), RoleId = model.RoleId };
        user.PasswordHash = hasher.HashPassword(user, model.Password!);
        db.Users.Add(user);
        if (!await SaveUser()) return View(await PopulateRoles(model));
        TempData["Message"] = "User created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        ViewData["UserId"] = id;
        return View(await PopulateRoles(new UserFormViewModel
        { Username = user.Username, FullName = user.FullName, RoleId = user.RoleId }));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UserFormViewModel model)
    {
        // A transaction protects the final Write user during concurrent edits.
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await db.Users.Include(u => u.Role).SingleOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();
        await ValidateForm(model, id);
        if (user.Role.Name == Role.Write && model.RoleId != user.RoleId && await IsLastWriter(id))
            ModelState.AddModelError(nameof(model.RoleId), "At least one user must keep the Write role.");
        if (!ModelState.IsValid)
        {
            ViewData["UserId"] = id;
            return View(await PopulateRoles(model));
        }
        user.Username = model.Username;
        user.FullName = model.FullName.Trim();
        user.RoleId = model.RoleId;
        if (!string.IsNullOrEmpty(model.Password))
        {
            user.PasswordHash = hasher.HashPassword(user, model.Password);
            user.SecurityStamp = Guid.NewGuid().ToString();
        }
        if (!await SaveUser())
        {
            ViewData["UserId"] = id;
            return View(await PopulateRoles(model));
        }
        await transaction.CommitAsync();
        TempData["Message"] = "User updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await db.Users.AsNoTracking().Include(u => u.Role).SingleOrDefaultAsync(u => u.Id == id);
        return user is null ? NotFound() : View(user);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await db.Users.Include(u => u.Role).SingleOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();
        if (user.Role.Name == Role.Write && await IsLastWriter(id))
        {
            ModelState.AddModelError(string.Empty, "The last Write user cannot be deleted.");
            return View("Delete", user);
        }
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Message"] = "User deleted.";
        return RedirectToAction(nameof(Index));
    }

    private Task<bool> IsLastWriter(int id) =>
        db.Users.AllAsync(u => u.Id == id || u.Role.Name != Role.Write);

    private async Task ValidateForm(UserFormViewModel model, int? id = null)
    {
        model.Username = (model.Username ?? string.Empty).Trim();
        if (await db.Users.AnyAsync(u => u.Username == model.Username && u.Id != id))
            ModelState.AddModelError(nameof(model.Username), "This username is already in use.");
        if (!await db.Roles.AnyAsync(r => r.Id == model.RoleId))
            ModelState.AddModelError(nameof(model.RoleId), "Select a valid role.");
    }

    private async Task<UserFormViewModel> PopulateRoles(UserFormViewModel model)
    {
        model.Roles = await db.Roles.OrderBy(r => r.Id)
            .Select(r => new SelectListItem(r.Name, r.Id.ToString())).ToListAsync();
        return model;
    }

    private async Task<bool> SaveUser()
    {
        try { await db.SaveChangesAsync(); return true; }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteErrorCode: 19 })
        {
            ModelState.AddModelError(string.Empty, "The user could not be saved. Check that the username is unique and the role is valid.");
            return false;
        }
    }
}
