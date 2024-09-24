using ValueOf;

namespace Order.Command.Domain.Abstractions;

public interface IEntity<T> : IEntity 
    where T : class
{
    public T Id { get; set; }
}

public interface IEntity
{
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}