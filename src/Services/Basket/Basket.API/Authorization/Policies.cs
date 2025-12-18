using Microsoft.AspNetCore.Authorization;

namespace Basket.API.Authorization;

public static class Policies
{
    public const string BasketUserBasketDeletePermission = "basket:user-basket:delete";
    public const string BasketUserBasketGetPermission = "basket:user-basket:get";
    public const string BasketUserBasketStorePermission = "basket:user-basket:store";
    public const string BasketUserBasketCheckoutPermission = "basket:user-basket:checkout";

    public const string CanDeleteBasket = nameof(CanDeleteBasket);
    public const string CanGetBasket = nameof(CanGetBasket);
    public const string CanStoreBasket = nameof(CanStoreBasket);
    public const string CanCheckoutBasket = nameof(CanCheckoutBasket);
    
    public static void ConfigureAuthorization(AuthorizationOptions options)
    {
        options.AddPolicy(CanDeleteBasket, 
            policy => policy.AddRequirements(new UserIdRequirement([BasketUserBasketDeletePermission])));
    
        options.AddPolicy(CanGetBasket, 
            policy => policy.AddRequirements(new UserIdRequirement([BasketUserBasketGetPermission])));
    
        options.AddPolicy(CanStoreBasket, 
            policy => policy.AddRequirements(new UserIdRequirement([BasketUserBasketStorePermission])));
        
        options.AddPolicy(CanCheckoutBasket, 
            policy => policy.AddRequirements(new UserIdRequirement([BasketUserBasketCheckoutPermission])));
    }
}