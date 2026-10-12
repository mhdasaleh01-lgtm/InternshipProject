using InternshipProject.Data;
using InternshipProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipProject.Controllers;

[Authorize(Roles = Role.Write + "," + Role.Read)]
public class ProductsController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() => View(await db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
        return product is null ? NotFound() : View(product);
    }

    [HttpGet, Authorize(Roles = Role.Write)]
    public IActionResult Create() => View(new Product());

    [HttpPost, Authorize(Roles = Role.Write)]
    public async Task<IActionResult> Create([Bind("Name,Price,Description")] Product product)
    {
        if (!ModelState.IsValid) return View(product);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        TempData["Message"] = "Product created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize(Roles = Role.Write)]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await db.Products.FindAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost, Authorize(Roles = Role.Write)]
    public async Task<IActionResult> Edit(int id, [Bind("Name,Price,Description")] Product input)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        if (!ModelState.IsValid) { input.Id = id; return View(input); }
        product.Name = input.Name;
        product.Price = input.Price;
        product.Description = input.Description;
        await db.SaveChangesAsync();
        TempData["Message"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet, Authorize(Roles = Role.Write)]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete"), Authorize(Roles = Role.Write)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        TempData["Message"] = "Product deleted.";
        return RedirectToAction(nameof(Index));
    }
}
