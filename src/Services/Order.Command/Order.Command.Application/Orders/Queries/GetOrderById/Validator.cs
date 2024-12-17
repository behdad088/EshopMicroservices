using FluentValidation;

namespace Order.Command.Application.Orders.Queries.GetOrderById;

public class Validator : AbstractValidator<GetOrdersByIdQuery>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty().Must(x => Ulid.TryParse(x, out _)).WithMessage("Invalid order id");
    }
}