using AutoFixture;
using Basket.API.Features.CheckoutBasket;

namespace Basket.API.IntegrationTests.AutoFixture;

public class CheckoutBasketCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<CheckoutBasketRequest.ModuleAddress>(composer => composer
            .With(a => a.Firstname, fixture.Create<string>())
            .With(a => a.Lastname, fixture.Create<string>())
            .With(a => a.EmailAddress, "test@integration.com")
            .With(a => a.AddressLine, fixture.Create<string>())
            .With(a => a.Country, "United States of America")
            .With(a => a.State, fixture.Create<string>())
            .With(a => a.ZipCode, "26534")
        );
        
        fixture.Customize<CheckoutBasketRequest.ModulePayment>(composer => composer
            .With(a => a.CardName, fixture.Create<string>())
            .With(a => a.CardNumber, "4234432454657532")
            .With(a => a.Expiration, "12/2026")
            .With(a => a.Cvv, "234")
            .With(a => a.PaymentMethod, fixture.Create<int>())
        );

        
        fixture.Customize<CheckoutBasketRequest>(composer => composer
            .With(x => x.Username, fixture.Create<string>())
            .With(x => x.OrderName, fixture.Create<string>())
            .With(x => x.BillingAddress, fixture.Create<CheckoutBasketRequest.ModuleAddress>())
            .With(x => x.Payment, fixture.Create<CheckoutBasketRequest.ModulePayment>()));
    }
}