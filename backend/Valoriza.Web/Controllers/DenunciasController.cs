using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;

namespace Valoriza.Web.Controllers
{
    [Authorize]
    public class DenunciasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DenunciasController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(string? status = null)
        {
            var query = _context.Denuncias
                .Include(d => d.Empresa)
                .Include(d => d.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(d => d.Status == status);

            var lista = await query.OrderByDescending(d => d.DataRegistro).ToListAsync();
            return View(lista);
        }

        public async Task<IActionResult> Details(int id)
        {
            var denuncia = await _context.Denuncias
                .Include(d => d.Empresa)
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (denuncia == null) return NotFound();
            return View(denuncia);
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarStatus(int id, string status, string? observacoes)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);
            if (denuncia == null) return NotFound();

            denuncia.Status = status;
            denuncia.ObservacoesInternas = observacoes;
            if (status == "Resolvida")
                denuncia.DataResolucao = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
