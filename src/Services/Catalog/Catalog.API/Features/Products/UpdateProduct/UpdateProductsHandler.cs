using eshop.Shared.CQRS.Command;
using eshop.Shared.Logger;
using Marten.Exceptions;

namespace Catalog.API.Features.Products.UpdateProduct;

public record UpdateProductCommand(
    string? Id,
    string Name,
    List<string>? Category,
    string? Description,
    string? ImageFile,
    decimal? Price,
    string? Etag) : ICommand<Result>;

public abstract record Result
{
    public record Success : Result;
    public record NotFound(string Id) : Result;
    public record InvalidEtag(string Etag) : Result;
}

internal class UpdateProductsCommandHandler(
    IDocumentSession session) : ICommandHandler<UpdateProductCommand, Result>
{
    private readonly ILogger _logger = Log.ForContext<UpdateProductsCommandHandler>();
    
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty(LogProperties.ProductId, command.Id);
        using var __ = LogContext.PushProperty(LogProperties.ETag, command.Etag);
        
        _logger.Information("Updating product.");
        
        var product = await session.LoadAsync<ProductDocument>(command.Id!, cancellationToken).ConfigureAwait(false);

        if (product is null)
        {
            _logger.Error("Product not found.");
            return new Result.NotFound(command.Id);
        }

        var version = GetVersionFromEtag(command.Etag!);

        if (product.Version != version)
        {
            _logger.Error("Invalid product version.");
            return new Result.InvalidEtag(version.ToString());
        }

        if (!string.IsNullOrEmpty(command.Name))
            product.Name = command.Name;
        if (command.Category?.Count != 0)
            product.Category = command.Category!;
        if (!string.IsNullOrEmpty(command.Description))
            product.Description = command.Description;
        if (!string.IsNullOrEmpty(command.ImageFile))
            product.ImageFile = command.ImageFile;

        product.Price = command.Price ?? product.Price;

        try
        {
            session.UpdateRevision(product, version + 1);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (ConcurrencyException)
        {
            _logger.Error("Invalid product version.");
            return new Result.InvalidEtag(command.Etag!);
        }

        _logger.Information("Successfully updated Product.");
        return new Result.Success();
    }

    private static int GetVersionFromEtag(string eTag)
    {
        return int.Parse(eTag[2..].Trim('"'));
    }
}