using Facturacion.Infrastructure.Siat.Common;

namespace Facturacion.Tests;

public class XmlFacturaBuilderTests
{
    // Réplica de los valores del ejemplo oficial del SIN
    // (facturaComputarizadaCompraVenta.xml, descargado de siatinfo.impuestos.gob.bo,
    // paquete "Factura de Compra y Venta"), con los campos nillable presentes tal
    // como aparecen ahí (telefono/direccion con valor, codigoPuntoVenta/complemento/
    // numeroTarjeta/montoGiftCard/codigoExcepcion/cafc en nil).
    private static FacturaXmlDatos DatosValidos() => new(
        NitEmisor: "1003579028",
        RazonSocialEmisor: "Carlos Loza",
        Municipio: "La Paz",
        Telefono: "78595684",
        NumeroFactura: 1,
        Cuf: "44AAEC00DBD34C53C3E2CCE1A3FA7AF1E2A08606A667A75AC82F24C74",
        Cufd: "BQUE+QytqQUDBKVUFOSVRPQkxVRFZNVFVJBMDAwMDAwM",
        CodigoSucursal: 0,
        Direccion: "AV. JORGE LOPEZ #123",
        CodigoPuntoVenta: null,
        FechaEmision: new DateTime(2021, 10, 6, 16, 3, 48, 675),
        NombreRazonSocial: "Mi razon social",
        CodigoTipoDocumentoIdentidad: 1,
        NumeroDocumento: "5115889",
        Complemento: null,
        CodigoCliente: "51158891",
        CodigoMetodoPago: 1,
        NumeroTarjeta: null,
        MontoTotal: 99m,
        MontoTotalSujetoIva: 99m,
        CodigoMoneda: 1,
        TipoCambio: 1m,
        MontoTotalMoneda: 99m,
        MontoGiftCard: null,
        DescuentoAdicional: 1m,
        CodigoExcepcion: null,
        Cafc: null,
        Leyenda: "Ley N° 453: Tienes derecho a recibir información sobre las características y contenidos de los servicios que utilices.",
        Usuario: "pperez",
        CodigoDocumentoSector: 1,
        Detalles: new[]
        {
            new FacturaXmlDetalle(
                ActividadEconomica: "451010",
                CodigoProductoSin: 49111,
                CodigoProducto: "JN-131231",
                Descripcion: "JUGO DE NARANJA EN VASO",
                Cantidad: 1m,
                UnidadMedida: 1,
                PrecioUnitario: 100m,
                MontoDescuento: 0m,
                SubTotal: 100m,
                NumeroSerie: "124548",
                NumeroImei: "545454"),
        });

    [Fact]
    public void Construir_ConDatosDelEjemploOficialDelSin_ProduceXmlValidoContraElXsd()
    {
        var xml = XmlFacturaBuilder.Construir(DatosValidos());

        var errores = XmlFacturaBuilder.Validar(xml);

        Assert.Empty(errores);
    }

    [Fact]
    public void Construir_CampoNillableConValorNulo_EmiteXsiNil()
    {
        var xml = XmlFacturaBuilder.Construir(DatosValidos());

        Assert.Contains("<codigoPuntoVenta xsi:nil=\"true\" />", xml);
        Assert.Contains("<complemento xsi:nil=\"true\" />", xml);
    }

    [Fact]
    public void Construir_CampoNillableConValor_EmiteElValorSinNil()
    {
        var xml = XmlFacturaBuilder.Construir(DatosValidos());

        Assert.Contains("<telefono>78595684</telefono>", xml);
        Assert.DoesNotContain("<telefono xsi:nil", xml);
    }

    [Fact]
    public void Validar_SinNingunDetalle_DevuelveError()
    {
        var datos = DatosValidos() with { Detalles = Array.Empty<FacturaXmlDetalle>() };
        var xml = XmlFacturaBuilder.Construir(datos);

        var errores = XmlFacturaBuilder.Validar(xml);

        Assert.NotEmpty(errores);
    }

    [Fact]
    public void Validar_CodigoDocumentoSectorDistintoDeUno_DevuelveError()
    {
        // El XSD de facturaComputarizadaCompraVenta fija codigoDocumentoSector en 1
        // (fixed="1") — cualquier otro valor debe fallar la validación.
        var datos = DatosValidos() with { CodigoDocumentoSector = 2 };
        var xml = XmlFacturaBuilder.Construir(datos);

        var errores = XmlFacturaBuilder.Validar(xml);

        Assert.NotEmpty(errores);
    }

    [Fact]
    public void Validar_MontoTotalCero_DevuelveError()
    {
        // montoTotal tiene minExclusive=0 en el XSD: cero no es válido.
        var datos = DatosValidos() with { MontoTotal = 0m };
        var xml = XmlFacturaBuilder.Construir(datos);

        var errores = XmlFacturaBuilder.Validar(xml);

        Assert.NotEmpty(errores);
    }

    [Fact]
    public void Construir_MontosDecimales_SeFormateanConDosDecimalesInvariantes()
    {
        var datos = DatosValidos() with { MontoTotal = 1234.5m, MontoTotalSujetoIva = 1234.5m, MontoTotalMoneda = 1234.5m };

        var xml = XmlFacturaBuilder.Construir(datos);

        Assert.Contains("<montoTotal>1234.50</montoTotal>", xml);
    }
}
