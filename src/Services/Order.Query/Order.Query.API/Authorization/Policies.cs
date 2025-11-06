using Microsoft.AspNetCore.Authorization;

namespace Order.Query.API.Authorization;

public class Policies
{
    public const string OrdersQueryCanGetOrderPermission = "orders_query:order:get";
    public const string OrdersQueryCanGetAllOrdersPermission = "orders_query:order:all";
    public const string OrdersQueryCanGetOrdersListsByCustomerIdPermission = "orders_query:order:all-by-customer-id";
    public const string OrdersQueryCanGetOrdersListsByOrderNamePermission = "orders_query:order:all-by-order-name";
    
    public const string CanGetOrder = nameof(CanGetOrder);
    public const string CanGetAllOrders = nameof(CanGetAllOrders);
    public const string CanGetListOfOrdersByCustomerId = nameof(CanGetListOfOrdersByCustomerId);
    public const string CanGetListOfOrdersByOrderName = nameof(CanGetListOfOrdersByOrderName);
    
    private const string ClaimType = "permissions";
    
    public static void ConfigureAuthorization(AuthorizationOptions options)
    {

        options.AddPolicy(CanGetOrder,
            policy => policy.AddRequirements(new UserIdRequirement([OrdersQueryCanGetOrderPermission])));
        
        options.AddPolicy(CanGetAllOrders,
            policy => policy.RequireClaim(ClaimType, OrdersQueryCanGetAllOrdersPermission));
        
        options.AddPolicy(CanGetListOfOrdersByCustomerId,
            policy => policy.AddRequirements(new UserIdRequirement([OrdersQueryCanGetOrdersListsByCustomerIdPermission])));
        
        options.AddPolicy(CanGetListOfOrdersByOrderName,
            policy => policy.RequireClaim(ClaimType, OrdersQueryCanGetOrdersListsByOrderNamePermission));
    }
}