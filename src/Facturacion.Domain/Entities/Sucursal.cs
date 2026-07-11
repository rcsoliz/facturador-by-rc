using Facturacion.Domain.Common;

namespace Facturacion.Domain.Entities;

public class Sucursal : Entity
{
    public Guid TenantId { get; private set; }

    /// <summary>Código de sucursal registrado en el SIN (0 = casa matriz).</summary>
    public int CodigoSiat { get; private set; }
    public string Direccion { get; private set; } = null!;
    public string Municipio { get; private set; } = null!;

    /// <summary>Código CAEB de actividad económica registrada ante el SIN para esta sucursal.</summary>
    public string ActividadEconomica { get; private set; } = null!;

    private readonly List<PuntoVenta> _puntosVenta = new();
    public IReadOnlyCollection<PuntoVenta> PuntosVenta => _puntosVenta.AsReadOnly();

    private Sucursal() { } // EF Core

    internal Sucursal(Guid tenantId, int codigoSiat, string direccion, string municipio, string actividadEconomica)
    {
        if (string.IsNullOrWhiteSpace(actividadEconomica) || actividadEconomica.Length > 10)
            throw new DomainException(
                "ACTIVIDAD_ECONOMICA_INVALIDA", "El código de actividad económica debe tener entre 1 y 10 caracteres.");

        TenantId = tenantId;
        CodigoSiat = codigoSiat;
        Direccion = direccion;
        Municipio = municipio;
        ActividadEconomica = actividadEconomica;
    }

    public PuntoVenta AgregarPuntoVenta(int codigoSiat, string nombre, int tipoPuntoVenta)
    {
        var pv = new PuntoVenta(Id, codigoSiat, nombre, tipoPuntoVenta);
        _puntosVenta.Add(pv);
        MarcarActualizado();
        return pv;
    }
}
