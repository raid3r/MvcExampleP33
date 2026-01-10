using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcExampleP33.Models;
using MvcExampleP33.Models.Forms;
using MvcExampleP33.Services;

namespace MvcExampleP33.Controllers;

// /Category/Index

public class CategoryController(StoreContext context, FileStorageService fileStorageService) : Controller
{
    /// <summary>
    /// Список категорій
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        return View(await context.Categories
            .Include(c => c.Image)
            .ToListAsync());
    }

    /// <summary>
    /// Сторінка створення категорії (GET) - показ форми
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new CategoryForm());
    }

    /// <summary>
    /// Cторінка створення категорії (POST) - обробка форми
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CategoryForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var category = new Category
        {
            Title = form.Title
        };

        // add image if exists
        if (form.Image != null)
        {
            var savedFileName = await fileStorageService.SaveFileAsync(form.Image);
            var imageFile = new ImageFile
            {
                FileName = savedFileName,
            };
            category.Image = imageFile;
        }

        context.Add(category);
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Редагування категорії (GET) - показ форми
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await context.Categories.FirstAsync(x => x.Id == id);
        return View(new CategoryForm
        {
            Title = category.Title
        }
        );
    }

    /// <summary>
    /// Редагування категорії (POST) - обробка форми
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [FromForm] CategoryForm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var category = await context.Categories
            .Include(c => c.Image)
            .FirstAsync(x => x.Id == id);

        category.Title = form.Title;

        if (form.Image != null)
        {
            // delete old image if exists
            if (category.Image != null)
            {
                fileStorageService.DeleteFile(category.Image.FileName);
                context.Remove(category.Image);
            }
            var savedFileName = await fileStorageService.SaveFileAsync(form.Image);
            var imageFile = new ImageFile
            {
                FileName = savedFileName,
            };
            category.Image = imageFile;
        }


        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Видалення категорії
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await context.Categories
            .Include(c => c.Image)
            .FirstAsync(x => x.Id == id);

        if (category.Image != null)
        {
            fileStorageService.DeleteFile(category.Image.FileName);
            context.Remove(category.Image);
        }

        context.Remove(category);
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
