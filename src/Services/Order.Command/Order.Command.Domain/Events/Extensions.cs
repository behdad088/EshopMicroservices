namespace Order.Command.Domain.Events;

public static class Extensions
{
    public static OrderCreatedEvent ToOrderCreatedEvent(this Models.Order order)
    {
        return new OrderCreatedEvent(
            Id: order.Id.Value,
            CreatedBy: order.CreatedBy,
            LastModified: order.LastModified,
            CustomerId: order.CustomerId.Value,
            OrderName: order.OrderName.Value,
            ShippingAddress: ToEventAddress(order.ShippingAddress),
            BillingAddress: ToEventAddress(order.BillingAddress),
            PaymentMethod: ToEventPayment(order.Payment),
            OrderStatus: order.Status.Value,
            DeletedDate: order.DeleteDate?.Value,
            TotalPrice: order.TotalPrice.Value,
            Version: order.RowVersion.Value);

        OrderCreatedEvent.Address ToEventAddress(Address address)
        {
            return new OrderCreatedEvent.Address(
                Firstname: address.FirstName,
                Lastname: address.LastName,
                EmailAddress: address.EmailAddress,
                AddressLine: address.AddressLine,
                Country: address.Country,
                State: address.State,
                ZipCode: address.ZipCode);
        }

        OrderCreatedEvent.Payment ToEventPayment(Payment payment)
        {
            return new OrderCreatedEvent.Payment(
                CardName: payment.CardName,
                CardNumber: payment.CardNumber,
                Expiration: payment.Expiration,
                Cvv: payment.CVV,
                PaymentMethod: payment.PaymentMethod);
        }
    }

    public static OrderUpdatedEvent ToOrderUpdatedEvent(this Models.Order order)
    {
        return new OrderUpdatedEvent(
            Id: order.Id.Value,
            CreatedBy: order.CreatedBy,
            LastModified: order.LastModified,
            CustomerId: order.CustomerId.Value,
            OrderName: order.OrderName.Value,
            ShippingAddress: ToEventAddress(order.ShippingAddress),
            BillingAddress: ToEventAddress(order.BillingAddress),
            PaymentMethod: ToEventPayment(order.Payment),
            OrderStatus: order.Status.Value,
            DeletedDate: order.DeleteDate?.Value,
            TotalPrice: order.TotalPrice.Value,
            Version: order.RowVersion.Value);

        OrderUpdatedEvent.Address ToEventAddress(Address address)
        {
            return new OrderUpdatedEvent.Address(
                Firstname: address.FirstName,
                Lastname: address.LastName,
                EmailAddress: address.EmailAddress,
                AddressLine: address.AddressLine,
                Country: address.Country,
                State: address.State,
                ZipCode: address.ZipCode);
        }

        OrderUpdatedEvent.Payment ToEventPayment(Payment payment)
        {
            return new OrderUpdatedEvent.Payment(
                CardName: payment.CardName,
                CardNumber: payment.CardNumber,
                Expiration: payment.Expiration,
                Cvv: payment.CVV,
                PaymentMethod: payment.PaymentMethod);
        }
    }
}