namespace Catalog.API.Features.Products.GetProductById;

public record GetProductByIdQuery(string Id) : IQuery<GetProductByIdResult>;

public record GetProductByIdResult(GetProductById Product);

internal class GetProductByIQueryHandler(
    IDocumentSession session) : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await session.LoadAsync<Product>(query.Id, cancellationToken).ConfigureAwait(false);

        if (product is null)
            throw new ProductNotFoundException(Ulid.Parse(query.Id));

        return MapResult(product);
    }

    private static GetProductByIdResult MapResult(Product product)
    {
        return new GetProductByIdResult(
            Product: new GetProductById(
                Id: Ulid.Parse(product.Id),
                Name: product.Name,
                Category: product.Category,
                Description: product.Description,
                ImageFile: product.ImageFile,
                Price: product.Price,
                product.Version));
    }
}