using eshop.Shared.CQRS.Query;
using eshop.Shared.Logger;

namespace Catalog.API.Features.Products.GetProductById;

public record GetProductByIdQuery(string Id) : IQuery<Result>;

public abstract record Result
{
    public record Success(GetProductById Product) : Result;
    public record NotFound(string Id) : Result;
}

internal class GetProductByIQueryHandler(
    IDocumentSession session) : IQueryHandler<GetProductByIdQuery, Result>
{
    private readonly ILogger _logger = Log.ForContext<GetProductByIQueryHandler>();
    
    public async Task<Result> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty(LogProperties.ProductId, query.Id);
        
        _logger.Information("Getting product.");
        var product = await session.LoadAsync<ProductDocument>(query.Id, cancellationToken).ConfigureAwait(false);

        if (product is null)
            return new Result.NotFound(query.Id);

        _logger.Information("Successfully retrieving the product.");
        return MapResult(product);
    }

    private static Result.Success MapResult(ProductDocument productDocument)
    {
        return new Result.Success(
            Product: new GetProductById(
                Id: Ulid.Parse(productDocument.Id),
                Name: productDocument.Name!,
                Category: productDocument.Category,
                Description: productDocument.Description,
                ImageFile: productDocument.ImageFile!,
                Price: productDocument.Price,
                productDocument.Version));
    }
}