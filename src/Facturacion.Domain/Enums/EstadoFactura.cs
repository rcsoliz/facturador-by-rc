namespace Facturacion.Domain.Enums;

/// <summary>
/// Máquina de estados de una factura.
/// Flujo normal:      Pendiente → Generada → Enviada → Validada
/// Rechazo:           Enviada → Rechazada (corregible → vuelve a Pendiente)
/// Anulación:         Validada → Anulada
/// Contingencia:      Pendiente → EnContingencia → Enviada (al recuperarse la conexión)
/// </summary>
public enum EstadoFactura
{
    /// <summary>Aceptada por la API, aún no procesada.</summary>
    Pendiente = 0,

    /// <summary>XML generado (y firmado/hasheado según modalidad).</summary>
    Generada = 1,

    /// <summary>Enviada al SIN, esperando validación.</summary>
    Enviada = 2,

    /// <summary>Validada por el SIN. Documento fiscal firme.</summary>
    Validada = 3,

    /// <summary>Rechazada por el SIN (lista de errores disponible).</summary>
    Rechazada = 4,

    /// <summary>Anulada ante el SIN (requiere código de motivo).</summary>
    Anulada = 5,

    /// <summary>Emitida fuera de línea; pendiente de envío por paquete de contingencia.</summary>
    EnContingencia = 6
}
