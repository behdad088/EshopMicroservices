using AutoFixture;
using Order.Command.API.Endpoints.CreateOrder;

namespace Order.Command.API.IntegrationTests.AutoFixture;

public class CreateOrderRequestCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Request.ModuleAddress>(composer => composer
            .With(a => a.Firstname, fixture.Create<string>())
            .With(a => a.Lastname, fixture.Create<string>())
            .With(a => a.EmailAddress, "test@integration.com")
            .With(a => a.AddressLine, fixture.Create<string>())
            .With(a => a.Country, "United States of America")
            .With(a => a.State, fixture.Create<string>())
            .With(a => a.ZipCode, "26534")
        );
        fixture.Customize<Request.ModulePayment>(composer => composer
            .With(a => a.CardName, fixture.Create<string>())
            .With(a => a.CardNumber, "4234432454657532")
            .With(a => a.Expiration, "12/2026")
            .With(a => a.Cvv, "234")
            .With(a => a.PaymentMethod, fixture.Create<int>())
        );
        
        fixture.Customize<Request.ModuleOrderItem>(composer => composer
            .With(a => a.ProductId, Ulid.NewUlid().ToString())
            .With(a => a.Quantity, 1)
            .With(a => a.Price, 100));
            
        
        fixture.Customize<Request>(composer => composer
            .With(r => r.Id, Ulid.NewUlid().ToString())
            .With(r => r.CustomerId, Guid.NewGuid().ToString())
            .With(r => r.OrderName, fixture.Create<string>())
            .With(r => r.ShippingAddress, fixture.Create<Request.ModuleAddress>())
            .With(r => r.BillingAddress, fixture.Create<Request.ModuleAddress>())
            .With(r => r.Payment, fixture.Create<Request.ModulePayment>())
            .With(r => r.OrderItems, fixture.CreateMany<Request.ModuleOrderItem>(3).ToList())
        );
    }
}