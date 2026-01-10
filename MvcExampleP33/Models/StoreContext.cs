using Microsoft.EntityFrameworkCore;

namespace MvcExampleP33.Models;


public class StoreContext : DbContext
{
    // Конструктор за замовченням
    public StoreContext() : base() { }

    // Конструктор з параметрами для налаштування контексту
    public StoreContext(DbContextOptions<StoreContext> options) : base(options) { }

    // Визначення DbSet для сутностей
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ImageFile> Images { get; set; }

    // Метод для налаштування моделі та конфігурації бази даних
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Data Source=SILVERSTONE\\SQLEXPRESS;Initial Catalog=McvExampleP33;Integrated Security=True;Persist Security Info=False;Pooling=False;Multiple Active Result Sets=False;Connect Timeout=60;Encrypt=True;Trust Server Certificate=True;");
        }
    }
}