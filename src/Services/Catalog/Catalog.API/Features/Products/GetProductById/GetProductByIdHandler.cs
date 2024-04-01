namespace Catalog.API.Features.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdResult>;

public record GetProductByIdResult(ProductModule Product);

internal class GetProductByIQuerydHandler(
    IDocumentSession session,
    ILogger<GetProductByIQuerydHandler> logger) : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Get product by Id={id}", query.Id);
        var product = await session.LoadAsync<Product>(query.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(query.Id);

        return new GetProductByIdResult(product.Adapt<ProductModule>());
    }
}