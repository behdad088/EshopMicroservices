using eshop.Shared.CQRS.Command;
using eshop.Shared.Logger;

namespace Catalog.API.Features.Products.CreateProduct;

public record CreateProductCommand(
    string Id,
    string Name,
    List<string> Category,
    string? Description,
    string ImageFile,
    decimal? Price) : ICommand<CreateProductResult>;

public record CreateProductResult(
    Ulid Id);

internal class CreateProductCommandHandler(IDocumentSession session)
    : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    private readonly ILogger _logger = Log.ForContext<CreateProductCommandHandler>();
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty(LogProperties.ProductId, command.Id);

        _logger.Information("Creating product.");
        
        var product = new ProductDocument
        {
            Id = command.Id,
            Name = command.Name,
            Category = command.Category,
            Description = command.Description,
            ImageFile = command.ImageFile,
            Price = command.Price
        };


        session.Store(product);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.Information("Successfully created product.");
        var result = MapResult(product);
        return result;
    }

    private static CreateProductResult MapResult(ProductDocument productDocument)
    {
        return new CreateProductResult(
            Id: Ulid.Parse(productDocument.Id)
        );
    }
}