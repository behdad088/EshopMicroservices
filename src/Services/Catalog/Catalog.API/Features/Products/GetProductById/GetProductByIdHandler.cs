namespace Catalog.API.Features.Products.GetProductById;

public record GetProductByIdQuery(string Id) : IQuery<GetProductByIdResult>;

public record GetProductByIdResult(ProductModule Product);

internal class GetProductByIQueryHandler(
    IDocumentSession session) : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await session.LoadAsync<Product>(query.Id, cancellationToken).ConfigureAwait(false);

        if (product is null)
            throw new ProductNotFoundException(Ulid.Parse(query.Id));

        return new GetProductByIdResult(product.Adapt<ProductModule>());
    }
}