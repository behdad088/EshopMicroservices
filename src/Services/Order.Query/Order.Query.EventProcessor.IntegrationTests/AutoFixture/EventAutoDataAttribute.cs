using AutoFixture;
using AutoFixture.Xunit2;

namespace Order.Query.EventProcessor.IntegrationTests.AutoFixture;

public class EventAutoDataAttribute() : AutoDataAttribute(EventFixtureFactory.Create)
{
    private static class EventFixtureFactory
    {
        public static IFixture Create()
        {
            var fixture = new Fixture();
            
            fixture.Customize(new OrderDeletedEventCustomization());
            fixture.Customize(new OrderCreatedEventCustomization());
            fixture.Customize(new OrderUpdatedEventCustomization());
            
            return fixture;
        }
    }
}