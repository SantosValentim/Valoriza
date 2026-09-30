/* Login, logout, perfil (só dados) e alterar senha (tela separada).
   Administradores NÃO alteram senha de terceiros. */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // LOGIN

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.Erro = "Informe e-mail e senha.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !user.Ativo)
            {
                ViewBag.Erro = "Credenciais inválidas ou usuário inativo.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, senha, true, false);
            if (!result.Succeeded)
            {
                ViewBag.Erro = "Credenciais inválidas.";
                return View();
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Colaborador -> Treinamentos; Admin/Gestor -> Dashboard
            var roles = await _userManager.GetRolesAsync(user);
            var isGestorOuAdmin =
                roles.Contains("AdminValoriza") ||
                roles.Contains("AdminEmpresa") ||
                roles.Contains("GestorDEI");

            if (!isGestorOuAdmin)
                return RedirectToAction("Index", "Treinamentos");

            return RedirectToAction("Index", "Home");
        }

        // LOGOUT

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }


        /// Painel do usuário: nome social, gênero, etnia.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            ViewBag.Roles = string.Join(", ", await _userManager.GetRolesAsync(user));
            return View(user);
        }

        /// Salva alterações do perfil.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(string? nomeSocial, string? genero, string? etnia)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            // Nome social vazio -> volta a usar NomeCompleto no painel
            user.NomeSocial = string.IsNullOrWhiteSpace(nomeSocial)
                ? null
                : nomeSocial.Trim();

            if (!string.IsNullOrWhiteSpace(genero))
                user.Genero = genero;

            if (!string.IsNullOrWhiteSpace(etnia))
                user.Etnia = etnia;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ViewBag.Erro = string.Join(" ", result.Errors.Select(e => e.Description));
                ViewBag.Roles = string.Join(", ", await _userManager.GetRolesAsync(user));
                return View(user);
            }

            TempData["Sucesso"] = "Perfil atualizado.";
            return RedirectToAction(nameof(Perfil));
        }

        //ALTERAR SENHA

        /// Exibe formulário exclusivo de troca de senha.
        [Authorize]
        [HttpGet]
        public IActionResult AlterarSenha()
        {
            return View();
        }

        /// Processa a troca de senha.
        /// Admins não usam este fluxo para terceiros.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(
            string senhaAtual,
            string novaSenha,
            string confirmarSenha)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction(nameof(Login));

            if (string.IsNullOrWhiteSpace(senhaAtual) || string.IsNullOrWhiteSpace(novaSenha))
            {
                TempData["Erro"] = "Preencha a senha atual e a nova senha.";
                return View();
            }

            if (novaSenha != confirmarSenha)
            {
                TempData["Erro"] = "A confirmação não confere com a nova senha.";
                return View();
            }

            if (novaSenha.Length < 6)
            {
                TempData["Erro"] = "A nova senha deve ter pelo menos 6 caracteres.";
                return View();
            }

            var result = await _userManager.ChangePasswordAsync(user, senhaAtual, novaSenha);
            if (!result.Succeeded)
            {
                TempData["Erro"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return View();
            }

            // Atualiza o cookie de autenticação
            await _signInManager.RefreshSignInAsync(user);

            TempData["Sucesso"] = "Senha alterada com sucesso.";
            return RedirectToAction(nameof(Perfil));
        }
    }
}