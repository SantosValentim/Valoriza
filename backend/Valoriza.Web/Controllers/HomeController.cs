/* Dashboard – conteúdo muda conforme o papel do usuário */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = user != null
                ? await _userManager.GetRolesAsync(user)
                : new List<string>();

            ViewBag.NomeUsuario = user?.NomeCompleto ?? "Usuário";
            ViewBag.IsAdmin = roles.Contains("AdminValoriza") || roles.Contains("AdminEmpresa");
            ViewBag.IsGestor = roles.Contains("GestorDEI") || (bool)ViewBag.IsAdmin;
            ViewBag.IsColaborador = roles.Contains("Colaborador") && !(bool)ViewBag.IsGestor;

            // Contadores
            ViewBag.TotalDenuncias = await _context.Denuncias.CountAsync();
            ViewBag.DenunciasAbertas = await _context.Denuncias.CountAsync(d => d.Status == "Aberta");
            ViewBag.TotalTrilhas = await _context.TrilhasTreinamento.CountAsync(t => t.Ativa);
            ViewBag.TotalUsuarios = await _context.Users.CountAsync(u => u.Ativo);

            // Último indicador de diversidade
            ViewBag.Indicador = await _context.IndicadoresDiversidade
                .OrderByDescending(i => i.Ano)
                .ThenByDescending(i => i.Mes)
                .FirstOrDefaultAsync();

            // Trilhas para o colaborador
            ViewBag.Trilhas = await _context.TrilhasTreinamento
                .Where(t => t.Ativa)
                .OrderByDescending(t => t.DataCriacao)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}