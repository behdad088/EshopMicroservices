using BuildingBlocks.CQRS.Command;

namespace Basket.API.Features.DeleteBasket;

public record DeleteBasketCommand(string Username) : ICommand<DeleteBasketResult>;

public record DeleteBasketResult(bool IsSuccess);

public class DeleteBasketHandler(IBasketRepository basketRepository) : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
{
    public async Task<DeleteBasketResult> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        var result = await basketRepository.DeleteBasketAsync(request.Username, cancellationToken).ConfigureAwait(false);
        return new DeleteBasketResult(result);
    }
}