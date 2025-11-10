using eshop.Shared.Pagination;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Order.Query.API.Authorization;
using Order.Query.Features.OrderView.GetOrdersByCustomer;

namespace Order.Query.API.Features.GetOrdersByCustomer;

public class Endpoint(IAuthorizationService authorizationService) : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/customer/{CustomerId}/orders");
        DontCatchExceptions();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        if (!await authorizationService.CanCanGetListOfOrdersByCustomerIdAsync(User, req.CustomerId))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var result = await new GetOrdersByCustomerQuery
        {
            CustomerId = req.CustomerId!,
            PageSize = req.PageSize,
            PageIndex = req.PageIndex,
        }.ExecuteAsync(ct)
        .ConfigureAwait(false);

        await Send.OkAsync(MapResponse(result), ct);
    }
    
    private Response MapResponse(PaginatedItems<GetOrdersByCustomerResult> res)
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

    private static Address MapAddress(GetOrdersByCustomerResult.Address address)
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

    private static Payment MapPayment(GetOrdersByCustomerResult.Payment payment)
    {
        return new Payment(
            payment.CardName,
            payment.CardNumber,
            PaymentMethod: payment.PaymentMethod,
            Expiration: payment.Expiration,
            Cvv: payment.Cvv);
    }

    private static List<OrderItem> MapOrderItems(
        IReadOnlyCollection<GetOrdersByCustomerResult.OrderItem> orderItems)
    {
        return orderItems.Select(x => new OrderItem(x.ProductId, x.Quantity, x.Price)).ToList();
    }
}