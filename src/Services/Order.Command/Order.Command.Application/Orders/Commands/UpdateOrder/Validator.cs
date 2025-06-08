using FluentValidation;

namespace Order.Command.Application.Orders.Commands.UpdateOrder;

public class Validator : AbstractValidator<UpdateOrderCommand>
{
    public Validator()
    {
        RuleFor(x => x.Order.Id).MustBeValidUlid();
        RuleFor(x => x.Order.OrderName).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Order.CustomerId).MustBeValidGuid();
        RuleFor(x => x.Order.OrderItems).NotEmpty().WithMessage("OrderItems should not be empty.");
        RuleFor(x => x.Order.Version).MustBeValidEtag();

        RuleFor(x => x.Order.Status).NotEmpty().Must(value =>
            value is
                OrderStatusDto.Cancelled or
                OrderStatusDto.Completed or
                OrderStatusDto.Pending).WithMessage("Invalid order status.");
        
        RuleForEach(x => x.Order.OrderItems).SetValidator(new OrderItemValidator());
        RuleFor(x => x.Order.BillingAddress!).NotNull().SetValidator(new AddressValidator());
        RuleFor(x => x.Order.ShippingAddress!).NotNull().SetValidator(new AddressValidator());
        RuleFor(x => x.Order.OrderPayment!).NotNull().SetValidator(new PaymentValidator());
    }
    
    private class AddressValidator : AbstractValidator<UpdateOrderParameter.Address>
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

    private class OrderItemValidator : AbstractValidator<UpdateOrderParameter.OrderItem>
    {
        public OrderItemValidator()
        {
            RuleFor(x => x.ProductId).MustBeValidUlid();
            RuleFor(x => x.Price).NotNull().GreaterThan(0);
            RuleFor(x => x.Quantity).NotNull().GreaterThan(0);
        }
    }
    
    private class PaymentValidator : AbstractValidator<UpdateOrderParameter.Payment>
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