using eshop.Shared;
using FastEndpoints;
using FluentValidation;

namespace Order.Query.API.Features.GetOrdersByCustomer;

public class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.CustomerId).MustBeValidGuid();
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}