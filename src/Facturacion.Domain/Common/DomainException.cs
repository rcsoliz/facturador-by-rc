namespace Facturacion.Domain.Common;

/// <summary>Excepción de regla de negocio del dominio de facturación.</summary>
public class DomainException : Exception
{
    public string Codigo { get; }

    public DomainException(string codigo, string mensaje) : base(mensaje)
        => Codigo = codigo;
}
