using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infrastructure.Persistence;

public class EfCatalogoRepository : ICatalogoRepository
{
    private readonly FacturacionDbContext _db;

    public EfCatalogoRepository(FacturacionDbContext db) => _db = db;

    public Task<ItemCatalogo?> ObtenerAsync(TipoCatalogo tipo, string codigo, CancellationToken ct = default) =>
        _db.Catalogos.SingleOrDefaultAsync(c => c.Tipo == tipo && c.Codigo == codigo, ct);

    public async Task ReemplazarAsync(TipoCatalogo tipo, IReadOnlyList<ItemCatalogo> items, CancellationToken ct = default)
    {
        // Dos SaveChanges separados (no uno solo con ambas operaciones): el índice
        // único (Tipo, Codigo) puede rechazar el INSERT de un código repetido si
        // EF Core llegara a ordenarlo antes que el DELETE del mismo código dentro
        // de un único batch.
        var existentes = await _db.Catalogos.Where(c => c.Tipo == tipo).ToListAsync(ct);
        _db.Catalogos.RemoveRange(existentes);
        await _db.SaveChangesAsync(ct);

        await _db.Catalogos.AddRangeAsync(items, ct);
        await _db.SaveChangesAsync(ct);
    }
}
