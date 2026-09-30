/* - Todos autenticados: criar e acompanhar as PRÓPRIAS
   - Colaborador: não vê denúncias de outros
   - Gestor/Admin: veem as da empresa (ou todas, AdminValoriza)
   - Anônima: autor ainda acompanha; gestores não veem identidade */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize]
    public class DenunciasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DenunciasController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsGestorOuAdmin =>
            User.IsInRole("AdminValoriza") ||
            User.IsInRole("AdminEmpresa") ||
            User.IsInRole("GestorDEI");

        // LISTA
        /// Colaborador: só minhas denúncias. Gestor/Admin: denúncias da empresa.
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Garante empresa Valoriza para AdminValoriza sem vínculo
            if (User.IsInRole("AdminValoriza") && user.EmpresaId == null)
            {
                var valoriza = await _context.Empresas
                    .FirstOrDefaultAsync(e =>
                        e.NomeFantasia == "Valoriza" ||
                        e.RazaoSocial.Contains("Valoriza"));
                if (valoriza != null)
                {
                    user.EmpresaId = valoriza.Id;
                    await _userManager.UpdateAsync(user);
                }
            }

            IQueryable<Denuncia> query = _context.Denuncias
                .Include(d => d.Empresa)
                .AsQueryable();

            if (!IsGestorOuAdmin)
            {
                // Colaborador: só as próprias
                query = query.Where(d => d.UsuarioId == user.Id);
            }
            else
            {
                // Admin/Gestor: própria empresa
                // Se ainda não tiver empresa, pelo menos vê as que ele criou
                if (user.EmpresaId == null)
                {
                    query = query.Where(d => d.UsuarioId == user.Id);
                }
                else
                {
                    query = query.Where(d => d.EmpresaId == user.EmpresaId);
                }
            }

            var lista = await query
                .OrderByDescending(d => d.DataRegistro)
                .ToListAsync();

            ViewBag.IsGestorOuAdmin = IsGestorOuAdmin;
            return View(lista);
        }

        // DETALHE / ACOMPANHAMENTO
        /// Autor sempre pode ver a própria (mesmo anônima). Gestor/Admin: conforme empresa.
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var denuncia = await _context.Denuncias
                .Include(d => d.Empresa)
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (denuncia == null) return NotFound();

            var isAutor = denuncia.UsuarioId == user.Id;

            if (!isAutor)
            {
                if (!IsGestorOuAdmin)
                    return Forbid(); // colaborador não vê denúncia alheia

                if (!User.IsInRole("AdminValoriza") &&
                    user.EmpresaId != null &&
                    denuncia.EmpresaId != user.EmpresaId)
                    return Forbid();
            }

            ViewBag.IsGestorOuAdmin = IsGestorOuAdmin;
            ViewBag.IsAutor = isAutor;
            // Gestores não veem identidade se anônima
            ViewBag.OcultarAutor = denuncia.Anonima && !isAutor;

            return View(denuncia);
        }

        // CRIAR (todos)
        /// Formulário de nova denúncia
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        /// Salva a denúncia e redireciona para o acompanhamento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string titulo,
            string descricao,
            string? categoria,
            bool anonima)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Validação
            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(descricao))
            {
                ViewBag.Erro = "Informe título e descrição.";
                return View();
            }

            if (descricao.Trim().Length < 20)
            {
                ViewBag.Erro = "A descrição deve ter pelo menos 20 caracteres.";
                return View();
            }

            // Empresa do usuário
            var empresaId = user.EmpresaId;

            if (empresaId == null)
            {
                var valoriza = await _context.Empresas
                    .FirstOrDefaultAsync(e =>
                        e.NomeFantasia == "Valoriza" ||
                        e.RazaoSocial.Contains("Valoriza"));

                if (valoriza != null)
                {
                    empresaId = valoriza.Id;

                    // Vincula o usuário à Valoriza para as próximas vezes
                    if (User.IsInRole("AdminValoriza"))
                    {
                        user.EmpresaId = valoriza.Id;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }

            if (empresaId == null)
            {
                ViewBag.Erro = "Seu usuário não está vinculado a uma empresa. Contate o administrador.";
                return View();
            }

            // Protocolo único
            var protocolo =
                $"DEN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            var denuncia = new Denuncia
            {
                Protocolo = protocolo,
                Titulo = titulo.Trim(),
                Descricao = descricao.Trim(),
                Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim(),
                Status = "Aberta",
                Anonima = anonima,
                // Sempre grava o autor (mesmo anônima) para ele acompanhar
                UsuarioId = user.Id,
                EmpresaId = empresaId.Value,
                DataRegistro = DateTime.UtcNow
            };

            _context.Denuncias.Add(denuncia);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] =
                $"Denúncia registrada. Protocolo: {protocolo}. Acompanhe o andamento em Denúncias.";

            return RedirectToAction(nameof(Details), new { id = denuncia.Id });
        }

        // ATUALIZAR STATUS (só gestor/admin).
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarStatus(int id, string status, string? observacoes)
        {
            var user = await _userManager.GetUserAsync(User);
            var denuncia = await _context.Denuncias.FindAsync(id);
            if (denuncia == null) return NotFound();

            if (!User.IsInRole("AdminValoriza") &&
                user?.EmpresaId != null &&
                denuncia.EmpresaId != user.EmpresaId)
                return Forbid();

            denuncia.Status = status;
            if (!string.IsNullOrWhiteSpace(observacoes))
                denuncia.Observacoes = observacoes.Trim();

            // Se o seu modelo tiver DataAtualizacao:
            // denuncia.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Status atualizado.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}