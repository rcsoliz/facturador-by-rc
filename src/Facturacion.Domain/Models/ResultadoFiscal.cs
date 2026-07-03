namespace Facturacion.Domain.Models;

/// <summary>Resultado de una operación contra el proveedor fiscal (SIN u otro futuro).</summary>
public sealed record ResultadoFiscal
{
    public bool Exitoso { get; init; }
    public string? CodigoRecepcion { get; init; }
    public string? CodigoEstado { get; init; }
    public IReadOnlyList<ErrorFiscal> Errores { get; init; } = Array.Empty<ErrorFiscal>();

    /// <summary>Respuesta cruda del proveedor (auditoría / debugging).</summary>
    public string RespuestaRaw { get; init; } = string.Empty;

    public static ResultadoFiscal Ok(string codigoRecepcion, string codigoEstado, string raw) =>
        new() { Exitoso = true, CodigoRecepcion = codigoRecepcion, CodigoEstado = codigoEstado, RespuestaRaw = raw };

    public static ResultadoFiscal Fallo(IReadOnlyList<ErrorFiscal> errores, string raw) =>
        new() { Exitoso = false, Errores = errores, RespuestaRaw = raw };
}

public sealed record ErrorFiscal(string Codigo, string Descripcion);
