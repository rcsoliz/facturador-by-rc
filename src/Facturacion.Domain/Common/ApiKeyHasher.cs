using System.Security.Cryptography;
using System.Text;

namespace Facturacion.Domain.Common;

/// <summary>
/// Hash determinístico de API keys: permite buscar el Tenant por
/// <see cref="Entities.Tenant.ApiKeyHash"/> sin guardar la key en texto plano.
/// Determinístico a propósito (no salteado) porque se busca por igualdad exacta.
/// </summary>
public static class ApiKeyHasher
{
    public static string Hash(string apiKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
