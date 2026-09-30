/* Canal de denúncias
   Modelo: Titulo, Descricao, Categoria, Observacoes, DataResolucao
   - Todos autenticados: criar e ver as próprias
   - Gestor/Admin: só da própria empresa
   - Auditoria: criar e alterar status */

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.DTOs;
using Valoriza.API.Models;
using Valoriza.API.Services;

namespace Valoriza.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DenunciasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public DenunciasController(
            ApplicationDbContext context,
            IAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        private string? UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        private string? UserEmail =>
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.Name);

        private bool IsGestorOuAdmin =>
            User.IsInRole("AdminValoriza") ||
            User.IsInRole("AdminEmpresa") ||
            User.IsInRole("GestorDEI");

        private int? EmpresaIdDoToken()
        {
            var claim = User.FindFirst("empresaId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private string? ClientIp =>
            HttpContext.Connection.RemoteIpAddress?.ToString();

        // LISTAR
        /// Colaborador: só as próprias. Gestor/Admin: só da própria empresa.
        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] int pagina = 1,
            [FromQuery] int itensPorPagina = 10,
            [FromQuery] string? status = null)
        {
            if (pagina < 1) pagina = 1;
            if (itensPorPagina < 1 || itensPorPagina > 50) itensPorPagina = 10;

            var query = _context.Denuncias
                .Include(d => d.Empresa)
                .Include(d => d.Usuario)
                .AsQueryable();

            if (!IsGestorOuAdmin)
            {
                query = query.Where(d => d.UsuarioId == UserId);
            }
            else
            {
                var empresaId = EmpresaIdDoToken();
                if (empresaId == null)
                    query = query.Where(d => false);
                else
                    query = query.Where(d => d.EmpresaId == empresaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(d => d.Status == status);

            var total = await query.CountAsync();

            var denuncias = await query
                .OrderByDescending(d => d.DataRegistro)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .Select(d => new DenunciaResponseDTO
                {
                    Id = d.Id,
                    Protocolo = d.Protocolo,
                    Titulo = d.Titulo,
                    Descricao = d.Descricao,
                    Categoria = d.Categoria,
                    Anonima = d.Anonima,
                    Status = d.Status,
                    Observacoes = d.Observacoes,
                    DataRegistro = d.DataRegistro,
                    DataResolucao = d.DataResolucao,
                    EmpresaNome = d.Empresa != null ? d.Empresa.NomeFantasia : null,
                    UsuarioNome = d.Anonima
                        ? null
                        : (d.Usuario != null
                            ? (d.Usuario.NomeSocial ?? d.Usuario.NomeCompleto)
                            : null)
                })
                .ToListAsync();

            return Ok(PagedResponse<DenunciaResponseDTO>.Criar(
                denuncias, pagina, itensPorPagina, total));
        }

        // DETALHE
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var denuncia = await _context.Denuncias
                .Include(d => d.Empresa)
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (denuncia is null)
                return NotFound(ApiErrorResponse.Criar("Denúncia não encontrada.", "DEN_404"));

            var isAutor = denuncia.UsuarioId == UserId;

            if (!isAutor)
            {
                if (!IsGestorOuAdmin)
                    return Forbid();

                var empresaId = EmpresaIdDoToken();
                if (empresaId == null || denuncia.EmpresaId != empresaId.Value)
                    return Forbid();
            }

            var dto = new DenunciaResponseDTO
            {
                Id = denuncia.Id,
                Protocolo = denuncia.Protocolo,
                Titulo = denuncia.Titulo,
                Descricao = denuncia.Descricao,
                Categoria = denuncia.Categoria,
                Anonima = denuncia.Anonima,
                Status = denuncia.Status,
                Observacoes = denuncia.Observacoes,
                DataRegistro = denuncia.DataRegistro,
                DataResolucao = denuncia.DataResolucao,
                EmpresaNome = denuncia.Empresa?.NomeFantasia,
                UsuarioNome = (denuncia.Anonima && !isAutor)
                    ? null
                    : (denuncia.Usuario != null
                        ? (denuncia.Usuario.NomeSocial ?? denuncia.Usuario.NomeCompleto)
                        : null)
            };

            return Ok(ApiResponse<DenunciaResponseDTO>.Ok(dto));
        }

        // CRIAR
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarDenunciaRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Descricao))
                return BadRequest(ApiErrorResponse.Criar("Informe a descrição.", "DEN_001"));

            var userId = UserId;
            var empresaId = EmpresaIdDoToken() ?? 1;

            var denuncia = new Denuncia
            {
                Protocolo = $"DEN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                Titulo = string.IsNullOrWhiteSpace(request.Titulo)
                    ? "Denúncia"
                    : request.Titulo.Trim(),
                Descricao = request.Descricao.Trim(),
                Categoria = request.Categoria,
                Anonima = request.Anonima,
                Status = "Aberta",
                DataRegistro = DateTime.UtcNow,
                UsuarioId = userId, // sempre grava para o autor acompanhar
                EmpresaId = empresaId
            };

            _context.Denuncias.Add(denuncia);
            await _context.SaveChangesAsync();

            // Auditoria – operação crítica
            await _auditoria.RegistrarAsync(
                userId ?? "",
                UserEmail,
                "Denuncia.Criar",
                "Denuncia",
                denuncia.Id.ToString(),
                $"Protocolo={denuncia.Protocolo}; Anonima={denuncia.Anonima}; EmpresaId={denuncia.EmpresaId}",
                ClientIp);

            var dto = new DenunciaResponseDTO
            {
                Id = denuncia.Id,
                Protocolo = denuncia.Protocolo,
                Titulo = denuncia.Titulo,
                Descricao = denuncia.Descricao,
                Categoria = denuncia.Categoria,
                Status = denuncia.Status,
                DataRegistro = denuncia.DataRegistro,
                Anonima = denuncia.Anonima
            };

            return CreatedAtAction(nameof(ObterPorId), new { id = denuncia.Id },
                ApiResponse<DenunciaResponseDTO>.Ok(dto, "Denúncia registrada com sucesso."));
        }

        // STATUS
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> AtualizarStatus(
            int id,
            [FromBody] AtualizarStatusDenunciaRequestDTO request)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);
            if (denuncia is null)
                return NotFound(ApiErrorResponse.Criar("Denúncia não encontrada.", "DEN_404"));

            var empresaId = EmpresaIdDoToken();
            if (empresaId == null || denuncia.EmpresaId != empresaId.Value)
                return Forbid();

            var statusAnterior = denuncia.Status;
            denuncia.Status = request.Status;
            denuncia.Observacoes = request.Observacoes;

            if (string.Equals(request.Status, "Resolvida", StringComparison.OrdinalIgnoreCase))
                denuncia.DataResolucao = DateTime.UtcNow;
            else
                denuncia.DataResolucao = null;

            await _context.SaveChangesAsync();

            // Auditoria – operação crítica
            await _auditoria.RegistrarAsync(
                UserId ?? "",
                UserEmail,
                "Denuncia.Status",
                "Denuncia",
                denuncia.Id.ToString(),
                $"Protocolo={denuncia.Protocolo}; De={statusAnterior}; Para={denuncia.Status}",
                ClientIp);

            return Ok(ApiResponse<object>.Ok(
                new { denuncia.Id, denuncia.Status },
                "Status atualizado com sucesso."));
        }
    }
}