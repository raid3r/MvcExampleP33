using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcExampleP33.Models;

namespace MvcExampleP33.Controllers;

public class HomeController(
    StoreContext context
    ) : Controller
{
    // /Home/Index
    public IActionResult Index()
    {
        var products = context.Products
            .Include(x => x.Category)
            .Include(x => x.Images)
            //.Take(5)
            .ToList();

        return View(products);
    }

    // /Home/Privacy
    public IActionResult Privacy()
    {
        return View();
    }

    // /Home/About
    public IActionResult About()
    {
        var siteInfo = new SiteInfo
        {
            SiteName = "MVC Example P33",
            Description = "This is an example MVC application demonstrating .NET 8 features."
        };

        ViewData["Message"] = "Hello";

        return View(siteInfo);
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
