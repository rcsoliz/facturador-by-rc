namespace Facturacion.Infrastructure.Siat.Common;

/// <summary>
/// Configuración del servicio SIN. v1: solo los valores que <see cref="XmlFacturaBuilder"/>
/// necesita hoy para campos que no dependen del tenant ni de la factura individual.
/// Endpoints SOAP (piloto/producción), timeouts y políticas Polly se agregan cuando
/// se implementen los clientes SOAP — ver Siat/Common/README.md.
/// </summary>
public sealed class SiatOptions
{
    public const string SeccionConfiguracion = "Siat";

    /// <summary>
    /// Texto legal (leyenda) que acompaña cada factura, del catálogo de leyendas del SIN.
    /// v1: un único valor fijo por instalación (appsettings/env var); cuando exista
    /// CatalogosService se podrá seleccionar dinámicamente según documento/sector.
    /// </summary>
    public string LeyendaFacturaDefault { get; set; } =
        "Ley N° 453: Tienes derecho a recibir información sobre las características y " +
        "contenidos de los servicios que utilices.";

    /// <summary>
    /// Valor fijo para el campo "usuario" del XML (quién emite, según el SIN — típicamente
    /// el operador de un punto de venta). La API es headless: no hay operador humano por
    /// factura, así que se usa este valor fijo para todos los tenants.
    /// </summary>
    public string UsuarioSistema { get; set; } = "api";

    /// <summary>
    /// URL base del verificador público de facturas del SIN, usada para armar el
    /// contenido del QR de la representación gráfica. Formato documentado
    /// públicamente (siatinfo.impuestos.gob.bo, "Código Respuesta Rápida (QR)"):
    /// <c>{base}?nit={nit}&amp;cuf={cuf}&amp;numero={numeroFactura}&amp;t={tamaño}</c>.
    /// Configurable por ambiente (Piloto/Producción) — ver
    /// RepresentacionGrafica/README.md para el detalle de los parámetros.
    /// </summary>
    public string UrlBaseVerificacionQr { get; set; } = "https://pilotosiat.impuestos.gob.bo/consulta/QR";
}
