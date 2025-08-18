using Microsoft.AspNetCore.Authorization;

namespace Order.Command.API.Authorization;

public static class Policies
{ 
    public const string OrdersCommandCanCreateOrderPermission = "orders_command:order:create";
    public const string OrdersCommandCanDeleteOrderPermission = "orders_command:order:delete";
    public const string OrdersCommandCanGetOrderPermission = "orders_command:order:get";
    public const string OrdersCommandCanGetAllOrdersPermission = "orders_command:order:all";
    public const string OrdersCommandCanGetOrdersListsByCustomerIdPermission = "orders_command:order:all-by-customer-id";
    public const string OrdersCommandCanGetOrdersListsByOrderNamePermission = "orders_command:order:all-by-order-name";
    public const string OrdersCommandCanUpdateOrderPermission = "orders_command:order:update";

    public const string CanCreateOrder = nameof(CanCreateOrder);
    public const string CanDeleteOrder = nameof(CanDeleteOrder);
    public const string CanGetOrder = nameof(CanGetOrder);
    public const string CanGetAllOrders = nameof(CanGetAllOrders);
    public const string CanGetListOfOrdersByCustomerId = nameof(CanGetListOfOrdersByCustomerId);
    public const string CanGetListOfOrdersByOrderName = nameof(CanGetListOfOrdersByOrderName);
    public const string CanUpdateOrder = nameof(CanUpdateOrder);

    private const string ClaimType = "permissions";
    public static void ConfigureAuthorization(AuthorizationOptions options)
    {
        options.AddPolicy(CanCreateOrder,
            policy => policy.AddRequirements(new UserIdRequirement([OrdersCommandCanCreateOrderPermission])));

        options.AddPolicy(CanDeleteOrder,
            policy => policy.AddRequirements(new UserIdRequirement([OrdersCommandCanDeleteOrderPermission])));

        options.AddPolicy(CanGetOrder,
            policy => policy.AddRequirements(new UserIdRequirement([OrdersCommandCanGetOrderPermission])));
        
        options.AddPolicy(CanGetAllOrders,
            policy => policy.RequireClaim(ClaimType, OrdersCommandCanGetAllOrdersPermission));
        
        options.AddPolicy(CanGetListOfOrdersByCustomerId,
            policy => policy.AddRequirements(new UserIdRequirement([OrdersCommandCanGetOrdersListsByCustomerIdPermission])));
        
        options.AddPolicy(CanGetListOfOrdersByOrderName,
            policy => policy.RequireClaim(ClaimType, OrdersCommandCanGetOrdersListsByOrderNamePermission));
        
        options.AddPolicy(CanUpdateOrder,
            policy => policy.AddRequirements(new UserIdRequirement([OrdersCommandCanUpdateOrderPermission])));
    }
}