using FluentValidation;

namespace Basket.API.Features.GetBasket;

public class GetBasketQueryValidator : AbstractValidator<GetBasketQuery>
{
    public GetBasketQueryValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username cannot be null");
    }
}