using Facturacion.Domain.Common;
using Facturacion.Domain.Enums;
using Facturacion.Domain.ValueObjects;

namespace Facturacion.Domain.Entities;

/// <summary>
/// Agregado raíz del dominio. Encapsula la máquina de estados:
/// toda transición pasa por métodos de esta clase — nunca se setea el estado desde afuera.
/// </summary>
public class Factura : Entity
{
    public Guid TenantId { get; private set; }
    public Guid SucursalId { get; private set; }
    public Guid PuntoVentaId { get; private set; }

    public EstadoFactura Estado { get; private set; } = EstadoFactura.Pendiente;

    /// <summary>Número correlativo por punto de venta (lo asigna el servicio, no el cliente).</summary>
    public long NumeroFactura { get; private set; }

    /// <summary>Código de documento sector según paramétrica SIAT (1 = compra-venta, etc.).</summary>
    public int CodigoDocumentoSector { get; private set; }

    public Cuf? Cuf { get; private set; }
    public string? CodigoRecepcionSin { get; private set; }

    // Datos del comprador
    public string RazonSocialComprador { get; private set; } = null!;
    public int CodigoTipoDocumentoIdentidad { get; private set; }
    public string NumeroDocumentoComprador { get; private set; } = null!;
    public string? Complemento { get; private set; }
    public string? EmailComprador { get; private set; }

    public decimal MontoTotal { get; private set; }
    public decimal MontoTotalSujetoIva { get; private set; }
    public int CodigoMoneda { get; private set; }
    public decimal TipoCambio { get; private set; }

    /// <summary>Código de método de pago según paramétrica SIAT (1..308).</summary>
    public int CodigoMetodoPago { get; private set; }

    /// <summary>Últimos dígitos de tarjeta, solo cuando el método de pago lo requiere.</summary>
    public long? NumeroTarjeta { get; private set; }

    public DateTime FechaEmision { get; private set; }

    /// <summary>XML enviado al SIN (auditoría / reprocesos). En Postgres: columna text o jsonb envolvente.</summary>
    public string? XmlGenerado { get; private set; }

    /// <summary>Última respuesta cruda del SIN (auditoría).</summary>
    public string? RespuestaSinRaw { get; private set; }

    public string? MotivoRechazo { get; private set; }
    public int? CodigoMotivoAnulacion { get; private set; }

    /// <summary>Referencia externa provista por el sistema cliente (idempotencia).</summary>
    public string ReferenciaExterna { get; private set; } = null!;

    private readonly List<DetalleFactura> _detalles = new();
    public IReadOnlyCollection<DetalleFactura> Detalles => _detalles.AsReadOnly();

    private Factura() { } // EF Core

    public Factura(
        Guid tenantId, Guid sucursalId, Guid puntoVentaId,
        int codigoDocumentoSector, string referenciaExterna,
        string razonSocialComprador, int codigoTipoDocumentoIdentidad,
        string numeroDocumentoComprador, string? complemento, string? emailComprador,
        int codigoMoneda, decimal tipoCambio,
        int codigoMetodoPago, long? numeroTarjeta,
        IEnumerable<DetalleFactura> detalles)
    {
        if (!detalles.Any())
            throw new DomainException("FACTURA_SIN_DETALLE", "La factura debe tener al menos un ítem.");
        if (codigoMetodoPago is < 1 or > 308)
            throw new DomainException(
                "METODO_PAGO_INVALIDO", "El código de método de pago debe estar entre 1 y 308.");

        TenantId = tenantId;
        SucursalId = sucursalId;
        PuntoVentaId = puntoVentaId;
        CodigoDocumentoSector = codigoDocumentoSector;
        ReferenciaExterna = referenciaExterna;
        RazonSocialComprador = razonSocialComprador;
        CodigoTipoDocumentoIdentidad = codigoTipoDocumentoIdentidad;
        NumeroDocumentoComprador = numeroDocumentoComprador;
        Complemento = complemento;
        EmailComprador = emailComprador;
        CodigoMoneda = codigoMoneda;
        TipoCambio = tipoCambio;
        CodigoMetodoPago = codigoMetodoPago;
        NumeroTarjeta = numeroTarjeta;
        FechaEmision = DateTime.UtcNow;

        _detalles.AddRange(detalles);
        MontoTotal = _detalles.Sum(d => d.SubTotal);
        MontoTotalSujetoIva = MontoTotal; // v1: sin descuentos globales ni gift cards
    }

    // ─── Máquina de estados ────────────────────────────────────────────────

    public void AsignarNumero(long numero)
    {
        ValidarEstado(EstadoFactura.Pendiente, "asignar número");
        NumeroFactura = numero;
        MarcarActualizado();
    }

    public void MarcarGenerada(Cuf cuf, string xml)
    {
        ValidarEstado(EstadoFactura.Pendiente, "generar");
        Cuf = cuf;
        XmlGenerado = xml;
        Estado = EstadoFactura.Generada;
        MarcarActualizado();
    }

    public void MarcarEnviada()
    {
        ValidarEstado(EstadoFactura.Generada, "enviar");
        Estado = EstadoFactura.Enviada;
        MarcarActualizado();
    }

    public void MarcarValidada(string codigoRecepcion, string respuestaRaw)
    {
        if (Estado is not (EstadoFactura.Enviada or EstadoFactura.EnContingencia))
            throw TransicionInvalida("validar");
        CodigoRecepcionSin = codigoRecepcion;
        RespuestaSinRaw = respuestaRaw;
        Estado = EstadoFactura.Validada;
        MarcarActualizado();
    }

    public void MarcarRechazada(string motivo, string respuestaRaw)
    {
        ValidarEstado(EstadoFactura.Enviada, "rechazar");
        MotivoRechazo = motivo;
        RespuestaSinRaw = respuestaRaw;
        Estado = EstadoFactura.Rechazada;
        MarcarActualizado();
    }

    public void MarcarEnContingencia()
    {
        if (Estado is not (EstadoFactura.Pendiente or EstadoFactura.Generada))
            throw TransicionInvalida("pasar a contingencia");
        Estado = EstadoFactura.EnContingencia;
        MarcarActualizado();
    }

    public void MarcarAnulada(int codigoMotivo, string respuestaRaw)
    {
        ValidarEstado(EstadoFactura.Validada, "anular");
        CodigoMotivoAnulacion = codigoMotivo;
        RespuestaSinRaw = respuestaRaw;
        Estado = EstadoFactura.Anulada;
        MarcarActualizado();
    }

    /// <summary>Una rechazada puede corregirse y volver al inicio del flujo.</summary>
    public void ReintentarTrasRechazo()
    {
        ValidarEstado(EstadoFactura.Rechazada, "reintentar");
        MotivoRechazo = null;
        Cuf = null;
        XmlGenerado = null;
        Estado = EstadoFactura.Pendiente;
        MarcarActualizado();
    }

    private void ValidarEstado(EstadoFactura esperado, string operacion)
    {
        if (Estado != esperado) throw TransicionInvalida(operacion);
    }

    private DomainException TransicionInvalida(string operacion) =>
        new("TRANSICION_INVALIDA",
            $"No se puede {operacion} una factura en estado {Estado}.");
}
