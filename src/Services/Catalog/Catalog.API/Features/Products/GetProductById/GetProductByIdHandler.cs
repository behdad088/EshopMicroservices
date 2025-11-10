using eshop.Shared.CQRS.Query;

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
    public async Task<Result> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await session.LoadAsync<ProductDocument>(query.Id, cancellationToken).ConfigureAwait(false);

        if (product is null)
            return new Result.NotFound(query.Id);

        return MapResult(product);
    }

    private static Result.Success MapResult(ProductDocument productDocument)
    {
        return new Result.Success(
            Product: new GetProductById(
                Id: Ulid.Parse(productDocument.Id),
                Name: productDocument.Name,
                Category: productDocument.Category,
                Description: productDocument.Description,
                ImageFile: productDocument.ImageFile,
                Price: productDocument.Price,
                productDocument.Version));
    }
}