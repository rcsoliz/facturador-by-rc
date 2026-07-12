using Facturacion.Domain.Entities;
using Facturacion.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infrastructure.Persistence;

public class EfCredencialSiatRepository : ICredencialSiatRepository
{
    private readonly FacturacionDbContext _db;

    public EfCredencialSiatRepository(FacturacionDbContext db) => _db = db;

    public Task<CredencialSiat?> ObtenerAsync(
        Guid tenantId, Guid sucursalId, Guid? puntoVentaId, CancellationToken ct = default) =>
        _db.CredencialesSiat.SingleOrDefaultAsync(
            c => c.TenantId == tenantId && c.SucursalId == sucursalId && c.PuntoVentaId == puntoVentaId, ct);

    public async Task AgregarAsync(CredencialSiat credencial, CancellationToken ct = default) =>
        await _db.CredencialesSiat.AddAsync(credencial, ct);

    public Task GuardarCambiosAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
