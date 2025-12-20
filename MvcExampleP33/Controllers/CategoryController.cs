using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcExampleP33.Models;

namespace MvcExampleP33.Controllers;

public class CategoryController(StoreContext context) : Controller
{
    /// <summary>
    /// Список категорій
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        return View(await context.Categories.ToListAsync());
    }

    /// <summary>
    /// Сторінка створення категорії (GET) - показ форми
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new Category());
    }

    /// <summary>
    /// Cторінка створення категорії (POST) - обробка форми
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromForm]Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
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
        return View(category);
    }

    /// <summary>
    /// Редагування категорії (POST) - обробка форми
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, [FromForm]Category form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var category =  await context.Categories.FirstAsync(x => x.Id == id);
        category.Title = form.Title;

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
        var category = await context.Categories.FirstAsync(x => x.Id == id);
        context.Remove(category);
        await context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
