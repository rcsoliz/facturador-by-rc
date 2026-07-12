using Facturacion.Application.Dtos;
using Facturacion.Domain.Ports;

namespace Facturacion.Application.Commands.RegistrarCredencialSiat;

/// <summary>
/// Caso de uso: el propio tenant registra (o rota) el token delegado que el SIN
/// le entregó para una sucursal/punto de venta (autoservicio, X-Api-Key).
/// Prerequisito para que <c>CredencialesService</c> pueda obtener CUIS/CUFD.
/// </summary>
public class RegistrarCredencialSiatHandler
{
    private readonly ITenantRepository _tenants;
    private readonly IGestorCredencialesSiat _gestorCredenciales;

    public RegistrarCredencialSiatHandler(ITenantRepository tenants, IGestorCredencialesSiat gestorCredenciales)
    {
        _tenants = tenants;
        _gestorCredenciales = gestorCredenciales;
    }

    /// <summary>Devuelve false si la sucursal (o el punto de venta) no existe o no pertenece al tenant.</summary>
    public async Task<bool> HandleAsync(
        Guid tenantId, Guid sucursalId, RegistrarCredencialSiatRequest request, CancellationToken ct = default)
    {
        var tenant = await _tenants.ObtenerAsync(tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {tenantId} no encontrado.");

        var sucursal = tenant.Sucursales.SingleOrDefault(s => s.Id == sucursalId);
        if (sucursal is null) return false;

        if (request.PuntoVentaId is { } puntoVentaId
            && sucursal.PuntosVenta.All(p => p.Id != puntoVentaId))
            return false;

        await _gestorCredenciales.RegistrarTokenDelegadoAsync(
            tenantId, sucursalId, request.PuntoVentaId, request.TokenDelegado, ct);

        return true;
    }
}
