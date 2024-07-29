namespace Catalog.API.Features.Products.GetProductsByCategory;

public record GetProductByCategoryQuery(
    PaginationRequest PaginationRequest,
    string? Category) : IQuery<GetProductByCategoryResult>;

public record GetProductByCategoryResult(PaginatedItems<ProductModule> Product);

internal class GetProductsByCategoryQueryHandler(
    IDocumentSession session) : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryResult>
{
    public async Task<GetProductByCategoryResult> Handle(GetProductByCategoryQuery query, CancellationToken cancellationToken)
    {
        var pageSize = query.PaginationRequest.PageSize;
        var pageIndex = query.PaginationRequest.PageIndex;
        var productAsQueryable = session.Query<Product>().Where(p => p.Category.Contains(query.Category!)).AsQueryable();
        var totalItems = await productAsQueryable.CountAsync(cancellationToken);
        var product = await productAsQueryable.Skip(pageSize * pageIndex).Take(pageSize).ToListAsync(cancellationToken);
        var result = product.Adapt<IEnumerable<ProductModule>>();

        return new GetProductByCategoryResult(new PaginatedItems<ProductModule>(pageIndex, pageSize, totalItems, result));
    }


}