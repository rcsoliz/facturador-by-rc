using Facturacion.Domain.Entities;
using Facturacion.Domain.Enums;
using Facturacion.Domain.ValueObjects;
using Facturacion.Infrastructure.RepresentacionGrafica;
using Facturacion.Infrastructure.Siat.Common;
using Microsoft.Extensions.Options;

namespace Facturacion.Tests;

public class GeneradorRepresentacionGraficaQuestPdfTests
{
    private static (Tenant Tenant, Sucursal Sucursal, PuntoVenta PuntoVenta, Factura Factura) CrearEscenario()
    {
        var tenant = new Tenant("Carlos Loza", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");
        var sucursal = tenant.AgregarSucursal(0, "AV. JORGE LOPEZ #123", "La Paz", "451010");
        var puntoVenta = sucursal.AgregarPuntoVenta(0, "Caja 1", 1);

        var factura = new Factura(
            tenant.Id, sucursal.Id, puntoVenta.Id,
            1, "REF-1", "Mi razon social", 1, "5115889", null, null,
            1, 1, 1, null,
            new[] { new DetalleFactura(49111, "JN-1", "Jugo de naranja en vaso", 1, 1, 100m, 0) });

        factura.AsignarNumero(1);
        factura.MarcarGenerada(new Cuf("44AAEC00DBD34C53C3E2CCE1A3FA7AF1E2A08606A667A75AC82F24C74"), "<xml/>");

        return (tenant, sucursal, puntoVenta, factura);
    }

    [Fact]
    public async Task GenerarPdfAsync_FacturaConCuf_ProduceUnPdfValidoNoVacio()
    {
        var (tenant, sucursal, puntoVenta, factura) = CrearEscenario();
        var generador = new GeneradorRepresentacionGraficaQuestPdf(Options.Create(new SiatOptions()));

        var pdf = await generador.GenerarPdfAsync(factura, tenant, sucursal, puntoVenta);

        Assert.NotEmpty(pdf);
        Assert.Equal("%PDF"u8.ToArray(), pdf.Take(4).ToArray());
    }

    [Fact]
    public async Task GenerarPdfAsync_SinPuntoVenta_NoFalla()
    {
        var (tenant, sucursal, _, factura) = CrearEscenario();
        var generador = new GeneradorRepresentacionGraficaQuestPdf(Options.Create(new SiatOptions()));

        var pdf = await generador.GenerarPdfAsync(factura, tenant, sucursal, null);

        Assert.NotEmpty(pdf);
    }

    [Fact]
    public async Task GenerarPdfAsync_FacturaSinCuf_Lanza()
    {
        var tenant = new Tenant("Carlos Loza", new Nit("1003579028"), ModalidadFacturacion.ComputarizadaEnLinea, "hash");
        var sucursal = tenant.AgregarSucursal(0, "AV. JORGE LOPEZ #123", "La Paz", "451010");
        var factura = new Factura(
            tenant.Id, sucursal.Id, Guid.NewGuid(),
            1, "REF-1", "Mi razon social", 1, "5115889", null, null,
            1, 1, 1, null,
            new[] { new DetalleFactura(49111, "JN-1", "Jugo de naranja en vaso", 1, 1, 100m, 0) });

        var generador = new GeneradorRepresentacionGraficaQuestPdf(Options.Create(new SiatOptions()));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            generador.GenerarPdfAsync(factura, tenant, sucursal, null));
    }
}
