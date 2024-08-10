using Discount.Grpc.Models;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data;

public class DiscountContext(DbContextOptions<DiscountContext> options) : DbContext(options)
{
    public DbSet<Coupon> Coupons { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>().HasData(
            new List<Coupon>
            {
                new() {Id = 1, Amount = 150, ProductName = "IPhone x", Description = "Discount for IPhone"},
                new() {Id = 2, Amount = 100, ProductName = "Samsung 10", Description = "Discount for Samsung 10"},
            }
        );
    }
}

// add migration in rider editor
// dotnet tool install --global dotnet-ef
// dotnet ef migrations add 'initial' --project Discount.Grpc.csproj --context DiscountContext
// dotnet ef database update --project Discount.Grpc.csproj --context DiscountContext