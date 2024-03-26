using Catalog.API.Features.Products.CreateProduct;
using Catalog.API.Features.Products.GetProduct;

namespace Catalog.API.Common
{
    public static class RoutingRegistrar
    {
        public static void RegisterEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapCreateProductEndpoint();
            app.MapGetProductEndpoint();
        }
    }
}
