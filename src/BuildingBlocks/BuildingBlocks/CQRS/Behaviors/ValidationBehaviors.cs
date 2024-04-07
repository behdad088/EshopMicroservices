using BuildingBlocks.CQRS.Command;
using BuildingBlocks.CQRS.Query;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.CQRS.Behaviors;

public class CommandValidationBehaviors<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : BaseValidator<TRequest, TResponse>(validators)
    where TRequest : ICommand<TResponse>
{
    public override Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return base.Handle(request, next, cancellationToken);
    }
}

public class QueryValidationBehaviors<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : BaseValidator<TRequest, TResponse>(validators)
    where TRequest : IQuery<TResponse>
{
    public override Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return base.Handle(request, next, cancellationToken);
    }
}