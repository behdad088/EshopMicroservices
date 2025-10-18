using MassTransit;

namespace Order.Query.EventProcessor.MassTransitConfiguration;

public class CustomErrorQueueNameFormatter : IErrorQueueNameFormatter
{
    public string FormatErrorQueueName(string queueName)
    {
        return queueName + "_dlq";;
    }
}