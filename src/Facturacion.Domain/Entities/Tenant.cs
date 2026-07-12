using Facturacion.Domain.Common;
using Facturacion.Domain.Enums;
using Facturacion.Domain.ValueObjects;

namespace Facturacion.Domain.Entities;

/// <summary>
/// Emisor (cliente del servicio). Raíz del aislamiento multi-tenant:
/// toda entidad operativa referencia a un Tenant.
/// </summary>
public class Tenant : Entity
{
    public string RazonSocial { get; private set; } = null!;
    public Nit Nit { get; private set; } = null!;
    public ModalidadFacturacion Modalidad { get; private set; }
    public bool Activo { get; private set; } = true;

    /// <summary>Hash de la API key con la que el sistema cliente se autentica.</summary>
    public string ApiKeyHash { get; private set; } = null!;

    /// <summary>URL a la que se notifican los cambios de estado de las facturas.</summary>
    public string? WebhookUrl { get; private set; }

    /// <summary>Secreto para firmar (HMAC-SHA256) el payload del webhook, cifrado en reposo.</summary>
    public string? WebhookSecretCifrado { get; private set; }

    private readonly List<Sucursal> _sucursales = new();
    public IReadOnlyCollection<Sucursal> Sucursales => _sucursales.AsReadOnly();

    private Tenant() { } // EF Core

    public Tenant(string razonSocial, Nit nit, ModalidadFacturacion modalidad, string apiKeyHash)
    {
        RazonSocial = razonSocial;
        Nit = nit;
        Modalidad = modalidad;
        ApiKeyHash = apiKeyHash;
    }

    /// <summary>El secreto se recibe ya cifrado — el cifrado es responsabilidad del caller (ver <c>IProteccionDatos</c>).</summary>
    public void ConfigurarWebhook(string url, string secretoCifrado)
    {
        WebhookUrl = url;
        WebhookSecretCifrado = secretoCifrado;
        MarcarActualizado();
    }
    public void Desactivar() { Activo = false; MarcarActualizado(); }

    public Sucursal AgregarSucursal(int codigoSiat, string direccion, string municipio, string actividadEconomica)
    {
        var sucursal = new Sucursal(Id, codigoSiat, direccion, municipio, actividadEconomica);
        _sucursales.Add(sucursal);
        MarcarActualizado();
        return sucursal;
    }
}
