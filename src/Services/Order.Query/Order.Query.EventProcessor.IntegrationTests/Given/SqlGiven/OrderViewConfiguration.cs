
using System.Globalization;
using Order.Query.Features.OrderView;

namespace Order.Query.EventProcessor.IntegrationTests.Given.SqlGiven;

public class OrderViewConfiguration
{
    public string Id { get; set; }

    public string CreatedAt { get; init; } =
        DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    public string? CreatedBy  { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? LastModified  { get; set; } = DateTimeOffset.UtcNow;
    public string CustomerId   { get; set; } = Guid.NewGuid().ToString();
    public string? OrderName   { get; set; } = "Test Order name";
    public List<OrderItem> OrderItems { get; set; } = new()
    {
        new OrderItem(Id: Ulid.NewUlid().ToString(), ProductId: Ulid.NewUlid().ToString(), Quantity: 1, Price: 100)
    };

    public Address ShippingAddress  { get; set; } = new()
    {
        Firstname = "Test",
        Lastname = "Test",
        EmailAddress = "test@test.com",
        AddressLine = "Test Address line",
        Country = "Sweden",
        State = "WA",
        ZipCode = "12345"
    };

    public Address BillingAddress { get; set; } = new()
    {
        Firstname = "Test",
        Lastname = "Test",
        EmailAddress = "test@test.com",
        AddressLine = "Test Address line",
        Country = "Sweden",
        State = "WA",
        ZipCode = "12345"
    };

    public Payment PaymentMethod { get; set; } = new()
    {
        CardName = "Test Card Name",
        CardNumber = "1234567890234342",
        Expiration = "03/29",
        Cvv = "123",
        PaymentMethod = 1
    };

    public string OrderStatus { get; set; } = "pending";
    public string? DeletedDate { get; set; } = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    public decimal TotalPrice {  get; set; } = 100;
    public int OrderCreatedEventVersion  { get; set; } = 1;
    public int OrderUpdatedEventVersion  { get; set; } = 0;
    public int OrderDeletedEventVersion { get; set; } = 0;

    public record OrderItem(
        string Id,
        string ProductId,
        int Quantity,
        decimal Price
    );

    public record Address
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddress { get; set; }
        public string AddressLine { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }

    public record Payment
    {
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string Cvv { get; set; }
        public int PaymentMethod { get; set; }
    };
}

public static class OrderViewConfigurationExtension
{
    public static OrderView ToDbModel(this OrderViewConfiguration configuration)
    {
        return new OrderView
        {
            Id = configuration.Id,
            CreatedAt = configuration.CreatedAt,
            CreatedBy = configuration.CreatedBy,
            LastModified = configuration.LastModified,
            CustomerId = configuration.CustomerId,
            OrderName = configuration.OrderName,
            OrderItems = configuration.OrderItems
                .Select(x => new OrderView.OrderItem(x.Id, x.ProductId, x.Quantity, x.Price)).ToList(),
            ShippingAddress = ToAddress(configuration.ShippingAddress),
            BillingAddress = ToAddress(configuration.BillingAddress),
            OrderStatus = configuration.OrderStatus,
            DeletedDate = configuration.DeletedDate,
            TotalPrice = configuration.TotalPrice,
            OrderCreatedEventVersion = configuration.OrderCreatedEventVersion,
            OrderUpdatedEventVersion = configuration.OrderUpdatedEventVersion,
            OrderDeletedEventVersion = configuration.OrderDeletedEventVersion

        };
    }

    private static OrderView.Address ToAddress(OrderViewConfiguration.Address address)
    {
        return new OrderView.Address
        {
            Firstname = address.Firstname,
            Lastname = address.Lastname,
            EmailAddress = address.EmailAddress,
            AddressLine = address.AddressLine,
            Country = address.Country,
            State = address.State,
            ZipCode = address.ZipCode
        };
    }
}