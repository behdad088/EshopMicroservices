using Marten;
using Marten.Schema;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebMotions.Fake.Authentication.JwtBearer;


namespace Catalog.API.IntegrationTests
{
    public class ApiFactory(string postgresConnection) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(service =>
            {
                service.RemoveAll(typeof(IInitialData));
                service.AddMarten(options =>
                {
                    options.Connection(postgresConnection);
                    options.Schema.For<Product>().UseNumericRevisions(true);
                }).UseLightweightSessions();
                
                service
                    .AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                    .AddFakeJwtBearer();
            });
        }
    }
}
