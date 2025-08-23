using Microsoft.AspNetCore.Authorization;

namespace Catalog.API.Authorization;

public static class Policies
{
    public const string CatalogProductCreatePermission = "catalog:product:create";
    public const string CatalogProductUpdatePermission = "catalog:product:update";
    public const string CatalogProductDeletePermission = "catalog:product:delete";

    public const string CanCreateProduct = nameof(CanCreateProduct);
    public const string CanUpdateProduct = nameof(CanUpdateProduct);
    public const string CanDeleteProduct = nameof(CanDeleteProduct);
    private const string PolicyName = "permissions";
    
    public static void ConfigureAuthorization(AuthorizationOptions options)
    {
        options.AddPolicy(CanCreateProduct, 
            policy => policy.RequireClaim(PolicyName, CatalogProductCreatePermission));
    
        options.AddPolicy(CanUpdateProduct, 
            policy => policy.RequireClaim(PolicyName, CatalogProductUpdatePermission));
    
        options.AddPolicy(CanDeleteProduct, 
            policy => policy.RequireClaim(PolicyName, CatalogProductDeletePermission));
    }
}