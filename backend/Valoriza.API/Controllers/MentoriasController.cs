/*  Mentoria: pareamento 1 a 1 (legado)
   MentoriaPrograma: programa + inscrição + fórum
   Isolamento por empresaId no token JWT. */

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

        public MentoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // helpers

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private int? EmpresaIdDoToken
        {
            get
            {
                var claim = User.FindFirst("empresaId")?.Value;
                return int.TryParse(claim, out var id) ? id : null;
            }
        }

        private bool IsAdminValoriza => User.IsInRole("AdminValoriza");
        private bool PodeGerenciar =>
            User.IsInRole("AdminValoriza")
            || User.IsInRole("AdminEmpresa")
            || User.IsInRole("GestorDEI");

        ///Lista pareamentos mentor–mentorado
        [HttpGet]
        public async Task<IActionResult> Listar(
            [FromQuery] string? status = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int itensPorPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (itensPorPagina < 1 || itensPorPagina > 50) itensPorPagina = 10;

            var query = _context.Mentorias
                .Include(m => m.Mentor)
                .Include(m => m.Mentorado)
                .AsQueryable();

            // Colaborador só vê onde é mentor ou mentorado
            if (User.IsInRole("Colaborador")
                && !User.IsInRole("GestorDEI")
                && !User.IsInRole("AdminEmpresa")
                && !IsAdminValoriza)
            {
                query = query.Where(m => m.MentorId == UserId || m.MentoradoId == UserId);
            }
            else if (!IsAdminValoriza && EmpresaIdDoToken is int empId)
            {
                // Empresa cliente: só a própria empresa
                query = query.Where(m => m.EmpresaId == empId);
            }
            // AdminValoriza: pode filtrar depois por empresa se quiser
            // por regra de negócio de isolação de fórum, pareamento 1-1 de clientes pode ser restrito

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(m => m.Status == status);

            var total = await query.CountAsync();

            var itens = await query
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

            return Ok(PagedResponse<MentoriaResponseDTO>.Criar(itens, pagina, itensPorPagina, total));
        }

        ///Detalhe de um pareamento
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var m = await _context.Mentorias
                .Include(x => x.Mentor)
                .Include(x => x.Mentorado)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (m is null)
                return NotFound(ApiErrorResponse.Criar("Mentoria não encontrada.", "MEN_404"));

            // Isolamento básico
            if (!IsAdminValoriza && EmpresaIdDoToken is int emp && m.EmpresaId != emp)
                return Forbid();

            var dto = new MentoriaResponseDTO
            {
                Id = m.Id,
                MentorNome = m.Mentor?.NomeCompleto ?? "",
                MentoradoNome = m.Mentorado?.NomeCompleto ?? "",
                Status = m.Status,
                DataInicio = m.DataInicio,
                DataFim = m.DataFim,
                Objetivos = m.Objetivos
            };

            return Ok(ApiResponse<MentoriaResponseDTO>.Ok(dto));
        }

        /// Cria pareamento 1 a 1.
        /// Mentor pode ser Admin, Gestor ou Colaborador da empresa.
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

            // Mesma empresa (exceto AdminValoriza operando na Valoriza)
            if (!IsAdminValoriza)
            {
                if (EmpresaIdDoToken is int emp)
                {
                    if (mentor.EmpresaId != emp || mentorado.EmpresaId != emp)
                        return BadRequest(ApiErrorResponse.Criar("Mentor e mentorado devem ser da sua empresa.", "MEN_003"));
                    request.EmpresaId = emp;
                }
            }

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

        ///Atualiza status / objetivos do pareamento
        [HttpPut("{id:int}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarMentoriaRequestDTO request)
        {
            var mentoria = await _context.Mentorias.FindAsync(id);
            if (mentoria is null)
                return NotFound(ApiErrorResponse.Criar("Mentoria não encontrada.", "MEN_404"));

            if (!IsAdminValoriza && EmpresaIdDoToken is int emp && mentoria.EmpresaId != emp)
                return Forbid();

            mentoria.Status = request.Status;
            mentoria.Objetivos = request.Objetivos ?? mentoria.Objetivos;
            mentoria.Observacoes = request.Observacoes ?? mentoria.Observacoes;

            if (request.Status is "Concluida" or "Cancelada")
                mentoria.DataFim = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Mentoria atualizada com sucesso."));
        }

        // PROGRAMA + FÓRUM (MentoriaProgramas)
        /// <summary>Lista programas de mentoria da empresa do token</summary>
        [HttpGet("programas")]
        public async Task<IActionResult> ListarProgramas()
        {
            // Isolamento: só a empresa do usuário
            // AdminValoriza sem empresaId no token -> lista vazia de clientes
            if (EmpresaIdDoToken is not int empresaId)
                return Ok(ApiResponse<object>.Ok(Array.Empty<object>(), "Sem empresa vinculada."));

            var lista = await _context.MentoriaProgramas
                .Include(p => p.Mentor)
                .Include(p => p.Inscricoes)
                .Where(p => p.EmpresaId == empresaId)
                .OrderByDescending(p => p.DataInicio)
                .Select(p => new
                {
                    p.Id,
                    p.Titulo,
                    p.Status,
                    p.DataInicio,
                    MentorNome = p.Mentor != null ? p.Mentor.NomeCompleto : "",
                    QtdInscritos = p.Inscricoes.Count(i => i.Status == "Ativa")
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(lista));
        }

        /// Detalhe do programa + posts do fórum
        [HttpGet("programas/{id:int}")]
        public async Task<IActionResult> ObterPrograma(int id)
        {
            if (EmpresaIdDoToken is not int empresaId)
                return Forbid();

            var p = await _context.MentoriaProgramas
                .Include(x => x.Mentor)
                .Include(x => x.Inscricoes).ThenInclude(i => i.Mentorado)
                .Include(x => x.Posts.OrderByDescending(post => post.DataPublicacao))
                    .ThenInclude(post => post.Respostas.OrderBy(r => r.Data))
                .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId);

            if (p is null)
                return NotFound(ApiErrorResponse.Criar("Programa não encontrado.", "MP_404"));

            return Ok(ApiResponse<object>.Ok(p));
        }

        /// Cria programa. Mentor = qualquer usuário ativo da empresa.
        [HttpPost("programas")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> CriarPrograma([FromBody] CriarProgramaMentoriaDto request)
        {
            if (EmpresaIdDoToken is not int empresaId)
                return BadRequest(ApiErrorResponse.Criar("Empresa não identificada no token.", "MP_001"));

            var mentor = await _context.Users.FindAsync(request.MentorId);
            if (mentor is null || !mentor.Ativo || mentor.EmpresaId != empresaId)
                return BadRequest(ApiErrorResponse.Criar("Mentor inválido ou de outra empresa.", "MP_002"));

            var programa = new MentoriaPrograma
            {
                EmpresaId = empresaId,
                MentorId = request.MentorId,
                Titulo = request.Titulo,
                Objetivos = request.Objetivos,
                Status = "Ativa",
                DataInicio = DateTime.UtcNow
            };

            _context.MentoriaProgramas.Add(programa);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { programa.Id }, "Programa criado."));
        }

        /// <summary>Inscreve o usuário logado como mentorado</summary>
        [HttpPost("programas/{id:int}/inscrever")]
        public async Task<IActionResult> Inscrever(int id)
        {
            if (EmpresaIdDoToken is not int empresaId || UserId is null)
                return Forbid();

            var programa = await _context.MentoriaProgramas
                .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId && p.Status == "Ativa");

            if (programa is null)
                return NotFound(ApiErrorResponse.Criar("Programa não encontrado.", "MP_404"));

            if (programa.MentorId == UserId)
                return BadRequest(ApiErrorResponse.Criar("O mentor não pode se inscrever como mentorado.", "MP_003"));

            var existe = await _context.MentoriaInscricoes
                .AnyAsync(i => i.MentoriaProgramaId == id && i.MentoradoId == UserId);

            if (!existe)
            {
                _context.MentoriaInscricoes.Add(new MentoriaInscricao
                {
                    MentoriaProgramaId = id,
                    MentoradoId = UserId,
                    Status = "Ativa",
                    DataInscricao = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse<object>.Ok(null, "Inscrição realizada."));
        }

        ///Mentor publica no fórum
        [HttpPost("programas/{id:int}/posts")]
        public async Task<IActionResult> CriarPost(int id, [FromBody] CriarPostMentoriaDto request)
        {
            if (EmpresaIdDoToken is not int empresaId || UserId is null)
                return Forbid();

            var programa = await _context.MentoriaProgramas
                .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresaId);

            if (programa is null)
                return NotFound(ApiErrorResponse.Criar("Programa não encontrado.", "MP_404"));

            if (programa.MentorId != UserId && !PodeGerenciar)
                return Forbid();

            var post = new MentoriaPost
            {
                MentoriaProgramaId = id,
                AutorId = UserId,
                Titulo = request.Titulo,
                Conteudo = request.Conteudo,
                UrlLink = request.UrlLink,
                UrlMidia = request.UrlMidia,
                TipoMidia = request.TipoMidia ?? "Nenhuma",
                DataPublicacao = DateTime.UtcNow
            };

            _context.MentoriaPosts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { post.Id }, "Post publicado."));
        }

        ///Resposta no fórum (mentorado inscrito ou mentor)
        [HttpPost("posts/{postId:int}/respostas")]
        public async Task<IActionResult> Responder(int postId, [FromBody] PostResponseDto request)
        {
            if (EmpresaIdDoToken is not int empresaId || UserId is null)
                return Forbid();

            var post = await _context.MentoriaPosts
                .Include(p => p.Programa)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post?.Programa is null || post.Programa.EmpresaId != empresaId)
                return NotFound(ApiErrorResponse.Criar("Post não encontrado.", "MP_405"));

            var isMentor = post.Programa.MentorId == UserId;
            var isInscrito = await _context.MentoriaInscricoes.AnyAsync(i =>
                i.MentoriaProgramaId == post.MentoriaProgramaId
                && i.MentoradoId == UserId
                && i.Status == "Ativa");

            if (!isMentor && !isInscrito && !PodeGerenciar)
                return Forbid();

            if (string.IsNullOrWhiteSpace(request.Texto))
                return BadRequest(ApiErrorResponse.Criar("Texto obrigatório.", "MP_006"));

            _context.MentoriaRespostas.Add(new MentoriaResposta
            {
                MentoriaPostId = postId,
                AutorId = UserId,
                Texto = request.Texto.Trim(),
                Data = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Resposta enviada."));
        }
    }
}