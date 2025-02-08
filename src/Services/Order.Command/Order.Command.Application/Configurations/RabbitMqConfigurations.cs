using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Order.Command.Application.Configurations;

public sealed record RabbitMqConfigurations
{
    [ConfigurationKeyName("RabbitMQ:URI")]
    [Required]
    public required string Uri { get; init; }
    [ConfigurationKeyName("RabbitMQ:Username")]
    [Required]
    public required string Username { get; init; }
    [ConfigurationKeyName("RabbitMQ:Password")]
    [Required]
    public required string Password { get; init; }
}