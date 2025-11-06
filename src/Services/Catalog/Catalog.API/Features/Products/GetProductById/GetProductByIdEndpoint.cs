namespace Catalog.API.Features.Products.GetProductById;

public static class GetProductByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", GetProductById)
            .WithName("GetProductById")
            .Produces<GetProductByIdResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Product by id")
            .WithDescription("Get Product by id")
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetProductById(
        HttpContext context,
        string id,
        ISender sender)
    {
        var queryResult = await sender.Send(new GetProductByIdQuery(id)).ConfigureAwait(false);

        switch (queryResult)
        {
            case Result.NotFound notFound:
                return Results.NotFound(notFound.Id);
            case Result.Success success:
                var etag = $"W/\"{success.Product.Version}\"";
                if (context.Request.Headers.IfNoneMatch == etag)
                {
                    return Results.StatusCode(StatusCodes.Status304NotModified);
                }
                context.Response.Headers.ETag = etag;
                var result = ToResponse(success);
                return Results.Ok(result);
            default:
                return Results.InternalServerError();
        }
    }

    private static GetProductByIdResponse? ToResponse(Result.Success? result)
    {
        return result is null
            ? null
            : new GetProductByIdResponse(
                Id: result.Product.Id,
                Name: result.Product.Name,
                Category: result.Product.Category,
                Description: result.Product.Description,
                ImageFile: result.Product.ImageFile,
                Price: result.Product.Price);
    }
}