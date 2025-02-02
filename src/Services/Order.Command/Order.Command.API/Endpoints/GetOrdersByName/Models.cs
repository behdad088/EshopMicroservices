using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Order.Command.Application.Dtos;

namespace Order.Command.API.Endpoints.GetOrdersByName;

public record Request
{
    [FromQuery(Name = "name")] public string Name { get; set; }

    [property: FromQuery(Name = "page_size")]
    public int PageSize { get; set; } = 10;

    [property: FromQuery(Name = "page_index")]
    public int PageIndex { get; set; } = 0;
}

public record Response([property: JsonPropertyName("orders")] PaginatedItems<ModuleOrder> Orders);

public record ModuleOrder(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("customer_id")]
    string CustomerId,
    [property: JsonPropertyName("order_name")]
    string OrderName,
    [property: JsonPropertyName("shipping_Address")]
    ModuleAddress ShippingAddress,
    [property: JsonPropertyName("billing_address")]
    ModuleAddress BillingAddress,
    [property: JsonPropertyName("payment")]
    ModulePayment Payment,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("order_items")]
    List<ModuleOrderItem> OrderItems);

public record ModuleOrderItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("product_id")]
    string? ProductId,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("price")] decimal? Price);

public record ModuleAddress(
    [property: JsonPropertyName("firstname")]
    string Firstname,
    [property: JsonPropertyName("lastname")]
    string Lastname,
    [property: JsonPropertyName("email_address")]
    string EmailAddress,
    [property: JsonPropertyName("address_line")]
    string AddressLine,
    [property: JsonPropertyName("country")]
    string Country,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("zip_code")]
    string ZipCode);

public record ModulePayment(
    [property: JsonPropertyName("card_name")]
    string CardName,
    [property: JsonPropertyName("card_number")]
    string CardNumber,
    [property: JsonPropertyName("expiration")]
    string Expiration,
    [property: JsonPropertyName("cvv")] string Cvv,
    [property: JsonPropertyName("payment_method")]
    int PaymentMethod);