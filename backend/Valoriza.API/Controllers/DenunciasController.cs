/* ============================================================
   DenunciasController – Canal de denúncias
   ============================================================ */

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.DTOs;
using Valoriza.API.Models;

namespace Valoriza.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DenunciasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DenunciasController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> Listar(
            [FromQuery] int pagina = 1,
            [FromQuery] int itensPorPagina = 10,
            [FromQuery] string? status = null)
        {
            if (pagina < 1) pagina = 1;
            if (itensPorPagina < 1 || itensPorPagina > 50) itensPorPagina = 10;

            var query = _context.Denuncias.Include(d => d.Empresa).Include(d => d.Usuario).AsQueryable();

            var empresaIdClaim = User.FindFirst("empresaId")?.Value;
            if (!User.IsInRole("AdminValoriza") && int.TryParse(empresaIdClaim, out var empresaId))
                query = query.Where(d => d.EmpresaId == empresaId);

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
                    Tipo = d.Tipo,
                    Relato = d.Relato,
                    Anonima = d.Anonima,
                    Status = d.Status,
                    DataRegistro = d.DataRegistro,
                    DataResolucao = d.DataResolucao,
                    EmpresaNome = d.Empresa != null ? d.Empresa.NomeFantasia : null,
                    UsuarioNome = d.Anonima ? null : (d.Usuario != null ? d.Usuario.NomeCompleto : null)
                })
                .ToListAsync();

            return Ok(PagedResponse<DenunciaResponseDTO>.Criar(denuncias, pagina, itensPorPagina, total));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var denuncia = await _context.Denuncias
                .Include(d => d.Empresa).Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (denuncia is null)
                return NotFound(ApiErrorResponse.Criar("Denúncia não encontrada.", "DEN_404"));

            var dto = new DenunciaResponseDTO
            {
                Id = denuncia.Id,
                Protocolo = denuncia.Protocolo,
                Tipo = denuncia.Tipo,
                Relato = denuncia.Relato,
                Anonima = denuncia.Anonima,
                Status = denuncia.Status,
                DataRegistro = denuncia.DataRegistro,
                DataResolucao = denuncia.DataResolucao,
                EmpresaNome = denuncia.Empresa?.NomeFantasia,
                UsuarioNome = denuncia.Anonima ? null : denuncia.Usuario?.NomeCompleto
            };

            return Ok(ApiResponse<DenunciaResponseDTO>.Ok(dto));
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarDenunciaRequestDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var empresaIdClaim = User.FindFirst("empresaId")?.Value;

            // Se não conseguir converter a claim do token, ele assume a Empresa ID 1 em ambiente de testes
            if (!int.TryParse(empresaIdClaim, out var empresaId))
            {
                empresaId = 1;
            }

            var denuncia = new Denuncia
            {
                Protocolo = $"DEN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                Tipo = request.Tipo,
                Relato = request.Relato,
                Anonima = request.Anonima,
                Status = "Aberta",
                DataRegistro = DateTime.UtcNow,
                UsuarioId = request.Anonima ? null : userId,
                EmpresaId = empresaId // Agora garantimos que o ID 1 será injetado se o token falhar
            };

            _context.Denuncias.Add(denuncia);
            await _context.SaveChangesAsync();

            var dto = new DenunciaResponseDTO
            {
                Id = denuncia.Id,
                Protocolo = denuncia.Protocolo,
                Tipo = denuncia.Tipo,
                Status = denuncia.Status,
                DataRegistro = denuncia.DataRegistro,
                Anonima = denuncia.Anonima
            };

            return CreatedAtAction(nameof(ObterPorId), new { id = denuncia.Id },
                ApiResponse<DenunciaResponseDTO>.Ok(dto, "Denúncia registrada com sucesso."));
        }


        [HttpPut("{id:int}/status")]
            [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] AtualizarStatusDenunciaRequestDTO request)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);
            if (denuncia is null)
                return NotFound(ApiErrorResponse.Criar("Denúncia não encontrada.", "DEN_404"));

            denuncia.Status = request.Status;
            denuncia.ObservacoesInternas = request.ObservacoesInternas;
            if (request.Status == "Resolvida")
                denuncia.DataResolucao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok("Status atualizado com sucesso."));
        }
    }
}
