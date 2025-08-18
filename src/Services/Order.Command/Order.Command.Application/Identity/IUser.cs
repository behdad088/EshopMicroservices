namespace Order.Command.Application.Identity;

public interface IUser
{
    public string? Id { get; }
    public List<string>? Roles { get; }
}