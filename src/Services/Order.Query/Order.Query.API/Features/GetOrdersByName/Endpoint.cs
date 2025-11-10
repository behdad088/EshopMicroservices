using eshop.Shared.Pagination;
using FastEndpoints;
using Order.Query.Features.OrderView.GetOrdersByName;

namespace Order.Query.API.Features.GetOrdersByName;

public class Endpoint : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/orders");
        DontCatchExceptions();
        Version(1);
        Policies(Authorization.Policies.CanGetListOfOrdersByOrderName);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var result = await new GetOrdersByOrderNameQuery
        {
            OrderName = req.OrderName!,
            PageSize = req.PageSize,
            PageIndex = req.PageIndex,
        }.ExecuteAsync(ct)
        .ConfigureAwait(false);

        await Send.OkAsync(MapResponse(result), ct);
    }
    
    private Response MapResponse(PaginatedItems<GetOrdersByOrderNameResult> res)
    {
        var result = res.Data.Select(x => new Order(
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

    private static Address MapAddress(GetOrdersByOrderNameResult.Address address)
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

    private static Payment MapPayment(GetOrdersByOrderNameResult.Payment payment)
    {
        return new Payment(
            payment.CardName,
            payment.CardNumber,
            PaymentMethod: payment.PaymentMethod,
            Expiration: payment.Expiration,
            Cvv: payment.Cvv);
    }

    private static List<OrderItem> MapOrderItems(
        IReadOnlyCollection<GetOrdersByOrderNameResult.OrderItem> orderItems)
    {
        return orderItems.Select(x => new OrderItem(x.ProductId, x.Quantity, x.Price)).ToList();
    }
}