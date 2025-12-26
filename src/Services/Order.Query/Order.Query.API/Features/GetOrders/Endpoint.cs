using Order.Query.Features.OrderView.GetOrders;

namespace Order.Query.API.Features.GetOrders;

public class Endpoint : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/orders/all");
        DontCatchExceptions();
        Version(1);
        Policies(Authorization.Policies.CanGetAllOrders);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var result = await new GetOrdersQuery
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize
        }.ExecuteAsync(ct);

        await Send.OkAsync(MapResponse(result), ct);
    }

    private static Response MapResponse(PaginatedItems<GetOrdersResult> res)
    {
        var result = res.Data?.Select(x => new Order(
            x.Id,
            x.CustomerId,
            x.OrderName,
            MapAddress(x.ShippingAddress),
            MapAddress(x.BillingAddress),
            MapPayment(x.PaymentDetails),
            x.Status,
            MapOrderItems(x.OrderItems))).ToArray();

        return new Response(new PaginatedItems<Order>(
            res.PageIndex, res.PageSize,
            res.Count, result));
    }

    private static Address MapAddress(GetOrdersResult.Address address)
    {
        return new Address(
            address.Firstname,
            address.Lastname,
            address.EmailAddress,
            address.AddressLine,
            address.Country,
            address.State,
            address.ZipCode);
    }

    private static Payment MapPayment(GetOrdersResult.Payment payment)
    {
        return new Payment(
            payment.CardName,
            payment.CardNumber,
            PaymentMethod: payment.PaymentMethod,
            Expiration: payment.Expiration,
            Cvv: payment.Cvv);
    }

    private static List<OrderItem> MapOrderItems(
        IReadOnlyCollection<GetOrdersResult.OrderItem> orderItems)
    {
        return orderItems.Select(x => new OrderItem(x.ProductId, x.Quantity, x.Price)).ToList();
    }
}