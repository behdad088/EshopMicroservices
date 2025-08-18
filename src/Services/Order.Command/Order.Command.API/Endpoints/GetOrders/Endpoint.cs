using BuildingBlocks.Pagination;
using Order.Command.Application.Orders.Queries.GetOrders;

namespace Order.Command.API.Endpoints.GetOrders;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Get("/orders/all", HandleAsync);
        Name("GetOrders");
        Produces();
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status403Forbidden);
        ProducesProblem(StatusCodes.Status401Unauthorized);
        Summary("Gets the list of orders.");
        Description("Gets the list of orders.");
        Policies(Authorization.Policies.CanGetAllOrders);
    }


    public override async Task<IResult> HandleAsync(Request request)
    {
        var query = MapQuery(request);
        var result = await SendAsync(query).ConfigureAwait(false);
        var response = MapResponse(result);
        return TypedResults.Ok(response);
    }

    private static GetOrdersQuery MapQuery(Request request)
    {
        return new GetOrdersQuery(request.PageSize, request.PageIndex);
    }

    private static Response MapResponse(GetOrdersResult result)
    {
        return new Response(new PaginatedItems<ModuleOrder>(
            result.Orders.PageIndex,
            result.Orders.PageSize,
            result.Orders.Count,
            MapOrders(result.Orders.Data)
        ));
    }

    private static List<ModuleOrder> MapOrders(IEnumerable<GetOrderResponse> orders)
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
        List<GetOrderResponse.OrderItemResponse> orderItemParameters)
    {
        return orderItemParameters.Select(x =>
            new ModuleOrderItem(
                x.ProductId,
                x.Quantity,
                x.Price)).ToList();
    }

    private static ModuleAddress MapModelAddress(
        GetOrderResponse.AddressResponse addressParameter)
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
        GetOrderResponse.PaymentResponse paymentParameter)
    {
        return new ModulePayment(
            paymentParameter.CardName,
            paymentParameter.CardNumber,
            paymentParameter.Expiration,
            paymentParameter.Cvv,
            paymentParameter.PaymentMethod);
    }
}