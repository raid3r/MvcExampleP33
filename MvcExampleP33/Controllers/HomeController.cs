using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcExampleP33.Models;

namespace MvcExampleP33.Controllers;

public class HomeController : Controller
{
    // /Home/Index
    public IActionResult Index()
    {
        return View();
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
