using Order.Command.Application.Orders.Queries.GetOrderById;

namespace Order.Command.API.Endpoints.GetOrderById;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Get("/customers/{customer_id}/orders/{id}");
        Name("GetOrdersById");
        Produces();
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        ProducesProblem(StatusCodes.Status403Forbidden);
        ProducesProblem(StatusCodes.Status401Unauthorized);
        Summary("Gets orders by Id.");
        Description("Gets orders by Id");
        Policies();
    }

    protected override async Task<IResult> HandleAsync(Request request, EndpointContext ctx)
    {
        using var _ = LogContext.PushProperty(LogProperties.OrderId, request.Id);
        using var __ = LogContext.PushProperty(LogProperties.CustomerId, request.CustomerId);

        var isAuthorize = await ctx.Authorization.CanGetOrderByIdAsync(
            ctx.HttpContext, request.CustomerId).ConfigureAwait(false);

        if (!isAuthorize)
            return Results.Forbid();

        var query = MapToQuery(request);
        var result = await ctx.SendAsync(query).ConfigureAwait(false);
        ctx.HttpContext.Response.Headers.ETag = $"W/\"{result.Order.Version}\"";

        var response = MapToResponse(result);
        return Results.Ok(response);
    }

    private static GetOrdersByIdQuery MapToQuery(Request request)
    {
        return new GetOrdersByIdQuery(request.Id, request.CustomerId);
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

    private static List<ModuleOrderItem> MapModuleOrderItem(
        List<GetOrderByIdResponse.OrderItemResponse> orderItemParameters)
    {
        return orderItemParameters.Select(x => new ModuleOrderItem(x.ProductId, x.Quantity, x.Price)).ToList();
    }

    private static ModuleAddress MapModelAddress(
        GetOrderByIdResponse.AddressResponse addressParameter)
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

    private static ModulePayment ModulePayment(
        GetOrderByIdResponse.PaymentResponse paymentParameter)
    {
        return new ModulePayment(
            paymentParameter.CardName,
            paymentParameter.CardNumber,
            paymentParameter.Expiration,
            paymentParameter.Cvv,
            paymentParameter.PaymentMethod);
    }
}
