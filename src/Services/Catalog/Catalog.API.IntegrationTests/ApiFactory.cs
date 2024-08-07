﻿using Marten;
using Marten.Schema;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;


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
            });
        }
    }
}
