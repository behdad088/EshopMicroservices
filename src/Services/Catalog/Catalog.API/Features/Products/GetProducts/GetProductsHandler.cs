using System.Collections.Immutable;
using System.Collections.ObjectModel;
using BuildingBlocks.Pagination;

namespace Catalog.API.Features.Products.GetProducts;

public record GetProductsQuery(PaginationRequest PaginationRequest) : IQuery<GetProductsResult>;

public record GetProductsResult(PaginatedItems<ProductModule> PaginatedResult);

internal class GetProductQueryHandler(
    IDocumentSession session) : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var pageSize = query.PaginationRequest.PageSize;
        var pageIndex = query.PaginationRequest.PageIndex;
        var productsAsQueryable = session.Query<ProductDocument>().AsQueryable();
        var totalItems = await productsAsQueryable.CountAsync(cancellationToken).ConfigureAwait(false);
        var products = await productsAsQueryable.Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = MapToResult(products);

        return new GetProductsResult(new PaginatedItems<ProductModule>(pageIndex, pageSize, totalItems, result));
    }

    private static ReadOnlyCollection<ProductModule>? MapToResult(IReadOnlyCollection<ProductDocument>? products)
    {
        return products?.Select(x => new ProductModule(
            Id: Ulid.Parse(x.Id),
            Name: x.Name,
            Category: x.Category,
            Description: x.Description,
            ImageFile: x.ImageFile,
            Price: x.Price)).ToList().AsReadOnly();
    }
}