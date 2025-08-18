namespace Order.Command.Domain.Abstractions;

public abstract class Entity<T> : IEntity<T> where T : class
{
    public T Id { get; set; } = default!;
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}