using FluentValidation;
using Order.Command.Application.Dtos;

namespace Order.Command.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Order.OrderName).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Order.CustomerId).NotNull().WithMessage("CustomerId is required.");
        RuleFor(x => x.Order.OrderItems).NotEmpty().WithMessage("OrderItems should not be empty.");

        RuleForEach(x => x.Order.OrderItems).SetValidator(new OrderItemValidator());
    }
    
    private class OrderItemValidator : AbstractValidator<OrderItemsDto>
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