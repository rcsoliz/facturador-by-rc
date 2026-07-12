namespace Facturacion.Infrastructure.Siat.Common;

/// <summary>
/// Puerto interno (no Domain) para las operaciones SOAP de sincronización de
/// catálogos/paramétricas del SIN. Sin implementación real todavía — ver
/// restricción "SIN ACCESO AL AMBIENTE PILOTO DEL SIN" en CLAUDE.md. Única
/// implementación hoy: <see cref="Fake.CatalogosClienteFake"/>.
/// </summary>
public interface ISinCatalogosClient
{
    Task<IReadOnlyList<ItemCatalogoSin>> ObtenerProductosServiciosAsync(CancellationToken ct = default);

    Task<IReadOnlyList<ItemCatalogoSin>> ObtenerActividadesEconomicasAsync(CancellationToken ct = default);
}

public sealed record ItemCatalogoSin(string Codigo, string Descripcion, bool Activo);
