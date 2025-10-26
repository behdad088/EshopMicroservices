using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseFastEndpoints(c =>
{
    c.Versioning.Prefix = "v";
    c.Versioning.PrependToRoute = true;
    c.Versioning.DefaultVersion = 1;
    c.Serializer.Options.PropertyNamingPolicy = null;
    c.Serializer.Options.AllowOutOfOrderMetadataProperties = true;
});

app.Run();


namespace Order.Query.Api
{
    public class Program
    {
    }
}