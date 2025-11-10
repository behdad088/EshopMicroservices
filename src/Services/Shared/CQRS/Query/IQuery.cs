using MediatR;

namespace eshop.Shared.CQRS.Query;

public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull
{
}