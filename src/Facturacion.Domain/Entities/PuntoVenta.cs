using Facturacion.Domain.Common;

namespace Facturacion.Domain.Entities;

public class PuntoVenta : Entity
{
    public Guid SucursalId { get; private set; }
    public int CodigoSiat { get; private set; }
    public string Nombre { get; private set; } = null!;

    /// <summary>Tipo según paramétrica SIAT (comisionista, ventanilla, móvil, etc.).</summary>
    public int TipoPuntoVenta { get; private set; }

    private PuntoVenta() { } // EF Core

    internal PuntoVenta(Guid sucursalId, int codigoSiat, string nombre, int tipoPuntoVenta)
    {
        SucursalId = sucursalId;
        CodigoSiat = codigoSiat;
        Nombre = nombre;
        TipoPuntoVenta = tipoPuntoVenta;
    }
}
