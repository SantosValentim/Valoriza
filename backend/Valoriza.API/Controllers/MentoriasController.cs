/* ============================================================
   MentoriasController
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
    public class MentoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MentoriasController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? status = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int itensPorPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (itensPorPagina < 1 || itensPorPagina > 50) itensPorPagina = 10;

            var query = _context.Mentorias.Include(m => m.Mentor).Include(m => m.Mentorado).AsQueryable();

            if (User.IsInRole("Colaborador") && !User.IsInRole("GestorDEI") && !User.IsInRole("AdminEmpresa"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(m => m.MentorId == userId || m.MentoradoId == userId);
            }
            else
            {
                var empresaClaim = User.FindFirst("empresaId")?.Value;
                if (!User.IsInRole("AdminValoriza") && int.TryParse(empresaClaim, out var empId))
                    query = query.Where(m => m.EmpresaId == empId);
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(m => m.Status == status);

            var total = await query.CountAsync();

            var mentorias = await query
                .OrderByDescending(m => m.DataInicio)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .Select(m => new MentoriaResponseDTO
                {
                    Id = m.Id,
                    MentorNome = m.Mentor != null ? m.Mentor.NomeCompleto : "",
                    MentoradoNome = m.Mentorado != null ? m.Mentorado.NomeCompleto : "",
                    Status = m.Status,
                    DataInicio = m.DataInicio,
                    DataFim = m.DataFim,
                    Objetivos = m.Objetivos
                })
                .ToListAsync();

            return Ok(PagedResponse<MentoriaResponseDTO>.Criar(mentorias, pagina, itensPorPagina, total));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var mentoria = await _context.Mentorias
                .Include(m => m.Mentor).Include(m => m.Mentorado)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mentoria is null)
                return NotFound(ApiErrorResponse.Criar("Mentoria não encontrada.", "MEN_404"));

            var dto = new MentoriaResponseDTO
            {
                Id = mentoria.Id,
                MentorNome = mentoria.Mentor?.NomeCompleto ?? "",
                MentoradoNome = mentoria.Mentorado?.NomeCompleto ?? "",
                Status = mentoria.Status,
                DataInicio = mentoria.DataInicio,
                DataFim = mentoria.DataFim,
                Objetivos = mentoria.Objetivos
            };

            return Ok(ApiResponse<MentoriaResponseDTO>.Ok(dto));
        }

        [HttpPost]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> Criar([FromBody] CriarMentoriaRequestDTO request)
        {
            var mentor = await _context.Users.FindAsync(request.MentorId);
            var mentorado = await _context.Users.FindAsync(request.MentoradoId);

            if (mentor is null || mentorado is null)
                return BadRequest(ApiErrorResponse.Criar("Mentor ou mentorado não encontrado.", "MEN_001"));

            if (request.MentorId == request.MentoradoId)
                return BadRequest(ApiErrorResponse.Criar("Mentor e mentorado não podem ser a mesma pessoa.", "MEN_002"));

            var mentoria = new Mentoria
            {
                MentorId = request.MentorId,
                MentoradoId = request.MentoradoId,
                EmpresaId = request.EmpresaId,
                Objetivos = request.Objetivos,
                Observacoes = request.Observacoes,
                Status = "Ativa",
                DataInicio = DateTime.UtcNow
            };

            _context.Mentorias.Add(mentoria);
            await _context.SaveChangesAsync();

            var dto = new MentoriaResponseDTO
            {
                Id = mentoria.Id,
                MentorNome = mentor.NomeCompleto,
                MentoradoNome = mentorado.NomeCompleto,
                Status = mentoria.Status,
                DataInicio = mentoria.DataInicio,
                Objetivos = mentoria.Objetivos
            };

            return CreatedAtAction(nameof(ObterPorId), new { id = mentoria.Id },
                ApiResponse<MentoriaResponseDTO>.Ok(dto, "Mentoria criada com sucesso."));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarMentoriaRequestDTO request)
        {
            var mentoria = await _context.Mentorias.FindAsync(id);
            if (mentoria is null)
                return NotFound(ApiErrorResponse.Criar("Mentoria não encontrada.", "MEN_404"));

            mentoria.Status = request.Status;
            mentoria.Objetivos = request.Objetivos ?? mentoria.Objetivos;
            mentoria.Observacoes = request.Observacoes ?? mentoria.Observacoes;

            if (request.Status == "Concluida" || request.Status == "Cancelada")
                mentoria.DataFim = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok("Mentoria atualizada com sucesso."));
        }
    }
}
