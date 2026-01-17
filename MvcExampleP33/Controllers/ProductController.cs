using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcExampleP33.Models;
using MvcExampleP33.Models.Forms;

namespace MvcExampleP33.Controllers;

[Authorize]
public class ProductController(StoreContext context) : Controller
{
    // /Product/Index
    
    public async Task<IActionResult> Index() 
    {
        var list = await context
            .Products
            .Include(p => p.Category)
            .ToListAsync();

        return View(list); // Views/Product/Index.cshtml
    }

    [HttpGet] //  GET /Product/Create
    public async Task<IActionResult> Create()
    {
        var categories = await context.Categories.ToListAsync();
        ViewData["Categories"] = categories;

        return View(new ProductForm());
    }

    [HttpPost] // POST /Product/Create
    public async Task<IActionResult> Create([FromForm] ProductForm form)
    {
        if (!ModelState.IsValid)
        {
            var categories = await context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            return View(form);
        }

        //form.Image

        var product = new Product
        {
            Title = form.Title,
            Description = form.Description,
            Price = form.Price,
            Category = await context.Categories.FirstAsync(c => c.Id == form.CategoryId)
        };

        context.Add(product);
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpGet] //  GET /Product/Create
    public async Task<IActionResult> Edit(int id) {

       var product = await context
            .Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) {
          return NotFound();
        }

        var categories = await context.Categories.ToListAsync();
        ViewData["Categories"] = categories;

        return View(new ProductForm
        {
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.Category.Id
        });

    }

    [HttpPost] // POST /Product/Create
    public async Task<IActionResult> Edit(int id, [FromForm] ProductForm form) {
    
        if (!ModelState.IsValid)
        {
            var categories = await context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            return View(form);
        }
        var product = await context
            .Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) {
          return NotFound();
        }
        product.Title = form.Title;
        product.Description = form.Description;
        product.Price = form.Price;
        product.Category = await context.Categories.FirstAsync(c => c.Id == form.CategoryId);
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id) 
    {
        var product = await context
            .Products
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) {
          return NotFound();
        }
        context.Remove(product);

        // TODO delete associated image file if exists

        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
