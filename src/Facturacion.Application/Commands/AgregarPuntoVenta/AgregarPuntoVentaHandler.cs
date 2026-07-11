using Facturacion.Application.Dtos;
using Facturacion.Domain.Ports;

namespace Facturacion.Application.Commands.AgregarPuntoVenta;

/// <summary>
/// Caso de uso: el propio tenant da de alta un punto de venta bajo una de sus
/// sucursales (autoservicio, X-Api-Key). PuntoVenta es parte del agregado
/// Tenant/Sucursal — se muta vía Sucursal.AgregarPuntoVenta.
/// </summary>
public class AgregarPuntoVentaHandler
{
    private readonly ITenantRepository _tenants;

    public AgregarPuntoVentaHandler(ITenantRepository tenants) => _tenants = tenants;

    /// <summary>Devuelve null si la sucursal no existe o no pertenece al tenant.</summary>
    public async Task<PuntoVentaResponse?> HandleAsync(
        Guid tenantId, Guid sucursalId, AgregarPuntoVentaRequest request, CancellationToken ct = default)
    {
        var tenant = await _tenants.ObtenerAsync(tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {tenantId} no encontrado.");

        var sucursal = tenant.Sucursales.SingleOrDefault(s => s.Id == sucursalId);
        if (sucursal is null) return null;

        var puntoVenta = sucursal.AgregarPuntoVenta(request.CodigoSiat, request.Nombre, request.TipoPuntoVenta);

        await _tenants.AgregarPuntoVentaAsync(puntoVenta, ct);
        await _tenants.GuardarCambiosAsync(ct);

        return PuntoVentaResponse.Desde(puntoVenta);
    }
}
