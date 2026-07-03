using Facturacion.Domain.ValueObjects;

namespace Facturacion.Domain.Models;

/// <summary>XML listo para envío + CUF calculado. Producto del paso "generar".</summary>
public sealed record FacturaGenerada(Cuf Cuf, string Xml);
