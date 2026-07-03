namespace Facturacion.Domain.Enums;

/// <summary>Modalidad SIAT configurada por tenant. Determina el adaptador a inyectar.</summary>
public enum ModalidadFacturacion
{
    /// <summary>v1 — Facturación Computarizada en Línea (hash SHA256 como huella, sin XMLDSig).</summary>
    ComputarizadaEnLinea = 1,

    /// <summary>v1.5 — Facturación Electrónica en Línea (firma digital XMLDSig con certificado).</summary>
    ElectronicaEnLinea = 2
}
