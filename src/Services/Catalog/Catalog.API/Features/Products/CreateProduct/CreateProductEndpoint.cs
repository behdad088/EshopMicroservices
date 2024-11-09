namespace Catalog.API.Features.Products.CreateProduct;

public static class CreateProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/products", CreateProduct)
            .WithName("CreateProduct")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Product")
            .WithDescription("Create Product");
        return app;
    }

    private static async Task<Created<CreateProductResponse>> CreateProduct(CreateProductRequest request,
        ISender sender)
    {
        var command = request.ToCommand();
        var result = await sender.Send(command).ConfigureAwait(false);
        var response = result.ToResponse();
        return TypedResults.Created($"/api/v1/catalog/products/{response?.Id}", response);
    }

    private static CreateProductCommand ToCommand(this CreateProductRequest request)
    {
        return new CreateProductCommand(Id: request.Id,
            Description: request.Description,
            ImageFile: request.ImageFile,
            Name: request.Name,
            Price: request.Price,
            Category: request.Category);
    }

    private static CreateProductResponse? ToResponse(this CreateProductResult? result)
    {
        return result is null
            ? null
            : new CreateProductResponse(
                Id: result.Id,
                Name: result.Name,
                Description: result.Description,
                ImageFile: result.ImageFile,
                Price: result.Price,
                Category: result.Category);
    }
}