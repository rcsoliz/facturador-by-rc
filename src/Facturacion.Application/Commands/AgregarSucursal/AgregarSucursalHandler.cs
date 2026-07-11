using Facturacion.Application.Dtos;
using Facturacion.Domain.Ports;

namespace Facturacion.Application.Commands.AgregarSucursal;

/// <summary>
/// Caso de uso: el propio tenant da de alta una sucursal (autoservicio, X-Api-Key).
/// Sucursal es parte del agregado Tenant — se muta vía Tenant.AgregarSucursal,
/// nunca se persiste por separado.
/// </summary>
public class AgregarSucursalHandler
{
    private readonly ITenantRepository _tenants;

    public AgregarSucursalHandler(ITenantRepository tenants) => _tenants = tenants;

    public async Task<SucursalResponse> HandleAsync(
        Guid tenantId, AgregarSucursalRequest request, CancellationToken ct = default)
    {
        var tenant = await _tenants.ObtenerAsync(tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant {tenantId} no encontrado.");

        var sucursal = tenant.AgregarSucursal(
            request.CodigoSiat, request.Direccion, request.Municipio, request.ActividadEconomica);

        await _tenants.AgregarSucursalAsync(sucursal, ct);
        await _tenants.GuardarCambiosAsync(ct);

        return SucursalResponse.Desde(sucursal);
    }
}
