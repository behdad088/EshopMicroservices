using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Command.Domain.Models;
using Order.Command.Domain.Models.ValueObjects;

namespace Order.Command.Infrastructure.Data.Configuration;

public class OutboxConfiguration : IEntityTypeConfiguration<Outbox>
{
    public void Configure(EntityTypeBuilder<Outbox> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.AggregateId, x.VersionId }).IsUnique();

        builder.Property(x => x.Id).HasConversion(
            id => id.ToString(),
            dbId => OutboxId.From(Ulid.Parse(dbId)));

        builder.Property(x => x.AggregateId).HasConversion(
            aggregateId => aggregateId.ToString(),
            dbAggregateId => AggregateId.From(Ulid.Parse(dbAggregateId)));

        builder.Property(x => x.CustomerId).HasConversion(
                customerId => customerId.Value,
                dbCustomerId => CustomerId.From(dbCustomerId))
            .IsRequired();
        
        builder.Property(p => p.VersionId).HasConversion(
                versionId => versionId.Value,
                dbVersionId => VersionId.From(dbVersionId))
            .IsConcurrencyToken();

        builder.Property(x => x.AggregateType).HasConversion(
            aggregateType => aggregateType.Value,
            dbAggregateType => AggregateType.From(dbAggregateType));

        builder.Property(x => x.DispatchDateTime).HasConversion(
            dispatchDateTime => dispatchDateTime.Value,
            dbDispatchDateTime => DispatchDateTime.From(dbDispatchDateTime));

        builder.Property(x => x.IsDispatched).HasConversion(
            isDispatched => isDispatched.Value,
            dbIsDispatched => IsDispatched.From(dbIsDispatched));

        builder.Property(x => x.NumberOfDispatchTry).HasConversion(
            numberOfDispatchTry => numberOfDispatchTry.Value,
            dbNumberOfDispatchTry => NumberOfDispatchTry.From(dbNumberOfDispatchTry));

        builder.Property(x => x.EventType).HasConversion(
            eventType => eventType.Value,
            dbEventType => EventType.From(dbEventType));

        builder.Property(x => x.Payload).HasConversion(
            payload => payload.Value,
            dbPayload => Payload.From(dbPayload));
    }
}