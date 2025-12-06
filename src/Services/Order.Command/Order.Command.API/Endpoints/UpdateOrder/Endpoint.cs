using System.Net;
using Order.Command.Application.Orders.Commands.UpdateOrder;

namespace Order.Command.API.Endpoints.UpdateOrder;

public class Endpoint : EndpointBase<Request>
{
    public override void MapEndpoint()
    {
        Put("/customers/{customer_id}/orders/{order_id}", HandleAsync);
        Name("UpdateOrder");
        Produces(StatusCodes.Status204NoContent);
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        ProducesProblem(StatusCodes.Status412PreconditionFailed);
        ProducesProblem(StatusCodes.Status403Forbidden);
        ProducesProblem(StatusCodes.Status401Unauthorized);
        Summary("Update an existing order.");
        Description("Update an existing order");
        Policies();
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        using var _ = LogContext.PushProperty(LogProperties.OrderId, request.Id);
        using var __ = LogContext.PushProperty(LogProperties.CustomerId, request.CustomerId);
        
        var isAuthorize = await AuthorizationService.CanGetUpdateOrderAsync(
            Context, request.CustomerId).ConfigureAwait(false);
        
        if (!isAuthorize)
            return Results.Forbid();
        
        var eTag = Context.Request.Headers.IfMatch;

        var command = MapToCommand(request, eTag);
        var result = await SendAsync(command).ConfigureAwait(false);

        return result switch
        {
            Result.Succeed => Results.NoContent(),
            Result.InvalidEtag => Results.StatusCode((int)HttpStatusCode.PreconditionFailed),
            Result.OrderNotFound => Results.NotFound(request.Id),
            _ => Results.InternalServerError()
        };
    }

    private static UpdateOrderCommand MapToCommand(Request request, string? eTag)
    {
        return new UpdateOrderCommand(new UpdateOrderParameter(
            request.Id,
            request.CustomerId,
            request.OrderName,
            MapAddress(request.ShippingAddress),
            MapAddress(request.BillingAddress),
            MapPayment(request.Payment),
            request.Status,
            eTag,
            request.OrderItems?.Select(x => new UpdateOrderParameter.OrderItem(
                x.ProductId,
                x.Quantity,
                x.Price)).ToList()
        ));
    }

    private static UpdateOrderParameter.Address? MapAddress(ModuleAddress? address)
    {
        return address is null? 
            null 
            : new UpdateOrderParameter.Address(
            Firstname: address.Firstname,
            Lastname: address.Lastname,
            EmailAddress: address.EmailAddress,
            AddressLine: address.AddressLine,
            Country: address.Country,
            State: address.State,
            ZipCode: address.ZipCode);
    }

    private static UpdateOrderParameter.Payment? MapPayment(ModulePayment? payment)
    {
        return payment is null? null : new UpdateOrderParameter.Payment(
            payment.CardName,
            payment.CardNumber,
            payment.Expiration,
            payment.Cvv,
            payment.PaymentMethod);
    }
}