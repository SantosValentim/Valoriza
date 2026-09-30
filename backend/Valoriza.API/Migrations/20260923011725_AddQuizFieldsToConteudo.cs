using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valoriza.API.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizFieldsToConteudo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpcaoA",
                table: "Conteudos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpcaoB",
                table: "Conteudos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpcaoC",
                table: "Conteudos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpcaoD",
                table: "Conteudos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RespostaCorreta",
                table: "Conteudos",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpcaoA",
                table: "Conteudos");

            migrationBuilder.DropColumn(
                name: "OpcaoB",
                table: "Conteudos");

            migrationBuilder.DropColumn(
                name: "OpcaoC",
                table: "Conteudos");

            migrationBuilder.DropColumn(
                name: "OpcaoD",
                table: "Conteudos");

            migrationBuilder.DropColumn(
                name: "RespostaCorreta",
                table: "Conteudos");
        }
    }
}
