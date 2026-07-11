using Facturacion.Domain.Entities;

namespace Facturacion.Application.Dtos;

public sealed record AgregarSucursalRequest(
    int CodigoSiat, string Direccion, string Municipio, string ActividadEconomica);

public sealed record SucursalResponse(
    Guid Id, int CodigoSiat, string Direccion, string Municipio, string ActividadEconomica,
    IReadOnlyList<PuntoVentaResponse> PuntosVenta)
{
    public static SucursalResponse Desde(Sucursal s) => new(
        s.Id, s.CodigoSiat, s.Direccion, s.Municipio, s.ActividadEconomica,
        s.PuntosVenta.Select(PuntoVentaResponse.Desde).ToList());
}

public sealed record AgregarPuntoVentaRequest(int CodigoSiat, string Nombre, int TipoPuntoVenta);

public sealed record PuntoVentaResponse(Guid Id, int CodigoSiat, string Nombre, int TipoPuntoVenta)
{
    public static PuntoVentaResponse Desde(PuntoVenta p) => new(p.Id, p.CodigoSiat, p.Nombre, p.TipoPuntoVenta);
}
