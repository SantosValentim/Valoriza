using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valoriza.API.Migrations
{
    /// <inheritdoc />
    public partial class AddChurnFieldsEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CicloCobranca",
                table: "Empresas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataChurn",
                table: "Empresas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoChurn",
                table: "Empresas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusAssinatura",
                table: "Empresas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorAssinatura",
                table: "Empresas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CicloCobranca",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "DataChurn",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "MotivoChurn",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "StatusAssinatura",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ValorAssinatura",
                table: "Empresas");
        }
    }
}
