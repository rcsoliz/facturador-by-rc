using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;

namespace Facturacion.Domain.Ports;

public interface ICatalogoRepository
{
    Task<ItemCatalogo?> ObtenerAsync(TipoCatalogo tipo, string codigo, CancellationToken ct = default);

    /// <summary>
    /// Reemplaza por completo los ítems de un tipo de catálogo (borra los
    /// anteriores, inserta los nuevos) en una sola operación atómica — así
    /// se aplica una sincronización diaria del SIN, que entrega el catálogo
    /// completo vigente, no un delta.
    /// </summary>
    Task ReemplazarAsync(TipoCatalogo tipo, IReadOnlyList<ItemCatalogo> items, CancellationToken ct = default);
}
