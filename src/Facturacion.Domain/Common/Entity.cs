namespace Facturacion.Domain.Common;

/// <summary>Base para todas las entidades del dominio.</summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreadoEn { get; protected set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEn { get; protected set; }

    protected void MarcarActualizado() => ActualizadoEn = DateTime.UtcNow;
}
