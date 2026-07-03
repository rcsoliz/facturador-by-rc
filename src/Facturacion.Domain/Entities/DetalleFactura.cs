using Facturacion.Domain.Common;

namespace Facturacion.Domain.Entities;

/// <summary>Ítem de la factura. Los códigos de producto/unidad provienen de las paramétricas SIAT.</summary>
public class DetalleFactura : Entity
{
    public Guid FacturaId { get; private set; }

    /// <summary>Código de producto/servicio SIN (homologado, paramétrica).</summary>
    public int CodigoProductoSin { get; private set; }

    /// <summary>Código interno del producto en el sistema del cliente.</summary>
    public string CodigoProducto { get; private set; } = null!;

    public string Descripcion { get; private set; } = null!;
    public decimal Cantidad { get; private set; }

    /// <summary>Unidad de medida según paramétrica SIAT.</summary>
    public int UnidadMedida { get; private set; }

    public decimal PrecioUnitario { get; private set; }
    public decimal MontoDescuento { get; private set; }

    public decimal SubTotal => Math.Round(Cantidad * PrecioUnitario - MontoDescuento, 2);

    private DetalleFactura() { } // EF Core

    public DetalleFactura(
        int codigoProductoSin, string codigoProducto, string descripcion,
        decimal cantidad, int unidadMedida, decimal precioUnitario, decimal montoDescuento = 0)
    {
        if (cantidad <= 0)
            throw new DomainException("CANTIDAD_INVALIDA", "La cantidad debe ser mayor a cero.");
        if (precioUnitario < 0)
            throw new DomainException("PRECIO_INVALIDO", "El precio unitario no puede ser negativo.");

        CodigoProductoSin = codigoProductoSin;
        CodigoProducto = codigoProducto;
        Descripcion = descripcion;
        Cantidad = cantidad;
        UnidadMedida = unidadMedida;
        PrecioUnitario = precioUnitario;
        MontoDescuento = montoDescuento;
    }
}
