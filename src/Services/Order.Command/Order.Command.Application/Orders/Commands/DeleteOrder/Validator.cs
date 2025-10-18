using eshop.Shared;
using FluentValidation;

namespace Order.Command.Application.Orders.Commands.DeleteOrder;

public class Validator : AbstractValidator<DeleteOrderCommand>
{
    public Validator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .MustBeValidUlid();
        
        RuleFor(x => x.CustomerId).MustBeValidGuid();

        RuleFor(x => x.Version).NotEmpty().MustBeValidEtag();
    }
}