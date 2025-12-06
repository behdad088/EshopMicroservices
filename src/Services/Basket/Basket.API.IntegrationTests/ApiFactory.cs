using Basket.API.Models;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using WebMotions.Fake.Authentication.JwtBearer;

namespace Basket.API.IntegrationTests;

public class ApiFactory(
    string postgresConnection,
    string redisConnectionString,
    string elasticSearchString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Database", postgresConnection);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", redisConnectionString);
        Environment.SetEnvironmentVariable("Logger__elasticsearch", elasticSearchString);

        builder.ConfigureTestServices(service =>
        {
            service.AddMarten(options =>
            {
                options.Connection(postgresConnection);
                options.Schema.For<ShoppingCart>().UseNumericRevisions(true);
            }).UseLightweightSessions();


            service.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });
            
            service
                .AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer();
        });
    }
}