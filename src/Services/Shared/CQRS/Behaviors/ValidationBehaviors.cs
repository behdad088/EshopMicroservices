using eshop.Shared.CQRS.Command;
using eshop.Shared.CQRS.Query;
using FluentValidation;

namespace eshop.Shared.CQRS.Behaviors;

public class CommandValidationBehaviors<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : BaseValidator<TRequest, TResponse>(validators)
    where TRequest : ICommand<TResponse> where TResponse : notnull
{
}

public class QueryValidationBehaviors<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : BaseValidator<TRequest, TResponse>(validators)
    where TRequest : IQuery<TResponse> where TResponse : notnull
{
}