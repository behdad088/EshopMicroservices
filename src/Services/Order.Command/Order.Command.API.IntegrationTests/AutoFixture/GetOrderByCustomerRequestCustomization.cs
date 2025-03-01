using AutoFixture;
using Order.Command.API.Endpoints.GetOrdersByCustomer;

namespace Order.Command.API.IntegrationTests.AutoFixture;

// public class GetOrderByCustomerRequestCustomization : ICustomization
// {
//     public void Customize(IFixture fixture)
//     {
//         fixture.Customize<Request>(composer => composer
//             .With(r => r.CustomerId, Guid.NewGuid().ToString())
//             .With(r => r.PageIndex, 0)
//             .With(r => r.PageSize, 10)
//         );
//     }
// }