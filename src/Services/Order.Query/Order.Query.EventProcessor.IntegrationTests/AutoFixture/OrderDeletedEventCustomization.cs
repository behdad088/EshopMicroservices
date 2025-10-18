using System.Net.Mime;
using AutoFixture;
using Order.Query.Data.Events;

namespace Order.Query.EventProcessor.IntegrationTests.AutoFixture;

public class OrderDeletedEventCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<OrderDeletedEvent>(x => x
            .With(r => r.OrderId, Ulid.NewUlid().ToString())
            .With(r => r.DeletedDate, DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
            .With(r => r.CreatedAt, DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
            .With(r => r.Version, 1)
        );
            
        fixture.Customize<CloudEvent<OrderDeletedEvent>>(c => c
            .With(e => e.Id, Ulid.NewUlid().ToString())
            .With(e => e.Type, "test.eshop.order-command.order-deleted")
            .With(e => e.Source, "urn:test:eshop:order-command-api")
            .With(e => e.SpecVersion, "1.0.0")
            .With(e => e.DataContentType, MediaTypeNames.Application.Json)
            .With(e => e.Time, DateTimeOffset.UtcNow)
            .With(e => e.Data, fixture.Create<OrderDeletedEvent>())
        );
    }
}