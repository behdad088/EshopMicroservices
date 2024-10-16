using System.Reflection;
using MediatR;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Order.Command.API.Endpoints;

public interface IEndpoint
{
    void MapEndpoint();

    void Configure(IEndpointRouteBuilder app);
}

/// <summary>
/// Only use this base class when the endpoint does not return any dto
/// </summary>
/// <typeparam name="TRequest">Request dto</typeparam>
public abstract class EndpointBase<TRequest> : EndpointBase<TRequest, object?> where TRequest : notnull;

/// <summary>
/// use this base class for defining endpoints that use both request and response dtos.
/// </summary>
/// <typeparam name="TRequest">Request dto</typeparam>
/// <typeparam name="TResponse">Response dto</typeparam>
public abstract class EndpointBase<TRequest, TResponse> : IEndpoint where TRequest : notnull
{
    protected HttpContext Context { get; set; } = default!;
    protected CancellationToken CancellationToken { get; set; }
    private IEndpointRouteBuilder _endpointRouteBuilder = default!;
    private RouteHandlerBuilder _routeHandlerBuilder = default!;

    /// <summary>
    /// Map the endpoint with a route options : <see cref="Post"/>, <see cref="Get"/>, <see cref="Delete"/>, <see cref="Put"/>, <see cref="Patch"/>
    /// Allowed metadata to add to an endpoint : <see cref="Name"/>, <see cref="Produces"/>, <see cref="ProducesProblem"/>, <see cref="Summary"/>,
    /// <see cref="Description"/>, <see cref="Tags"/>
    /// Following 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public virtual void MapEndpoint() => throw new NotImplementedException();

    public void Configure(IEndpointRouteBuilder app)
    {
        _endpointRouteBuilder = app;
    }

    public virtual Task<IResult> HandleAsync(TRequest request, ISender sender) =>
        throw new NotImplementedException();

    public virtual IResult Handle(TRequest request, ISender sender) =>
        throw new NotImplementedException();

    protected void Post(string pattern,
        Delegate handler)
    {
        _routeHandlerBuilder = _endpointRouteBuilder.MapPost(pattern, (
            HttpContext context,
            CancellationToken ct,
            TRequest request,
            ISender sender) => DelegateHandler(context, handler, request, sender, ct));
    }

    protected void Get(string pattern, Delegate handler)
    {
        _routeHandlerBuilder = _endpointRouteBuilder.MapGet(pattern, (
            HttpContext context,
            CancellationToken ct,
            TRequest request,
            ISender sender) => DelegateHandler(context, handler, request, sender, ct));
    }

    protected void Delete(string pattern, Delegate handler)
    {
        _routeHandlerBuilder = _endpointRouteBuilder.MapDelete(pattern, (
            HttpContext context,
            CancellationToken ct,
            TRequest request,
            ISender sender) => DelegateHandler(context, handler, request, sender, ct));
    }

    protected void Put(string pattern, Delegate handler)
    {
        _routeHandlerBuilder = _endpointRouteBuilder.MapPut(pattern, (
            HttpContext context,
            CancellationToken ct,
            TRequest request,
            ISender sender) => DelegateHandler(context, handler, request, sender, ct));
    }

    protected void Patch(string pattern, Delegate handler)
    {
        _routeHandlerBuilder = _endpointRouteBuilder.MapPatch(pattern, (
            HttpContext context,
            CancellationToken ct,
            TRequest request,
            ISender sender) => DelegateHandler(context, handler, request, sender, ct));
    }

    private void DelegateHandler(HttpContext context, Delegate handler, TRequest request, ISender sender,
        CancellationToken ct)
    {
        Context = context;
        CancellationToken = ct;
        _routeHandlerBuilder.WithRequestTimeout(TimeSpan.FromSeconds(10));

        handler.DynamicInvoke(request, sender);
    }

    protected void Name(string name) => _routeHandlerBuilder.WithName(name);

    protected void Produces(
        int statusCode = StatusCodes.Status200OK,
        string? contentType = null,
        params string[] additionalContentTypes) =>
        _routeHandlerBuilder.Produces<TResponse>(statusCode, contentType, additionalContentTypes);

    protected void ProducesProblem(int statusCode, string? contentType = null) =>
        _routeHandlerBuilder.ProducesProblem(statusCode, contentType);

    protected void Summary(string summary) => _routeHandlerBuilder.WithSummary(summary);

    protected void Description(string description) => _routeHandlerBuilder.WithDescription(description);

    protected void Tags(params string[] tags) => _routeHandlerBuilder.WithTags(tags);
}