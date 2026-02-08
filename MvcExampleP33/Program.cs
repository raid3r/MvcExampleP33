using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcExampleP33.Models;
using MvcExampleP33.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<FileStorageService>();

builder.Services.AddDbContext<StoreContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    //options.Lockout.MaxFailedAccessAttempts = 5;
    //options.Lockout.AllowedForNewUsers = true;
   
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequiredLength = 3;
})
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<StoreContext>()
    .AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/Account/Login");
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});


var app = builder.Build();

// Ініціалізація ролей
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    
    foreach (var role in RoleConstants.AllRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }

    // if production, use migrations
    if (app.Environment.IsProduction())
    {
        var context = scope.ServiceProvider.GetRequiredService<StoreContext>();
        // Ініціалізація бази даних (створення таблиць, якщо вони не існують)
        await context.Database.MigrateAsync();
        // або
        // Створення бази даних, якщо вона не існує (без застосування міграцій)
        // await context.Database.EnsureCreatedAsync();
    }
}

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

app.UseAuthentication(); /// шукає користувача (в даних запиту - кукіс)
app.UseAuthorization(); // перевіряє чи має користувач доступ до ресурсу

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
// /
//   /Home/Index
//  GET /Person/Edit/5 
//  Person        Edit          5 
//  {controller}  {action}     {id?}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();


/*
 * Створити контролер для керування категоріями
 * 
 * Створити контролер для керування продуктами
 * Новоствореному продукту присвоювати першу категорію зі списку
 */




/*
 * Додати можливість завантаження зображення для категорії
 * Додати можливість завантаження зображення для продукту (декілька зображень)
 */