namespace Order.Command.Application.Dtos;


public static class OrderStatusDto
{
    public const string Draft = "draft";
    public const string Pending = "pending";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";
}