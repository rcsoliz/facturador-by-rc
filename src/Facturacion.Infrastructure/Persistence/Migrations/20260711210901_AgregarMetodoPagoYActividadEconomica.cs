using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMetodoPagoYActividadEconomica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActividadEconomica",
                table: "Sucursales",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CodigoMetodoPago",
                table: "Facturas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "NumeroTarjeta",
                table: "Facturas",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActividadEconomica",
                table: "Sucursales");

            migrationBuilder.DropColumn(
                name: "CodigoMetodoPago",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "NumeroTarjeta",
                table: "Facturas");
        }
    }
}
