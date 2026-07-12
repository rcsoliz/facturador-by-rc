using Facturacion.Domain.Common;
using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.ValueObjects;
using Xunit;

namespace Facturacion.Tests;

public class FacturaStateMachineTests
{
    private static Factura CrearFactura() => new(
        tenantId: Guid.NewGuid(), sucursalId: Guid.NewGuid(), puntoVentaId: Guid.NewGuid(),
        codigoDocumentoSector: 1, referenciaExterna: "REF-001",
        razonSocialComprador: "Cliente Prueba", codigoTipoDocumentoIdentidad: 1,
        numeroDocumentoComprador: "1234567", complemento: null, emailComprador: null,
        codigoMoneda: 1, tipoCambio: 1,
        codigoMetodoPago: 1, numeroTarjeta: null,
        detalles: new[] { new DetalleFactura(99100, "P-1", "Servicio de prueba", 1, 58, 100m) });

    [Fact]
    public void FlujoNormal_PendienteAValidada()
    {
        var f = CrearFactura();
        Assert.Equal(EstadoFactura.Pendiente, f.Estado);

        f.AsignarNumero(1);
        f.MarcarGenerada(new Cuf("ABC123"), "<xml/>");
        Assert.Equal(EstadoFactura.Generada, f.Estado);

        f.MarcarEnviada();
        f.MarcarValidada("REC-999", "{}");
        Assert.Equal(EstadoFactura.Validada, f.Estado);
        Assert.Equal("REC-999", f.CodigoRecepcionSin);
    }

    [Fact]
    public void NoSePuedeAnular_SiNoEstaValidada()
    {
        var f = CrearFactura();
        var ex = Assert.Throws<DomainException>(() => f.MarcarAnulada(1, "{}"));
        Assert.Equal("TRANSICION_INVALIDA", ex.Codigo);
    }

    [Fact]
    public void Rechazada_PuedeReintentarse()
    {
        var f = CrearFactura();
        f.AsignarNumero(1);
        f.MarcarGenerada(new Cuf("ABC123"), "<xml/>");
        f.MarcarEnviada();
        f.MarcarRechazada("Error 905", "{}");

        f.ReintentarTrasRechazo();

        Assert.Equal(EstadoFactura.Pendiente, f.Estado);
        Assert.Null(f.Cuf);
        Assert.Null(f.MotivoRechazo);
    }

    [Fact]
    public void Contingencia_PuedeMarcarseValidadaORechazadaAlEnviarElPaquete()
    {
        var validada = CrearFactura();
        validada.MarcarEnContingencia();
        validada.MarcarValidada("REC-1", "{}");
        Assert.Equal(EstadoFactura.Validada, validada.Estado);

        var rechazada = CrearFactura();
        rechazada.MarcarEnContingencia();
        rechazada.MarcarRechazada("Error 905", "{}");
        Assert.Equal(EstadoFactura.Rechazada, rechazada.Estado);
    }

    [Fact]
    public void MontoTotal_SeCalculaDesdeDetalles()
    {
        var f = new Factura(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, "REF-002",
            "Cliente", 1, "111", null, null, 1, 1, 1, null,
            new[]
            {
                new DetalleFactura(99100, "A", "Item A", 2, 58, 50m),      // 100
                new DetalleFactura(99100, "B", "Item B", 1, 58, 30m, 5m)   // 25
            });

        Assert.Equal(125m, f.MontoTotal);
    }

    [Fact]
    public void CodigoMetodoPagoFueraDeRango_LanzaExcepcion()
    {
        var ex = Assert.Throws<DomainException>(() => new Factura(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, "REF-004",
            "Cliente", 1, "111", null, null, 1, 1, 309, null,
            new[] { new DetalleFactura(99100, "P-1", "Servicio", 1, 58, 10m) }));
        Assert.Equal("METODO_PAGO_INVALIDO", ex.Codigo);
    }

    [Fact]
    public void FacturaSinDetalles_LanzaExcepcion()
    {
        var ex = Assert.Throws<DomainException>(() => new Factura(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, "REF-003",
            "Cliente", 1, "111", null, null, 1, 1, 1, null,
            Array.Empty<DetalleFactura>()));
        Assert.Equal("FACTURA_SIN_DETALLE", ex.Codigo);
    }
}
