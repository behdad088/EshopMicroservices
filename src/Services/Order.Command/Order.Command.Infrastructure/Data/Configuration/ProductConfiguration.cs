using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Command.Domain.Models;
using Order.Command.Domain.Models.ValueObjects;

namespace Order.Command.Infrastructure.Data.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(
            id => id.Value,
            dbId => ProductId.From(dbId));

        builder.Property(x => x.Name).HasConversion(
            name => name != null? name.Value : null,
            dbName => ProductName.FromNullable(dbName))
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.Price).HasConversion(
            price => price.Value,
            dbPrice => Price.From(dbPrice));
        
    }
}