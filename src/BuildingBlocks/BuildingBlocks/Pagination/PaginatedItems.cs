using System.Text.Json.Serialization;

namespace BuildingBlocks.Pagination;

public class PaginatedItems<TEntity>(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data) where TEntity : class
{
    [JsonPropertyName("page_index")]
    public int PageIndex { get; } = pageIndex;

    [JsonPropertyName("page_size")]
    public int PageSize { get; } = pageSize;

    [JsonPropertyName("count")]
    public long Count { get; } = count;

    [JsonPropertyName("data")]
    public IEnumerable<TEntity> Data { get; } = data;
}
