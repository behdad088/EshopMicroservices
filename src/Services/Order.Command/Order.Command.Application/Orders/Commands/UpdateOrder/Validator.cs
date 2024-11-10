using FluentValidation;

namespace Order.Command.Application.Orders.Commands.UpdateOrder;

public class Validator : AbstractValidator<UpdateOrderCommand>
{
    public Validator()
    {
        RuleFor(x => x.Order.OrderName).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Order.CustomerId).NotNull().WithMessage("CustomerId is required.");
        RuleFor(x => x.Order.OrderItems).NotEmpty().WithMessage("OrderItems should not be empty.");
        RuleFor(x => x.Order.Status).NotEmpty().Must(value =>
            value is
                OrderStatusDto.Cancelled or
                OrderStatusDto.Completed or
                OrderStatusDto.Pending);
        RuleForEach(x => x.Order.OrderItems).SetValidator(new OrderItemValidator());
    }

    private class OrderItemValidator : AbstractValidator<OrderItems>
    {
        public OrderItemValidator()
        {
            RuleFor(x => x.OrderId).NotNull();
            RuleFor(x => x.ProductId).NotNull();
            RuleFor(x => x.Price).NotNull();
            RuleFor(x => x.Quantity).NotNull();
        }
    }
}