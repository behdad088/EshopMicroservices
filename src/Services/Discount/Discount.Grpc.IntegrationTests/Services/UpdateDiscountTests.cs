namespace Discount.Grpc.IntegrationTests.Services;

public class UpdateDiscountTests(ITestOutputHelper outputHelper) : IAsyncLifetime
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
    public async Task UpdateDiscount_Updates_Discount_With_Correct_Value()
    {
        // Arrange
        var cancellationToken = _apiSpecification.CancellationToken;
        var client = new DiscountProtoService.DiscountProtoServiceClient(_channel);

        var coupon = new Coupon()
        {
            Amount = 100,
            Description = "Test Description",
            ProductName = "Test Name 2"
        };

        coupon = await _apiSpecification.DatabaseSeeder.CreateCouponAsync(coupon);

        var updatedCoupon = new Coupon()
        {
            Id = coupon!.Id,
            Amount = 100,
            Description = coupon.Description + " - Updated",
            ProductName = coupon.ProductName + " - Updated"
        };

        var request = new UpdateDiscountRequest()
        {
            Coupon = MapToCoupon(updatedCoupon)
        };

        // Act
        var result = await client.UpdateDiscountAsync(request, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Assert
        MapToCoupon(result).ShouldBeEquivalentTo(updatedCoupon);
    }

    public async Task DisposeAsync()
    {
        await _apiSpecification.DisposeAsync().ConfigureAwait(false);
    }

    private static Coupon? MapToCoupon(CouponModel? couponModel)
    {
        return couponModel is null
            ? null
            : new Coupon
            {
                Id = couponModel.Id,
                ProductName = couponModel.ProductName,
                Amount = couponModel.Amount,
                Description = couponModel.Description
            };
    }

    private static CouponModel? MapToCoupon(Coupon? coupon)
    {
        return coupon is null
            ? null
            : new CouponModel
            {
                Id = coupon.Id,
                Amount = coupon.Amount,
                Description = coupon.Description,
                ProductName = coupon.ProductName
            };
    }
}