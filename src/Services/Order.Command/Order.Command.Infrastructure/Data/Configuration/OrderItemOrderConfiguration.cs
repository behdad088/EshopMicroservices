using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Command.Domain.Models;
using Order.Command.Domain.Models.ValueObjects;

namespace Order.Command.Infrastructure.Data.Configuration;

public class OrderItemOrderConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(
            id => id.ToString(),
            dbId => OrderItemId.From(Ulid.Parse(dbId)));

        builder.Property(x => x.ProductId).HasConversion(
                productId => productId.ToString(),
                dbProductId => ProductId.From(Ulid.Parse(dbProductId)))
            .IsRequired();

        builder.Property(x => x.Quantity).IsRequired();

        builder.Property(x => x.Price).HasConversion(
                price => price.Value,
                dbPrice => Price.From(dbPrice))
            .IsRequired();
    }
}