using QRCoder;

namespace Facturacion.Infrastructure.RepresentacionGrafica;

/// <summary>
/// Genera el PNG del código QR. Usa <see cref="PngByteQRCode"/> (no
/// <c>QRCode</c>/<c>System.Drawing</c>) porque QRCoder desacopló ese modo de
/// System.Drawing.Common, que no es multiplataforma — relevante si este
/// servicio termina corriendo en un contenedor Linux.
/// </summary>
public static class GeneradorQr
{
    public static byte[] GenerarPng(string contenido)
    {
        using var generador = new QRCodeGenerator();
        using var datos = generador.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(datos);
        return png.GetGraphic(20);
    }
}
