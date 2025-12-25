using Marten.Schema;

namespace Catalog.API.Models;

public class ProductDocument
{
    public string? Id { get; init; }
    public string? Name { get; set; }
    public List<string> Category { get; set; } = [];
    public string? Description { get; set; }
    public string? ImageFile { get; set; }

    [Version] public int Version { get; set; }

    public decimal? Price { get; set; }
}