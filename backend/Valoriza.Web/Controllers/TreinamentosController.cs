using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize]
    public class TreinamentosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TreinamentosController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var trilhas = await _context.TrilhasTreinamento
                .Include(t => t.Conteudos)
                .OrderBy(t => t.Titulo)
                .ToListAsync();
            return View(trilhas);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(string titulo, string? descricao, int cargaHoraria, string nivel, int empresaId)
        {
            var trilha = new TrilhaTreinamento
            {
                Titulo = titulo,
                Descricao = descricao,
                CargaHoraria = cargaHoraria,
                Nivel = nivel,
                EmpresaId = empresaId,
                Ativa = true,
                DataCriacao = DateTime.UtcNow
            };

            _context.TrilhasTreinamento.Add(trilha);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
