using Discount.Grpc.Data;
using Discount.Grpc.Models;
using eshop.Shared.Logger;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using ILogger = Serilog.ILogger;

namespace Discount.Grpc.Services;

public class DiscountService(DiscountContext dbContext)
    : DiscountProtoService.DiscountProtoServiceBase
{
    private readonly ILogger _logger = Log.ForContext<DiscountService>();
    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        if (request.Coupon?.Id is not null)
        {
            using var _ = LogContext.PushProperty(LogProperties.CouponId, request.Coupon.Id);
        }
        
        var coupon = MapToCoupon(request.Coupon);

        if (coupon is null)
        {
            _logger.Error("Invalid request object, coupon is null.");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object"));
        }

        var couponInDb = await dbContext.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductName == request.Coupon.ProductName).ConfigureAwait(false);

        if (couponInDb != null)
            return await UpdateDiscount(new UpdateDiscountRequest() { Coupon = MapToCoupon(couponInDb) }, context);

        dbContext.Coupons.Add(coupon);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        _logger.Information("Discount is successfully created.");

        var result = MapToCoupon(coupon);
        return result!;
    }

    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.ProductName)
            .ConfigureAwait(false);

        if (coupon == null)
            coupon = new Coupon { Amount = 0, Description = "No discount", ProductName = request.ProductName };

        _logger.Information("Discount is retrieved for product");

        var result = MapToCoupon(coupon);
        return result!;
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        var coupon = MapToCoupon(request.Coupon);

        if (coupon is null)
        {
            _logger.Error("Invalid request object, coupon is null.");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object"));
        }

        dbContext.Coupons.Update(coupon);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        _logger.Information("Discount is successfully updated");

        var result = MapToCoupon(coupon);
        return result!;
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request,
        ServerCallContext context)
    {
        var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.ProductName)
            .ConfigureAwait(false);

        if (coupon is null)
        {
            _logger.Error("Invalid request object, coupon is null.");
            throw new RpcException(new Status(StatusCode.NotFound,
                $"$Coupon not found with productName={request.ProductName}."));
        }

        dbContext.Coupons.Remove(coupon);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        _logger.Information("Discount was successfully deleted");

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