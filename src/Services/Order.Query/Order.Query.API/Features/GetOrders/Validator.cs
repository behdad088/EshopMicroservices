using FluentValidation;

namespace Order.Query.API.Features.GetOrders;

public class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}