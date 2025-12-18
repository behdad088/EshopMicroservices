namespace Basket.API.Models.Dtos;

public record CheckoutRequestModel(
    string? Username,
    string? CustomerId,
    string? OrderName,
    CheckoutRequestModel.ModuleAddress? BillingAddress,
    CheckoutRequestModel.ModulePayment? Payment)
{
    public record ModulePayment(
        string? CardName,
        string? CardNumber,
        string? Expiration,
        string? Cvv,
        int? PaymentMethod);
    
    public record ModuleAddress(
        string? Firstname,
        string? Lastname,
        string? EmailAddress,
        string? AddressLine,
        string? Country,
        string? State,
        string? ZipCode);
}