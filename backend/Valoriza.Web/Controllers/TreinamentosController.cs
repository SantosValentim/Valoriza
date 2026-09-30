/* Trilhas e conteúdos (Texto, Vídeo, Quiz).
   Editar/excluir: AdminValoriza, AdminEmpresa, GestorDEI
   Demais autenticados: listar, abrir e concluir conteúdos */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;
using Valoriza.API.Services;

namespace Valoriza.Web.Controllers
{
    [Authorize]
    public class TreinamentosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ProgressoService? _progresso;

        public TreinamentosController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ProgressoService? progresso = null)
        {
            _context = context;
            _userManager = userManager;
            _progresso = progresso;
        }

        /// Admin ou Gestor DEI podem editar trilhas/conteúdos
        private bool PodeEditar =>
            User.IsInRole("AdminValoriza") ||
            User.IsInRole("AdminEmpresa") ||
            User.IsInRole("GestorDEI");

        ///Lista todas as trilhas
        public async Task<IActionResult> Index()
        {
            var trilhas = await _context.TrilhasTreinamento
                .Include(t => t.Conteudos)
                .OrderBy(t => t.Titulo)
                .ToListAsync();

            ViewBag.PodeEditar = PodeEditar;
            return View(trilhas);
        }

        /// Abre a trilha, conteúdos e progresso do usuário
        public async Task<IActionResult> Details(int id)
        {
            var trilha = await _context.TrilhasTreinamento
                .Include(t => t.Conteudos.OrderBy(c => c.Ordem))
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trilha == null)
                return NotFound();

            ViewBag.PodeEditar = PodeEditar;

            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId) && _progresso != null)
            {
                ViewBag.Progresso = await _progresso.ObterProgressoAsync(userId, id);
                ViewBag.ConcluidosIds = await _progresso.ConteudosConcluidosIdsAsync(userId, id);
            }
            else
            {
                ViewBag.Progresso = null;
                ViewBag.ConcluidosIds = new HashSet<int>();
            }

            return View(trilha);
        }

        //CRIAR TRILHA 
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpGet]
        public IActionResult Create() => View();

        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string titulo, string? descricao, int cargaHoraria, string nivel, int empresaId)
        {
            var user = await _userManager.GetUserAsync(User);
            // Preferência: empresa do admin logado
            if (user?.EmpresaId != null && empresaId <= 0)
                empresaId = user.EmpresaId.Value;
            if (empresaId <= 0)
                empresaId = 1;

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
            return RedirectToAction(nameof(Details), new { id = trilha.Id });
        }

        //EDITAR TRILHA
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var trilha = await _context.TrilhasTreinamento.FindAsync(id);
            if (trilha == null) return NotFound();
            return View(trilha);
        }

        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, string titulo, string? descricao, int cargaHoraria, string nivel, bool ativa)
        {
            var trilha = await _context.TrilhasTreinamento.FindAsync(id);
            if (trilha == null) return NotFound();

            trilha.Titulo = titulo;
            trilha.Descricao = descricao;
            trilha.CargaHoraria = cargaHoraria;
            trilha.Nivel = nivel;
            trilha.Ativa = ativa;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        // EXCLUIR TRILHA
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var trilha = await _context.TrilhasTreinamento
                .Include(t => t.Conteudos)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trilha == null) return NotFound();

            _context.Conteudos.RemoveRange(trilha.Conteudos);
            _context.TrilhasTreinamento.Remove(trilha);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // CONTEÚDOS
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpGet]
        public async Task<IActionResult> AddConteudo(int trilhaId)
        {
            var trilha = await _context.TrilhasTreinamento.FindAsync(trilhaId);
            if (trilha == null) return NotFound();

            ViewBag.TrilhaId = trilhaId;
            ViewBag.TrilhaTitulo = trilha.Titulo;
            return View();
        }

        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConteudo(
            int trilhaId, string titulo, string? texto, string? urlVideo,
            string tipo, int ordem, int duracaoMinutos,
            string? opcaoA, string? opcaoB, string? opcaoC, string? opcaoD,
            string? respostaCorreta)
        {
            if (await _context.TrilhasTreinamento.FindAsync(trilhaId) == null)
                return NotFound();

            if (string.Equals(tipo, "Quiz", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(opcaoA) || string.IsNullOrWhiteSpace(opcaoB) ||
                    string.IsNullOrWhiteSpace(opcaoC) || string.IsNullOrWhiteSpace(opcaoD))
                {
                    ViewBag.Erro = "Preencha as quatro opções do quiz.";
                    ViewBag.TrilhaId = trilhaId;
                    ViewBag.TrilhaTitulo = (await _context.TrilhasTreinamento.FindAsync(trilhaId))?.Titulo;
                    return View();
                }

                var resp = (respostaCorreta ?? "").Trim().ToUpperInvariant();
                if (resp is not ("A" or "B" or "C" or "D"))
                {
                    ViewBag.Erro = "Selecione a resposta correta (A, B, C ou D).";
                    ViewBag.TrilhaId = trilhaId;
                    ViewBag.TrilhaTitulo = (await _context.TrilhasTreinamento.FindAsync(trilhaId))?.Titulo;
                    return View();
                }
                respostaCorreta = resp;
            }
            else
            {
                opcaoA = opcaoB = opcaoC = opcaoD = respostaCorreta = null;
            }

            _context.Conteudos.Add(new Conteudo
            {
                TrilhaId = trilhaId,
                Titulo = titulo,
                Texto = texto,
                UrlVideo = urlVideo,
                Tipo = tipo,
                Ordem = ordem,
                DuracaoMinutos = duracaoMinutos,
                OpcaoA = opcaoA,
                OpcaoB = opcaoB,
                OpcaoC = opcaoC,
                OpcaoD = opcaoD,
                RespostaCorreta = respostaCorreta
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = trilhaId });
        }

        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpGet]
        public async Task<IActionResult> EditConteudo(int id)
        {
            var conteudo = await _context.Conteudos.FindAsync(id);
            if (conteudo == null) return NotFound();
            return View(conteudo);
        }

        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConteudo(
            int id, string titulo, string? texto, string? urlVideo,
            string tipo, int ordem, int duracaoMinutos,
            string? opcaoA, string? opcaoB, string? opcaoC, string? opcaoD,
            string? respostaCorreta)
        {
            var conteudo = await _context.Conteudos.FindAsync(id);
            if (conteudo == null) return NotFound();

            if (string.Equals(tipo, "Quiz", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(opcaoA) || string.IsNullOrWhiteSpace(opcaoB) ||
                    string.IsNullOrWhiteSpace(opcaoC) || string.IsNullOrWhiteSpace(opcaoD))
                {
                    ViewBag.Erro = "Preencha as quatro opções do quiz.";
                    return View(conteudo);
                }

                var resp = (respostaCorreta ?? "").Trim().ToUpperInvariant();
                if (resp is not ("A" or "B" or "C" or "D"))
                {
                    ViewBag.Erro = "Selecione a resposta correta (A, B, C ou D).";
                    return View(conteudo);
                }
                respostaCorreta = resp;
            }
            else
            {
                opcaoA = opcaoB = opcaoC = opcaoD = respostaCorreta = null;
            }

            conteudo.Titulo = titulo;
            conteudo.Texto = texto;
            conteudo.UrlVideo = urlVideo;
            conteudo.Tipo = tipo;
            conteudo.Ordem = ordem;
            conteudo.DuracaoMinutos = duracaoMinutos;
            conteudo.OpcaoA = opcaoA;
            conteudo.OpcaoB = opcaoB;
            conteudo.OpcaoC = opcaoC;
            conteudo.OpcaoD = opcaoD;
            conteudo.RespostaCorreta = respostaCorreta;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = conteudo.TrilhaId });
        }

        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConteudo(int id)
        {
            var conteudo = await _context.Conteudos.FindAsync(id);
            if (conteudo == null) return NotFound();

            var trilhaId = conteudo.TrilhaId;
            _context.Conteudos.Remove(conteudo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = trilhaId });
        }

        /// Marca conteúdo como concluído (e quiz, se houver)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConcluirConteudo(
            int conteudoId, int trilhaId, string? respostaQuiz)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            if (_progresso != null)
                await _progresso.ConcluirConteudoAsync(userId, conteudoId, respostaQuiz);

            return RedirectToAction(nameof(Details), new { id = trilhaId });
        }
    }
}