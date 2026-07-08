using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infrastructure.Persistence;

public class EfFacturaRepository : IFacturaRepository
{
    private readonly FacturacionDbContext _db;

    public EfFacturaRepository(FacturacionDbContext db) => _db = db;

    public Task<Factura?> ObtenerAsync(Guid tenantId, Guid facturaId, CancellationToken ct = default) =>
        _db.Facturas.Include(f => f.Detalles)
            .SingleOrDefaultAsync(f => f.TenantId == tenantId && f.Id == facturaId, ct);

    public Task<Factura?> ObtenerPorReferenciaAsync(Guid tenantId, string referenciaExterna, CancellationToken ct = default) =>
        _db.Facturas.Include(f => f.Detalles)
            .SingleOrDefaultAsync(f => f.TenantId == tenantId && f.ReferenciaExterna == referenciaExterna, ct);

    public async Task<IReadOnlyList<Factura>> ListarPorEstadoAsync(Guid tenantId, EstadoFactura estado, CancellationToken ct = default) =>
        await _db.Facturas.Include(f => f.Detalles)
            .Where(f => f.TenantId == tenantId && f.Estado == estado)
            .ToListAsync(ct);

    /// <summary>
    /// Correlativo atómico por punto de venta: un único INSERT ... ON CONFLICT ... DO UPDATE
    /// ... RETURNING, resuelto por Postgres con el lock de fila de la PK. Se ejecuta y confirma
    /// en el acto — NO participa del SaveChanges() de GuardarCambiosAsync — porque el número
    /// debe "quemarse" apenas se pide, gane o pierda la operación que lo solicitó (el SIN
    /// tolera huecos en la numeración pero nunca duplicados).
    /// </summary>
    public async Task<long> SiguienteNumeroAsync(Guid puntoVentaId, CancellationToken ct = default)
    {
        // ToListAsync (no SingleAsync): el INSERT ... RETURNING no es SQL "componible" —
        // EF intentaría envolverlo en otro SELECT si se sigue componiendo la IQueryable.
        var numeros = await _db.Database
            .SqlQuery<long>($"""
                INSERT INTO "PuntoVentaCorrelativos" ("PuntoVentaId", "UltimoNumero")
                VALUES ({puntoVentaId}, 1)
                ON CONFLICT ("PuntoVentaId")
                DO UPDATE SET "UltimoNumero" = "PuntoVentaCorrelativos"."UltimoNumero" + 1
                RETURNING "UltimoNumero"
                """)
            .ToListAsync(ct);

        return numeros.Single();
    }

    public async Task AgregarAsync(Factura factura, CancellationToken ct = default) =>
        await _db.Facturas.AddAsync(factura, ct);

    public Task GuardarCambiosAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
