using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valoriza.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMentoriaForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MentoriaProgramas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    MentorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Objetivos = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentoriaProgramas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentoriaProgramas_AspNetUsers_MentorId",
                        column: x => x.MentorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentoriaProgramas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentoriaInscricoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MentoriaProgramaId = table.Column<int>(type: "int", nullable: false),
                    MentoradoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DataInscricao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentoriaInscricoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentoriaInscricoes_AspNetUsers_MentoradoId",
                        column: x => x.MentoradoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentoriaInscricoes_MentoriaProgramas_MentoriaProgramaId",
                        column: x => x.MentoriaProgramaId,
                        principalTable: "MentoriaProgramas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentoriaPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MentoriaProgramaId = table.Column<int>(type: "int", nullable: false),
                    AutorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    UrlLink = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UrlMidia = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TipoMidia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataPublicacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentoriaPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentoriaPosts_AspNetUsers_AutorId",
                        column: x => x.AutorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentoriaPosts_MentoriaProgramas_MentoriaProgramaId",
                        column: x => x.MentoriaProgramaId,
                        principalTable: "MentoriaProgramas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentoriaRespostas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MentoriaPostId = table.Column<int>(type: "int", nullable: false),
                    AutorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentoriaRespostas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MentoriaRespostas_AspNetUsers_AutorId",
                        column: x => x.AutorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentoriaRespostas_MentoriaPosts_MentoriaPostId",
                        column: x => x.MentoriaPostId,
                        principalTable: "MentoriaPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MentoriaInscricoes_MentoradoId",
                table: "MentoriaInscricoes",
                column: "MentoradoId");

            migrationBuilder.CreateIndex(
                name: "IX_MentoriaInscricoes_MentoriaProgramaId_MentoradoId",
                table: "MentoriaInscricoes",
                columns: new[] { "MentoriaProgramaId", "MentoradoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MentoriaPosts_AutorId",
                table: "MentoriaPosts",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_MentoriaPosts_MentoriaProgramaId",
                table: "MentoriaPosts",
                column: "MentoriaProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_MentoriaProgramas_EmpresaId",
                table: "MentoriaProgramas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_MentoriaProgramas_MentorId",
                table: "MentoriaProgramas",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_MentoriaRespostas_AutorId",
                table: "MentoriaRespostas",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_MentoriaRespostas_MentoriaPostId",
                table: "MentoriaRespostas",
                column: "MentoriaPostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentoriaInscricoes");

            migrationBuilder.DropTable(
                name: "MentoriaRespostas");

            migrationBuilder.DropTable(
                name: "MentoriaPosts");

            migrationBuilder.DropTable(
                name: "MentoriaProgramas");
        }
    }
}
