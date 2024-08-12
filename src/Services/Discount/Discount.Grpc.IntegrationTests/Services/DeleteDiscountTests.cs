namespace Discount.Grpc.IntegrationTests.Services;

public class DeleteDiscountTests(ITestOutputHelper outputHelper): IAsyncLifetime
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
    public async Task DeleteDiscount_No_Discount_Was_Found()
    {
        // Arrange
        var cancellationToken = _apiSpecification.CancellationToken;
        var client = new DiscountProtoService.DiscountProtoServiceClient(_channel);
        var request = new DeleteDiscountRequest { ProductName = "Delete test" };
        
        // Act - Assert
        var exception = Assert.Throws<RpcException>(() => client.DeleteDiscount(request, cancellationToken: cancellationToken));
        exception.Status.StatusCode.ShouldBe(StatusCode.NotFound);
        exception.Message.ShouldContain($"Coupon not found with productName={request.ProductName}.");
        await Task.CompletedTask;
    }
    
    [Fact]
    public async Task DeleteDiscount_DeleteDiscount()
    {
        // Arrange
        var cancellationToken = _apiSpecification.CancellationToken;
        var client = new DiscountProtoService.DiscountProtoServiceClient(_channel);
        var coupon = new Coupon()
        {
            Amount = 100,
            Description = "Test Description",
            ProductName = "Delete Test"
        };

        await _apiSpecification.DatabaseSeeder.CreateCouponAsync(coupon);

        var request = new DeleteDiscountRequest()
        {
            ProductName = "Delete Test"
        };
        
        // Act
        var result = await client.DeleteDiscountAsync(request, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        
        // Assert
        result.Success.ShouldBeTrue();
        var couponInDb = await _apiSpecification.DatabaseSeeder.GetCouponAsync(coupon.ProductName);
        couponInDb.ShouldBeNull();
    }
    
    public async Task DisposeAsync()
    {
        await _apiSpecification.DisposeAsync().ConfigureAwait(false);
    }
}