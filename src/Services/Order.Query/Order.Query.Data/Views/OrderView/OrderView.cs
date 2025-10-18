using System.Globalization;
using Order.Query.Data.Events;
using Order.Query.Data.Projections;

namespace Order.Query.Data.Views.OrderView;

public sealed class OrderView :
    IProjection<OrderView, OrderCreatedEvent>,
    IProjection<OrderView, OrderUpdatedEvent>,
    IProjection<OrderView, OrderDeletedEvent>
{
    public required string Id { get; set; }

    public string? CreatedAt { get; set; }
    public string? CreatedBy  { get; set; }
    public DateTimeOffset? LastModified  { get; set; }
    public string CustomerId   { get; set; } = null!;
    public string? OrderName   { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
    public Address ShippingAddress  { get; set; } = new();
    public Address BillingAddress  { get; set; } = new();
    public Payment PaymentMethod { get; set; } = new();
    public string OrderStatus { get; set; } = null!;
    public string? DeletedDate { get; set; }
    public decimal TotalPrice {  get; set; }
    public int OrderCreatedEventVersion  { get; set; }
    public int OrderUpdatedEventVersion  { get; set; }
    public int OrderDeletedEventVersion { get; set; }
    
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
    
    public bool CanUpdate(OrderCreatedEvent @event)
    {
        return @event.Version! > OrderCreatedEventVersion;
    }

    public void Apply(OrderCreatedEvent @event)
    {
        Id = @event.Id;
        CreatedAt = @event.CreatedAt;
        CreatedBy = @event.CreatedBy;
        LastModified = @event.LastModified;
        CustomerId = @event.CustomerId;
        OrderName = @event.OrderName;
        OrderItems =  @event.OrderItems.Select(x => 
            new OrderItem(x.Id.ToString(), x.ProductId.ToString(), x.Quantity!.Value, x.Price!.Value )).ToList();
        
        ShippingAddress = ShippingAddress with
        {
            Firstname = @event.ShippingAddress.Firstname,
            Lastname = @event.ShippingAddress.Lastname,
            EmailAddress = @event.ShippingAddress.EmailAddress,
            AddressLine = @event.ShippingAddress.AddressLine,
            Country = @event.ShippingAddress.Country,
            State = @event.ShippingAddress.State,
            ZipCode = @event.ShippingAddress.ZipCode
        };
        
        BillingAddress = BillingAddress with
        {
            Firstname = @event.BillingAddress.Firstname,
            Lastname = @event.BillingAddress.Lastname,
            EmailAddress = @event.BillingAddress.EmailAddress,
            AddressLine = @event.BillingAddress.AddressLine,
            Country = @event.BillingAddress.Country,
            State = @event.BillingAddress.State,
            ZipCode = @event.BillingAddress.ZipCode
        };
        
        PaymentMethod = PaymentMethod with
        {
            CardName = @event.PaymentMethod.CardName,
            CardNumber = @event.PaymentMethod.CardNumber,
            Expiration = @event.PaymentMethod.Expiration,
            Cvv = @event.PaymentMethod.Cvv,
            PaymentMethod = @event.PaymentMethod.PaymentMethod!.Value
        };
        OrderStatus = @event.OrderStatus;
        TotalPrice = @event.TotalPrice;
        OrderCreatedEventVersion = @event.Version!.Value;
        
    }

    public static OrderView CreateView(OrderCreatedEvent @event)
    {
        return new OrderView
        {
            Id = @event.Id
        };
    }

    public bool CanUpdate(OrderUpdatedEvent @event)
    {
        return @event.Version! > OrderUpdatedEventVersion;
    }

    public void Apply(OrderUpdatedEvent @event)
    {
        LastModified = @event.LastModified;
        OrderName = @event.OrderName;
        OrderItems =  @event.OrderItems.Select(x => 
            new OrderItem(x.Id.ToString(), x.ProductId.ToString(), x.Quantity!.Value, x.Price!.Value )).ToList();
        
        ShippingAddress = ShippingAddress with
        {
            Firstname = @event.ShippingAddress.Firstname,
            Lastname = @event.ShippingAddress.Lastname,
            EmailAddress = @event.ShippingAddress.EmailAddress,
            AddressLine = @event.ShippingAddress.AddressLine,
            Country = @event.ShippingAddress.Country,
            State = @event.ShippingAddress.State,
            ZipCode = @event.ShippingAddress.ZipCode
        };
        
        BillingAddress = BillingAddress with
        {
            Firstname = @event.BillingAddress.Firstname,
            Lastname = @event.BillingAddress.Lastname,
            EmailAddress = @event.BillingAddress.EmailAddress,
            AddressLine = @event.BillingAddress.AddressLine,
            Country = @event.BillingAddress.Country,
            State = @event.BillingAddress.State,
            ZipCode = @event.BillingAddress.ZipCode
        };
        
        PaymentMethod = PaymentMethod with
        {
            CardName = @event.PaymentMethod.CardName,
            CardNumber = @event.PaymentMethod.CardNumber,
            Expiration = @event.PaymentMethod.Expiration,
            Cvv = @event.PaymentMethod.Cvv,
            PaymentMethod = @event.PaymentMethod.PaymentMethod!.Value
        };
        OrderStatus = @event.OrderStatus;
        TotalPrice = @event.TotalPrice;
        OrderUpdatedEventVersion = @event.Version!.Value;
    }

    public static OrderView CreateView(OrderUpdatedEvent @event)
    {
        return new OrderView
        {
            Id = @event.Id
        };
    }

    public bool CanUpdate(OrderDeletedEvent @event)
    {
        return @event.Version! > OrderDeletedEventVersion;
    }

    public void Apply(OrderDeletedEvent @event)
    {
        DeletedDate = @event.DeletedDate;
        OrderDeletedEventVersion = @event.Version!.Value;
    }

    public static OrderView CreateView(OrderDeletedEvent @event)
    {
        return new OrderView
        {
            Id = @event.OrderId
        };
    }
}