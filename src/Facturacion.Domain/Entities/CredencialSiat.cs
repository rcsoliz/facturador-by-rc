using Facturacion.Domain.Common;

namespace Facturacion.Domain.Entities;

/// <summary>
/// Credenciales operativas frente al SIN por tenant/sucursal/punto de venta.
/// El CUIS dura ~1 año; el CUFD dura 24h y debe renovarse antes de vencer
/// (job programado en Workers). El token delegado se guarda cifrado.
/// </summary>
public class CredencialSiat : Entity
{
    public Guid TenantId { get; private set; }
    public Guid SucursalId { get; private set; }
    public Guid? PuntoVentaId { get; private set; }

    /// <summary>Token delegado SIAT, cifrado en reposo.</summary>
    public string TokenDelegadoCifrado { get; private set; } = null!;

    public string? Cuis { get; private set; }
    public DateTime? CuisVence { get; private set; }

    public string? Cufd { get; private set; }
    public string? CufdCodigoControl { get; private set; }
    public DateTime? CufdVence { get; private set; }

    private CredencialSiat() { } // EF Core

    public CredencialSiat(Guid tenantId, Guid sucursalId, Guid? puntoVentaId, string tokenDelegadoCifrado)
    {
        TenantId = tenantId;
        SucursalId = sucursalId;
        PuntoVentaId = puntoVentaId;
        TokenDelegadoCifrado = tokenDelegadoCifrado;
    }

    public bool CuisVigente(DateTime ahoraUtc) => Cuis is not null && CuisVence > ahoraUtc;
    public bool CufdVigente(DateTime ahoraUtc) => Cufd is not null && CufdVence > ahoraUtc;

    public void ActualizarCuis(string cuis, DateTime vence)
    {
        Cuis = cuis; CuisVence = vence; MarcarActualizado();
    }

    public void ActualizarCufd(string cufd, string codigoControl, DateTime vence)
    {
        Cufd = cufd; CufdCodigoControl = codigoControl; CufdVence = vence; MarcarActualizado();
    }

    /// <summary>Rotación del token delegado (p.ej. reemisión desde el portal del SIN).</summary>
    public void ActualizarTokenDelegado(string tokenDelegadoCifrado)
    {
        TokenDelegadoCifrado = tokenDelegadoCifrado; MarcarActualizado();
    }
}
