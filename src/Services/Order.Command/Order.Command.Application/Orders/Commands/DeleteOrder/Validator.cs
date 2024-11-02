using FluentValidation;

namespace Order.Command.Application.Orders.Commands.DeleteOrder;

public class Validator : AbstractValidator<DeleteOrderCommand>
{
    public Validator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .Must(x => Guid.TryParse(x, out _)).WithMessage("Valid OrderId is required.");
    }
}