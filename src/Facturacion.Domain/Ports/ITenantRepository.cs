using Facturacion.Domain.Entities;

namespace Facturacion.Domain.Ports;

public interface ITenantRepository
{
    Task<Tenant?> ObtenerAsync(Guid tenantId, CancellationToken ct = default);
    Task<Tenant?> ObtenerPorApiKeyHashAsync(string apiKeyHash, CancellationToken ct = default);
    Task AgregarAsync(Tenant tenant, CancellationToken ct = default);

    /// <summary>
    /// Marca una Sucursal recién creada (vía Tenant.AgregarSucursal, sobre un
    /// Tenant que ya existe en la base) como nueva explícitamente. Necesario
    /// porque su Id (Guid generado en el dominio, no por la base) hace que EF
    /// Core, si la descubre solo por el grafo de navegación de un Tenant ya
    /// trackeado, la infiera como existente (UPDATE) en vez de nueva (INSERT).
    /// </summary>
    Task AgregarSucursalAsync(Sucursal sucursal, CancellationToken ct = default);

    /// <summary>Mismo motivo que <see cref="AgregarSucursalAsync"/>, para PuntoVenta.</summary>
    Task AgregarPuntoVentaAsync(PuntoVenta puntoVenta, CancellationToken ct = default);

    Task GuardarCambiosAsync(CancellationToken ct = default);
}
