using BuildingBlocks.CQRS.Command;
using Catalog.API.Models;

namespace Catalog.API.Features.Products.CreateProduct;

public record CreateProductCommand(
        Guid Id,
        string Name,
        List<string> Category,
        string? Description,
        string ImageFile,
        decimal? Price) : ICommand<CreateProductResult>;

public record CreateProductResult(Guid Id);

internal class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = command.Name,
            Category = command.Category,
            Description = command.Description,
            ImageFile = command.ImageFile,
            Price = command.Price
        };

        // db saving will add later

        return await Task.FromResult(new CreateProductResult(Guid.NewGuid()));
    }
}