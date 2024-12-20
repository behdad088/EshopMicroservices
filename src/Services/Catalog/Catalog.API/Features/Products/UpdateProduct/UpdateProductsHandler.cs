﻿using Marten.Exceptions;
using Exception = System.Exception;

namespace Catalog.API.Features.Products.UpdateProduct;

public record UpdateProductCommand(
    string Id,
    string Name,
    List<string>? Category,
    string? Description,
    string? ImageFile,
    decimal? Price,
    string? Etag) : ICommand<UpdateProductResult>;

public record UpdateProductResult(bool IsSuccess);

internal class UpdateProductsCommandHandler(
    IDocumentSession session) : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await session.LoadAsync<Product>(command.Id, cancellationToken).ConfigureAwait(false);

        if (product is null)
            throw new ProductNotFoundException(Ulid.Parse(command.Id));

        var version = GetVersionFromEtag(command.Etag!);

        if (product.Version != version)
            throw new InvalidEtagException(etag: version);

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
            throw new InvalidEtagException(etag: version);
        }

        return new UpdateProductResult(true);
    }

    private static int GetVersionFromEtag(string eTag)
    {
        return int.Parse(eTag[2..].Trim('"'));
    }
}