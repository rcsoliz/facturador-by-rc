using System.Text;
using System.Text.Json;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace Facturacion.Infrastructure.Webhooks;

/// <summary>
/// Entrega real por HTTP, firmada con HMAC-SHA256 (<see cref="FirmaWebhook"/>).
/// Los reintentos + circuit breaker viven en la política Polly del HttpClient
/// nombrado <see cref="NombreCliente"/> (ver <c>PoliticasWebhook</c> y su
/// registro en <c>Program.cs</c>), no en esta clase.
/// </summary>
public class NotificadorWebhookHttp : INotificadorWebhook
{
    public const string NombreCliente = "webhooks";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IProteccionDatos _proteccion;
    private readonly ILogger<NotificadorWebhookHttp> _logger;

    public NotificadorWebhookHttp(
        IHttpClientFactory httpClientFactory, IProteccionDatos proteccion, ILogger<NotificadorWebhookHttp> logger)
    {
        _httpClientFactory = httpClientFactory;
        _proteccion = proteccion;
        _logger = logger;
    }

    public async Task NotificarCambioEstadoAsync(Tenant tenant, Factura factura, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenant.WebhookUrl) || string.IsNullOrWhiteSpace(tenant.WebhookSecretCifrado))
        {
            _logger.LogInformation("Tenant {TenantId} sin webhook configurado, se omite notificación.", tenant.Id);
            return;
        }

        var cuerpoJson = JsonSerializer.Serialize(WebhookFacturaPayload.Desde(factura), JsonOptions);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var firma = FirmaWebhook.Calcular(_proteccion.Descifrar(tenant.WebhookSecretCifrado), timestamp, cuerpoJson);

        using var mensaje = new HttpRequestMessage(HttpMethod.Post, tenant.WebhookUrl)
        {
            Content = new StringContent(cuerpoJson, Encoding.UTF8, "application/json"),
        };
        mensaje.Headers.Add("X-Facturacion-Signature", $"sha256={firma}");
        mensaje.Headers.Add("X-Facturacion-Timestamp", timestamp);

        try
        {
            var cliente = _httpClientFactory.CreateClient(NombreCliente);
            using var respuesta = await cliente.SendAsync(mensaje, ct);

            if (!respuesta.IsSuccessStatusCode)
                _logger.LogWarning(
                    "Webhook a tenant {TenantId} (factura {FacturaId}) devolvió {StatusCode} tras los reintentos configurados.",
                    tenant.Id, factura.Id, (int)respuesta.StatusCode);
        }
        catch (Exception ex)
        {
            // Un webhook caído no debe romper el flujo de emisión: la factura ya
            // quedó persistida en su estado final, solo falló la notificación.
            _logger.LogError(
                ex, "Fallo definitivo notificando webhook a tenant {TenantId} (factura {FacturaId}) tras los reintentos configurados.",
                tenant.Id, factura.Id);
        }
    }
}
