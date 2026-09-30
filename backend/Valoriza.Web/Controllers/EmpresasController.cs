/* Painel de Empresas exclusivo para AdminValoriza
   Lista, cadastra e altera situação/plano/setor + filtro com autocomplete.
   Id é sempre gerado pelo banco (identity).
   Ao mudar o Plano, ValorAssinatura é recalculado (fixo por plano) */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize(Roles = "AdminValoriza")]
    public class EmpresasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmpresasController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// Valor mensal fixo conforme o plano.Basico R$ 499 | Profissional R$ 1.299 | Empresarial R$ 2.999
        public static decimal ValorPorPlano(string? plano) => plano switch
        {
            "Basico" or "Básico" => 499.00m,
            "Profissional" => 1299.00m,
            "Empresarial" => 2999.00m,
            _ => 499.00m
        };

        // LISTA
        public async Task<IActionResult> Index(string? nome, string? situacao)
        {
            var q = _context.Empresas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                q = q.Where(e =>
                    e.RazaoSocial.Contains(nome) ||
                    (e.NomeFantasia != null && e.NomeFantasia.Contains(nome)) ||
                    e.Cnpj.Contains(nome));
            }

            if (situacao == "Ativa")
                q = q.Where(e => e.Ativa);
            else if (situacao == "Inativa")
                q = q.Where(e => !e.Ativa);

            ViewBag.Sugestoes = await _context.Empresas
                .Select(e => e.NomeFantasia ?? e.RazaoSocial)
                .Distinct()
                .OrderBy(n => n)
                .Take(200)
                .ToListAsync();

            ViewBag.FiltroNome = nome;
            ViewBag.FiltroSituacao = situacao;

            var lista = await q.OrderBy(e => e.RazaoSocial).ToListAsync();
            return View(lista);
        }

        // EDITAR
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound();

            return View(empresa);
        }

        /// Salva dados da empresa. Sempre recalcula ValorAssinatura pelo plano (Churn usa esse valor).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string razaoSocial,
            string? nomeFantasia,
            string cnpj,
            string? segmento,
            int quantidadeColaboradores,
            string plano,
            string cicloCobranca,
            bool ativa)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(razaoSocial) || string.IsNullOrWhiteSpace(cnpj))
            {
                ViewBag.Erro = "Razão social e CNPJ são obrigatórios.";
                return View(empresa);
            }

            empresa.RazaoSocial = razaoSocial.Trim();
            empresa.NomeFantasia = string.IsNullOrWhiteSpace(nomeFantasia)
                ? null
                : nomeFantasia.Trim();
            empresa.Cnpj = cnpj.Trim();
            empresa.Segmento = segmento;
            empresa.QuantidadeColaboradores = quantidadeColaboradores;
            empresa.Plano = plano;
            empresa.CicloCobranca = string.IsNullOrWhiteSpace(cicloCobranca)
                ? "Mensal"
                : cicloCobranca;
            empresa.Ativa = ativa;

            // Valor fixo pelo plano → refletido no painel de Churn
            empresa.ValorAssinatura = ValorPorPlano(plano);

            // Se estava em churn e voltou a ficar ativa no sistema
            if (ativa && empresa.StatusAssinatura == "Churn")
            {
                empresa.StatusAssinatura = "Ativa";
                empresa.DataChurn = null;
                empresa.MotivoChurn = null;
                empresa.DataInicioAssinatura ??= DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Empresa atualizada. Valor da assinatura ajustado ao plano.";
            return RedirectToAction(nameof(Index));
        }
    }
}