namespace Order.Command.Domain.Models;

public class Product : Entity<ProductId>
{
    public ProductId Id { get; private set; } = default!;
    public ProductName? Name { get; private set; }
    public Price Price { get; private set; } = default!;

    public static Product Create(ProductId id, ProductName? name, Price price)
    {
        return new Product
        {
            Id = id,
            Name = name,
            Price = price
        };
    }
}