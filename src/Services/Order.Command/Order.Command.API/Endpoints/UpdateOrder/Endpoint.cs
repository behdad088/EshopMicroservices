using Order.Command.Application.Dtos;
using Order.Command.Application.Orders.Commands.UpdateOrder;

namespace Order.Command.API.Endpoints.UpdateOrder;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Put("/orders", HandleAsync);
        Name("UpdateOrder");
        Produces(StatusCodes.Status204NoContent);
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        ProducesProblem(StatusCodes.Status412PreconditionFailed);
        Summary("Update an existing order.");
        Description("Update an existing order");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var eTag = Context.Request.Headers.IfMatch;

        var command = MapToCommand(request, eTag);
        await SendAsync(command).ConfigureAwait(false);

        return TypedResults.NoContent();
    }

    private static UpdateOrderCommand MapToCommand(Request request, string? eTag)
    {
        return new UpdateOrderCommand(new UpdateOrderDto(
            request.Id,
            request.CustomerId,
            request.OrderName,
            MapAddress(request.ShippingAddress),
            MapAddress(request.BillingAddress),
            MapPayment(request.Payment),
            request.Status,
            eTag,
            request.OrderItems.Select(x => new OrderItems(
                x.Id,
                x.OrderId,
                x.ProductId,
                x.Quantity,
                x.Price)).ToList()
        ));
    }

    private static AddressDto MapAddress(Address addressDto)
    {
        return new AddressDto(
            Firstname: addressDto.Firstname,
            Lastname: addressDto.Lastname,
            EmailAddress: addressDto.EmailAddress,
            AddressLine: addressDto.AddressLine,
            Country: addressDto.Country,
            State: addressDto.State,
            ZipCode: addressDto.ZipCode);
    }

    private static PaymentDto MapPayment(Payment paymentDto)
    {
        return new PaymentDto(
            paymentDto.CardName,
            paymentDto.CardNumber,
            paymentDto.Expiration,
            paymentDto.Cvv,
            paymentDto.PaymentMethod);
    }


    private static Response MapToResponse(UpdateOrderResult result)
    {
        return new Response(result.IsSuccess);
    }
}