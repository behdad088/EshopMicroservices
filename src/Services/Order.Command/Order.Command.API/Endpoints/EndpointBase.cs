using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Order.Command.API.Endpoints;

public interface IEndpoint
{
    void Configure(IEndpointRouteBuilder routes);
    void MapEndpoint();
}

/// <summary>
///     Per-request context handed to <see cref="EndpointBase{TRequest,TResponse}.HandleAsync"/>.
///     Replaces the instance-level state that the previous version stored on the endpoint object,
///     which was unsafe under concurrent requests because endpoint instances are shared across requests.
/// </summary>
public sealed record EndpointContext(
    HttpContext HttpContext,
    IAuthorizationService Authorization,
    ISender Sender,
    CancellationToken CancellationToken)
{
    /// <summary>Dispatch a MediatR request using this request's cancellation token.</summary>
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        => Sender.Send(request, CancellationToken);
}

/// <summary>
///     Base for endpoints that don't return a response body (typically 204).
/// </summary>
public abstract class EndpointBase<TRequest> : EndpointBase<TRequest, object?>
    where TRequest : notnull, new();

/// <summary>
///     Base for endpoints with a request and a typed response body.
///
///     Subclasses override <see cref="MapEndpoint"/> (declaring the route + metadata)
///     and <see cref="HandleAsync"/> (the per-request logic).
///
///     Binding:
///     <list type="bullet">
///         <item><description>POST/PUT/PATCH: body is bound by the framework via <c>[FromBody]</c> (uses the configured JSON pipeline).</description></item>
///         <item><description>GET/DELETE: no body is read. The Request DTO's <c>[FromRoute]</c> and <c>[FromQuery]</c> properties are populated from <c>HttpContext.Request.RouteValues</c> / <c>HttpContext.Request.Query</c>.</description></item>
///         <item><description>For body verbs, any <c>[FromRoute]</c> / <c>[FromQuery]</c> properties on the Request DTO are also populated after body deserialization.</description></item>
///     </list>
///     Route/query introspection runs once per closed generic type (cached).
/// </summary>
public abstract class EndpointBase<TRequest, TResponse> : IEndpoint
    where TRequest : notnull, new()
{
    private IEndpointRouteBuilder _routes = default!;
    private RouteHandlerBuilder _builder = default!;

    public void Configure(IEndpointRouteBuilder routes) => _routes = routes;

    /// <summary>
    ///     Declare the route, verb, and OpenAPI metadata. Use the helpers
    ///     (<see cref="Post"/>, <see cref="Get"/>, <see cref="Put"/>, <see cref="Delete"/>, <see cref="Patch"/>),
    ///     then optionally chain <see cref="Name"/>, <see cref="Produces"/>, <see cref="ProducesProblem"/>,
    ///     <see cref="Summary"/>, <see cref="Description"/>, <see cref="Tags"/>, <see cref="Policies"/>,
    ///     <see cref="AllowAnonymous"/>.
    /// </summary>
    public abstract void MapEndpoint();

    /// <summary>
    ///     Handle a single request. Use <paramref name="ctx"/> to reach the HttpContext, the
    ///     authorization service, MediatR sender, and the request's cancellation token. Do NOT
    ///     stash these on the instance — endpoint instances are shared across concurrent requests.
    /// </summary>
    protected abstract Task<IResult> HandleAsync(TRequest request, EndpointContext ctx);

    protected RouteHandlerBuilder Post(string pattern)
        => _builder = _routes.MapPost(pattern, HandleWithBodyAsync);

    protected RouteHandlerBuilder Put(string pattern)
        => _builder = _routes.MapPut(pattern, HandleWithBodyAsync);

    protected RouteHandlerBuilder Patch(string pattern)
        => _builder = _routes.MapPatch(pattern, HandleWithBodyAsync);

    protected RouteHandlerBuilder Get(string pattern)
        => _builder = _routes.MapGet(pattern, HandleNoBodyAsync);

    protected RouteHandlerBuilder Delete(string pattern)
        => _builder = _routes.MapDelete(pattern, HandleNoBodyAsync);

    private Task<IResult> HandleNoBodyAsync(
        HttpContext http,
        IAuthorizationService auth,
        ISender sender,
        CancellationToken ct)
    {
        var request = new TRequest();
        ApplyRouteAndQuery(request, http);
        return HandleAsync(request, new EndpointContext(http, auth, sender, ct));
    }

    private async Task<IResult> HandleWithBodyAsync(
        [FromBody] TRequest? body,
        HttpContext http,
        IAuthorizationService auth,
        ISender sender,
        CancellationToken ct)
    {
        var request = body ?? new TRequest();
        ApplyRouteAndQuery(request, http);
        return await HandleAsync(request, new EndpointContext(http, auth, sender, ct));
    }

    private sealed record BindableProperty(PropertyInfo Property, string? RouteName, string? QueryName);

    private static readonly Lazy<BindableProperty[]> BindableProperties = new(() =>
        typeof(TRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .Select(p =>
            {
                var route = p.GetCustomAttribute<FromRouteAttribute>();
                var query = p.GetCustomAttribute<FromQueryAttribute>();
                return new BindableProperty(
                    p,
                    route is not null ? (route.Name ?? p.Name) : null,
                    query is not null ? (query.Name ?? p.Name) : null);
            })
            .Where(b => b.RouteName is not null || b.QueryName is not null)
            .ToArray());

    private static void ApplyRouteAndQuery(TRequest request, HttpContext http)
    {
        foreach (var info in BindableProperties.Value)
        {
            if (info.RouteName is not null
                && http.Request.RouteValues.TryGetValue(info.RouteName, out var routeRaw)
                && routeRaw is not null)
            {
                var converted = ConvertValue(routeRaw, info.Property.PropertyType);
                if (converted is not null) info.Property.SetValue(request, converted);
                continue;
            }

            if (info.QueryName is not null
                && http.Request.Query.TryGetValue(info.QueryName, out var queryRaw)
                && queryRaw.Count > 0
                && !string.IsNullOrEmpty(queryRaw[0]))
            {
                var converted = ConvertValue(queryRaw[0]!, info.Property.PropertyType);
                if (converted is not null) info.Property.SetValue(request, converted);
            }
        }
    }

    private static object? ConvertValue(object raw, Type target)
    {
        if (target == typeof(string)) return raw.ToString();
        var underlying = Nullable.GetUnderlyingType(target) ?? target;
        if (underlying == typeof(Guid) && Guid.TryParse(raw.ToString(), out var g)) return g;
        if (underlying == typeof(Ulid) && Ulid.TryParse(raw.ToString(), out var u)) return u;

        try
        {
            return Convert.ChangeType(raw, underlying);
        }
        catch (FormatException e)
        {
            Log.Error(e, "Failed to convert value {Raw} to type {Target}", raw, target);
            return null;
        }
        catch (InvalidCastException e)
        {
            Log.Error(e, "Failed to convert value {Raw} to type {Target}", raw, target);
            return null;
        }
        catch (OverflowException e)
        {
            Log.Error(e, "Failed to convert value {Raw} to type {Target}", raw, target);
            return null;
        }
    }

    protected RouteHandlerBuilder Name(string name)
        => _builder.WithName(name);

    protected RouteHandlerBuilder Produces(
        int statusCode = StatusCodes.Status200OK,
        string? contentType = null,
        params string[] additionalContentTypes)
        => _builder.Produces<TResponse>(statusCode, contentType, additionalContentTypes);

    protected RouteHandlerBuilder ProducesProblem(int statusCode, string? contentType = null)
        => _builder.ProducesProblem(statusCode, contentType);

    protected RouteHandlerBuilder Summary(string summary)
        => _builder.WithSummary(summary);

    protected RouteHandlerBuilder Description(string description)
        => _builder.WithDescription(description);

    protected RouteHandlerBuilder Tags(params string[] tags)
        => _builder.WithTags(tags);

    protected RouteHandlerBuilder Policies(params string[] policyNames)
        => policyNames.Length == 0
            ? _builder.RequireAuthorization()
            : _builder.RequireAuthorization(policyNames);

    protected RouteHandlerBuilder AllowAnonymous()
        => _builder.AllowAnonymous();
}
