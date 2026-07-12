using Facturacion.Domain.Entities;
using Facturacion.Domain.Ports;

namespace Facturacion.Infrastructure.Siat.Common;

/// <summary>
/// Gestión de CUIS (vigencia ~1 año) y CUFD (vigencia 24h) por sucursal/punto de
/// venta. Renueva bajo demanda contra <see cref="ISinCredencialesClient"/> cuando
/// lo que hay en base está vencido o no existe todavía; la renovación programada
/// (antes de que venza) queda para el job de Workers (roadmap v1).
/// </summary>
public class CredencialesService : IGestorCredencialesSiat
{
    private readonly ICredencialSiatRepository _credenciales;
    private readonly ISinCredencialesClient _cliente;
    private readonly IProteccionDatos _proteccion;

    public CredencialesService(
        ICredencialSiatRepository credenciales, ISinCredencialesClient cliente, IProteccionDatos proteccion)
    {
        _credenciales = credenciales;
        _cliente = cliente;
        _proteccion = proteccion;
    }

    /// <summary>
    /// Devuelve el CUFD vigente (y su código de control) para el punto de venta
    /// indicado, renovando CUIS y/o CUFD contra el SIN si están vencidos.
    /// Requiere que el tenant ya haya registrado su token delegado — ver
    /// <see cref="RegistrarTokenDelegadoAsync"/>.
    /// </summary>
    public async Task<(string Cufd, string CodigoControl)> ObtenerCufdVigenteAsync(
        Guid tenantId, Guid sucursalId, Guid? puntoVentaId,
        string nit, int codigoSucursal, int? codigoPuntoVenta, CancellationToken ct = default)
    {
        // Prioriza la credencial del punto de venta puntual; si no existe, cae a
        // la credencial de sucursal (PuntoVentaId null) — el CUIS/CUFD suele
        // gestionarse a nivel sucursal y reutilizarse entre puntos de venta.
        var credencial =
            (puntoVentaId is not null ? await _credenciales.ObtenerAsync(tenantId, sucursalId, puntoVentaId, ct) : null)
            ?? await _credenciales.ObtenerAsync(tenantId, sucursalId, null, ct)
            ?? throw new InvalidOperationException(
                $"No hay token delegado registrado para la sucursal {sucursalId} del tenant {tenantId}. " +
                "Debe registrarse (POST /api/v1/sucursales/{id}/credencial-siat) antes de emitir facturas.");

        var ahora = DateTime.UtcNow;
        var solicitud = new SolicitudCredencialSin(
            nit, _proteccion.Descifrar(credencial.TokenDelegadoCifrado), codigoSucursal, codigoPuntoVenta);

        if (!credencial.CuisVigente(ahora))
        {
            var cuis = await _cliente.ObtenerCuisAsync(solicitud, ct);
            credencial.ActualizarCuis(cuis.Cuis, cuis.VenceUtc);
        }

        if (!credencial.CufdVigente(ahora))
        {
            var cufd = await _cliente.ObtenerCufdAsync(solicitud, credencial.Cuis!, ct);
            credencial.ActualizarCufd(cufd.Cufd, cufd.CodigoControl, cufd.VenceUtc);
        }

        await _credenciales.GuardarCambiosAsync(ct);
        return (credencial.Cufd!, credencial.CufdCodigoControl!);
    }

    /// <summary>
    /// Registra (o rota) el token delegado obtenido del portal del SIN para una
    /// sucursal/punto de venta. Se cifra antes de persistirse — nunca en texto
    /// plano (ver regla de CLAUDE.md).
    /// </summary>
    public async Task RegistrarTokenDelegadoAsync(
        Guid tenantId, Guid sucursalId, Guid? puntoVentaId, string tokenDelegadoPlano, CancellationToken ct = default)
    {
        var tokenCifrado = _proteccion.Cifrar(tokenDelegadoPlano);
        var credencial = await _credenciales.ObtenerAsync(tenantId, sucursalId, puntoVentaId, ct);

        if (credencial is null)
        {
            credencial = new CredencialSiat(tenantId, sucursalId, puntoVentaId, tokenCifrado);
            await _credenciales.AgregarAsync(credencial, ct);
        }
        else
        {
            credencial.ActualizarTokenDelegado(tokenCifrado);
        }

        await _credenciales.GuardarCambiosAsync(ct);
    }
}
