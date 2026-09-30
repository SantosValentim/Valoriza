using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valoriza.API.Migrations
{
    /// <inheritdoc />
    public partial class AddComunicacaoInclusiva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChecklistInclusivoItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Texto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Contexto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistInclusivoItens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrientacoesInclusivas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrientacoesInclusivas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistInclusivoConclusoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DataReferencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistInclusivoConclusoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistInclusivoConclusoes_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChecklistInclusivoConclusoes_ChecklistInclusivoItens_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ChecklistInclusivoItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistInclusivoConclusoes_ItemId_UsuarioId_DataReferencia",
                table: "ChecklistInclusivoConclusoes",
                columns: new[] { "ItemId", "UsuarioId", "DataReferencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistInclusivoConclusoes_UsuarioId",
                table: "ChecklistInclusivoConclusoes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistInclusivoConclusoes");

            migrationBuilder.DropTable(
                name: "OrientacoesInclusivas");

            migrationBuilder.DropTable(
                name: "ChecklistInclusivoItens");
        }
    }
}
