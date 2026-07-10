using System.Numerics;
using Facturacion.Domain.Common;

namespace Facturacion.Infrastructure.Siat.Common;

/// <summary>
/// Datos de entrada para el cálculo del CUF. Los códigos numéricos (modalidad,
/// tipo de emisión, código de documento fiscal) son los códigos de la tabla
/// oficial del SIN para generación del CUF — <b>no</b> los valores del enum
/// de dominio <see cref="Facturacion.Domain.Enums.ModalidadFacturacion"/>,
/// que usa otra numeración interna (ComputarizadaEnLinea=1, ElectronicaEnLinea=2
/// vs. el código SIN: 1=Electrónica, 2=Computarizada, 3=Manual). Quien arme
/// este record debe mapear explícitamente, no castear el enum de dominio.
/// </summary>
public sealed record CufDatos(
    string Nit,
    DateTime FechaHoraEmision,
    int CodigoSucursal,
    int Modalidad,
    int TipoEmision,
    int CodigoDocumentoFiscal,
    int CodigoDocumentoSector,
    long NumeroFactura,
    int PuntoVenta,
    string CodigoControlCufd);

/// <summary>
/// Cálculo del CUF (Código Único de Factura): concatenación de 52 dígitos +
/// dígito auto-verificador módulo 11 + codificación base 16, concatenando al
/// final el código de control del CUFD vigente.
///
/// Algoritmo verificado dígito por dígito contra el ejemplo oficial del SIN
/// ("Generación CUF", Sistema de Facturación Electrónica, La Paz, 21/11/2018,
/// www.impuestos.gob.bo): con NIT=123456789, fecha=2019-01-13T16:37:21.231,
/// sucursal=0, modalidad=1 (Electrónica), tipoEmision=1, codigoDocFiscal=1,
/// docSector=1, número=1, POS=0, el resultado hexadecimal esperado es
/// "159FFE6FB1986A24BB32DBE5A2A34214B245A6A3".
/// </summary>
public static class CufCalculator
{
    public static string Calcular(CufDatos datos)
    {
        var cadena =
            Formatear(datos.Nit, 13) +
            datos.FechaHoraEmision.ToString("yyyyMMddHHmmssfff") +
            Formatear(datos.CodigoSucursal, 4) +
            Formatear(datos.Modalidad, 1) +
            Formatear(datos.TipoEmision, 1) +
            Formatear(datos.CodigoDocumentoFiscal, 1) +
            Formatear(datos.CodigoDocumentoSector, 2) +
            Formatear(datos.NumeroFactura, 8) +
            Formatear(datos.PuntoVenta, 4);

        var digitoVerificador = CalcularDigitoModulo11(cadena);

        return AHexadecimal(cadena + digitoVerificador) + datos.CodigoControlCufd;
    }

    /// <summary>
    /// Dígito auto-verificador módulo 11: pesos 2..9 cíclicos aplicados de
    /// derecha a izquierda; el resto de un módulo 11 cae siempre en 0..10,
    /// y si es 10 el dígito es 1 (nunca hay dos dígitos que codificar).
    /// </summary>
    public static char CalcularDigitoModulo11(string cadena)
    {
        var suma = 0;
        var peso = 2;
        for (var i = cadena.Length - 1; i >= 0; i--)
        {
            suma += peso * (cadena[i] - '0');
            peso = peso == 9 ? 2 : peso + 1;
        }

        var resto = suma % 11;
        return resto == 10 ? '1' : (char)('0' + resto);
    }

    private static string AHexadecimal(string cadenaNumerica)
    {
        var numero = BigInteger.Parse(cadenaNumerica);
        return numero.ToString("X").TrimStart('0');
    }

    private static string Formatear(long valor, int longitud) => Formatear(valor.ToString(), longitud);

    private static string Formatear(int valor, int longitud) => Formatear(valor.ToString(), longitud);

    private static string Formatear(string valor, int longitud)
    {
        if (valor.Length > longitud)
            throw new DomainException(
                "CUF_CAMPO_EXCEDE_LONGITUD",
                $"El valor '{valor}' excede la longitud de {longitud} dígitos requerida para el CUF.");

        return valor.PadLeft(longitud, '0');
    }
}
