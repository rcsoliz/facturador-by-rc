using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facturacion.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarWebhookSecretATenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WebhookUrl",
                table: "Tenants",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookSecretCifrado",
                table: "Tenants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebhookSecretCifrado",
                table: "Tenants");

            migrationBuilder.AlterColumn<string>(
                name: "WebhookUrl",
                table: "Tenants",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldNullable: true);
        }
    }
}
