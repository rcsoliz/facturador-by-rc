using Facturacion.Domain.Common;
using Facturacion.Infrastructure.Siat.Common;

namespace Facturacion.Tests;

public class CufCalculatorTests
{
    // Ejemplo oficial del SIN ("Generación CUF", Sistema de Facturación
    // Electrónica, La Paz, 21/11/2018, www.impuestos.gob.bo). El código de
    // control del CUFD no está en el ejemplo oficial (termina en el paso del
    // hexadecimal); se agrega vacío acá para comparar solo la parte calculada.
    [Fact]
    public void Calcular_EjemploOficialSin_ProduceElHexadecimalDocumentado()
    {
        var datos = new CufDatos(
            Nit: "123456789",
            FechaHoraEmision: new DateTime(2019, 1, 13, 16, 37, 21, 231),
            CodigoSucursal: 0,
            Modalidad: 1, // Electrónica (código SIN, no el enum de dominio)
            TipoEmision: 1, // Online
            CodigoDocumentoFiscal: 1, // Factura
            CodigoDocumentoSector: 1, // Factura estándar
            NumeroFactura: 1,
            PuntoVenta: 0,
            CodigoControlCufd: "");

        var cuf = CufCalculator.Calcular(datos);

        Assert.Equal("159FFE6FB1986A24BB32DBE5A2A34214B245A6A3", cuf);
    }

    [Fact]
    public void Calcular_ConcatenaElCodigoDeControlDelCufdAlFinal()
    {
        var datos = new CufDatos(
            "123456789", new DateTime(2019, 1, 13, 16, 37, 21, 231),
            0, 1, 1, 1, 1, 1, 0, "A19E23EF34124CD");

        var cuf = CufCalculator.Calcular(datos);

        Assert.EndsWith("A19E23EF34124CD", cuf);
        Assert.Equal("159FFE6FB1986A24BB32DBE5A2A34214B245A6A3A19E23EF34124CD", cuf);
    }

    [Fact]
    public void CalcularDigitoModulo11_Resto10_DevuelveDigito1()
    {
        // Cadena elegida por búsqueda: con los pesos 2..9 cíclicos de derecha
        // a izquierda, su suma módulo 11 da resto 10 (verificado con un
        // cálculo independiente en Python antes de fijar este valor).
        const string cadena = "2037788925546659051518644925192546291486528168";

        Assert.Equal('1', CufCalculator.CalcularDigitoModulo11(cadena));
    }

    [Fact]
    public void Calcular_CampoExcedeLongitudPermitida_LanzaDomainException()
    {
        var datos = new CufDatos(
            Nit: "12345678901234", // 14 dígitos, excede los 13 permitidos
            FechaHoraEmision: DateTime.UtcNow,
            CodigoSucursal: 0, Modalidad: 1, TipoEmision: 1,
            CodigoDocumentoFiscal: 1, CodigoDocumentoSector: 1,
            NumeroFactura: 1, PuntoVenta: 0, CodigoControlCufd: "");

        var ex = Assert.Throws<DomainException>(() => CufCalculator.Calcular(datos));
        Assert.Equal("CUF_CAMPO_EXCEDE_LONGITUD", ex.Codigo);
    }
}
