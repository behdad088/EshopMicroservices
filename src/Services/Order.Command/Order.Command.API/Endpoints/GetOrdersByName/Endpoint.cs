using eshop.Shared.Pagination;
using Order.Command.Application.Orders.Queries.GetOrdersByName;

namespace Order.Command.API.Endpoints.GetOrdersByName;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Get("/orders", HandleAsync);
        Name("GetOrdersByName");
        Produces();
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        ProducesProblem(StatusCodes.Status403Forbidden);
        ProducesProblem(StatusCodes.Status401Unauthorized);
        Summary("Gets orders by name.");
        Description("Gets orders by name");
        Policies(Authorization.Policies.CanGetListOfOrdersByOrderName);
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        using var _ = LogContext.PushProperty(LogProperties.CustomerName, request.CustomerName);
        
        var query = MapToQuery(request);
        var result = await SendAsync(query).ConfigureAwait(false);
        var response = MapToResponse(result);
        return Results.Ok(response);
    }

    private static GetOrdersByNameQuery MapToQuery(Request request)
    {
        return new GetOrdersByNameQuery(request.CustomerName, request.PageSize, request.PageIndex);
    }

    private static Response MapToResponse(GetOrdersByNameResult result)
    {
        return new Response(new PaginatedItems<ModuleOrder>(
            result.Orders.PageIndex,
            result.Orders.PageSize,
            result.Orders.Count,
            MapOrders(result.Orders.Data)
        ));
    }

    private static List<ModuleOrder> MapOrders(IEnumerable<GetOrderByNameResponse> orders)
    {
        return orders.Select(x => new ModuleOrder(
            x.Id.ToString(),
            x.CustomerId.ToString(),
            x.OrderName,
            MapModelAddress(x.ShippingAddress),
            MapModelAddress(x.BillingAddress),
            ModulePayment(x.Payment),
            x.Status,
            MapModuleOrderItem(x.OrderItems)
        )).ToList();
    }

    private static List<ModuleOrderItem> MapModuleOrderItem(
        List<GetOrderByNameResponse.OrderItemResponse> orderItemParameters)
    {
        return orderItemParameters.Select(x =>
            new ModuleOrderItem(
                x.ProductId,
                x.Quantity,
                x.Price)).ToList();
    }

    private static ModuleAddress MapModelAddress(
        GetOrderByNameResponse.AddressResponse addressParameter)
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
        GetOrderByNameResponse.PaymentResponse paymentParameter)
    {
        return new ModulePayment(
            paymentParameter.CardName,
            paymentParameter.CardNumber,
            paymentParameter.Expiration,
            paymentParameter.Cvv,
            paymentParameter.PaymentMethod);
    }
}