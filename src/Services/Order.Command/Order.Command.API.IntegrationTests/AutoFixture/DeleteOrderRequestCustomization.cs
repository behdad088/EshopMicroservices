using AutoFixture;
using Order.Command.API.Endpoints.DeleteOrder;

namespace Order.Command.API.IntegrationTests.AutoFixture;

public class DeleteOrderRequestCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Request>(composer => composer
            .With(r => r.OrderId, Ulid.NewUlid().ToString())
        );
    }
}