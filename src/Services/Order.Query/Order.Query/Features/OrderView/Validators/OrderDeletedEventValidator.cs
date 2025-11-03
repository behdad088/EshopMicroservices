using eshop.Shared;
using FluentValidation;
using Order.Query.Events;

namespace Order.Query.Features.OrderView.Validators;

public class OrderDeletedEventValidator : AbstractValidator<OrderDeletedEvent>
{
    public OrderDeletedEventValidator()
    {
        RuleFor(x => x.OrderId)
            .MustBeValidUlid();
        
        RuleFor(x => x.DeletedDate).MustBeValidTimestamp();
        RuleFor(x => x.Version).GreaterThanOrEqualTo(1);
    }
}