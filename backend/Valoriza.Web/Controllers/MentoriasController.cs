/* Painel HTML: programas, inscrição, fórum com upload.
   Isolamento por EmpresaId do usuário logado.
   AdminValoriza gerencia só a empresa Valoriza (config).
   Mentor = qualquer usuário ativo da mesma empresa. */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize]
    public class MentoriasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public MentoriasController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _config = config;
        }

        /// Empresa do usuário.
        private async Task<int?> GetEmpresaIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.EmpresaId != null)
                return user.EmpresaId;

            if (User.IsInRole("AdminValoriza"))
            {
                if (int.TryParse(_config["ValorizaEmpresaId"], out var id) && id > 0)
                    return id;

                var valoriza = await _context.Empresas.FirstOrDefaultAsync(e =>
                    e.NomeFantasia == "Valoriza" || e.RazaoSocial.Contains("Valoriza"));
                return valoriza?.Id;
            }

            return null;
        }

        private bool PodeGerenciar =>
            User.IsInRole("AdminValoriza")
            || User.IsInRole("AdminEmpresa")
            || User.IsInRole("GestorDEI");

        // LISTA
        public async Task<IActionResult> Index()
        {
            var empresaId = await GetEmpresaIdAsync();
            ViewBag.PodeGerenciar = PodeGerenciar;

            if (empresaId == null)
            {
                ViewBag.Aviso = "Empresa não identificada. Configure ValorizaEmpresaId ou vincule o usuário a uma empresa.";
                return View(new List<MentoriaPrograma>());
            }

            var lista = await _context.MentoriaProgramas
                .Include(m => m.Mentor)
                .Include(m => m.Inscricoes)
                .Where(m => m.EmpresaId == empresaId)
                .OrderByDescending(m => m.DataInicio)
                .ToListAsync();

            return View(lista);
        }

        // CRIAR PROGRAMA
        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var empresaId = await GetEmpresaIdAsync();
            if (empresaId == null)
            {
                TempData["Erro"] = "Empresa não identificada.";
                return RedirectToAction(nameof(Index));
            }

            // Qualquer usuário ativo da empresa pode ser mentor
            ViewBag.Colaboradores = await _userManager.Users
                .Where(u => u.EmpresaId == empresaId && u.Ativo)
                .OrderBy(u => u.NomeCompleto)
                .ToListAsync();

            return View();
        }

        [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
        [HttpPost]
        public async Task<IActionResult> Create(string titulo, string? objetivos, string mentorId)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (empresaId == null) return Forbid();

            var mentor = await _userManager.FindByIdAsync(mentorId);
            if (mentor == null || mentor.EmpresaId != empresaId || !mentor.Ativo)
            {
                ViewBag.Erro = "Selecione um usuário ativo da sua empresa como mentor.";
                ViewBag.Colaboradores = await _userManager.Users
                    .Where(u => u.EmpresaId == empresaId && u.Ativo)
                    .OrderBy(u => u.NomeCompleto)
                    .ToListAsync();
                return View();
            }

            var programa = new MentoriaPrograma
            {
                EmpresaId = empresaId.Value,
                MentorId = mentorId,
                Titulo = titulo,
                Objetivos = objetivos,
                Status = "Ativa",
                DataInicio = DateTime.UtcNow
            };

            _context.MentoriaProgramas.Add(programa);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = programa.Id });
        }

        // DETALHES + FÓRUM
        public async Task<IActionResult> Details(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            if (empresaId == null) return Forbid();

            var programa = await _context.MentoriaProgramas
                .Include(m => m.Mentor)
                .Include(m => m.Inscricoes).ThenInclude(i => i.Mentorado)
                .Include(m => m.Posts.OrderByDescending(p => p.DataPublicacao))
                    .ThenInclude(p => p.Autor)
                .Include(m => m.Posts)
                    .ThenInclude(p => p.Respostas.OrderBy(r => r.Data))
                        .ThenInclude(r => r.Autor)
                .FirstOrDefaultAsync(m => m.Id == id && m.EmpresaId == empresaId);

            if (programa == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            ViewBag.IsMentor = programa.MentorId == userId;
            ViewBag.IsInscrito = programa.Inscricoes.Any(i => i.MentoradoId == userId && i.Status == "Ativa");
            ViewBag.PodeGerenciar = PodeGerenciar;
            ViewBag.UserId = userId;

            return View(programa);
        }

        // INSCREVER
        [HttpPost]
        public async Task<IActionResult> Inscrever(int id)
        {
            var empresaId = await GetEmpresaIdAsync();
            var userId = _userManager.GetUserId(User);
            if (empresaId == null || userId == null) return Forbid();

            var programa = await _context.MentoriaProgramas
                .FirstOrDefaultAsync(m => m.Id == id && m.EmpresaId == empresaId && m.Status == "Ativa");

            if (programa == null) return NotFound();

            if (programa.MentorId == userId)
            {
                TempData["Erro"] = "O mentor não pode se inscrever como mentorado.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var jaExiste = await _context.MentoriaInscricoes
                .AnyAsync(i => i.MentoriaProgramaId == id && i.MentoradoId == userId);

            if (!jaExiste)
            {
                _context.MentoriaInscricoes.Add(new MentoriaInscricao
                {
                    MentoriaProgramaId = id,
                    MentoradoId = userId,
                    Status = "Ativa",
                    DataInscricao = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Inscrição realizada.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST NO FÓRUM
        [HttpPost]
        public async Task<IActionResult> CriarPost(
            int programaId,
            string titulo,
            string? conteudo,
            string? urlLink,
            IFormFile? arquivo)
        {
            var empresaId = await GetEmpresaIdAsync();
            var userId = _userManager.GetUserId(User);
            if (empresaId == null || userId == null) return Forbid();

            var programa = await _context.MentoriaProgramas
                .FirstOrDefaultAsync(m => m.Id == programaId && m.EmpresaId == empresaId);

            if (programa == null) return NotFound();

            if (programa.MentorId != userId && !PodeGerenciar)
            {
                TempData["Erro"] = "Apenas o mentor ou gestores podem publicar.";
                return RedirectToAction(nameof(Details), new { id = programaId });
            }

            string? urlMidia = null;
            var tipoMidia = "Nenhuma";

            if (arquivo != null && arquivo.Length > 0)
            {
                var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
                var ok = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".webm" };
                if (!ok.Contains(ext))
                {
                    TempData["Erro"] = "Formato de arquivo não permitido.";
                    return RedirectToAction(nameof(Details), new { id = programaId });
                }

                var pasta = Path.Combine(_env.WebRootPath, "uploads", "mentoria");
                Directory.CreateDirectory(pasta);
                var nome = $"{Guid.NewGuid():N}{ext}";
                var caminho = Path.Combine(pasta, nome);

                using (var stream = System.IO.File.Create(caminho))
                    await arquivo.CopyToAsync(stream);

                urlMidia = $"/uploads/mentoria/{nome}";
                tipoMidia = ext is ".mp4" or ".webm" ? "Video" : "Imagem";
            }

            _context.MentoriaPosts.Add(new MentoriaPost
            {
                MentoriaProgramaId = programaId,
                AutorId = userId,
                Titulo = titulo,
                Conteudo = conteudo,
                UrlLink = urlLink,
                UrlMidia = urlMidia,
                TipoMidia = tipoMidia,
                DataPublicacao = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = programaId });
        }

        // RESPONDER
        [HttpPost]
        public async Task<IActionResult> Responder(int postId, string texto)
        {
            var empresaId = await GetEmpresaIdAsync();
            var userId = _userManager.GetUserId(User);
            if (empresaId == null || userId == null) return Forbid();

            var post = await _context.MentoriaPosts
                .Include(p => p.Programa)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post?.Programa == null || post.Programa.EmpresaId != empresaId)
                return NotFound();

            var programaId = post.MentoriaProgramaId;
            var isMentor = post.Programa.MentorId == userId;
            var isInscrito = await _context.MentoriaInscricoes.AnyAsync(i =>
                i.MentoriaProgramaId == programaId && i.MentoradoId == userId && i.Status == "Ativa");

            if (!isMentor && !isInscrito && !PodeGerenciar)
            {
                TempData["Erro"] = "Inscreva-se na mentoria para responder.";
                return RedirectToAction(nameof(Details), new { id = programaId });
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                TempData["Erro"] = "Digite uma resposta.";
                return RedirectToAction(nameof(Details), new { id = programaId });
            }

            _context.MentoriaRespostas.Add(new MentoriaResposta
            {
                MentoriaPostId = postId,
                AutorId = userId,
                Texto = texto.Trim(),
                Data = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = programaId });
        }
    }
}