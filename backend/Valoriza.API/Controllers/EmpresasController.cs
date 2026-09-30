/* Endpoints REST para empresas (Swagger / mobile / integrações). */

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
    [Authorize(Roles = "AdminValoriza")]
    public class EmpresasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmpresasController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// GET /api/empresas?busca=texto
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] string? busca = null)
        {
            var query = _context.Empresas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                busca = busca.Trim();
                query = query.Where(e =>
                    e.RazaoSocial.Contains(busca) ||
                    (e.NomeFantasia != null && e.NomeFantasia.Contains(busca)) ||
                    e.Cnpj.Contains(busca) ||
                    (e.Segmento != null && e.Segmento.Contains(busca)));
            }

            var lista = await query
                .OrderBy(e => e.NomeFantasia ?? e.RazaoSocial)
                .Select(e => new
                {
                    e.Id,
                    e.RazaoSocial,
                    e.NomeFantasia,
                    e.Cnpj,
                    e.Segmento,
                    e.Plano,
                    e.QuantidadeColaboradores,
                    e.Ativa,
                    e.DataInicioAssinatura
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(lista, "Empresas listadas com sucesso."));
        }

        /// GET /api/empresas/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obter(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound(ApiErrorResponse.Criar("Empresa não encontrada.", "EMP_404"));

            return Ok(ApiResponse<object>.Ok(empresa));
        }

        /// POST /api/empresas
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] EmpresaCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RazaoSocial) || string.IsNullOrWhiteSpace(dto.Cnpj))
                return BadRequest(ApiErrorResponse.Criar("Razão social e CNPJ são obrigatórios.", "EMP_001"));

            if (await _context.Empresas.AnyAsync(e => e.Cnpj == dto.Cnpj.Trim()))
                return Conflict(ApiErrorResponse.Criar("CNPJ já cadastrado.", "EMP_002"));

            var empresa = new Empresa
            {
                RazaoSocial = dto.RazaoSocial.Trim(),
                NomeFantasia = string.IsNullOrWhiteSpace(dto.NomeFantasia)
                    ? null
                    : dto.NomeFantasia.Trim(),
                Cnpj = dto.Cnpj.Trim(),
                Segmento = dto.Segmento,
                QuantidadeColaboradores = dto.QuantidadeColaboradores,
                Plano = string.IsNullOrWhiteSpace(dto.Plano) ? "Basico" : dto.Plano,
                Ativa = true,
                DataInicioAssinatura = DateTime.UtcNow
            };

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Obter), new { id = empresa.Id },
                ApiResponse<object>.Ok(empresa, "Empresa criada com sucesso."));
        }

        /// PUT /api/empresas/{id}
        /// Salva a edição da empresa via API
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] EmpresaUpdateDto dto)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound(ApiErrorResponse.Criar("Empresa não encontrada.", "EMP_404"));

            if (string.IsNullOrWhiteSpace(dto.RazaoSocial) || string.IsNullOrWhiteSpace(dto.Cnpj))
                return BadRequest(ApiErrorResponse.Criar("Informe razão social e CNPJ.", "EMP_001"));

            // CNPJ único (exceto a própria empresa)
            if (await _context.Empresas.AnyAsync(e => e.Cnpj == dto.Cnpj.Trim() && e.Id != id))
                return Conflict(ApiErrorResponse.Criar("Já existe outra empresa com este CNPJ.", "EMP_002"));

            empresa.RazaoSocial = dto.RazaoSocial.Trim();
            empresa.NomeFantasia = string.IsNullOrWhiteSpace(dto.NomeFantasia) ? null : dto.NomeFantasia.Trim();
            empresa.Cnpj = dto.Cnpj.Trim();
            empresa.Segmento = dto.Segmento;
            empresa.QuantidadeColaboradores = dto.QuantidadeColaboradores;
            empresa.Plano = string.IsNullOrWhiteSpace(dto.Plano) ? "Basico" : dto.Plano;
            empresa.Ativa = dto.Ativa;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(empresa, "Empresa atualizada com sucesso."));
        }

        /// POST /api/empresas/{id}/desativar
        /// Desativa a empresa. Se houver denúncias, nunca exclui.
        [HttpPost("{id:int}/desativar")]
        public async Task<IActionResult> Desativar(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound(ApiErrorResponse.Criar("Empresa não encontrada.", "EMP_404"));

            empresa.Ativa = false;
            await _context.SaveChangesAsync();

            var temDenuncias = await _context.Denuncias.AnyAsync(d => d.EmpresaId == id);
            var msg = temDenuncias ? "Empresa desativada (há denúncias; exclusão não permitida)." : "Empresa desativada.";

            return Ok(ApiResponse<object>.Ok(new { empresa.Id, empresa.Ativa }, msg));
        }

        /// POST /api/empresas/{id}/ativar
        [HttpPost("{id:int}/ativar")]
        public async Task<IActionResult> Ativar(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound(ApiErrorResponse.Criar("Empresa não encontrada.", "EMP_404"));

            empresa.Ativa = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { empresa.Id, empresa.Ativa }, "Empresa reativada."));
        }

        /// DELETE /api/empresas/{id}
        /// Só exclui se NÃO houver denúncias.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound(ApiErrorResponse.Criar("Empresa não encontrada.", "EMP_404"));

            if (await _context.Denuncias.AnyAsync(d => d.EmpresaId == id))
                return BadRequest(ApiErrorResponse.Criar(
                    "Não é possível excluir: existem denúncias vinculadas.", "EMP_003"));

            try
            {
                _context.Empresas.Remove(empresa);
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Empresa excluída."));
            }
            catch
            {
                empresa.Ativa = false;
                await _context.SaveChangesAsync();
                return BadRequest(ApiErrorResponse.Criar(
                    "Não foi possível excluir por outros vínculos. Empresa desativada.", "EMP_004"));
            }
        }
    }

    /// DTO de entrada para criação de empresa via API.
    public class EmpresaCreateDto
    {
        public string RazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string? Segmento { get; set; }
        public int QuantidadeColaboradores { get; set; }
        public string? Plano { get; set; }
    }

    /// DTO de entrada para atualização de empresa via API.
    public class EmpresaUpdateDto
    {
        public string RazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string? Segmento { get; set; }
        public int QuantidadeColaboradores { get; set; }
        public string Plano { get; set; } = "Basico";
        public bool Ativa { get; set; }
    }
}
