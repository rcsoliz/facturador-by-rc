using Facturacion.Domain.Common;

namespace Facturacion.Domain.ValueObjects;

/// <summary>
/// Código Único de Facturación. Se calcula a partir de: NIT emisor, fecha/hora,
/// sucursal, modalidad, tipo emisión, tipo factura, tipo documento sector,
/// número de factura y punto de venta, con dígito módulo 11 y codificación base 16,
/// concatenando el código de control del CUFD vigente.
/// La implementación del cálculo vive en Infrastructure (SiatCommon).
/// </summary>
public sealed record Cuf
{
    public string Valor { get; }

    public Cuf(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("CUF_INVALIDO", "El CUF no puede estar vacío.");
        Valor = valor;
    }

    public override string ToString() => Valor;
}
