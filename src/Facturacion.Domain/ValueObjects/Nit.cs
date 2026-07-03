using Facturacion.Domain.Common;

namespace Facturacion.Domain.ValueObjects;

/// <summary>Número de Identificación Tributaria (Bolivia).</summary>
public sealed record Nit
{
    public string Valor { get; }

    public Nit(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || !valor.All(char.IsDigit))
            throw new DomainException("NIT_INVALIDO", $"El NIT '{valor}' debe ser numérico.");
        Valor = valor;
    }

    public override string ToString() => Valor;
}
