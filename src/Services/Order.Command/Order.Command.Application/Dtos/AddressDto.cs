namespace Order.Command.Application.Dtos;

public record AddressDto(
    string Firstname,
    string Lastname,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode);