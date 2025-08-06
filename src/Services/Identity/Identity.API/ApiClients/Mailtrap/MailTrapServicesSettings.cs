namespace Identity.API.ApiClients.Mailtrap;

public class MailTrapServicesSettings
{
    public string? BaseUrl { get; set; } = default!;
    public string? Sender { get; set; } = default!;
    public string? AuthorizationKey { get; set; } = default!;
}