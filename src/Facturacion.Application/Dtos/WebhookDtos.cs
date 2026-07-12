namespace Facturacion.Application.Dtos;

/// <param name="Url">URL HTTPS a la que se enviarán los cambios de estado de las facturas.</param>
/// <param name="Secreto">Secreto en texto plano usado para firmar (HMAC-SHA256) cada envío; se cifra al persistir.</param>
public sealed record ConfigurarWebhookRequest(string Url, string Secreto);
