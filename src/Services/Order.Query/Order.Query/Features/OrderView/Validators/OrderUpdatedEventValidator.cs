using eshop.Shared;
using FluentValidation;
using Order.Query.Events;

namespace Order.Query.Features.OrderView.Validators;

public class OrderUpdatedEventValidator : AbstractValidator<OrderUpdatedEvent>
{
    public OrderUpdatedEventValidator()
    {
        RuleFor(x => x.Id).MustBeValidUlid();
        RuleFor(x => x.OrderName).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.OrderItems).NotEmpty().WithMessage("OrderItems should not be empty.");
        RuleFor(x => x.Version).GreaterThanOrEqualTo(1);

        RuleFor(x => x.OrderStatus).NotEmpty().Must(value =>
            value is
                OrderStatus.Cancelled or
                OrderStatus.Completed or
                OrderStatus.Pending).WithMessage("Invalid order status.");
        
        RuleForEach(x => x.OrderItems).SetValidator(new OrderItemValidator());
        RuleFor(x => x.BillingAddress).NotNull().SetValidator(new AddressValidator());
        RuleFor(x => x.ShippingAddress).NotNull().SetValidator(new AddressValidator());
        RuleFor(x => x.PaymentMethod).NotNull().SetValidator(new PaymentValidator());
        RuleFor(x => x).SetValidator(new TotalPriceValidator());
    }
    
    private class TotalPriceValidator : AbstractValidator<OrderUpdatedEvent>
    {
        public TotalPriceValidator()
        {
            RuleFor(x => x)
                .Custom(ValidateTotalPrice);
        }

        private static void ValidateTotalPrice(OrderUpdatedEvent @event, ValidationContext<OrderUpdatedEvent> context)
        {
            var totalPrice = @event.TotalPrice;
            var totalPriceFromProductItems = @event.OrderItems.Sum(x => x.Quantity * x.Price);

            if (totalPriceFromProductItems != totalPrice)
                context.AddFailure(
                    $"Total price {totalPrice} does not match total price {totalPriceFromProductItems} from products");
        }
    }
    
    private class AddressValidator : AbstractValidator<OrderUpdatedEvent.Address>
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

    private class OrderItemValidator : AbstractValidator<OrderUpdatedEvent.OrderItem>
    {
        public OrderItemValidator()
        {
            RuleFor(x => x.ProductId).MustBeValidUlid();
            RuleFor(x => x.Price).NotNull().GreaterThan(0);
            RuleFor(x => x.Quantity).NotNull().GreaterThan(0);
        }
    }
    
    private class PaymentValidator : AbstractValidator<OrderUpdatedEvent.Payment>
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

    private static class OrderStatus
    {
        public const string Pending = "pending";
        public const string Completed = "completed";
        public const string Cancelled = "cancelled";
    }
}