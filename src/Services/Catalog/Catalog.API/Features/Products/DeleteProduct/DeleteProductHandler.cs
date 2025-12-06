using eshop.Shared.CQRS.Command;
using eshop.Shared.Logger;

namespace Catalog.API.Features.Products.DeleteProduct;

public record DeleteProductCommand(string? Id) : ICommand<DeleteProductResult>;

public record DeleteProductResult(bool IsSuccess);

internal class DeleteProductCommandHandler(
    IDocumentSession session) : ICommandHandler<DeleteProductCommand, DeleteProductResult>
{
    private readonly ILogger _logger = Log.ForContext<DeleteProductCommandHandler>();
    
    public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty(LogProperties.ProductId, command.Id);

        _logger.Information("Deleting product.");
        
        session.Delete<ProductDocument>(Ulid.Parse(command.Id!).ToString());
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        
        _logger.Information("Successfully deleted product.");
        
        return new DeleteProductResult(true);
    }
}