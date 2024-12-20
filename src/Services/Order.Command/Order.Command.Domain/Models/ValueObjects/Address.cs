namespace Order.Command.Domain.Models.ValueObjects;

public record Address
{
    public Address()
    {
    }

    public Address(string firstName, string lastName, string emailAddress, string addressLine, string country,
        string state, string zipCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine);

        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        AddressLine = addressLine;
        Country = country;
        State = state;
        ZipCode = zipCode;
    }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string EmailAddress { get; private set; } = default!;
    public string AddressLine { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    public string State { get; private set; } = default!;
    public string ZipCode { get; private set; } = default!;
}