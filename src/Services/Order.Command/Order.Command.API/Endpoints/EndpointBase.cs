using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Order.Command.API.Endpoints;

public interface IEndpoint
{
    void MapEndpoint();

    void Configure(IEndpointRouteBuilder app);
}

/// <summary>
///     Only use this base class when the endpoint does not return any dto
/// </summary>
/// <typeparam name="TRequest">Request dto</typeparam>
public abstract class EndpointBase<TRequest> : EndpointBase<TRequest, object?> where TRequest : notnull, new();

/// <summary>
///     use this base class for defining endpoints that use both request and response dtos.
/// </summary>
/// <typeparam name="TRequest">Request dto</typeparam>
/// <typeparam name="TResponse">Response dto</typeparam>
public abstract class EndpointBase<TRequest, TResponse> : IEndpoint where TRequest : notnull, new()
{
    private IEndpointRouteBuilder _endpointRouteBuilder = default!;
    private RouteHandlerBuilder _routeHandlerBuilder = default!;
    private ISender _sender = default!;
    protected HttpContext Context { get; set; } = default!;
    protected CancellationToken CancellationToken { get; set; }

    private static JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    ///     Map the endpoint with a route options : <see cref="Post" />, <see cref="Get" />, <see cref="Delete" />,
    ///     <see cref="Put" />, <see cref="Patch" />
    ///     Allowed metadata to add to an endpoint : <see cref="Name" />, <see cref="Produces" />,
    ///     <see cref="ProducesProblem" />, <see cref="Summary" />,
    ///     <see cref="Description" />, <see cref="Tags" />
    ///     Following
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public virtual void MapEndpoint()
    {
        throw new NotImplementedException();
    }

    public void Configure(IEndpointRouteBuilder app)
    {
        _endpointRouteBuilder = app;
    }

    public virtual Task<IResult> HandleAsync(TRequest request)
    {
        throw new NotImplementedException();
    }

    public virtual IResult Handle(TRequest request)
    {
        throw new NotImplementedException();
    }

    protected void Post(string pattern, Func<TRequest, Task<IResult>> handler)
    {
        _routeHandlerBuilder = _endpointRouteBuilder.MapPost(pattern, (
            HttpContext context,
            CancellationToken ct,
            ISender sender) => DelegateHandlerAsync(context, handler, sender, ct));
        _routeHandlerBuilder.WithRequestTimeout(TimeSpan.FromSeconds(10));
    }

    protected void Get(string pattern, Func<TRequest, Task<IResult>> handler)
    {
        _routeHandlerBuilder = _endpointRouteBuilder.MapGet(pattern, (
            HttpContext context,
            CancellationToken ct,
            ISender sender) => DelegateHandlerAsync(context, handler, sender, ct));
    }

    protected void Delete(string pattern, Func<TRequest, Task<IResult>> handler)
    {
        _routeHandlerBuilder = _endpointRouteBuilder.MapDelete(pattern, (
            HttpContext context,
            CancellationToken ct,
            [AsParameters] TRequest request,
            ISender sender) => DelegateHandlerAsync(context, handler, sender, ct));
    }

    protected void Put(string pattern, Func<TRequest, Task<IResult>> handler)
    {
        _routeHandlerBuilder = _endpointRouteBuilder.MapPut(pattern, (
            HttpContext context,
            CancellationToken ct,
            ISender sender) => DelegateHandlerAsync(context, handler, sender, ct));
    }

    protected void Patch(string pattern, Func<TRequest, Task<IResult>> handler)
    {
        _routeHandlerBuilder = _endpointRouteBuilder.MapPatch(pattern, (
            HttpContext context,
            CancellationToken ct,
            ISender sender) => DelegateHandlerAsync(context, handler, sender, ct));
    }

    private async Task<IResult> DelegateHandlerAsync(
        HttpContext context,
        Func<TRequest, Task<IResult>> handler,
        ISender sender,
        CancellationToken ct)
    {
        Context = context;
        CancellationToken = ct;
        _sender = sender;

        var request = await BindRequestAsync(context).ConfigureAwait(false);
        return await handler(request).ConfigureAwait(false);
    }

    protected async Task<T> SendAsync<T>(IRequest<T> request)
    {
        return await _sender.Send(request, CancellationToken).ConfigureAwait(false);
    }

    private static async Task<TRequest> BindRequestAsync(HttpContext context)
    {
        var request = new TRequest();
        var properties = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        request = await BindRequestBody(properties, context, request);
        BindQueryAndRoute(properties, context, request);
        return request;
    }

    private static void BindQueryAndRoute(PropertyInfo[] properties, HttpContext context, TRequest request)
    {
        foreach (var property in properties)
        {
            var queryName = GetAttributeName<FromQueryAttribute>(property);
            var routeName = GetAttributeName<FromRouteAttribute>(property);

            var queryValue = context.Request.Query[queryName ?? property.Name];
            var routeValue = context.Request.RouteValues[routeName ?? property.Name];

            if (!string.IsNullOrEmpty(queryValue))
                property.SetValue(request, Convert.ChangeType(queryValue.ToString(), property.PropertyType));
            else if (routeValue != null)
                property.SetValue(request, Convert.ChangeType(routeValue.ToString(), property.PropertyType));
        }
    }

    private static async Task<TRequest> BindRequestBody(PropertyInfo[] properties, HttpContext context,
        TRequest request)
    {
        if (!context.Request.HasJsonContentType() || !(context.Request.ContentLength > 0)) return new TRequest();

        context.Request.EnableBuffering(); // Allow the stream to be read multiple times
        context.Request.Body.Position = 0; // Ensure we're at the start of the stream

        using var reader = new StreamReader(context.Request.Body);
        var jsonBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0; // Reset stream position after reading

        if (string.IsNullOrWhiteSpace(jsonBody))
            return new TRequest();

        try
        {
            var bodyData = JsonSerializer.Deserialize<TRequest>(jsonBody, JsonSerializerOptions);
            return bodyData ?? new TRequest();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON Deserialization error: {ex.Message}");
            throw;
        }
    }

    private static string? GetAttributeName<TAttribute>(PropertyInfo property) where TAttribute : Attribute
    {
        var attribute = property.GetCustomAttribute<TAttribute>();
        return attribute switch
        {
            FromQueryAttribute fromQuery => fromQuery.Name,
            FromRouteAttribute fromRoute => fromRoute.Name,
            JsonPropertyNameAttribute jsonProperty => jsonProperty.Name,
            _ => null
        };
    }

    protected void Name(string name)
    {
        _routeHandlerBuilder.WithName(name);
    }

    protected void Produces(
        int statusCode = StatusCodes.Status200OK,
        string? contentType = null,
        params string[] additionalContentTypes)
    {
        _routeHandlerBuilder.Produces<TResponse>(statusCode, contentType, additionalContentTypes);
    }

    protected void ProducesProblem(int statusCode, string? contentType = null)
    {
        _routeHandlerBuilder.ProducesProblem(statusCode, contentType);
    }

    protected void Summary(string summary)
    {
        _routeHandlerBuilder.WithSummary(summary);
    }

    protected void Description(string description)
    {
        _routeHandlerBuilder.WithDescription(description);
    }

    protected void Tags(params string[] tags)
    {
        _routeHandlerBuilder.WithTags(tags);
    }

    protected void Policies(params string[] policyNames)
    {
        if (policyNames.Length == 0)
            _routeHandlerBuilder.RequireAuthorization();
        else
            _routeHandlerBuilder.RequireAuthorization(policyNames);
    }
}