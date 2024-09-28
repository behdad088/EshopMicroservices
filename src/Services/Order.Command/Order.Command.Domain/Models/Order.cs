using System.ComponentModel.DataAnnotations;
using Order.Command.Domain.Events;

namespace Order.Command.Domain.Models;

public class Order : Aggregate<OrderId>
{
    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyList<OrderItem> OrderItems => _orderItems;
    public CustomerId? CustomerId { get; private set; } = default!;
    public OrderName OrderName { get; private set; } = default!;
    public Address ShippingAddress { get; private set; } = default!;
    public Address BillingAddress { get; private set; } = default!;
    public Payment Payment { get; private set; } = default!;
    public OrderStatus Status { get; private set; } = default!;

    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;
    
    public Price TotalPrice
    {
        get { return Price.From(OrderItems.Sum(x => x.Price.Value * x.Quantity)); }
        set{ }
    }

    public Order Create(
        OrderId id,
        CustomerId? customerId,
        OrderName orderName,
        Address shippingAddress,
        Address billingAddress,
        Payment payment,
        List<OrderItem>? orderItems = null)
    {
        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            OrderName = orderName,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Payment = payment,
            Status = OrderStatus.Pending,
        };
        order._orderItems.AddRange(orderItems?.Select(x => new OrderItem(id, x.ProductId, x.Quantity, x.Price)).ToArray() ?? []);
        order.AddDomainEvent(new OrderCreatedEvent(order));
        
        return order;
    }
    
    public void Update(
        OrderName orderName,
        Address shippingAddress,
        Address billingAddress,
        OrderStatus orderStatus)
    {
        OrderName = orderName;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        Status = orderStatus;
        
        AddDomainEvent(new OrderUpdatedEvent(this));
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