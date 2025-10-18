using System.ComponentModel.DataAnnotations;

namespace Order.Query.EventProcessor.Configurations;

public class QueueConfigurations
{
    [ConfigurationKeyName("QueueSettings:OrderViewQueues")]
    [Required]
    public OrderViewQueues OrderViewQueues { get; init; } = new();
}

public class OrderViewQueues
{
    public Queue OrderCreatedEvent { get; init; } = new();
    public Queue OrderUpdatedEvent { get; init; } = new();
    public Queue OrderDeletedEvent { get; init; } = new();
}

public class Queue
{
    public string Name { get; set; } = string.Empty;
    public string Dlq { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
}