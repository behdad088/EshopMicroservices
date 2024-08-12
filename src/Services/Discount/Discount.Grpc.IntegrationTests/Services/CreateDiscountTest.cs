namespace Discount.Grpc.IntegrationTests.Services;

public class CreateDiscountTest(ITestOutputHelper outputHelper) : IAsyncLifetime
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
    public async Task CreateDiscount_Null_Coupon_Throws()
    {
        // Arrange
        var cancellationToken = _apiSpecification.CancellationToken;
        var client = new DiscountProtoService.DiscountProtoServiceClient(_channel);
        var request = new CreateDiscountRequest();
        
        // Act-Assert
        var exception = Assert.Throws<RpcException>(() => client.CreateDiscount(request, cancellationToken: cancellationToken));
        exception.Status.StatusCode.ShouldBe(StatusCode.InvalidArgument);
        exception.Message.ShouldContain("Invalid request object");
        await Task.CompletedTask;
    }
    
    [Fact]
    public async Task CreateDiscount_Creates_Coupon()
    {
        // Arrange
        var cancellationToken = _apiSpecification.CancellationToken;
        var client = new DiscountProtoService.DiscountProtoServiceClient(_channel);
        var request = new CreateDiscountRequest()
        {
            Coupon = new CouponModel()
            {
                Amount = 100,
                Description = "Test Description",
                ProductName = "Test Name 1"
            }
        };
        
        
        // Act
        var result = await client.CreateDiscountAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        
        // Assert
        result.ShouldNotBeNull();
        var resultInDb = await _apiSpecification.DatabaseSeeder.GetCouponAsync(request.Coupon.ProductName);
        resultInDb.ShouldNotBeNull();
    }

    public async Task DisposeAsync()
    {
        await _apiSpecification.DisposeAsync().ConfigureAwait(false);
    }
}