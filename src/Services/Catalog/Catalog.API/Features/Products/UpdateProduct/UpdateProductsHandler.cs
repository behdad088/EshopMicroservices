namespace Catalog.API.Features.Products.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    List<string> Category,
    string? Description,
    string ImageFile,
    decimal? Price) : ICommand<UpdateProductResult>;

public record UpdateProductResult(ProductModule Product);

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Product Id is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 150).WithMessage("Name must be between 2 and 150 characters");


        When(x => x.Category is not null,
            () => RuleForEach(x => x.Category)
            .NotEmpty()
            .WithMessage("Category item cannot be null"));

        RuleFor(x => x.ImageFile)
            .NotEmpty()
            .WithMessage("ImageFile cannot be null")
            .When(x => x.ImageFile is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0")
            .When(x => x.Price is not null);
    }
}

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

        if (!string.IsNullOrEmpty(command.Name))
            product.Name = command.Name;
        if (command.Category.Count != 0)
            product.Category = command.Category;
        if (!string.IsNullOrEmpty(command.Description))
            product.Description = command.Description;
        if (!string.IsNullOrEmpty(command.ImageFile))
            product.ImageFile = command.ImageFile;

        product.Price = command.Price ?? product.Price;
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
