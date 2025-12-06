namespace Order.Command.API.IntegrationTests.Given.SqlGiven;

public record OrderConfiguration
{
    private readonly Ulid _orderId = Ulid.NewUlid();
    
    public OrderId Id { get; set; }
    public CustomerId CustomerId { get; set; } = CustomerId.From(Guid.NewGuid());
    public OrderName OrderName { get; set; } = OrderName.From("Test order");

    public Address ShippingAddress { get; set; } = new(
        "Test first name",
        "Test last name",
        "Test email",
        "Test street",
        "Test city",
        "Test country",
        "TestZ"
    );
    public Address BillingAddress { get; set; } = new(
        "Test first name",
        "Test last name",
        "Test email",
        "Test street",
        "Test city",
        "Test country",
        "TestZ"
    );

    public Payment Payment { get; set; } = new(
        "Test card holder",
        "4234432454657532",
        "12/2026",
        "123",
        0);

    public List<OrderItem> OrderItems { get; set; }

    public OrderConfiguration()
    {
        Id = OrderId.From(_orderId);
        OrderItems = [new OrderItem(Id, ProductId.From(Ulid.NewUlid()), 1, Price.From(10))];
    }
}



public static class CreateOrderConfiguration
{
    public static Order.Command.Domain.Models.Order ToOrderDb(this OrderConfiguration order)
    {
        return new Domain.Models.Order().Create(
            order.Id,
            order.CustomerId,
            order.OrderName,
            order.ShippingAddress,
            order.BillingAddress,
            order.Payment,
            order.OrderItems
        );
    }
}