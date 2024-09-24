using Order.Command.Domain.Events;

namespace Order.Command.Domain.Models;

public class Order : Aggregate<OrderId>
{
    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyList<OrderItem> OrderItems => _orderItems;
    public CustomerId? CustomerId { get; private set; } = default!;
    public OrderName OrderName { get; private set; } = default!;
    public ShippingAddress ShippingAddress { get; private set; } = default!;
    public BillingAddress BillingAddress { get; private set; } = default!;
    public OrderPayment Payment { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = default!;

    public Price TotalPrice => Price.From(OrderItems.Sum(x => x.Price.Value * x.Quantity));

    public Order Create(
        OrderId id,
        CustomerId? customerId,
        OrderName orderName,
        ShippingAddress shippingAddress,
        BillingAddress billingAddress,
        OrderPayment payment)
    {
        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            OrderName = orderName,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Payment = payment,
            Status = OrderStatus.Pending
        };

        AddDomainEvent(new OrderCreatedEvent(order));
        
        return order;
    }
    
    public Order Update(
        OrderName orderName,
        ShippingAddress shippingAddress,
        BillingAddress billingAddress,
        OrderStatus orderStatus)
    {
        var order = new Order
        {
            OrderName = orderName,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Status = orderStatus
        };
        
        AddDomainEvent(new OrderUpdatedEvent(order));
        return order;
    }

    public void Add(ProductId productId, int quantity, Price price)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price.Value);

        var orderItem = new OrderItem(Id, productId, quantity, price);
        _orderItems.Add(orderItem);
        
        AddDomainEvent(new OrderItemAddedEvent(orderItem));

    }

    public void Remove(ProductId productId)
    {
        var orderItem = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (orderItem is null) return;
        
        _orderItems.Remove(orderItem);
        AddDomainEvent(new OrderItemDeletedEvent(orderItem));
    }
}