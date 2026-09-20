/* ============================================================
   UsuariosController
   ============================================================ */

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsuariosController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> Listar(
            [FromQuery] int? empresaId = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int itensPorPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (itensPorPagina < 1 || itensPorPagina > 50) itensPorPagina = 10;

            var query = _userManager.Users.Include(u => u.Empresa).AsQueryable();

            var empresaClaim = User.FindFirst("empresaId")?.Value;
            if (!User.IsInRole("AdminValoriza") && int.TryParse(empresaClaim, out var empIdUsuario))
                query = query.Where(u => u.EmpresaId == empIdUsuario);
            else if (empresaId.HasValue)
                query = query.Where(u => u.EmpresaId == empresaId.Value);

            var total = await query.CountAsync();
            var usuarios = await query.OrderBy(u => u.NomeCompleto)
                .Skip((pagina - 1) * itensPorPagina).Take(itensPorPagina).ToListAsync();

            var resultado = new List<UsuarioResponseDTO>();
            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                resultado.Add(new UsuarioResponseDTO
                {
                    Id = u.Id,
                    NomeCompleto = u.NomeCompleto,
                    Email = u.Email ?? "",
                    EmpresaId = u.EmpresaId,
                    EmpresaNome = u.Empresa?.NomeFantasia,
                    Roles = roles.ToList(),
                    Ativo = u.Ativo,
                    DataCadastro = u.DataCadastro
                });
            }

            return Ok(PagedResponse<UsuarioResponseDTO>.Criar(resultado, pagina, itensPorPagina, total));
        }

        [HttpGet("me")]
        public async Task<IActionResult> MeuPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.Users.Include(u => u.Empresa)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
                return NotFound(ApiErrorResponse.Criar("Usuário não encontrado.", "USR_404"));

            var roles = await _userManager.GetRolesAsync(user);
            var dto = new UsuarioResponseDTO
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email ?? "",
                EmpresaId = user.EmpresaId,
                EmpresaNome = user.Empresa?.NomeFantasia,
                Roles = roles.ToList(),
                Ativo = user.Ativo,
                DataCadastro = user.DataCadastro
            };

            return Ok(ApiResponse<UsuarioResponseDTO>.Ok(dto));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        public async Task<IActionResult> ObterPorId(string id)
        {
            var user = await _userManager.Users.Include(u => u.Empresa)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
                return NotFound(ApiErrorResponse.Criar("Usuário não encontrado.", "USR_404"));

            var roles = await _userManager.GetRolesAsync(user);
            var dto = new UsuarioResponseDTO
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email ?? "",
                EmpresaId = user.EmpresaId,
                EmpresaNome = user.Empresa?.NomeFantasia,
                Roles = roles.ToList(),
                Ativo = user.Ativo,
                DataCadastro = user.DataCadastro
            };

            return Ok(ApiResponse<UsuarioResponseDTO>.Ok(dto));
        }

        [HttpPost]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        public async Task<IActionResult> Criar([FromBody] CriarUsuarioRequestDTO request)
        {
            var existente = await _userManager.FindByEmailAsync(request.Email);
            if (existente is not null)
                return BadRequest(ApiErrorResponse.Criar("Já existe um usuário com este e-mail.", "USR_001"));

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                NomeCompleto = request.NomeCompleto,
                Cpf = request.Cpf,
                EmpresaId = request.EmpresaId,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Senha);
            if (!result.Succeeded)
            {
                var erros = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(ApiErrorResponse.Criar("Erro ao criar usuário.", "USR_002",
                    errosValidacao: new Dictionary<string, string[]> { { "Senha", erros } }));
            }

            await _userManager.AddToRoleAsync(user, request.Role);

            var dto = new UsuarioResponseDTO
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email ?? "",
                EmpresaId = user.EmpresaId,
                Roles = new List<string> { request.Role },
                Ativo = user.Ativo,
                DataCadastro = user.DataCadastro
            };

            return CreatedAtAction(nameof(ObterPorId), new { id = user.Id },
                ApiResponse<UsuarioResponseDTO>.Ok(dto, "Usuário criado com sucesso."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        public async Task<IActionResult> Atualizar(string id, [FromBody] AtualizarUsuarioRequestDTO request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound(ApiErrorResponse.Criar("Usuário não encontrado.", "USR_404"));

            if (!string.IsNullOrWhiteSpace(request.NomeCompleto))
                user.NomeCompleto = request.NomeCompleto;
            if (request.Cpf is not null) user.Cpf = request.Cpf;
            if (request.Ativo.HasValue) user.Ativo = request.Ativo.Value;
            if (request.EmpresaId.HasValue) user.EmpresaId = request.EmpresaId.Value;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(ApiErrorResponse.Criar("Erro ao atualizar usuário.", "USR_003"));

            return Ok(ApiResponse<object>.Ok("Usuário atualizado com sucesso."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        public async Task<IActionResult> Desativar(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound(ApiErrorResponse.Criar("Usuário não encontrado.", "USR_404"));

            user.Ativo = false;
            await _userManager.UpdateAsync(user);
            return Ok(ApiResponse<object>.Ok("Usuário desativado com sucesso."));
        }
    }
}
