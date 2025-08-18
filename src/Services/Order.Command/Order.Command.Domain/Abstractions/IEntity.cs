namespace Order.Command.Domain.Abstractions;

public interface IEntity<T> : IEntity
    where T : class
{
    public T Id { get; set; }
}

public interface IEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}