using Order.Command.API.Authorization;
using Order.Command.Application.Orders.Commands.CreateOrder;

namespace Order.Command.API.Endpoints.CreateOrder;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Post("/customers/{customer_id}/orders/{order_id}", HandleAsync);
        Name("CreateOrder");
        Produces(StatusCodes.Status201Created);
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        ProducesProblem(StatusCodes.Status403Forbidden);
        ProducesProblem(StatusCodes.Status401Unauthorized);
        Summary("Creates a new order");
        Description("Creates a new order");
        Policies();
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var isAuthorize = await AuthorizationService.CanCreateOrderAsync(
            Context, request.CustomerId).ConfigureAwait(false);
        
        if (!isAuthorize)
            return Results.Forbid();
        
        var command = ToCommand(request);
        if (command is null)
            return Results.BadRequest("Null request");

        var result = await SendAsync(command).ConfigureAwait(false);
        var response = MapResult(result);
        return Results.Created($"customers/{request.CustomerId}/orders/{response.Id}", response);
    }

    private static CreateOrderCommand? ToCommand(Request? request)
    {
        return request is null
            ? null
            : new CreateOrderCommand(
                new OrderParameter(
                    Id: request.Id,
                    CustomerId: request.CustomerId,
                    OrderName: request.OrderName,
                    ShippingAddress: MapAddress(request.ShippingAddress),
                    BillingAddress: MapAddress(request.BillingAddress),
                    OrderPayment: MapPayment(request.Payment),
                    OrderItems: MapOrderItems(request.OrderItems)));
    }

    private static OrderParameter.Address? MapAddress(Request.ModuleAddress? address)
    {
        return address is null
            ? null
            : new OrderParameter.Address(
                Firstname: address.Firstname,
                Lastname: address.Lastname,
                EmailAddress: address.EmailAddress,
                AddressLine: address.AddressLine,
                Country: address.Country,
                State: address.State,
                ZipCode: address.ZipCode);
    }

    private static OrderParameter.Payment? MapPayment(Request.ModulePayment? payment)
    {
        return payment is null
            ? null
            : new OrderParameter.Payment(
                payment.CardName,
                payment.CardNumber,
                payment.Expiration,
                payment.Cvv,
                payment.PaymentMethod);
    }

    private static List<OrderParameter.OrderItem>? MapOrderItems(List<Request.ModuleOrderItem>? orderItems)
    {
        return orderItems?.Select(x =>
            new OrderParameter.OrderItem(
                x.ProductId,
                x.Quantity,
                x.Price
            )).ToList();
    }

    private static Response MapResult(CreateOrderResult result)
    {
        return new Response(result.Id.ToString());
    }
}