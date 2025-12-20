using Microsoft.EntityFrameworkCore;
using MvcExampleP33.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<StoreContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

/*
 * Razor Pages
 * /PersonSkills
 * /PersonSkillCreate 
 * 
 * MVC Controller
 * PersonConroller    метод Index
 * /Person/Index
 * /Person/Create
 * /Person/Edit/5
 * /Person/Delete/5
 * 
 * 
 */
//   /Home/Index
//  GET /Person/Edit/5 
//  Person        Edit          5 
//  {controller}  {action}     {id?}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


/*
 * Створити контролер для керування категоріями
 * 
 * Створити контролер для керування продуктами
 * Новоствореному продукту присвоювати першу категорію зі списку
 */