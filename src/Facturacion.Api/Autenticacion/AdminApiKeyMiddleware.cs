using System.Security.Cryptography;
using System.Text;

namespace Facturacion.Api.Autenticacion;

/// <summary>
/// Protege las rutas /api/v1/admin/* con un secreto de administración
/// (X-Admin-Key), separado del X-Api-Key de tenant: estos endpoints existen
/// para dar de alta tenants, antes de que el tenant exista.
/// </summary>
public class AdminApiKeyMiddleware
{
    private const string HeaderName = "X-Admin-Key";
    private const string RutaAdmin = "/api/v1/admin";
    private readonly RequestDelegate _next;

    public AdminApiKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AdminOptions options)
    {
        if (!context.Request.Path.StartsWithSegments(RutaAdmin))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var key) ||
            !CoincideEnTiempoConstante(key.ToString(), options.ApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "No autorizado",
                status = 401,
                detail = "Header X-Admin-Key ausente o inválido.",
            });
            return;
        }

        await _next(context);
    }

    private static bool CoincideEnTiempoConstante(string a, string b)
    {
        var bytesA = Encoding.UTF8.GetBytes(a);
        var bytesB = Encoding.UTF8.GetBytes(b);
        return bytesA.Length == bytesB.Length && CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
    }
}
