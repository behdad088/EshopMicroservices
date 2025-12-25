using Catalog.API.Authorization;

namespace Catalog.API.Features.Products.UpdateProduct;

public static class UpdateProductEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/products", UpdateProduct)
            .WithName("UpdateProduct")
            .Produces<UpdateProductResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status412PreconditionFailed)
            .WithSummary("Update Product")
            .WithDescription("Update Product")
            .RequireAuthorization(Policies.CanUpdateProduct);
        return app;
    }

    private static async Task<IResult> UpdateProduct(
        HttpContext context,
        UpdateProductRequest request,
        ISender sender)
    {
        var eTag = context.Request.Headers.IfMatch;
        var command = request.ToCommand(eTag);

        if (command is null)
            return Results.BadRequest("Request is null");

        var result = await sender.Send(command).ConfigureAwait(false);
        
        return result switch
        {
            Result.NotFound notFound => Results.NotFound(notFound.Id),
            Result.InvalidEtag invalidEtag => Results.Problem($"Invalid Etag {invalidEtag}",
                statusCode: StatusCodes.Status412PreconditionFailed),
            Result.Success => Results.NoContent(),
            _ => Results.InternalServerError()
        };
    }

    private static UpdateProductCommand? ToCommand(this UpdateProductRequest? request, string? etag)
    {
        return request is null
            ? null
            : new UpdateProductCommand(
                Id: request.Id,
                Name: request.Name,
                Category: request.Category,
                Description: request.Description,
                ImageFile: request.ImageFile,
                Price: request.Price,
                Etag: etag);
    }
}