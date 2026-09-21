using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valoriza.API.Migrations
{
    /// <inheritdoc />
    public partial class CascadeEmpresaDenunciasRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Denuncias_AspNetUsers_UsuarioId",
                table: "Denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_Denuncias_Empresas_EmpresaId",
                table: "Denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressosTreinamento_AspNetUsers_UsuarioId",
                table: "ProgressosTreinamento");

            migrationBuilder.AddForeignKey(
                name: "FK_Denuncias_AspNetUsers_UsuarioId",
                table: "Denuncias",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Denuncias_Empresas_EmpresaId",
                table: "Denuncias",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressosTreinamento_AspNetUsers_UsuarioId",
                table: "ProgressosTreinamento",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Denuncias_AspNetUsers_UsuarioId",
                table: "Denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_Denuncias_Empresas_EmpresaId",
                table: "Denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressosTreinamento_AspNetUsers_UsuarioId",
                table: "ProgressosTreinamento");

            migrationBuilder.AddForeignKey(
                name: "FK_Denuncias_AspNetUsers_UsuarioId",
                table: "Denuncias",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Denuncias_Empresas_EmpresaId",
                table: "Denuncias",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressosTreinamento_AspNetUsers_UsuarioId",
                table: "ProgressosTreinamento",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
