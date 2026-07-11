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
}
