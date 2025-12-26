using FluentValidation;

namespace Order.Query.API.Features.GetOrdersByName;

public class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.OrderName).NotEmpty();
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}