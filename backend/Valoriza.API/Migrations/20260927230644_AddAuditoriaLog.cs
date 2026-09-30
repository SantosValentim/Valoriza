using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valoriza.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditoriaLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditoriaLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsuarioEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Acao = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Entidade = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EntidadeId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Detalhes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaLogs");
        }
    }
}
