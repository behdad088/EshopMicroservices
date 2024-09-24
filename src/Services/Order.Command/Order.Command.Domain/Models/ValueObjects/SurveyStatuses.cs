namespace Order.Command.Domain.Models.ValueObjects;

public static class OrderStatuses
{
    public const string Draft = "draft";
    public const string Pending = "pending";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";
}