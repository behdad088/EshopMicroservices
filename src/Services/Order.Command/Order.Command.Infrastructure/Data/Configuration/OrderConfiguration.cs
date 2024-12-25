using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Command.Domain.Models;
using Order.Command.Domain.Models.ValueObjects;

namespace Order.Command.Infrastructure.Data.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Domain.Models.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Models.Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(
            id => id.ToString(),
            dbId => OrderId.From(Ulid.Parse(dbId)));

        builder.Property(p => p.RowVersion).HasConversion(
                versionId => versionId.Value,
                dbVersionId => VersionId.From(dbVersionId))
            .IsConcurrencyToken();


        builder.Property(x => x.DeleteDate).HasConversion(
            deleteDate => (deleteDate == null) ? null : deleteDate.Value,
            dbDeleteDate => DeleteDate.From(dbDeleteDate ?? string.Empty));

        builder.Property(x => x.CustomerId).HasConversion(
                customerId => customerId.Value,
                dbCustomerId => CustomerId.From(dbCustomerId))
            .IsRequired();

        builder.HasMany<OrderItem>()
            .WithOne()
            .HasForeignKey(x => x.OrderId);

        builder.Property(x => x.OrderName).HasConversion(
                name => name.Value,
                dbName => OrderName.From(dbName))
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status).HasConversion(
            status => status.Value,
            dbStatus => OrderStatus.Parse(dbStatus));

        builder.Property(x => x.TotalPrice).HasConversion(
            totalPrice => totalPrice.Value,
            dbTotalPrice => Price.From(dbTotalPrice));

        builder.ComplexProperty(
            x => x.ShippingAddress, addressBuilder =>
            {
                addressBuilder.Property(a => a.FirstName)
                    .HasMaxLength(50)
                    .IsRequired();

                addressBuilder.Property(a => a.LastName)
                    .HasMaxLength(50)
                    .IsRequired();

                addressBuilder.Property(a => a.EmailAddress)
                    .HasMaxLength(400)
                    .IsRequired();

                addressBuilder.Property(a => a.AddressLine)
                    .HasMaxLength(180)
                    .IsRequired();

                addressBuilder.Property(a => a.State)
                    .HasMaxLength(50)
                    .IsRequired();

                addressBuilder.Property(a => a.ZipCode)
                    .HasMaxLength(5)
                    .IsRequired();
            });

        builder.ComplexProperty(
            x => x.BillingAddress, addressBuilder =>
            {
                addressBuilder.Property(a => a.FirstName)
                    .HasMaxLength(50)
                    .IsRequired();

                addressBuilder.Property(a => a.LastName)
                    .HasMaxLength(50)
                    .IsRequired();

                addressBuilder.Property(a => a.EmailAddress)
                    .HasMaxLength(400)
                    .IsRequired();

                addressBuilder.Property(a => a.AddressLine)
                    .HasMaxLength(180)
                    .IsRequired();

                addressBuilder.Property(a => a.State)
                    .HasMaxLength(50)
                    .IsRequired();

                addressBuilder.Property(a => a.ZipCode)
                    .HasMaxLength(5)
                    .IsRequired();
            });

        builder.ComplexProperty(
            x => x.Payment, addressBuilder =>
            {
                addressBuilder.Property(a => a.CardName)
                    .HasMaxLength(50)
                    .IsRequired();

                addressBuilder.Property(a => a.CardNumber)
                    .HasMaxLength(24)
                    .IsRequired();

                addressBuilder.Property(a => a.Expiration)
                    .HasMaxLength(10)
                    .IsRequired();

                addressBuilder.Property(a => a.CVV)
                    .HasMaxLength(3)
                    .IsRequired();

                addressBuilder.Property(a => a.PaymentMethod);
            });
    }
}