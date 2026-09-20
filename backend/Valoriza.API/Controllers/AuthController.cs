/* ============================================================
   AuthController – Login e geração de JWT
   Autor: Oliver Valentim Carvalho Santos - RA 2632071
   ============================================================ */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Valoriza.API.DTOs;
using Valoriza.API.Models;

namespace Valoriza.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null || !user.Ativo)
                return Unauthorized(ApiErrorResponse.Criar("Credenciais inválidas ou usuário inativo.", "AUTH_001"));

            var senhaValida = await _userManager.CheckPasswordAsync(user, request.Senha);
            if (!senhaValida)
                return Unauthorized(ApiErrorResponse.Criar("Credenciais inválidas.", "AUTH_002"));

            var roles = await _userManager.GetRolesAsync(user);
            var token = GerarToken(user, roles);

            var response = new LoginResponseDTO
            {
                Token = token,
                Expiracao = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpireHours"] ?? "8")),
                Usuario = new UsuarioResponseDTO
                {
                    Id = user.Id,
                    NomeCompleto = user.NomeCompleto,
                    Email = user.Email ?? "",
                    EmpresaId = user.EmpresaId,
                    Roles = roles.ToList(),
                    Ativo = user.Ativo,
                    DataCadastro = user.DataCadastro
                }
            };

            return Ok(ApiResponse<LoginResponseDTO>.Ok(response, "Login realizado com sucesso."));
        }

        private string GerarToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email ?? ""),
                new(ClaimTypes.Name, user.NomeCompleto),
                new("empresaId", user.EmpresaId?.ToString() ?? "")
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credenciais = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpireHours"] ?? "8")),
                signingCredentials: credenciais);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
