using FluentValidation;

namespace Order.Command.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OrderDto.OrderName).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.OrderDto.CustomerId).NotNull().WithMessage("CustomerId is required.");
        RuleFor(x => x.OrderDto.OrderItems).NotEmpty().WithMessage("OrderItems should not be empty.");

        RuleForEach(x => x.OrderDto.OrderItems).SetValidator(new OrderItemValidator());
    }

    private class OrderItemValidator : AbstractValidator<OrderDto.OrderItem>
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