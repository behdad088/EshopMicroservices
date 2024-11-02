using Order.Command.Domain.Models;
using Order.Command.Domain.Models.ValueObjects;

namespace Order.Command.Infrastructure.Data.Extensions;

internal static class InitialData
{
    internal static IEnumerable<Order.Command.Domain.Models.Order> OrdersWithItems()
    {
        var address1 = new Address("customer firstname", "customer1 lastname", "customer1@test.com",
            "customer 1 address line", "customer 1 country" ,"customer 1 state", "13245");
        
        var address2 = new Address("customer firstname", "customer2 lastname", "customer2@test.com",
            "customer 2 address line", "customer 2 country" ,"customer 2 state", "13245");

        var payment1 = new Payment("Customer 1", "123456789123123123123321", "12/3", "123", 1);
        var payment2 = new Payment("Customer 2", "123456789123123123123321", "12/3", "123", 2);

        var order1 = new Domain.Models.Order().Create(
            OrderId.From(Guid.NewGuid()),
            CustomerId.From(new Guid("d0fad6c2-19d2-4c1c-ad9f-834076d50e2d")),
            OrderName.From("order 1"),
            address1,
            address1,
            payment1);
        
        order1.Add(ProductId.From(new Guid("19dc8c88-097a-4a19-9f3c-f2ab0be2ee34")), 1, Price.From(500));
        order1.Add(ProductId.From(new Guid("bba65837-3960-4ec2-b49f-4e0b25f9eab8")), 1, Price.From(400));
        
        var order2 = new Domain.Models.Order().Create(
            OrderId.From(Guid.NewGuid()),
            CustomerId.From(new Guid("d232b9e2-d2dc-4356-95a4-fc7aec1b3028")),
            OrderName.From("order 2"),
            address2,
            address2,
            payment2);
        
        order2.Add(ProductId.From(new Guid("909d92f3-c752-4ef8-9a14-a833987ab9c8")), 1, Price.From(650));
        order2.Add(ProductId.From(new Guid("909d92f3-c752-4ef8-9a14-a833987ab9c8")), 1, Price.From(450));

        return new[] { order1, order2 };
    }
}