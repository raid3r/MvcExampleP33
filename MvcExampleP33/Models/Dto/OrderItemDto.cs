namespace MvcExampleP33.Models.Dto;

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductTitle { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public decimal TotalPrice => UnitPrice * Quantity;
}