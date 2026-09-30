/* Visível apenas para AdminValoriza.
   Acompanhar empresas clientes ativas, inadimplentes e canceladas,
   taxa de churn e receita em risco. */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize(Roles = "AdminValoriza")]
    public class ChurnController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChurnController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// Painel de churn: métricas + lista de empresas.
        public async Task<IActionResult> Index(string? status, string? busca)
        {
            var query = _context.Empresas.AsQueryable();

            // Filtro por status de assinatura
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.StatusAssinatura == status);

            if (!string.IsNullOrWhiteSpace(busca))
            {
                query = query.Where(e =>
                    e.RazaoSocial.Contains(busca) ||
                    (e.NomeFantasia != null && e.NomeFantasia.Contains(busca)) ||
                    e.Cnpj.Contains(busca));
            }

            var empresas = await query
                .OrderByDescending(e => e.DataChurn.HasValue)
                .ThenBy(e => e.NomeFantasia ?? e.RazaoSocial)
                .ToListAsync();

            // Métricas
            var todas = await _context.Empresas.ToListAsync();
            var ativas = todas.Count(e => e.StatusAssinatura == "Ativa" && e.Ativa);
            var churned = todas.Count(e => e.StatusAssinatura == "Churn" || e.DataChurn != null);
            var inadimplentes = todas.Count(e => e.StatusAssinatura == "Inadimplente");

            // Churn rate no mês corrente
            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var churnMes = todas.Count(e =>
                e.DataChurn != null && e.DataChurn >= inicioMes);

            // Base no início do mês ≈ ativas agora + churn do mês
            var baseInicioMes = ativas + churnMes;
            var taxaChurnMes = baseInicioMes > 0
                ? Math.Round(100.0 * churnMes / baseInicioMes, 1)
                : 0;

            // MRR aproximado (só ativas com ciclo mensal; anual / 12)
            decimal mrr = 0;
            foreach (var e in todas.Where(x => x.StatusAssinatura == "Ativa" && x.Ativa))
            {
                if (string.Equals(e.CicloCobranca, "Anual", StringComparison.OrdinalIgnoreCase))
                    mrr += e.ValorAssinatura / 12m;
                else
                    mrr += e.ValorAssinatura;
            }

            // Receita perdida no mês (churn do mês)
            decimal receitaPerdidaMes = todas
                .Where(e => e.DataChurn != null && e.DataChurn >= inicioMes)
                .Sum(e =>
                    string.Equals(e.CicloCobranca, "Anual", StringComparison.OrdinalIgnoreCase)
                        ? e.ValorAssinatura / 12m
                        : e.ValorAssinatura);

            ViewBag.TotalEmpresas = todas.Count;
            ViewBag.Ativas = ativas;
            ViewBag.Churned = churned;
            ViewBag.Inadimplentes = inadimplentes;
            ViewBag.ChurnMes = churnMes;
            ViewBag.TaxaChurnMes = taxaChurnMes;
            ViewBag.Mrr = mrr;
            ViewBag.ReceitaPerdidaMes = receitaPerdidaMes;
            ViewBag.FiltroStatus = status;
            ViewBag.FiltroBusca = busca;

            return View(empresas);
        }

        /// Formulário para registrar churn de uma empresa.
        [HttpGet]
        public async Task<IActionResult> Registrar(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) return NotFound();
            return View(empresa);
        }

        /// Confirma churn: marca empresa, data e motivo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(int id, string? motivoChurn)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) return NotFound();

            empresa.StatusAssinatura = "Churn";
            empresa.DataChurn = DateTime.UtcNow;
            empresa.MotivoChurn = string.IsNullOrWhiteSpace(motivoChurn)
                ? null
                : motivoChurn.Trim();
            empresa.Ativa = false; // deixa de contar como cliente ativo

            await _context.SaveChangesAsync();
            TempData["Sucesso"] = $"Churn registrado: {empresa.NomeFantasia ?? empresa.RazaoSocial}.";
            return RedirectToAction(nameof(Index));
        }

        /// Reativa empresa (win-back).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reativar(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) return NotFound();

            empresa.StatusAssinatura = "Ativa";
            empresa.DataChurn = null;
            empresa.MotivoChurn = null;
            empresa.Ativa = true;
            empresa.DataInicioAssinatura ??= DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Sucesso"] = $"Empresa reativada: {empresa.NomeFantasia ?? empresa.RazaoSocial}.";
            return RedirectToAction(nameof(Index));
        }

        /// Marca como inadimplente (risco de churn).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarInadimplente(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) return NotFound();

            empresa.StatusAssinatura = "Inadimplente";
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Empresa marcada como inadimplente.";
            return RedirectToAction(nameof(Index));
        }
    }
}