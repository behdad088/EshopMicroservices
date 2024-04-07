namespace Catalog.API.Models;

public record PaginationRequest(
    [property: JsonPropertyName("page_page")] 
    int PageSize = 10,
    [property: JsonPropertyName("page_index")]
    int PageIndex = 0);