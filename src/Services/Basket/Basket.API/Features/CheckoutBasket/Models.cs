namespace Basket.API.Features.CheckoutBasket;

public record CheckoutBasketResponse(
    [property: JsonPropertyName("order_id")]
    string OrderId);

public record CheckoutBasketRequest(
    [property: JsonPropertyName("username")]
    string? Username,
    [property: JsonPropertyName("order_name")]
    string? OrderName,
    [property: JsonPropertyName("billing_address")]
    CheckoutBasketRequest.ModuleAddress? BillingAddress,
    [property: JsonPropertyName("payment")]
    CheckoutBasketRequest.ModulePayment? Payment)
{
    public record ModulePayment(
        [property: JsonPropertyName("card_name")]
        string? CardName,
        [property: JsonPropertyName("card_number")]
        string? CardNumber,
        [property: JsonPropertyName("expiration")]
        string? Expiration,
        [property: JsonPropertyName("cvv")] 
        string? Cvv,
        [property: JsonPropertyName("payment_method")]
        int? PaymentMethod);
    
    public record ModuleAddress(
        [property: JsonPropertyName("firstname")]
        string? Firstname,
        [property: JsonPropertyName("lastname")]
        string? Lastname,
        [property: JsonPropertyName("email_address")]
        string? EmailAddress,
        [property: JsonPropertyName("address_line")]
        string? AddressLine,
        [property: JsonPropertyName("country")]
        string? Country,
        [property: JsonPropertyName("state")]
        string? State,
        [property: JsonPropertyName("zip_code")]
        string? ZipCode);
}