using System.Text.RegularExpressions;
using ValueOf;

namespace Order.Command.Domain.Models.ValueObjects;

public class CustomerId : ValueOf<Guid, CustomerId>
{
}

public class ProductId : ValueOf<Ulid, ProductId>
{
}

public class OrderId : ValueOf<Ulid, OrderId>
{
}

public class OrderItemId : ValueOf<Ulid, OrderItemId>
{
}

public partial class VersionId : ValueOf<int, VersionId>
{
    public static VersionId InitialVersion => From(1);

    public static VersionId FromWeakEtag(string etag)
    {
        if (string.IsNullOrWhiteSpace(etag) || !EtagRegex().IsMatch(etag))
        {
            throw new InvalidOperationException($"Invalid Etag value: {etag}.");
        }

        return From(int.Parse(etag[3..^1]));
    }

    public VersionId Increment() => From(Value + 1);
    
    [GeneratedRegex("""^W\/"\d+"$""")]
    private static partial Regex EtagRegex();
}

public class ProductName : ValueOf<string, ProductName>
{
    public static ProductName? FromNullable(string? value)
    {
        return value == null ? null : From(value);
    }
}

public class OrderName : ValueOf<string, OrderName>
{
    private const int DefaultLength = 5;

    protected override void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(Value.Length, DefaultLength);
    }
}

public class OrderPayment : ValueOf<Payment, OrderPayment>
{
    protected override void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Value.CardName);
        ArgumentException.ThrowIfNullOrWhiteSpace(Value.CardNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(Value.Expiration);
        ArgumentOutOfRangeException.ThrowIfEqual(Value.CVV.Length, 3);
    }
}

public class Price : ValueOf<decimal, Price>
{
}

public class OrderStatus : ValueOf<string, OrderStatus>
{
    public static readonly OrderStatus Pending = From(OrderStatuses.Pending);
    public static readonly OrderStatus Completed = From(OrderStatuses.Completed);
    public static readonly OrderStatus Cancelled = From(OrderStatuses.Cancelled);

    public static OrderStatus Parse(string orderStatusRaw)
    {
        return orderStatusRaw.ToLowerInvariant() switch
        {
            OrderStatuses.Pending => Pending,
            OrderStatuses.Completed => Completed,
            OrderStatuses.Cancelled => Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(orderStatusRaw))
        };
    }
}