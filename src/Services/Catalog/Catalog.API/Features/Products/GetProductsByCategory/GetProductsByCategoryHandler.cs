using System.Collections.ObjectModel;
using BuildingBlocks.Pagination;

namespace Catalog.API.Features.Products.GetProductsByCategory;

public record GetProductByCategoryQuery(
    PaginationRequest PaginationRequest,
    string? Category) : IQuery<GetProductByCategoryResult>;

public record GetProductByCategoryResult(PaginatedItems<ProductModule> Product);

internal class GetProductsByCategoryQueryHandler(
    IDocumentSession session) : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryResult>
{
    public async Task<GetProductByCategoryResult> Handle(GetProductByCategoryQuery query,
        CancellationToken cancellationToken)
    {
        var pageSize = query.PaginationRequest.PageSize;
        var pageIndex = query.PaginationRequest.PageIndex;
        var productAsQueryable =
            session.Query<ProductDocument>().Where(p => p.Category.Contains(query.Category!)).AsQueryable();
        var totalItems = await productAsQueryable.CountAsync(cancellationToken).ConfigureAwait(false);
        var product = await productAsQueryable
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var result = MapToResult(product);

        return new GetProductByCategoryResult(
            new PaginatedItems<ProductModule>(pageIndex, pageSize, totalItems, result));
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