using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;

namespace Valoriza.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalDenuncias = await _context.Denuncias.CountAsync();
            ViewBag.DenunciasAbertas = await _context.Denuncias.CountAsync(d => d.Status == "Aberta");
            ViewBag.TotalTrilhas = await _context.TrilhasTreinamento.CountAsync(t => t.Ativa);
            ViewBag.TotalUsuarios = await _context.Users.CountAsync(u => u.Ativo);
            return View();
        }

        public IActionResult Error() => View();
    }
}
