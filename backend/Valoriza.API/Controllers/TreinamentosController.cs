/* Trilhas + conteúdos (Texto, Video, Quiz)
   Gerenciar: AdminValoriza, AdminEmpresa, GestorDEI*/

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

        public TreinamentosController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool PodeGerenciar =>
            User.IsInRole("AdminValoriza")
            || User.IsInRole("AdminEmpresa")
            || User.IsInRole("GestorDEI");

        // GET /api/treinamentos
        [HttpGet]
        public async Task<IActionResult> ListarTrilhas(
            [FromQuery] int pagina = 1,
            [FromQuery] int itensPorPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (itensPorPagina < 1 || itensPorPagina > 50) itensPorPagina = 10;

            var query = _context.TrilhasTreinamento
                .Where(t => t.Ativa)
                .Include(t => t.Conteudos);

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

            return Ok(PagedResponse<TrilhaResponseDTO>.Criar(
                trilhas, pagina, itensPorPagina, total));
        }

        // GET /api/treinamentos/{id}
        // Resposta correta do quiz: só quem PodeGerenciar
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
                QuantidadeConteudos = trilha.Conteudos.Count,
                Conteudos = trilha.Conteudos.Select(c => new
                {
                    c.Id,
                    c.Titulo,
                    c.Texto,
                    c.UrlVideo,
                    c.Tipo,
                    c.Ordem,
                    c.DuracaoMinutos,
                    OpcaoA = c.Tipo == "Quiz" ? c.OpcaoA : null,
                    OpcaoB = c.Tipo == "Quiz" ? c.OpcaoB : null,
                    OpcaoC = c.Tipo == "Quiz" ? c.OpcaoC : null,
                    OpcaoD = c.Tipo == "Quiz" ? c.OpcaoD : null,
                    // Colaborador não vê no GET; só após POST .../responder
                    RespostaCorreta = PodeGerenciar && c.Tipo == "Quiz"
                        ? c.RespostaCorreta
                        : null
                }).ToList()
            };

            return Ok(ApiResponse<object>.Ok(dto));
        }

        // POST /api/treinamentos
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
                QuantidadeConteudos = 0,
                DataCriacao = trilha.DataCriacao
            };

            return CreatedAtAction(
                nameof(ObterTrilha),
                new { id = trilha.Id },
                ApiResponse<TrilhaResponseDTO>.Ok(dto, "Trilha criada com sucesso."));
        }

        // PUT /api/treinamentos/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> AtualizarTrilha(int id, [FromBody] CriarTrilhaRequestDTO request)
        {
            var trilha = await _context.TrilhasTreinamento.FindAsync(id);
            if (trilha is null)
                return NotFound(ApiErrorResponse.Criar("Trilha não encontrada.", "TRI_404"));

            trilha.Titulo = request.Titulo;
            trilha.Descricao = request.Descricao;
            trilha.CargaHoraria = request.CargaHoraria;
            trilha.Nivel = request.Nivel;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { trilha.Id }, "Trilha atualizada."));
        }

        // DELETE /api/treinamentos/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> ExcluirTrilha(int id)
        {
            var trilha = await _context.TrilhasTreinamento
                .Include(t => t.Conteudos)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trilha is null)
                return NotFound(ApiErrorResponse.Criar("Trilha não encontrada.", "TRI_404"));

            _context.Conteudos.RemoveRange(trilha.Conteudos);
            _context.TrilhasTreinamento.Remove(trilha);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Trilha excluída."));
        }

        // POST /api/treinamentos/{trilhaId}/conteudos
        [HttpPost("{trilhaId:int}/conteudos")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> AdicionarConteudo(
            int trilhaId,
            [FromBody] CriarConteudoRequestDTO request)
        {
            var trilha = await _context.TrilhasTreinamento.FindAsync(trilhaId);
            if (trilha is null)
                return NotFound(ApiErrorResponse.Criar("Trilha não encontrada.", "TRI_404"));

            if (string.Equals(request.Tipo, "Quiz", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.OpcaoA) ||
                    string.IsNullOrWhiteSpace(request.OpcaoB) ||
                    string.IsNullOrWhiteSpace(request.OpcaoC) ||
                    string.IsNullOrWhiteSpace(request.OpcaoD))
                {
                    return BadRequest(ApiErrorResponse.Criar(
                        "Quiz exige as opções A, B, C e D.", "QUIZ_001"));
                }

                var resp = (request.RespostaCorreta ?? "").Trim().ToUpperInvariant();
                if (resp is not ("A" or "B" or "C" or "D"))
                {
                    return BadRequest(ApiErrorResponse.Criar(
                        "Resposta correta deve ser A, B, C ou D.", "QUIZ_002"));
                }

                request.RespostaCorreta = resp;
            }
            else
            {
                request.OpcaoA = request.OpcaoB = request.OpcaoC = request.OpcaoD = null;
                request.RespostaCorreta = null;
            }

            var conteudo = new Conteudo
            {
                Titulo = request.Titulo,
                Texto = request.Texto,
                UrlVideo = request.UrlVideo,
                Tipo = request.Tipo,
                Ordem = request.Ordem,
                DuracaoMinutos = request.DuracaoMinutos,
                TrilhaId = trilhaId,
                OpcaoA = request.OpcaoA,
                OpcaoB = request.OpcaoB,
                OpcaoC = request.OpcaoC,
                OpcaoD = request.OpcaoD,
                RespostaCorreta = request.RespostaCorreta
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

            return Ok(ApiResponse<ConteudoResponseDTO>.Ok(dto, "Conteúdo adicionado."));
        }

        // PUT /api/treinamentos/conteudos/{id}
        [HttpPut("conteudos/{id:int}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> AtualizarConteudo(
            int id,
            [FromBody] CriarConteudoRequestDTO request)
        {
            var conteudo = await _context.Conteudos.FindAsync(id);
            if (conteudo is null)
                return NotFound(ApiErrorResponse.Criar("Conteúdo não encontrado.", "CON_404"));

            if (string.Equals(request.Tipo, "Quiz", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.OpcaoA) ||
                    string.IsNullOrWhiteSpace(request.OpcaoB) ||
                    string.IsNullOrWhiteSpace(request.OpcaoC) ||
                    string.IsNullOrWhiteSpace(request.OpcaoD))
                {
                    return BadRequest(ApiErrorResponse.Criar(
                        "Quiz exige as opções A, B, C e D.", "QUIZ_001"));
                }

                var resp = (request.RespostaCorreta ?? "").Trim().ToUpperInvariant();
                if (resp is not ("A" or "B" or "C" or "D"))
                {
                    return BadRequest(ApiErrorResponse.Criar(
                        "Resposta correta deve ser A, B, C ou D.", "QUIZ_002"));
                }

                request.RespostaCorreta = resp;
            }
            else
            {
                request.OpcaoA = request.OpcaoB = request.OpcaoC = request.OpcaoD = null;
                request.RespostaCorreta = null;
            }

            conteudo.Titulo = request.Titulo;
            conteudo.Texto = request.Texto;
            conteudo.UrlVideo = request.UrlVideo;
            conteudo.Tipo = request.Tipo;
            conteudo.Ordem = request.Ordem;
            conteudo.DuracaoMinutos = request.DuracaoMinutos;
            conteudo.OpcaoA = request.OpcaoA;
            conteudo.OpcaoB = request.OpcaoB;
            conteudo.OpcaoC = request.OpcaoC;
            conteudo.OpcaoD = request.OpcaoD;
            conteudo.RespostaCorreta = request.RespostaCorreta;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { conteudo.Id }, "Conteúdo atualizado."));
        }

        // DELETE /api/treinamentos/conteudos/{id}
        [HttpDelete("conteudos/{id:int}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> ExcluirConteudo(int id)
        {
            var conteudo = await _context.Conteudos.FindAsync(id);
            if (conteudo is null)
                return NotFound(ApiErrorResponse.Criar("Conteúdo não encontrado.", "CON_404"));

            _context.Conteudos.Remove(conteudo);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Conteúdo excluído."));
        }

        // POST /api/treinamentos/conteudos/{id}/responder
        // Revela a correta só DEPOIS de responder
        [HttpPost("conteudos/{id:int}/responder")]
        public async Task<IActionResult> ResponderQuiz(
            int id,
            [FromBody] ResponderQuizRequestDTO request)
        {
            var conteudo = await _context.Conteudos.FindAsync(id);
            if (conteudo is null)
                return NotFound(ApiErrorResponse.Criar("Conteúdo não encontrado.", "CON_404"));

            if (!string.Equals(conteudo.Tipo, "Quiz", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiErrorResponse.Criar("Este conteúdo não é um quiz.", "QUIZ_003"));

            var resposta = (request.Resposta ?? "").Trim().ToUpperInvariant();
            if (resposta is not ("A" or "B" or "C" or "D"))
                return BadRequest(ApiErrorResponse.Criar("Use A, B, C ou D.", "QUIZ_004"));

            var acertou = string.Equals(
                resposta,
                conteudo.RespostaCorreta,
                StringComparison.OrdinalIgnoreCase);

            return Ok(ApiResponse<object>.Ok(new
            {
                Acertou = acertou,
                SuaResposta = resposta,
                RespostaCorreta = conteudo.RespostaCorreta,
                Mensagem = acertou ? "Resposta correta!": $"Resposta incorreta. A correta era {conteudo.RespostaCorreta}."
            }));
        }
    }
}