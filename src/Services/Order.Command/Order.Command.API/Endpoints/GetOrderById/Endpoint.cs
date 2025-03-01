using Order.Command.Application.Orders.Queries.GetOrderById;

namespace Order.Command.API.Endpoints.GetOrderById;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Get("/orders/{id}", HandleAsync);
        Name("GetOrdersById");
        Produces();
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        Summary("Gets orders by Id.");
        Description("Gets orders by Id");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var query = MapToQuery(request);
        var result = await SendAsync(query).ConfigureAwait(false);
        Context.Response.Headers.ETag = $"W/\"{result.Order.Version}\"";

        var response = MapToResponse(result);
        return TypedResults.Ok(response);
    }

    private static GetOrdersByIdQuery MapToQuery(Request request)
    {
        return new GetOrdersByIdQuery(request.Id);
    }

    private static Response MapToResponse(GetOrdersByIdResult result)
    {
        return new Response(
            result.Order.Id.ToString(),
            result.Order.CustomerId.ToString(),
            result.Order.OrderName,
            MapModelAddress(result.Order.ShippingAddress),
            MapModelAddress(result.Order.BillingAddress),
            ModulePayment(result.Order.Payment),
            result.Order.Status,
            MapModuleOrderItem(result.Order.OrderItems));
    }

    private static List<ModuleOrderItem> MapModuleOrderItem(List<OrderItemParameter> orderItemParameters)
    {
        return orderItemParameters.Select(x => new ModuleOrderItem(x.ProductId, x.Quantity, x.Price)).ToList();
    }

    private static ModuleAddress MapModelAddress(AddressParameter addressParameter)
    {
        return new ModuleAddress(
            addressParameter.Firstname,
            addressParameter.Lastname,
            addressParameter.EmailAddress,
            addressParameter.AddressLine,
            addressParameter.Country,
            addressParameter.State,
            addressParameter.ZipCode);
    }

    private static ModulePayment ModulePayment(PaymentParameter paymentParameter)
    {
        return new ModulePayment(
            paymentParameter.CardName,
            paymentParameter.CardNumber,
            paymentParameter.Expiration,
            paymentParameter.Cvv,
            paymentParameter.PaymentMethod);
    }
}