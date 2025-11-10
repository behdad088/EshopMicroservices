using eshop.Shared.Exceptions;

namespace Catalog.API.Exceptions;

public class ProductNotFoundException(Ulid id) : NotFoundException("Product", id)
{
}