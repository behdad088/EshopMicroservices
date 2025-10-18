using Microsoft.Data.SqlClient;
using Order.Command.Application.Exceptions;
using Order.Command.Application.Identity;

namespace Order.Command.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(OrderParameter OrderParameter) : ICommand<CreateOrderResult>;

public record CreateOrderResult(Ulid Id);

public class CreateOrderCommandHandler(
    IApplicationDbContext dbContext)
    : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var customerId = CustomerId.From(Guid.Parse(command.OrderParameter.CustomerId!));
            var order = MapOrder(command.OrderParameter);
            var outbox = MapOutbox(customerId, order);

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            dbContext.Orders.Add(order);
            dbContext.Outboxes.Add(outbox);

            AddOrderCreatedEvent(order, customerId);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken);
            return new CreateOrderResult(order.Id.Value);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is SqlException { Number: 2627 })
            {
                throw new DuplicatedOrderIdException($"Order Id already exists: {command.OrderParameter.Id}"); 
            }
            throw;
        }
    }

    private static Domain.Models.Order MapOrder(OrderParameter orderParameterParameter)
    {
        var order = new Domain.Models.Order().Create(
            id: OrderId.From(Ulid.Parse(orderParameterParameter.Id)),
            customerId: CustomerId.From(Guid.Parse(orderParameterParameter.CustomerId!)),
            orderName: OrderName.From(orderParameterParameter.OrderName!),
            shippingAddress: MapAddress(orderParameterParameter.ShippingAddress!),
            billingAddress: MapAddress(orderParameterParameter.BillingAddress!),
            payment: MapPayment(orderParameterParameter.OrderPayment!),
            orderItems: orderParameterParameter.OrderItems!.Select(x =>
                new OrderItem(
                    orderId: OrderId.From(Ulid.Parse(orderParameterParameter.Id)),
                    productId: ProductId.From(Ulid.Parse(x.ProductId!)),
                    quantity: x.Quantity!.Value,
                    price: Price.From(x.Price!.Value))).ToList());

        return order;
    }

    private static Address MapAddress(OrderParameter.Address addressParameter)
        => new(
            firstName: addressParameter.Firstname!,
            lastName: addressParameter.Lastname!,
            emailAddress: addressParameter.EmailAddress!,
            addressLine: addressParameter.AddressLine!,
            country: addressParameter.Country!,
            state: addressParameter.State!,
            zipCode: addressParameter.ZipCode!);

    private static Payment MapPayment(OrderParameter.Payment paymentParameter) =>
        new(
            cardName: paymentParameter.CardName!,
            cardNumber: paymentParameter.CardNumber!,
            expiration: paymentParameter.Expiration!,
            cvv: paymentParameter.Cvv!,
            paymentMethod: paymentParameter.PaymentMethod!.Value);

    private static void AddOrderCreatedEvent(Domain.Models.Order order, CustomerId customerId)
    {
        order.AddDomainEvent(order.ToOrderCreatedEvent(customerId.Value.ToString()));
    }

    private Domain.Models.Outbox MapOutbox(CustomerId customerId, Domain.Models.Order order)
    {
        var outbox = new Domain.Models.Outbox().Create(
            aggregateId: AggregateId.From(order.Id.Value),
            customerId: customerId,
            aggregateType: AggregateType.From(order.GetType().Name),
            versionId: VersionId.From(order.RowVersion.Value),
            dispatchDateTime: DispatchDateTime.InTwoMinutes(),
            eventType: EventType.From(nameof(OrderCreatedEvent)),
            payload: Payload.Serialize(order.ToOrderCreatedEvent(customerId.Value.ToString())));

        return outbox;
    }
}