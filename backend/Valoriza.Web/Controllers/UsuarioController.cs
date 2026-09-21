/* Cadastro e gestão de usuários (somente Admin) */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuariosController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();

            var lista = new List<(ApplicationUser User, IList<string> Roles)>();
            foreach (var u in users)
                lista.Add((u, await _userManager.GetRolesAsync(u)));

            return View(lista);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(
            string nomeCompleto, string email, string senha,
            string role, int? empresaId, string? cpf)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.Erro = "Preencha nome, e-mail e senha.";
                return View();
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ViewBag.Erro = "Já existe um usuário com este e-mail.";
                return View();
            }

            // AdminEmpresa não pode criar AdminValoriza
            if (!User.IsInRole("AdminValoriza") && role == "AdminValoriza")
            {
                ViewBag.Erro = "Sem permissão para criar este tipo de usuário.";
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NomeCompleto = nomeCompleto,
                Cpf = cpf,
                EmpresaId = empresaId,
                Ativo = true,
                EmailConfirmed = true,
                DataCadastro = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, senha);
            if (!result.Succeeded)
            {
                ViewBag.Erro = string.Join(" ", result.Errors.Select(e => e.Description));
                return View();
            }

            await _userManager.AddToRoleAsync(user, string.IsNullOrWhiteSpace(role) ? "Colaborador" : role);
            TempData["Sucesso"] = $"Usuário {nomeCompleto} criado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Desativar(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Ativo = false;
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}