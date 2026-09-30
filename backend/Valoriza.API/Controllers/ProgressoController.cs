/* Progresso em trilhas e conteúdos */

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.DTOs;
using Valoriza.API.Services;

namespace Valoriza.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgressoController : ControllerBase
    {
        private readonly ProgressoService _progresso;
        private readonly ApplicationDbContext _context;

        public ProgressoController(ProgressoService progresso, ApplicationDbContext context)
        {
            _progresso = progresso;
            _context = context;
        }

        private string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// Progresso do usuário logado em uma trilha
        [HttpGet("trilha/{trilhaId:int}")]
        public async Task<IActionResult> GetByTrilha(int trilhaId)
        {
            var p = await _progresso.ObterProgressoAsync(UserId, trilhaId);
            var concluidos = await _progresso.ConteudosConcluidosIdsAsync(UserId, trilhaId);

            return Ok(ApiResponse<object>.Ok(new
            {
                trilhaId,
                percentual = p?.PercentualConcluido ?? 0,
                concluido = p?.Concluido ?? false,
                dataConclusao = p?.DataConclusao,
                conteudosConcluidosIds = concluidos.ToList()
            }));
        }

        /// Marca conteúdo como concluído.
        /// Body opcional para quiz: { "respostaQuiz": "B" }
        [HttpPost("conteudo/{conteudoId:int}/concluir")]
        public async Task<IActionResult> ConcluirConteudo(
            int conteudoId,
            [FromBody] ConcluirConteudoDto? dto)
        {
            try
            {
                var progresso = await _progresso.ConcluirConteudoAsync(
                    UserId, conteudoId, dto?.RespostaQuiz);

                return Ok(ApiResponse<object>.Ok(new
                {
                    progresso.TrilhaId,
                    progresso.PercentualConcluido,
                    progresso.Concluido,
                    progresso.DataConclusao
                }, "Progresso atualizado."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiErrorResponse.Criar(ex.Message, "PROG_001"));
            }
        }

        /// Lista progresso do usuário em todas as trilhas
        [HttpGet("meus")]
        public async Task<IActionResult> Meus()
        {
            var lista = await _context.ProgressosTreinamento
                .Include(p => p.Trilha)
                .Where(p => p.UsuarioId == UserId)
                .Select(p => new
                {
                    p.TrilhaId,
                    titulo = p.Trilha!.Titulo,
                    p.PercentualConcluido,
                    p.Concluido,
                    p.DataConclusao
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(lista));
        }
    }

    public class ConcluirConteudoDto
    {
        public string? RespostaQuiz { get; set; }
    }
}