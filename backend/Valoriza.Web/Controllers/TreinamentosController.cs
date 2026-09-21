/* Lista, abre, cria, edita e exclui trilhas e conteúdos.
   AdminValoriza / AdminEmpresa: CRUD completo
   Demais usuários autenticados: listar e abrir trilha */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize] // precisa estar logado
    public class TreinamentosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TreinamentosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Verifica se o usuário logado é administrador
        private bool IsAdmin =>
            User.IsInRole("AdminValoriza") || User.IsInRole("AdminEmpresa");

        // LISTA DE TRILHAS
        public async Task<IActionResult> Index()
        {
            var trilhas = await _context.TrilhasTreinamento
                .Include(t => t.Conteudos) // para contar conteúdos
                .OrderBy(t => t.Titulo)
                .ToListAsync();

            ViewBag.IsAdmin = IsAdmin; // a View usa isso para mostrar botões
            return View(trilhas);
        }

        // ABRIR TRILHA (ver conteúdos)
        public async Task<IActionResult> Details(int id)
        {
            var trilha = await _context.TrilhasTreinamento
                .Include(t => t.Conteudos.OrderBy(c => c.Ordem))
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trilha == null)
                return NotFound();

            ViewBag.IsAdmin = IsAdmin;
            return View(trilha);
        }

        // CRIAR TRILHA (só Admin)
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpGet]
        public IActionResult Create() => View();

        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpPost]
        public async Task<IActionResult> Create(
            string titulo, string? descricao, int cargaHoraria, string nivel, int empresaId)
        {
            var trilha = new TrilhaTreinamento
            {
                Titulo = titulo,
                Descricao = descricao,
                CargaHoraria = cargaHoraria,
                Nivel = nivel,
                EmpresaId = empresaId > 0 ? empresaId : 1,
                Ativa = true,
                DataCriacao = DateTime.UtcNow
            };

            _context.TrilhasTreinamento.Add(trilha);
            await _context.SaveChangesAsync();

            // Abre a trilha recém-criada
            return RedirectToAction(nameof(Details), new { id = trilha.Id });
        }

        // EDITAR TRILHA (só Admin)
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var trilha = await _context.TrilhasTreinamento.FindAsync(id);
            if (trilha == null) return NotFound();
            return View(trilha);
        }

        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpPost]
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

        // EXCLUIR TRILHA (só Admin)
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var trilha = await _context.TrilhasTreinamento
                .Include(t => t.Conteudos)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trilha == null) return NotFound();

            // Remove conteúdos e depois a trilha
            _context.Conteudos.RemoveRange(trilha.Conteudos);
            _context.TrilhasTreinamento.Remove(trilha);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Formulário para adicionar conteúdo
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpGet]
        public async Task<IActionResult> AddConteudo(int trilhaId)
        {
            var trilha = await _context.TrilhasTreinamento.FindAsync(trilhaId);
            if (trilha == null) return NotFound();

            ViewBag.TrilhaId = trilhaId;
            ViewBag.TrilhaTitulo = trilha.Titulo;
            return View();
        }

        // Salva novo conteúdo
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpPost]
        public async Task<IActionResult> AddConteudo(
            int trilhaId, string titulo, string? texto, string? urlVideo,
            string tipo, int ordem, int duracaoMinutos)
        {
            if (await _context.TrilhasTreinamento.FindAsync(trilhaId) == null)
                return NotFound();

            _context.Conteudos.Add(new Conteudo
            {
                TrilhaId = trilhaId,
                Titulo = titulo,
                Texto = texto,
                UrlVideo = urlVideo,
                Tipo = tipo,
                Ordem = ordem,
                DuracaoMinutos = duracaoMinutos
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = trilhaId });
        }

        // Formulário editar conteúdo
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpGet]
        public async Task<IActionResult> EditConteudo(int id)
        {
            var conteudo = await _context.Conteudos.FindAsync(id);
            if (conteudo == null) return NotFound();
            return View(conteudo);
        }

        // Salva edição do conteúdo
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpPost]
        public async Task<IActionResult> EditConteudo(
            int id, string titulo, string? texto, string? urlVideo,
            string tipo, int ordem, int duracaoMinutos)
        {
            var conteudo = await _context.Conteudos.FindAsync(id);
            if (conteudo == null) return NotFound();

            conteudo.Titulo = titulo;
            conteudo.Texto = texto;
            conteudo.UrlVideo = urlVideo;
            conteudo.Tipo = tipo;
            conteudo.Ordem = ordem;
            conteudo.DuracaoMinutos = duracaoMinutos;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = conteudo.TrilhaId });
        }

        // Exclui conteúdo
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpPost]
        public async Task<IActionResult> DeleteConteudo(int id)
        {
            var conteudo = await _context.Conteudos.FindAsync(id);
            if (conteudo == null) return NotFound();

            var trilhaId = conteudo.TrilhaId;
            _context.Conteudos.Remove(conteudo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = trilhaId });
        }
    }
}