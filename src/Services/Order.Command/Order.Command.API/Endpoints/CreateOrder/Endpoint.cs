using Order.Command.Application.Orders.Commands.CreateOrder;

namespace Order.Command.API.Endpoints.CreateOrder;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Post("/orders", HandleAsync);
        Name("CreateOrder");
        Produces(StatusCodes.Status201Created);
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        Summary("Creates a new order");
        Description("Creates a new order");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var command = ToCommand(request);
        if (command is null)
            return Results.BadRequest("Null request");

        var result = await SendAsync(command).ConfigureAwait(false);
        var response = MapResult(result);
        return Results.Created($"/orders/{response.Id}", response);
    }

    private static CreateOrderCommand? ToCommand(Request? request)
    {
        return request is null
            ? null
            : new CreateOrderCommand(
                new OrderDto(
                    Id: request.Id,
                    CustomerId: request.CustomerId,
                    OrderName: request.OrderName,
                    ShippingAddress: MapAddress(request.ShippingAddress),
                    BillingAddress: MapAddress(request.BillingAddress),
                    OrderPayment: MapPayment(request.Payment),
                    OrderItems: request.OrderItems.Select(x =>
                        new OrderDto.OrderItem(
                            x.Id,
                            x.OrderId,
                            x.ProductId,
                            x.Quantity,
                            x.Price
                        )).ToList()));
    }

    private static OrderDto.Address MapAddress(AddressDto addressDto)
    {
        return new OrderDto.Address(
            Firstname: addressDto.Firstname,
            Lastname: addressDto.Lastname,
            EmailAddress: addressDto.EmailAddress,
            AddressLine: addressDto.AddressLine,
            Country: addressDto.Country,
            State: addressDto.State,
            ZipCode: addressDto.ZipCode);
    }

    private static OrderDto.Payment MapPayment(PaymentDto paymentDto)
    {
        return new OrderDto.Payment(
            paymentDto.CardName,
            paymentDto.CardNumber,
            paymentDto.Expiration,
            paymentDto.Cvv,
            paymentDto.PaymentMethod);
    }


    private static Response MapResult(CreateOrderResult result)
    {
        return new Response(result.Id);
    }
}