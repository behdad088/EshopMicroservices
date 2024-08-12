using Grpc.Net.Client;
using Xunit.Abstractions;
using Shouldly;

namespace Discount.Grpc.IntegrationTests.Services;

public class GetDiscountTests(ITestOutputHelper outputHelper) : IAsyncLifetime
{
    private ApiSpecification _apiSpecification = default!;
    private GrpcChannel _channel = default!;
    
    public async Task InitializeAsync()
    {
        _apiSpecification = new ApiSpecification(outputHelper);
        await _apiSpecification.InitializeAsync().ConfigureAwait(false);
        _channel = _apiSpecification.Channel;
    }
    
    [Fact]
    public async Task GetDiscount_No_Discount_Was_Found()
    {
        // Arrange
        var cancellationToken = _apiSpecification.CancellationToken;
        var client = new DiscountProtoService.DiscountProtoServiceClient(_channel);
        var request = new GetDiscountRequest() { ProductName = "IPhone x" };
        
        // Act
        var result = await client.GetDiscountAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

        // Assert
        result.Description.ShouldBe("No discount");
    }
    
    public async Task DisposeAsync()
    {
        await _apiSpecification.DisposeAsync().ConfigureAwait(false);
    }
}