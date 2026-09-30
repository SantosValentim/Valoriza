/* Comunicação inclusiva – gestores (GestorDEI, AdminEmpresa)
   Orientações de consulta + checklists marcáveis */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
    public class ComunicacaoInclusivaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ComunicacaoInclusivaController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// Painel: orientações + checklist da semana
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var inicioSemana = InicioSemana(DateTime.UtcNow.Date);

            var orientacoes = await _context.OrientacoesInclusivas
                .Where(o => o.Ativa)
                .OrderBy(o => o.Ordem)
                .ToListAsync();

            var itens = await _context.ChecklistInclusivoItens
                .Where(i => i.Ativo)
                .OrderBy(i => i.Contexto)
                .ThenBy(i => i.Ordem)
                .ToListAsync();

            var concluidos = await _context.ChecklistInclusivoConclusoes
                .Where(c => c.UsuarioId == userId && c.DataReferencia == inicioSemana)
                .Select(c => c.ItemId)
                .ToListAsync();

            ViewBag.InicioSemana = inicioSemana;
            ViewBag.ConcluidosIds = concluidos.ToHashSet();
            ViewBag.Orientacoes = orientacoes;
            return View(itens);
        }

        /// Marca / desmarca item do checklist da semana
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleChecklist(int itemId)
        {
            var userId = _userManager.GetUserId(User)!;
            var inicioSemana = InicioSemana(DateTime.UtcNow.Date);

            var existente = await _context.ChecklistInclusivoConclusoes
                .FirstOrDefaultAsync(c =>
                    c.ItemId == itemId &&
                    c.UsuarioId == userId &&
                    c.DataReferencia == inicioSemana);

            if (existente != null)
                _context.ChecklistInclusivoConclusoes.Remove(existente);
            else
            {
                _context.ChecklistInclusivoConclusoes.Add(new ChecklistInclusivoConclusao
                {
                    ItemId = itemId,
                    UsuarioId = userId,
                    DataReferencia = inicioSemana,
                    DataRegistro = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private static DateTime InicioSemana(DateTime data)
        {
            // Segunda-feira como início
            int diff = (7 + (data.DayOfWeek - DayOfWeek.Monday)) % 7;
            return data.AddDays(-diff).Date;
        }
    }
}