using Facturacion.Infrastructure.RepresentacionGrafica;

namespace Facturacion.Tests;

public class GeneradorQrTests
{
    [Fact]
    public void GenerarPng_ProduceUnPngValidoNoVacio()
    {
        var png = GeneradorQr.GenerarPng("https://pilotosiat.impuestos.gob.bo/consulta/QR?nit=123&cuf=ABC&numero=1");

        Assert.NotEmpty(png);
        // Firma PNG: 89 50 4E 47 0D 0A 1A 0A
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, png.Take(8).ToArray());
    }

    [Fact]
    public void GenerarPng_MismoContenido_ProduceElMismoPng()
    {
        var png1 = GeneradorQr.GenerarPng("https://ejemplo.com/a");
        var png2 = GeneradorQr.GenerarPng("https://ejemplo.com/a");

        Assert.Equal(png1, png2);
    }

    [Fact]
    public void GenerarPng_ContenidoDistinto_ProducePngDistinto()
    {
        var png1 = GeneradorQr.GenerarPng("https://ejemplo.com/a");
        var png2 = GeneradorQr.GenerarPng("https://ejemplo.com/b");

        Assert.NotEqual(png1, png2);
    }
}
