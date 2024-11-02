using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Order.Command.Application.Dtos;

namespace Order.Command.API.Endpoints.GetOrders;

public record Request
{
    [FromQuery(Name = "page_size")] public int PageSize { get; set; } = 10;
    [FromQuery(Name = "page_index")] private int PageIndex { get; set; } = 0;
}

public record Response([property: JsonPropertyName("orders")] PaginatedItems<OrderDto> Orders);