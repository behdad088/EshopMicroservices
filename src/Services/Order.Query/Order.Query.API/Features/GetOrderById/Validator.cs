using eshop.Shared;
using FastEndpoints;

namespace Order.Query.API.Features.GetOrderById;

public class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.OrderId).MustBeValidUlid();
        RuleFor(x => x.CustomerId).MustBeValidGuid();
    }
}