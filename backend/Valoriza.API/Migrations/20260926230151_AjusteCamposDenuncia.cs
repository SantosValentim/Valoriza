using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valoriza.API.Migrations
{
    /// <inheritdoc />
    public partial class AjusteCamposDenuncia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Denuncias_Protocolo",
                table: "Denuncias");

            migrationBuilder.DropColumn(
                name: "Relato",
                table: "Denuncias");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Denuncias");

            migrationBuilder.RenameColumn(
                name: "ObservacoesInternas",
                table: "Denuncias",
                newName: "Observacoes");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Denuncias",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Protocolo",
                table: "Denuncias",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Denuncias",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Denuncias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Denuncias",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_Protocolo",
                table: "Denuncias",
                column: "Protocolo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Denuncias_Protocolo",
                table: "Denuncias");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Denuncias");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Denuncias");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Denuncias");

            migrationBuilder.RenameColumn(
                name: "Observacoes",
                table: "Denuncias",
                newName: "ObservacoesInternas");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Denuncias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Protocolo",
                table: "Denuncias",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AddColumn<string>(
                name: "Relato",
                table: "Denuncias",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Denuncias",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_Protocolo",
                table: "Denuncias",
                column: "Protocolo",
                unique: true,
                filter: "[Protocolo] IS NOT NULL");
        }
    }
}
