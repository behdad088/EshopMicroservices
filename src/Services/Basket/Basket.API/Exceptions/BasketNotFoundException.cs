using BuildingBlocks.Exceptions;

namespace Basket.API.Exceptions;

public class BasketNotFoundException(string username) : NotFoundException("basket", username)
{
}