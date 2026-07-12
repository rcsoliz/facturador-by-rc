using System.Security.Cryptography;
using System.Text;

namespace Facturacion.Infrastructure.Webhooks;

/// <summary>
/// Firma HMAC-SHA256 del payload de un webhook, para que el sistema cliente
/// pueda verificar que la notificación viene realmente de este servicio.
/// El timestamp se incluye en el mensaje firmado (no solo en el header) para
/// que un receptor estricto pueda rechazar reintentos con timestamp viejo
/// (mitigación de replay).
/// </summary>
public static class FirmaWebhook
{
    public static string Calcular(string secreto, string timestampUnixSegundos, string cuerpoJson)
    {
        var mensaje = $"{timestampUnixSegundos}.{cuerpoJson}";
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secreto), Encoding.UTF8.GetBytes(mensaje));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
