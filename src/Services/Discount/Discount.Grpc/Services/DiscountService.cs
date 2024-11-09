using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services;

public class DiscountService(DiscountContext dbContext, ILogger<DiscountService> logger)
    : DiscountProtoService.DiscountProtoServiceBase
{
    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        var coupon = MapToCoupon(request.Coupon);

        if (coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object"));

        var couponInDb = await dbContext.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductName == request.Coupon.ProductName).ConfigureAwait(false);

        if (couponInDb != null)
            return await UpdateDiscount(new UpdateDiscountRequest() { Coupon = MapToCoupon(couponInDb) }, context);

        dbContext.Coupons.Add(coupon);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Discount is successfully created for product {ProductName} with amount {Amount}.",
            coupon.ProductName, coupon.Amount);

        var result = MapToCoupon(coupon);
        return result!;
    }

    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.ProductName)
            .ConfigureAwait(false);

        if (coupon == null)
            coupon = new Coupon { Amount = 0, Description = "No discount", ProductName = request.ProductName };

        logger.LogInformation("Discount is retrieved for product : {ProductName} with amount {Amount}",
            request.ProductName, coupon.Amount);

        var result = MapToCoupon(coupon);
        return result!;
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        var coupon = MapToCoupon(request.Coupon);

        if (coupon is null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object"));

        dbContext.Coupons.Update(coupon);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Discount is successfully updated for product {ProductName} with amount {Amount}.",
            coupon.ProductName, coupon.Amount);

        var result = MapToCoupon(coupon);
        return result!;
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request,
        ServerCallContext context)
    {
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.ProductName)
            .ConfigureAwait(false);

        if (coupon is null)
            throw new RpcException(new Status(StatusCode.NotFound,
                $"$Coupon not found with productName={request.ProductName}."));

        dbContext.Coupons.Remove(coupon);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Discount was successfully deleted for product {ProductName} with amount {Amount}",
            coupon.ProductName, coupon.Amount);

        return new DeleteDiscountResponse() { Success = true };
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