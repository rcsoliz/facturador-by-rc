namespace Facturacion.Domain.Enums;

/// <summary>
/// Catálogos/paramétricas del SIN que se sincronizan diariamente (ver
/// "Contexto SIAT" en CLAUDE.md). Ampliable a medida que se necesiten más
/// (países, motivos de anulación, eventos significativos, etc.).
/// </summary>
public enum TipoCatalogo
{
    ProductosServicios = 1,
    ActividadesEconomicas = 2,
}
