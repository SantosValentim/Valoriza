/* ============================================================
   TreinamentosController – Trilhas e conteúdos
   ============================================================ */

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
    public class TreinamentosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TreinamentosController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> ListarTrilhas(
            [FromQuery] int pagina = 1,
            [FromQuery] int itensPorPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (itensPorPagina < 1 || itensPorPagina > 50) itensPorPagina = 10;

            var query = _context.TrilhasTreinamento.Where(t => t.Ativa).Include(t => t.Conteudos);
            var total = await query.CountAsync();

            var trilhas = await query
                .OrderBy(t => t.Titulo)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .Select(t => new TrilhaResponseDTO
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    Descricao = t.Descricao,
                    CargaHoraria = t.CargaHoraria,
                    Nivel = t.Nivel,
                    Ativa = t.Ativa,
                    QuantidadeConteudos = t.Conteudos.Count,
                    DataCriacao = t.DataCriacao
                })
                .ToListAsync();

            return Ok(PagedResponse<TrilhaResponseDTO>.Criar(trilhas, pagina, itensPorPagina, total));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterTrilha(int id)
        {
            var trilha = await _context.TrilhasTreinamento
                .Include(t => t.Conteudos.OrderBy(c => c.Ordem))
                .FirstOrDefaultAsync(t => t.Id == id && t.Ativa);

            if (trilha is null)
                return NotFound(ApiErrorResponse.Criar("Trilha não encontrada.", "TRI_404"));

            var dto = new
            {
                trilha.Id,
                trilha.Titulo,
                trilha.Descricao,
                trilha.CargaHoraria,
                trilha.Nivel,
                Conteudos = trilha.Conteudos.Select(c => new ConteudoResponseDTO
                {
                    Id = c.Id,
                    Titulo = c.Titulo,
                    Texto = c.Texto,
                    UrlVideo = c.UrlVideo,
                    Tipo = c.Tipo,
                    Ordem = c.Ordem,
                    DuracaoMinutos = c.DuracaoMinutos
                }).ToList()
            };

            return Ok(ApiResponse<object>.Ok(dto));
        }

        [HttpPost]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> CriarTrilha([FromBody] CriarTrilhaRequestDTO request)
        {
            var trilha = new TrilhaTreinamento
            {
                Titulo = request.Titulo,
                Descricao = request.Descricao,
                CargaHoraria = request.CargaHoraria,
                Nivel = request.Nivel,
                EmpresaId = request.EmpresaId,
                Ativa = true,
                DataCriacao = DateTime.UtcNow
            };

            _context.TrilhasTreinamento.Add(trilha);
            await _context.SaveChangesAsync();

            var dto = new TrilhaResponseDTO
            {
                Id = trilha.Id,
                Titulo = trilha.Titulo,
                Descricao = trilha.Descricao,
                CargaHoraria = trilha.CargaHoraria,
                Nivel = trilha.Nivel,
                Ativa = trilha.Ativa,
                DataCriacao = trilha.DataCriacao
            };

            return CreatedAtAction(nameof(ObterTrilha), new { id = trilha.Id },
                ApiResponse<TrilhaResponseDTO>.Ok(dto, "Trilha criada com sucesso."));
        }

        [HttpPost("{trilhaId:int}/conteudos")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> AdicionarConteudo(int trilhaId, [FromBody] CriarConteudoRequestDTO request)
        {
            var trilha = await _context.TrilhasTreinamento.FindAsync(trilhaId);
            if (trilha is null)
                return NotFound(ApiErrorResponse.Criar("Trilha não encontrada.", "TRI_404"));

            var conteudo = new Conteudo
            {
                Titulo = request.Titulo,
                Texto = request.Texto,
                UrlVideo = request.UrlVideo,
                Tipo = request.Tipo,
                Ordem = request.Ordem,
                DuracaoMinutos = request.DuracaoMinutos,
                TrilhaId = trilhaId
            };

            _context.Conteudos.Add(conteudo);
            await _context.SaveChangesAsync();

            var dto = new ConteudoResponseDTO
            {
                Id = conteudo.Id,
                Titulo = conteudo.Titulo,
                Tipo = conteudo.Tipo,
                Ordem = conteudo.Ordem,
                DuracaoMinutos = conteudo.DuracaoMinutos
            };

            return Ok(ApiResponse<ConteudoResponseDTO>.Ok(dto, "Conteúdo adicionado com sucesso."));
        }
    }
}
