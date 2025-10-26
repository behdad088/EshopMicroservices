using System.Net.Mime;
using AutoFixture;
using Order.Query.Events;

namespace Order.Query.EventProcessor.IntegrationTests.AutoFixture;

public class OrderUpdatedEventCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<OrderUpdatedEvent.Address>(x => x
            .With(r => r.Firstname, fixture.Create<string>())
            .With(r => r.Lastname, fixture.Create<string>())
            .With(r => r.EmailAddress, "test@test.com")
            .With(r => r.AddressLine, fixture.Create<string>())
            .With(r => r.Country, "Sweden")
            .With(r => r.State, fixture.Create<string>())
            .With(r => r.ZipCode, "12345")
        );

        fixture.Customize<OrderUpdatedEvent.OrderItem>(x => x
            .With(r => r.Id, Ulid.NewUlid().ToString)
            .With(r => r.ProductId, Ulid.NewUlid().ToString)
            .With(r => r.Quantity, 1)
            .With(r => r.Price, 100)
        );

        fixture.Customize<OrderUpdatedEvent.Payment>(x => x
            .With(r => r.CardName, fixture.Create<string>())
            .With(r => r.Cvv, "123")
            .With(r => r.CardNumber, "4234432454657532")
            .With(r => r.Expiration, "12/2026")
            .With(r => r.PaymentMethod, 0)
        );
        
        fixture.Customize<OrderUpdatedEvent>(x => x
            .With(r => r.Id, Ulid.NewUlid().ToString())
            .With(r => r.LastModified, DateTimeOffset.UtcNow)
            .With(r => r.OrderName, "Test Order Name")
            .With(r => r.OrderStatus, "pending")
            .With(r => r.BillingAddress, fixture.Create<OrderUpdatedEvent.Address>())
            .With(r => r.ShippingAddress, fixture.Create<OrderUpdatedEvent.Address>())
            .With(r => r.OrderItems, [fixture.Create<OrderUpdatedEvent.OrderItem>()])
            .With(r => r.PaymentMethod, fixture.Create<OrderUpdatedEvent.Payment>())
            .With(r => r.TotalPrice, 100)
            .With(r => r.Version, 1)
        );
        
        fixture.Customize<CloudEvent<OrderUpdatedEvent>>(c => c
            .With(e => e.Id, Ulid.NewUlid().ToString())
            .With(e => e.Type, "test.eshop.order-command.order-updated")
            .With(e => e.Source, "urn:test:eshop:order-command-api")
            .With(e => e.SpecVersion, "1.0.0")
            .With(e => e.DataContentType, MediaTypeNames.Application.Json)
            .With(e => e.Time, DateTimeOffset.UtcNow)
            .With(e => e.Data, fixture.Create<OrderUpdatedEvent>())
        );
    }
}