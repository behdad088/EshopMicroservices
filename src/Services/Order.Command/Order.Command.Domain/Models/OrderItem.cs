namespace Order.Command.Domain.Models;

public class OrderItem : Entity<OrderItemId>
{
    public OrderItem(OrderId orderId, ProductId productId, int quantity, Price price)
    {
        Id = OrderItemId.From(Guid.NewGuid());
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }
    
    public OrderId OrderId { get; private set; }
    public ProductId ProductId { get; private set; }
    public int Quantity { get; private set; } = default!;
    public Price Price { get; private set; }
}