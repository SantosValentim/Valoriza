using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valoriza.API.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "Empresas");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicioAssinatura",
                table: "Empresas",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataInicioAssinatura",
                table: "Empresas");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCadastro",
                table: "Empresas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
