/* VALORIZA Web – AccountController
   Login e logout do painel administrativo (cookie Identity) */

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    public class AccountController : Controller
    {
        // Gerencia o login por cookie (sessão do painel Web)
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Gerencia usuários (busca, criação, papéis etc.)
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>  Exibe a tela de login.
        /// returnUrl: página para redirecionar após o login (opcional). </summary>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary> Processa o login com e-mail e senha.
        /// Valida usuário ativo e autentica via Identity. </summary>
        [HttpPost]
        public async Task<IActionResult> Login(string email, string senha, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Validação básica dos campos
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.Erro = "Informe e-mail e senha.";
                return View();
            }

            // Busca o usuário pelo e-mail
            var user = await _userManager.FindByEmailAsync(email);

            // Bloqueia login se não existir ou estiver inativo
            if (user == null || !user.Ativo)
            {
                ViewBag.Erro = "Credenciais inválidas ou usuário inativo.";
                return View();
            }

            // Tenta autenticar (isPersistent: true = mantém cookie)
            var result = await _signInManager.PasswordSignInAsync(user, senha, true, false);

            if (result.Succeeded)
            {
                // Redireciona para a URL original, se for local e válida
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                // Padrão: vai para o Dashboard
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Erro = "Credenciais inválidas.";
            return View();
        }

        /// Encerra a sessão do usuário e volta para a tela de login.
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}