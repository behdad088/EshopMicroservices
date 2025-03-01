using FluentValidation;

namespace Order.Command.Application.Orders.Queries.GetOrders;

public class Validator : AbstractValidator<GetOrdersQuery>
{
    public Validator()
    {
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}