using FluentValidation;

namespace Order.Command.Application.Orders.Queries.GetOrderByCustomer;

public class Validators : AbstractValidator<GetOrderByCustomerQuery>
{
    public Validators()
    {
        RuleFor(x => x.CustomerId).NotEmpty().Must(x => Guid.TryParse(x, out _));
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(0);
    }
}