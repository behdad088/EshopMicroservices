namespace Catalog.API.Features.Products.DeleteProduct;

public record DeleteProductCommand(string? Id) : ICommand<DeleteProductResult>;

public record DeleteProductResult(bool IsSuccess);

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
          .NotEmpty()
          .Must(x => Guid.TryParse(x, out _)).WithMessage("Product Id is not a valid UUID");
    }
}

internal class DeleteProductCommandHandler(
    IDocumentSession session) : ICommandHandler<DeleteProductCommand, DeleteProductResult>
{
    public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        session.Delete<Product>(Guid.Parse(command.Id!));
        await session.SaveChangesAsync(cancellationToken);

        return new DeleteProductResult(true);
    }
}