using BuildingBlocks.Pagination;
using Order.Command.Application.Orders.Queries.GetOrderByCustomer;

namespace Order.Command.API.Endpoints.GetOrdersByCustomer;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Get("/orders/customer", HandleAsync);
        Name("GetOrdersByCustomer");
        Produces();
        ProducesProblem(StatusCodes.Status400BadRequest);
        Summary("Gets orders by customer.");
        Description("Gets orders by customer.");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var query = ToQuery(request);
        var result = await SendAsync(query).ConfigureAwait(false);
        var response = MapResponse(result);
        return TypedResults.Ok(response);
    }

    private static GetOrderByCustomerQuery ToQuery(Request request)
    {
        return new GetOrderByCustomerQuery(request.CustomerId, request.PageSize, request.PageIndex);
    }

    private static Response MapResponse(GetOrderByCustomerResult result)
    {
        return new Response(new PaginatedItems<ModuleOrder>(
            result.Orders.PageIndex,
            result.Orders.PageSize,
            result.Orders.Count,
            MapOrders(result.Orders.Data)
        ));
    }

    private static List<ModuleOrder> MapOrders(IEnumerable<GetOrderByCustomerParameter> orders)
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

    private static List<ModuleOrderItem> MapModuleOrderItem(List<OrderItemParameter> orderItemParameters)
    {
        return orderItemParameters.Select(x =>
            new ModuleOrderItem(
                x.Id,
                x.ProductId,
                x.Quantity,
                x.Price)).ToList();
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