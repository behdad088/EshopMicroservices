using BuildingBlocks.CQRS.Query;
using Newtonsoft.Json;

namespace Catalog.API.Features.Products.GetProduct;

public record GetProductsQuery(PaginationRequest PaginationRequest) : IQuery<GetProductsResult>;

public record GetProductsResult(PaginatedItems<ProductModule> Result);

internal class GetProductQueryHandler(IDocumentSession session, ILogger<GetProductQueryHandler> logger) : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating products with {@query}", GetLogStringValue(JsonConvert.SerializeObject(query)));
        var pageSize = query.PaginationRequest.PageSize;
        var pageIndex = query.PaginationRequest.PageIndex;
        var totalItems = await session.Query<Product>().CountAsync(cancellationToken);
        var products = await session.Query<Product>().Skip(pageSize * pageIndex).Take(pageSize).ToListAsync(cancellationToken);
        var result = products.Adapt<IEnumerable<ProductModule>>();

        return new GetProductsResult(new PaginatedItems<ProductModule>(pageIndex, pageSize, totalItems, result));
    }

    /// <summary>
    /// Prevents Log Injection attacks
    /// https://owasp.org/www-community/attacks/Log_Injection
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string GetLogStringValue(string value)
    {
        return value.Replace(Environment.NewLine, "");
    }
}