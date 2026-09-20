/* ============================================================
   IndicadoresController – Indicadores de diversidade
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
    public class IndicadoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IndicadoresController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> Listar(
            [FromQuery] int? empresaId = null,
            [FromQuery] int? ano = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int itensPorPagina = 12)
        {
            if (pagina < 1) pagina = 1;
            if (itensPorPagina < 1 || itensPorPagina > 50) itensPorPagina = 12;

            var query = _context.IndicadoresDiversidade.Include(i => i.Empresa).AsQueryable();

            var empresaClaim = User.FindFirst("empresaId")?.Value;
            if (!User.IsInRole("AdminValoriza") && int.TryParse(empresaClaim, out var empIdUsuario))
                query = query.Where(i => i.EmpresaId == empIdUsuario);
            else if (empresaId.HasValue)
                query = query.Where(i => i.EmpresaId == empresaId.Value);

            if (ano.HasValue)
                query = query.Where(i => i.Ano == ano.Value);

            var total = await query.CountAsync();

            var indicadores = await query
                .OrderByDescending(i => i.Ano).ThenByDescending(i => i.Mes)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .Select(i => new IndicadorResponseDTO
                {
                    Id = i.Id,
                    Ano = i.Ano,
                    Mes = i.Mes,
                    TotalColaboradores = i.TotalColaboradores,
                    ColaboradoresNegros = i.ColaboradoresNegros,
                    ColaboradoresIndigenas = i.ColaboradoresIndigenas,
                    ColaboradoresPcd = i.ColaboradoresPcd,
                    MulheresLideranca = i.MulheresLideranca,
                    NegrosLideranca = i.NegrosLideranca,
                    PercentualAdesaoTreinamentos = i.PercentualAdesaoTreinamentos,
                    TotalDenunciasPeriodo = i.TotalDenunciasPeriodo,
                    DenunciasResolvidas = i.DenunciasResolvidas
                })
                .ToListAsync();

            return Ok(PagedResponse<IndicadorResponseDTO>.Criar(indicadores, pagina, itensPorPagina, total));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var indicador = await _context.IndicadoresDiversidade.FindAsync(id);
            if (indicador is null)
                return NotFound(ApiErrorResponse.Criar("Indicador não encontrado.", "IND_404"));

            var dto = new IndicadorResponseDTO
            {
                Id = indicador.Id,
                Ano = indicador.Ano,
                Mes = indicador.Mes,
                TotalColaboradores = indicador.TotalColaboradores,
                ColaboradoresNegros = indicador.ColaboradoresNegros,
                ColaboradoresIndigenas = indicador.ColaboradoresIndigenas,
                ColaboradoresPcd = indicador.ColaboradoresPcd,
                MulheresLideranca = indicador.MulheresLideranca,
                NegrosLideranca = indicador.NegrosLideranca,
                PercentualAdesaoTreinamentos = indicador.PercentualAdesaoTreinamentos,
                TotalDenunciasPeriodo = indicador.TotalDenunciasPeriodo,
                DenunciasResolvidas = indicador.DenunciasResolvidas
            };

            return Ok(ApiResponse<IndicadorResponseDTO>.Ok(dto));
        }

        [HttpPost]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> Criar([FromBody] CriarIndicadorRequestDTO request)
        {
            var existe = await _context.IndicadoresDiversidade
                .AnyAsync(i => i.EmpresaId == request.EmpresaId && i.Ano == request.Ano && i.Mes == request.Mes);

            if (existe)
                return BadRequest(ApiErrorResponse.Criar(
                    "Já existe um indicador cadastrado para este mês/ano nesta empresa.", "IND_001"));

            var indicador = new IndicadorDiversidade
            {
                EmpresaId = request.EmpresaId,
                Ano = request.Ano,
                Mes = request.Mes,
                TotalColaboradores = request.TotalColaboradores,
                ColaboradoresNegros = request.ColaboradoresNegros,
                ColaboradoresIndigenas = request.ColaboradoresIndigenas,
                ColaboradoresPcd = request.ColaboradoresPcd,
                MulheresLideranca = request.MulheresLideranca,
                NegrosLideranca = request.NegrosLideranca,
                PercentualAdesaoTreinamentos = request.PercentualAdesaoTreinamentos,
                TotalDenunciasPeriodo = request.TotalDenunciasPeriodo,
                DenunciasResolvidas = request.DenunciasResolvidas,
                DataRegistro = DateTime.UtcNow
            };

            _context.IndicadoresDiversidade.Add(indicador);
            await _context.SaveChangesAsync();

            var dto = new IndicadorResponseDTO
            {
                Id = indicador.Id,
                Ano = indicador.Ano,
                Mes = indicador.Mes,
                TotalColaboradores = indicador.TotalColaboradores,
                ColaboradoresNegros = indicador.ColaboradoresNegros,
                ColaboradoresIndigenas = indicador.ColaboradoresIndigenas,
                ColaboradoresPcd = indicador.ColaboradoresPcd,
                MulheresLideranca = indicador.MulheresLideranca,
                NegrosLideranca = indicador.NegrosLideranca,
                PercentualAdesaoTreinamentos = indicador.PercentualAdesaoTreinamentos,
                TotalDenunciasPeriodo = indicador.TotalDenunciasPeriodo,
                DenunciasResolvidas = indicador.DenunciasResolvidas
            };

            return CreatedAtAction(nameof(ObterPorId), new { id = indicador.Id },
                ApiResponse<IndicadorResponseDTO>.Ok(dto, "Indicador cadastrado com sucesso."));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        public async Task<IActionResult> Remover(int id)
        {
            var indicador = await _context.IndicadoresDiversidade.FindAsync(id);
            if (indicador is null)
                return NotFound(ApiErrorResponse.Criar("Indicador não encontrado.", "IND_404"));

            _context.IndicadoresDiversidade.Remove(indicador);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok("Indicador removido com sucesso."));
        }
    }
}
