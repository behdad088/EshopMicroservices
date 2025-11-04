using AutoFixture.Xunit2;

namespace Order.Query.API.IntegrationTests.AutoFixture;

public class CustomInlineDomainDataAutoAttribute(params object[]? values)
    : CompositeDataAttribute(new InlineDataAttribute(values), new DomainDataAutoAttribute());