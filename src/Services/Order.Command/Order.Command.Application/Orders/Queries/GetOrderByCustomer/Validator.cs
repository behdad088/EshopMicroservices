using FluentValidation;

namespace Order.Command.Application.Orders.Queries.GetOrderByCustomer;

public class Validators : AbstractValidator<GetOrderByCustomerQuery>
{
    public Validators()
    {
        RuleFor(x => x.CustomerId).NotEmpty().Must(x => Guid.TryParse(x, out _))
            .WithMessage("Customer Id must be a valid GUID.");
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}