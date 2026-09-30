using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valoriza.API.Migrations
{
    /// <inheritdoc />
    public partial class AddConteudoProgresso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConteudosProgresso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConteudoId = table.Column<int>(type: "int", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespostaQuiz = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Acertou = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConteudosProgresso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConteudosProgresso_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConteudosProgresso_Conteudos_ConteudoId",
                        column: x => x.ConteudoId,
                        principalTable: "Conteudos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConteudosProgresso_ConteudoId",
                table: "ConteudosProgresso",
                column: "ConteudoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConteudosProgresso_UsuarioId_ConteudoId",
                table: "ConteudosProgresso",
                columns: new[] { "UsuarioId", "ConteudoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConteudosProgresso");
        }
    }
}
