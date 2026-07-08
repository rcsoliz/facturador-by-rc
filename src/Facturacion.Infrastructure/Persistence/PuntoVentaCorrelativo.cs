namespace Facturacion.Infrastructure.Persistence;

/// <summary>
/// Fila de contador atómico por punto de venta. Detalle de persistencia,
/// no es un concepto de dominio (por eso no hereda de Domain.Common.Entity).
/// </summary>
public class PuntoVentaCorrelativo
{
    public Guid PuntoVentaId { get; set; }
    public long UltimoNumero { get; set; }
}
