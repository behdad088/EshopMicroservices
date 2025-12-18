using eshop.Shared;
using FluentValidation;

namespace Basket.API.Features.CheckoutBasket;

public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketCommandValidator()
    {
        RuleFor(x => x.CheckoutRequestModel).NotNull();
        When(x => x.CheckoutRequestModel is not null, () =>
        {
            RuleFor(x => x.CheckoutRequestModel!.OrderName).MinimumLength(5)
                .WithMessage("order_name must be at least 5 characters long.");
            
            RuleFor(x => x.CheckoutRequestModel!.CustomerId).MustBeValidGuid();
            RuleFor(x => x.CheckoutRequestModel!.Username).NotEmpty();
            
            RuleFor(x => x.CheckoutRequestModel!.BillingAddress!).NotNull().SetValidator(new AddressValidator());
            RuleFor(x => x.CheckoutRequestModel!.Payment!).NotNull().SetValidator(new PaymentValidator());
        });
    }
    
    private class AddressValidator : AbstractValidator<CheckoutRequestModel.ModuleAddress>
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
    
    private class PaymentValidator : AbstractValidator<CheckoutRequestModel.ModulePayment>
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