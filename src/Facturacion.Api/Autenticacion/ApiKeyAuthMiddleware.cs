using Facturacion.Domain.Common;
using Facturacion.Domain.Ports;

namespace Facturacion.Api.Autenticacion;

/// <summary>
/// Autenticación por header X-Api-Key: resuelve el tenant y lo publica en
/// <see cref="ICurrentTenant"/> para el resto del pipeline. Los sistemas
/// cliente solo conocen su API key, nunca el tenantId.
/// </summary>
public class ApiKeyAuthMiddleware
{
    private const string HeaderName = "X-Api-Key";
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantRepository tenants, ICurrentTenant currentTenant)
    {
        // Los endpoints /api/v1/admin (onboarding) los protege AdminApiKeyMiddleware:
        // no hay tenant todavía cuando se está dando de alta uno. El dashboard de
        // Hangfire (/hangfire) tiene su propia autorización (ver Program.cs, hoy
        // sin restricción real porque solo se expone en Development) — no es un
        // endpoint de tenant, no tiene sentido exigirle X-Api-Key.
        if (context.Request.Path.StartsWithSegments("/api/v1/admin") ||
            context.Request.Path.StartsWithSegments("/hangfire"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
        {
            await EscribirNoAutorizado(context, "Falta el header X-Api-Key.");
            return;
        }

        var hash = ApiKeyHasher.Hash(apiKey.ToString());
        var tenant = await tenants.ObtenerPorApiKeyHashAsync(hash, context.RequestAborted);

        if (tenant is null || !tenant.Activo)
        {
            await EscribirNoAutorizado(context, "API key inválida.");
            return;
        }

        currentTenant.Establecer(tenant);
        await _next(context);
    }

    private static Task EscribirNoAutorizado(HttpContext context, string detalle)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            title = "No autorizado",
            status = 401,
            detail = detalle,
        });
    }
}
