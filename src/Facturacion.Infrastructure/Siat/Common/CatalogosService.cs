using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.Ports;

namespace Facturacion.Infrastructure.Siat.Common;

/// <summary>
/// Sincronización diaria de catálogos/paramétricas del SIN (ver "Contexto
/// SIAT" en CLAUDE.md). Hoy se invoca bajo demanda; el job programado diario
/// queda para Workers (roadmap v1, junto con la renovación programada de CUFD).
/// </summary>
public class CatalogosService
{
    private readonly ICatalogoRepository _catalogos;
    private readonly ISinCatalogosClient _cliente;

    public CatalogosService(ICatalogoRepository catalogos, ISinCatalogosClient cliente)
    {
        _catalogos = catalogos;
        _cliente = cliente;
    }

    public async Task SincronizarAsync(CancellationToken ct = default)
    {
        await SincronizarTipoAsync(
            TipoCatalogo.ProductosServicios, await _cliente.ObtenerProductosServiciosAsync(ct), ct);
        await SincronizarTipoAsync(
            TipoCatalogo.ActividadesEconomicas, await _cliente.ObtenerActividadesEconomicasAsync(ct), ct);
    }

    private async Task SincronizarTipoAsync(TipoCatalogo tipo, IReadOnlyList<ItemCatalogoSin> items, CancellationToken ct)
    {
        var entidades = items.Select(i => new ItemCatalogo(tipo, i.Codigo, i.Descripcion, i.Activo)).ToList();
        await _catalogos.ReemplazarAsync(tipo, entidades, ct);
    }

    /// <summary>Para futura validación de campos que hoy pasan sin chequear (p.ej. código de producto, actividad económica).</summary>
    public async Task<bool> ExisteYActivoAsync(TipoCatalogo tipo, string codigo, CancellationToken ct = default)
    {
        var item = await _catalogos.ObtenerAsync(tipo, codigo, ct);
        return item is { Activo: true };
    }
}
