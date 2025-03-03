using FluentValidation;

namespace Order.Command.Application.Orders.Queries.GetOrdersByName;

public class Validator : AbstractValidator<GetOrdersByNameQuery>
{
    public Validator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}