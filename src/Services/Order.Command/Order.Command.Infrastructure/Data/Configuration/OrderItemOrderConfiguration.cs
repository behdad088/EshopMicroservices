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
            id => id.Value,
            dbId => OrderItemId.From(dbId));

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId);

        builder.Property(x => x.Quantity).IsRequired();
        
        builder.Property(x => x.Price).HasConversion(
            price => price.Value,
            dbPrice => Price.From(dbPrice))
            .IsRequired();
    }
}