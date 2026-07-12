using Facturacion.Infrastructure.Siat.Common;

namespace Facturacion.Infrastructure.Siat.Fake;

/// <summary>
/// Implementación de <see cref="ISinCatalogosClient"/> para desarrollo local,
/// sin tocar el SIN (ver restricción "SIN ACCESO AL AMBIENTE PILOTO DEL SIN"
/// en CLAUDE.md). Los códigos/descripciones son de ejemplo, con prefijo
/// "FAKE-" para no confundirlos con catálogos reales del SIN (CAEB, códigos
/// de producto/servicio, etc.).
/// </summary>
public sealed class CatalogosClienteFake : ISinCatalogosClient
{
    public Task<IReadOnlyList<ItemCatalogoSin>> ObtenerProductosServiciosAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ItemCatalogoSin>>(new[]
        {
            new ItemCatalogoSin("FAKE-PROD-001", "Producto de prueba 1 (fake, no representa catálogo real del SIN)", true),
            new ItemCatalogoSin("FAKE-PROD-002", "Producto de prueba 2 (fake, no representa catálogo real del SIN)", true),
            new ItemCatalogoSin("FAKE-PROD-003", "Producto de prueba 3, dado de baja (fake)", false),
        });

    public Task<IReadOnlyList<ItemCatalogoSin>> ObtenerActividadesEconomicasAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ItemCatalogoSin>>(new[]
        {
            new ItemCatalogoSin("FAKE-CAEB-001", "Actividad económica de prueba 1 (fake, no representa catálogo real del SIN)", true),
            new ItemCatalogoSin("FAKE-CAEB-002", "Actividad económica de prueba 2 (fake, no representa catálogo real del SIN)", true),
        });
}
