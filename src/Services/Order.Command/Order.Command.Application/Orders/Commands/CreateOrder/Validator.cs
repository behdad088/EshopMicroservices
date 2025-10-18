using eshop.Shared;
using FluentValidation;

namespace Order.Command.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OrderParameter.Id).MustBeValidUlid();
        RuleFor(x => x.OrderParameter.OrderName).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.OrderParameter.CustomerId).MustBeValidGuid();
        RuleFor(x => x.OrderParameter.OrderItems).NotEmpty().WithMessage("OrderItems should not be empty.");

        RuleForEach(x => x.OrderParameter.OrderItems).SetValidator(new OrderItemValidator());
        RuleFor(x => x.OrderParameter.BillingAddress!).NotNull().SetValidator(new AddressValidator());
        RuleFor(x => x.OrderParameter.ShippingAddress!).NotNull().SetValidator(new AddressValidator());
        RuleFor(x => x.OrderParameter.OrderPayment!).NotNull().SetValidator(new PaymentValidator());
    }

    private class AddressValidator : AbstractValidator<OrderParameter.Address>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Firstname).NotEmpty().WithMessage("firstname is required.");
            RuleFor(x => x.Lastname).NotEmpty().WithMessage("lastname is required.");
            RuleFor(x => x.EmailAddress).NotEmpty().WithMessage("email_address is required.");
            RuleFor(x => x.EmailAddress)
                .EmailAddress()
                .WithMessage("email_address is not valid.")
                .When(x => !string.IsNullOrWhiteSpace(x.EmailAddress));

            RuleFor(x => x.AddressLine).NotEmpty().WithMessage("address_line is required.");
            RuleFor(x => x.Country)
                .MustBeValidCountryName();
            RuleFor(x => x.State).NotEmpty().WithMessage("state is required.");
            RuleFor(x => x.ZipCode).NotEmpty().Length(5).WithMessage("zip_code is not valid.");
        }
    }

    private class OrderItemValidator : AbstractValidator<OrderParameter.OrderItem>
    {
        public OrderItemValidator()
        {
            RuleFor(x => x.ProductId).MustBeValidUlid();
            RuleFor(x => x.Price).NotNull().GreaterThan(0);
            RuleFor(x => x.Quantity).NotNull().GreaterThan(0);
        }
    }

    private class PaymentValidator : AbstractValidator<OrderParameter.Payment>
    {
        public PaymentValidator()
        {
            RuleFor(x => x.Cvv).NotEmpty()
                .Must(x => int.TryParse(x, out _)).WithMessage("CVV is not valid.")
                .Length(3).WithMessage("CVV is not valid.");
            RuleFor(x => x.CardName).NotEmpty().WithMessage("card_name is required.");
            RuleFor(x => x.CardNumber).MustBeValidCardNumber();
            RuleFor(x => x.Expiration).MustBeValidExpiryDate();
            RuleFor(x => x.PaymentMethod).NotNull();
        }
    }
}