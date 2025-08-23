namespace Catalog.API.IntegrationTests.Database;

public record ProductConfiguration
{
    public string Id { get; set; }
    public string Name { get; set; } = null!;
    public List<string> Category { get; set; } = [];
    public string? Description { get; set; }
    public string ImageFile { get; set; } = null!;

    public int Version { get; set; }

    public decimal? Price { get; set; }
}

public static class CreateProductConfiguration
{
    public static Product ToDbProduct(this ProductConfiguration configuration)
    {
        return new Product
        {
            Id = configuration.Id,
            Name = configuration.Name,
            Category = configuration.Category,
            Description = configuration.Description,
            ImageFile = configuration.ImageFile,
            Price = configuration.Price
        };
    }
}
