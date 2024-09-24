namespace Order.Command.Domain.Models;

public class Product(ProductName name, Price price) : Entity<ProductId>
{
    public ProductName? Name { get; private set; } = name;
    public Price Price { get; private set; } = price;
}