
using Newtonsoft.Json;

namespace Catalog.API.Features.Products.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    List<string> Category,
    string? Description,
    string ImageFile,
    decimal? Price) : ICommand<UpdateProductResult>;

public record UpdateProductResult(ProductModule Product);

internal class UpdateProductsCommandHandler(
    IDocumentSession session,
    ILogger<UpdateProductsCommandHandler> logger) : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Update product with {@Command}", GetLogStringValue(JsonConvert.SerializeObject(command)));

        var product = await session.LoadAsync<Product>(command.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException();

        product.Name = command.Name;
        product.Category = command.Category;
        product.Description = command.Description;
        product.ImageFile = command.ImageFile;
        product.Price = command.Price;
        session.Update(product);
        await session.SaveChangesAsync(cancellationToken);

        var updatedEntity = product.Adapt<ProductModule>();

        return new UpdateProductResult(updatedEntity);
    }

    /// <summary>
    /// Prevents Log Injection attacks
    /// https://owasp.org/www-community/attacks/Log_Injection
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string GetLogStringValue(string value)
    {
        return value.Replace(Environment.NewLine, "");
    }
}
