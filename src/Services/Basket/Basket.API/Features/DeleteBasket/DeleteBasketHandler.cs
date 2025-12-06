using eshop.Shared.CQRS.Command;
using eshop.Shared.Logger;

namespace Basket.API.Features.DeleteBasket;

public record DeleteBasketCommand(string Username) : ICommand<DeleteBasketResult>;

public record DeleteBasketResult(bool IsSuccess);

public class DeleteBasketHandler(IBasketRepository basketRepository)
    : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
{
    private readonly ILogger _logger = Log.ForContext<DeleteBasketHandler>();
    
    public async Task<DeleteBasketResult> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty(LogProperties.Username, request.Username);

        _logger.Information("Deleting Basket for user");
        
        var result = await basketRepository.DeleteBasketAsync(request.Username, cancellationToken)
            .ConfigureAwait(false);
        return new DeleteBasketResult(result);
    }
}