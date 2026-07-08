using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CredencialesSiat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PuntoVentaId = table.Column<Guid>(type: "uuid", nullable: true),
                    TokenDelegadoCifrado = table.Column<string>(type: "text", nullable: false),
                    Cuis = table.Column<string>(type: "text", nullable: true),
                    CuisVence = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Cufd = table.Column<string>(type: "text", nullable: true),
                    CufdCodigoControl = table.Column<string>(type: "text", nullable: true),
                    CufdVence = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredencialesSiat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PuntoVentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    NumeroFactura = table.Column<long>(type: "bigint", nullable: false),
                    CodigoDocumentoSector = table.Column<int>(type: "integer", nullable: false),
                    Cuf = table.Column<string>(type: "text", nullable: true),
                    CodigoRecepcionSin = table.Column<string>(type: "text", nullable: true),
                    RazonSocialComprador = table.Column<string>(type: "text", nullable: false),
                    CodigoTipoDocumentoIdentidad = table.Column<int>(type: "integer", nullable: false),
                    NumeroDocumentoComprador = table.Column<string>(type: "text", nullable: false),
                    Complemento = table.Column<string>(type: "text", nullable: true),
                    EmailComprador = table.Column<string>(type: "text", nullable: true),
                    MontoTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoTotalSujetoIva = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CodigoMoneda = table.Column<int>(type: "integer", nullable: false),
                    TipoCambio = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    XmlGenerado = table.Column<string>(type: "text", nullable: true),
                    RespuestaSinRaw = table.Column<string>(type: "text", nullable: true),
                    MotivoRechazo = table.Column<string>(type: "text", nullable: true),
                    CodigoMotivoAnulacion = table.Column<int>(type: "integer", nullable: true),
                    ReferenciaExterna = table.Column<string>(type: "text", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PuntoVentaCorrelativos",
                columns: table => new
                {
                    PuntoVentaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UltimoNumero = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuntoVentaCorrelativos", x => x.PuntoVentaId);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RazonSocial = table.Column<string>(type: "text", nullable: false),
                    Nit = table.Column<string>(type: "text", nullable: false),
                    Modalidad = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    ApiKeyHash = table.Column<string>(type: "text", nullable: false),
                    WebhookUrl = table.Column<string>(type: "text", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetallesFactura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoProductoSin = table.Column<int>(type: "integer", nullable: false),
                    CodigoProducto = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    UnidadMedida = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesFactura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoSiat = table.Column<int>(type: "integer", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Municipio = table.Column<string>(type: "text", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sucursales_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PuntosVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoSiat = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    TipoPuntoVenta = table.Column<int>(type: "integer", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuntosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PuntosVenta_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_FacturaId",
                table: "DetallesFactura",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_TenantId_Estado",
                table: "Facturas",
                columns: new[] { "TenantId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_TenantId_ReferenciaExterna",
                table: "Facturas",
                columns: new[] { "TenantId", "ReferenciaExterna" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PuntosVenta_SucursalId",
                table: "PuntosVenta",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_TenantId",
                table: "Sucursales",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ApiKeyHash",
                table: "Tenants",
                column: "ApiKeyHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CredencialesSiat");

            migrationBuilder.DropTable(
                name: "DetallesFactura");

            migrationBuilder.DropTable(
                name: "PuntosVenta");

            migrationBuilder.DropTable(
                name: "PuntoVentaCorrelativos");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "Sucursales");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
