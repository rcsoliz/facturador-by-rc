using Facturacion.Application.Dtos;
using Facturacion.Domain.Common;
using Facturacion.Domain.Ports;

namespace Facturacion.Application.Commands.ConfigurarWebhook;

/// <summary>
/// Caso de uso: el propio tenant configura (o rota) la URL y el secreto HMAC
/// de su webhook (autoservicio, X-Api-Key). El secreto se cifra antes de
/// persistirse (regla de CLAUDE.md: secretos siempre cifrados en reposo).
/// </summary>
public class ConfigurarWebhookHandler
{
    private readonly ITenantRepository _tenants;
    private readonly IProteccionDatos _proteccion;

    public ConfigurarWebhookHandler(ITenantRepository tenants, IProteccionDatos proteccion)
    {
        _tenants = tenants;
        _proteccion = proteccion;
    }

    public async Task HandleAsync(Guid tenantId, ConfigurarWebhookRequest request, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new DomainException("WEBHOOK_URL_INVALIDA", "La URL del webhook debe ser HTTPS absoluta.");
        if (string.IsNullOrWhiteSpace(request.Secreto))
            throw new DomainException("WEBHOOK_SECRETO_REQUERIDO", "El secreto del webhook es obligatorio.");

        var tenant = await _tenants.ObtenerAsync(tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {tenantId} no encontrado.");

        tenant.ConfigurarWebhook(request.Url, _proteccion.Cifrar(request.Secreto));

        await _tenants.GuardarCambiosAsync(ct);
    }
}
