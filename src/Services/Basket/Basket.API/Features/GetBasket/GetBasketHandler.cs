using eshop.Shared.CQRS.Query;
using eshop.Shared.Logger;

namespace Basket.API.Features.GetBasket;

public record GetBasketQuery(string Username) : IQuery<GetBasketResult>;

public record GetBasketResult(ShoppingCart ShoppingCart);

public class GetBasketQueryHandler(IBasketRepository basketRepository) : IQueryHandler<GetBasketQuery, GetBasketResult>
{
    private readonly ILogger _logger = Log.ForContext<GetBasketQueryHandler>();
    public async Task<GetBasketResult> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty(LogProperties.Username, request.Username);

        _logger.Information("Get Basket for user");
        
        var basket = await basketRepository.GetBasketAsync(request.Username, cancellationToken).ConfigureAwait(false);
        return new GetBasketResult(basket);
    }
}