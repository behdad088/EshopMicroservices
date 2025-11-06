using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Order.Query.API.Authorization;
using Order.Query.Features.OrderView.GetOrderById;

namespace Order.Query.API.Features.GetOrderById;

public class Endpoint(IAuthorizationService authorizationService) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/customer/{CustomerId}/orders/{OrderId}");
        DontCatchExceptions();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        if (!await authorizationService.CanGetOrderByIdAsync(User, req.CustomerId))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }
        
        var result = await new GetOrderByIdQuery
        {
            OrderId = req.OrderId!,
            CustomerId = req.CustomerId!
        }.ExecuteAsync(ct);

        switch (result)
        {
            case Result.NotFound:
                await Send.NotFoundAsync(ct);
                break;
            case Result.Success success:
                var version = success.Result.Version;
                HttpContext.Response.Headers.ETag = $"W/\"{version}\"";
                await Send.OkAsync(MapResponse(success.Result), ct); 
                break;
        }
    }

    private Response MapResponse(GetOrderByIdResult result)
    {
        return new Response(
            Id: result.Id,
            CustomerId: result.CustomerId,
            OrderName: result.OrderName,
            ShippingAddress: MapAddress(result.ShippingAddress),
            BillingAddress: MapAddress(result.BillingAddress),
            Payment: MapPayment(result.PaymentDetails),
            Status: result.Status,
            Version: result.Version,
            OrderItems: MapOrderItems(result.OrderItems));
    }

    private static Address MapAddress(GetOrderByIdResult.Address address)
    {
        return new Address(
            Firstname: address.Firstname,
            Lastname: address.Lastname,
            EmailAddress: address.EmailAddress,
            AddressLine: address.AddressLine,
            Country: address.Country,
            State: address.State,
            ZipCode: address.ZipCode);
    }

    private static Payment MapPayment(GetOrderByIdResult.Payment payment)
    {
        return new Payment(
            CardName: payment.CardName,
            CardNumber: payment.CardNumber,
            PaymentMethod: payment.PaymentMethod,
            Expiration: payment.Expiration,
            Cvv: payment.Cvv);
    }

    private static List<OrderItem> MapOrderItems(
        IReadOnlyCollection<GetOrderByIdResult.OrderItem> orderItems)
    {
        return orderItems.Select(x => new OrderItem(x.ProductId, x.Quantity, x.Price)).ToList();
    }
}