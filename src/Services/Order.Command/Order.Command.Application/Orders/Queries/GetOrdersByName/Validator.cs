using FluentValidation;

namespace Order.Command.Application.Orders.Queries.GetOrdersByName;

public class Validator : AbstractValidator<GetOrdersByNameQuery>
{
    public Validator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}