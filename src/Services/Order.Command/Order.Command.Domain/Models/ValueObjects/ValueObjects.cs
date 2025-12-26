using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using eshop.Shared.Exceptions;
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

public class OutboxId : ValueOf<Ulid, OutboxId>
{
}

public class AggregateId : ValueOf<Ulid, AggregateId>
{
}

public class EventType : ValueOf<string, EventType>
{
}

public class NumberOfDispatchTry : ValueOf<int, NumberOfDispatchTry>
{
    public static NumberOfDispatchTry InitialValue => From(0);
    public NumberOfDispatchTry Increment() => From(Value + 1);
}

public class Payload : ValueOf<string, Payload>
{
    private static readonly JsonSerializerOptions
        SCaseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };

    public static Payload Serialize(IDomainEvent @event)
    {
        return From(JsonSerializer.Serialize(@event, @event.GetType(), SCaseInsensitiveOptions));
    }
}

public class DeleteDate : ValueOf<string, DeleteDate>
{
    public static DeleteDate ToIso8601UtcFormat(DateTimeOffset dateTimeOffset)
    {
        return From(dateTimeOffset.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
    }
}

public class DispatchDateTime : ValueOf<DateTimeOffset, DispatchDateTime>
{
    public static string ToIso8601UtcFormat(DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
    }

    public static DispatchDateTime Now()
    {
        return From(DateTimeOffset.Now);
    }

    public static DispatchDateTime InTwoMinutes()
    {
        return From(DateTimeOffset.UtcNow.AddMinutes(2));
    }

    public static bool operator <=(DispatchDateTime? a, DispatchDateTime? b)
    {
        switch (a)
        {
            case null when b is null:
            case null:
                return true;
        }

        if (b is null)
            return false;

        return a.Value <= b.Value;
    }

    public static bool operator >=(DispatchDateTime? a, DispatchDateTime? b)
    {
        switch (a)
        {
            case null when b is null:
                return true;
            case null:
                return false;
        }

        if (b is null)
            return true;

        return a.Value >= b.Value;
    }
}

public class IsDispatched : ValueOf<bool, IsDispatched>
{
    public static IsDispatched Yes => From(true);
    public static IsDispatched No => From(false);
}

public class OrderItemId : ValueOf<Ulid, OrderItemId>
{
}

public class AggregateType : ValueOf<string, AggregateType>
{
}

public partial class VersionId : ValueOf<int, VersionId>
{
    public static VersionId InitialVersion => From(1);

    public static VersionId FromWeakEtag(string etag)
    {
        if (string.IsNullOrWhiteSpace(etag) || !EtagRegex().IsMatch(etag))
        {
            throw new InvalidEtagException($"Invalid Etag value: {etag}.");
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

    protected override void Validate()
    {
        Parse(Value);
    }
}