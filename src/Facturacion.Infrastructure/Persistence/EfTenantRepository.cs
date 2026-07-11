using Facturacion.Domain.Entities;
using Facturacion.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infrastructure.Persistence;

public class EfTenantRepository : ITenantRepository
{
    private readonly FacturacionDbContext _db;

    public EfTenantRepository(FacturacionDbContext db) => _db = db;

    public Task<Tenant?> ObtenerAsync(Guid tenantId, CancellationToken ct = default) =>
        _db.Tenants.Include(t => t.Sucursales).ThenInclude(s => s.PuntosVenta)
            .AsSplitQuery()
            .SingleOrDefaultAsync(t => t.Id == tenantId, ct);

    public Task<Tenant?> ObtenerPorApiKeyHashAsync(string apiKeyHash, CancellationToken ct = default) =>
        _db.Tenants.Include(t => t.Sucursales).ThenInclude(s => s.PuntosVenta)
            .AsSplitQuery()
            .SingleOrDefaultAsync(t => t.ApiKeyHash == apiKeyHash, ct);

    public async Task AgregarAsync(Tenant tenant, CancellationToken ct = default) =>
        await _db.Tenants.AddAsync(tenant, ct);

    public async Task AgregarSucursalAsync(Sucursal sucursal, CancellationToken ct = default) =>
        await _db.Sucursales.AddAsync(sucursal, ct);

    public async Task AgregarPuntoVentaAsync(PuntoVenta puntoVenta, CancellationToken ct = default) =>
        await _db.PuntosVenta.AddAsync(puntoVenta, ct);

    public Task GuardarCambiosAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
