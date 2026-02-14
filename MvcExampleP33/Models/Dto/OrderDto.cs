using MvcExampleP33.Models;

namespace MvcExampleP33.Models.Dto;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

    public decimal TotalPrice => Items.Sum(i => i.TotalPrice);
}