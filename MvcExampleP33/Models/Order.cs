namespace MvcExampleP33.Models;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.New;

    public virtual User User { get; set; }

    public virtual ICollection<OrderItem> Items { get; set; } = [];

    public bool IsPaid { get; set; } = false;
}
