using Facturacion.Domain.Common;
using Facturacion.Domain.Enums;

namespace Facturacion.Domain.Entities;

/// <summary>
/// Un ítem sincronizado de un catálogo/paramétrica del SIN (p.ej. un producto
/// o una actividad económica). Los catálogos son globales, no por tenant, y se
/// reemplazan por completo en cada sincronización (<see cref="Ports.ICatalogoRepository.ReemplazarAsync"/>) —
/// el SIN no expone deltas, entrega el catálogo completo vigente.
/// </summary>
public class ItemCatalogo : Entity
{
    public TipoCatalogo Tipo { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Descripcion { get; private set; } = null!;
    public bool Activo { get; private set; }

    private ItemCatalogo() { } // EF Core

    public ItemCatalogo(TipoCatalogo tipo, string codigo, string descripcion, bool activo)
    {
        Tipo = tipo;
        Codigo = codigo;
        Descripcion = descripcion;
        Activo = activo;
    }
}
