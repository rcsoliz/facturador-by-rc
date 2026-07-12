using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIndiceCredencialSiat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Cuis",
                table: "CredencialesSiat",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CufdCodigoControl",
                table: "CredencialesSiat",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cufd",
                table: "CredencialesSiat",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CredencialesSiat_TenantId_SucursalId_PuntoVentaId",
                table: "CredencialesSiat",
                columns: new[] { "TenantId", "SucursalId", "PuntoVentaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CredencialesSiat_TenantId_SucursalId_PuntoVentaId",
                table: "CredencialesSiat");

            migrationBuilder.AlterColumn<string>(
                name: "Cuis",
                table: "CredencialesSiat",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CufdCodigoControl",
                table: "CredencialesSiat",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cufd",
                table: "CredencialesSiat",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
