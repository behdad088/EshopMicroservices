using MediatR;

namespace eshop.Shared.CQRS.Command;

public interface ICommand : IRequest<Unit>
{
}

public interface ICommand<out TResponse> : IRequest<TResponse> where TResponse : notnull
{
}