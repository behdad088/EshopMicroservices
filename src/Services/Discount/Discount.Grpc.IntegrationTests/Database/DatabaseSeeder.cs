using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Discount.Grpc.IntegrationTests.Database;

public class DatabaseSeeder : IDisposable
{
    private readonly DiscountContext _discountContext;
    private readonly IServiceScope _scope;
    public DatabaseSeeder(IServiceProvider serviceProvider)
    {
        var scopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
        _scope = scopeFactory!.CreateScope();
        _discountContext = _scope.ServiceProvider.GetRequiredService<DiscountContext>();
        
        _discountContext.Coupons.RemoveRange(_discountContext.Coupons);
        _discountContext.SaveChanges();
    }

    public async Task<Coupon?> GetCouponAsync(string productName)
    {
        var coupon = await _discountContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == productName)
            .ConfigureAwait(false);

        return coupon;
    }

    public async Task<Coupon?> CreateCouponAsync(Coupon coupon)
    {
        _discountContext.Coupons.Add(coupon);
        await _discountContext.SaveChangesAsync().ConfigureAwait(false);
        return coupon;
    }
    
    public async Task<bool?> DeleteCouponAsync(Coupon coupon)
    {
        _discountContext.Coupons.Remove(coupon);
        await _discountContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }
    
    public void Dispose()
    {
        _discountContext.Database.EnsureDeleted();
        _discountContext.Dispose();
        _scope.Dispose();
    }
}