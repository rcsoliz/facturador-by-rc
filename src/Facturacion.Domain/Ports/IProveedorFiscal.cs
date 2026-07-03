using Facturacion.Domain.Entities;
using Facturacion.Domain.Models;

namespace Facturacion.Domain.Ports;

/// <summary>
/// ★ Puerto central del sistema — el "comodín".
///
/// Abstrae al proveedor fiscal: hoy SIAT Bolivia (computarizada v1, electrónica v1.5),
/// mañana SUNAT Perú, AFIP Argentina, etc. Domain y Application NUNCA conocen
/// SOAP, XML, CUFD ni ningún detalle del SIN: eso vive en los adaptadores
/// de Infrastructure que implementan esta interfaz.
///
/// La configuración del tenant (ModalidadFacturacion / país) decide qué
/// implementación inyecta el contenedor de DI.
/// </summary>
public interface IProveedorFiscal
{
    /// <summary>
    /// Genera el documento fiscal (XML + CUF) para la factura.
    /// En computarizada: XML + hash SHA256 como huella.
    /// En electrónica: XML firmado con XMLDSig.
    /// </summary>
    Task<FacturaGenerada> GenerarDocumentoAsync(Factura factura, CancellationToken ct = default);

    /// <summary>Envía el documento al ente fiscal y retorna el resultado de recepción/validación.</summary>
    Task<ResultadoFiscal> EnviarAsync(Factura factura, CancellationToken ct = default);

    /// <summary>Consulta el estado actual del documento en el ente fiscal.</summary>
    Task<ResultadoFiscal> ConsultarEstadoAsync(Factura factura, CancellationToken ct = default);

    /// <summary>Anula un documento validado, con el código de motivo de la paramétrica.</summary>
    Task<ResultadoFiscal> AnularAsync(Factura factura, int codigoMotivo, CancellationToken ct = default);

    /// <summary>Envía un paquete de facturas emitidas en contingencia.</summary>
    Task<ResultadoFiscal> EnviarPaqueteContingenciaAsync(
        IReadOnlyList<Factura> facturas, CancellationToken ct = default);

    /// <summary>Verifica disponibilidad del ente fiscal (health check → decide contingencia).</summary>
    Task<bool> ComunicacionDisponibleAsync(CancellationToken ct = default);
}
