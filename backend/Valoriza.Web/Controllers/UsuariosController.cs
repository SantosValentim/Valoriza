/* Lista, cadastro, detalhes, edição, foto e ativar/desativar */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
        }

        /// Salva a foto do usuário e devolve a URL pública (/uploads/avatars/...).  Se não houver arquivo novo, mantém fotoAtual.
        private async Task<string?> SalvarFotoAsync(IFormFile? foto, string? fotoAtual = null)
        {
            if (foto == null || foto.Length == 0)
                return fotoAtual;

            var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
            var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!permitidas.Contains(ext))
                return fotoAtual;

            // Máx. 2 MB
            if (foto.Length > 2 * 1024 * 1024)
                return fotoAtual;

            var pasta = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(pasta);

            var nome = $"{Guid.NewGuid():N}{ext}";
            var caminho = Path.Combine(pasta, nome);

            using (var stream = System.IO.File.Create(caminho))
                await foto.CopyToAsync(stream);

            // Remove arquivo antigo, se for do nosso diretório
            if (!string.IsNullOrEmpty(fotoAtual) && fotoAtual.StartsWith("/uploads/avatars/"))
            {
                var antigo = Path.Combine(
                    _env.WebRootPath,
                    fotoAtual.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(antigo))
                    System.IO.File.Delete(antigo);
            }

            return $"/uploads/avatars/{nome}";
        }

        // LISTA + FILTROS
        public async Task<IActionResult> Index(
            string? nome, string? papel, string? etnia, string? genero, string? situacao)
        {
            var current = await _userManager.GetUserAsync(User);
            var isAdminValoriza = User.IsInRole("AdminValoriza");

            IQueryable<ApplicationUser> query = _userManager.Users.Include(u => u.Empresa);

            if (!isAdminValoriza && current?.EmpresaId != null)
                query = query.Where(u => u.EmpresaId == current.EmpresaId);

            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(u =>
                    u.NomeCompleto.Contains(nome) ||
                    (u.NomeSocial != null && u.NomeSocial.Contains(nome)) ||
                    (u.Email != null && u.Email.Contains(nome)));
            }

            if (!string.IsNullOrWhiteSpace(etnia))
                query = query.Where(u => u.Etnia == etnia);

            if (!string.IsNullOrWhiteSpace(genero))
                query = query.Where(u => u.Genero == genero);

            if (situacao == "Ativo")
                query = query.Where(u => u.Ativo);
            else if (situacao == "Inativo")
                query = query.Where(u => !u.Ativo);

            var users = await query.OrderBy(u => u.NomeCompleto).ToListAsync();
            var lista = new List<(ApplicationUser User, IList<string> Roles)>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (!string.IsNullOrWhiteSpace(papel) &&
                    !roles.Any(r => r.Equals(papel, StringComparison.OrdinalIgnoreCase)))
                    continue;
                lista.Add((u, roles));
            }

            var baseUsers = _userManager.Users.AsQueryable();
            if (!isAdminValoriza && current?.EmpresaId != null)
                baseUsers = baseUsers.Where(u => u.EmpresaId == current.EmpresaId);

            ViewBag.SugestoesNomes = await baseUsers
                .Select(u => u.NomeCompleto).Distinct().OrderBy(n => n).Take(200).ToListAsync();

            ViewBag.FiltroNome = nome;
            ViewBag.FiltroPapel = papel;
            ViewBag.FiltroEtnia = etnia;
            ViewBag.FiltroGenero = genero;
            ViewBag.FiltroSituacao = situacao;
            ViewBag.IsAdminValoriza = isAdminValoriza;

            return View(lista);
        }

        // CADASTRO
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var current = await _userManager.GetUserAsync(User);
            await PreencherViewBagsCreate(User.IsInRole("AdminValoriza"), current);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            string nomeCompleto,
            string? nomeSocial,
            string email,
            string senha,
            string role,
            string? cpf,
            string? genero,
            string? etnia,
            string? cargo,
            decimal? salario,
            int? empresaId,
            IFormFile? foto) // arquivo da foto
        {
            var current = await _userManager.GetUserAsync(User);
            var isAdminValoriza = User.IsInRole("AdminValoriza");

            if (string.IsNullOrWhiteSpace(nomeCompleto) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.Erro = "Preencha nome, e-mail e senha.";
                await PreencherViewBagsCreate(isAdminValoriza, current);
                return View();
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ViewBag.Erro = "Já existe um usuário com este e-mail.";
                await PreencherViewBagsCreate(isAdminValoriza, current);
                return View();
            }

            var roleFinal = string.IsNullOrWhiteSpace(role) ? "Colaborador" : role;

            if (!isAdminValoriza && roleFinal == "AdminValoriza")
            {
                ViewBag.Erro = "Sem permissão para criar Admin Valoriza.";
                await PreencherViewBagsCreate(isAdminValoriza, current);
                return View();
            }

            int? empresaFinal;
            if (!isAdminValoriza)
            {
                empresaFinal = current?.EmpresaId;
                if (empresaFinal == null)
                {
                    ViewBag.Erro = "Seu usuário não está vinculado a uma empresa.";
                    await PreencherViewBagsCreate(isAdminValoriza, current);
                    return View();
                }
            }
            else if (roleFinal == "AdminEmpresa")
            {
                if (empresaId == null || empresaId <= 0)
                {
                    ViewBag.Erro = "Selecione a empresa cliente.";
                    await PreencherViewBagsCreate(isAdminValoriza, current);
                    return View();
                }
                var emp = await _context.Empresas.FindAsync(empresaId.Value);
                if (emp == null || !emp.Ativa)
                {
                    ViewBag.Erro = "Empresa inválida ou inativa.";
                    await PreencherViewBagsCreate(isAdminValoriza, current);
                    return View();
                }
                empresaFinal = empresaId;
                etnia = null;
                cargo = null;
                salario = null;
            }
            else
            {
                empresaFinal = null;
            }

            // Salva foto (se enviada)
            var fotoUrl = await SalvarFotoAsync(foto);

            var novo = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NomeCompleto = nomeCompleto.Trim(),
                NomeSocial = string.IsNullOrWhiteSpace(nomeSocial) ? null : nomeSocial.Trim(),
                Cpf = cpf,
                Genero = genero,
                Etnia = etnia,
                Cargo = cargo,
                Salario = salario,
                FotoUrl = fotoUrl,
                EmpresaId = empresaFinal,
                Ativo = true,
                EmailConfirmed = true,
                DataCadastro = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(novo, senha);
            if (!result.Succeeded)
            {
                ViewBag.Erro = string.Join(" ", result.Errors.Select(e => e.Description));
                await PreencherViewBagsCreate(isAdminValoriza, current);
                return View();
            }

            await _userManager.AddToRoleAsync(novo, roleFinal);
            TempData["Sucesso"] = $"Usuário {novo.NomeSocial ?? novo.NomeCompleto} criado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // DETALHES
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.Users.Include(u => u.Empresa)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            if (!User.IsInRole("AdminValoriza"))
            {
                var current = await _userManager.GetUserAsync(User);
                if (current?.EmpresaId == null || user.EmpresaId != current.EmpresaId)
                    return Forbid();
            }

            ViewBag.Roles = await _userManager.GetRolesAsync(user);
            return View(user);
        }

        // EDITAR
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.Users.Include(u => u.Empresa)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            if (!User.IsInRole("AdminValoriza"))
            {
                var current = await _userManager.GetUserAsync(User);
                if (current?.EmpresaId == null || user.EmpresaId != current.EmpresaId)
                    return Forbid();
            }

            ViewBag.Roles = await _userManager.GetRolesAsync(user);
            ViewBag.IsAdminValoriza = User.IsInRole("AdminValoriza");
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            string id,
            string nomeCompleto,
            string? nomeSocial,
            string email,
            string? cpf,
            string? genero,
            string? etnia,
            string? cargo,
            decimal? salario,
            string? role,
            IFormFile? foto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!User.IsInRole("AdminValoriza"))
            {
                var current = await _userManager.GetUserAsync(User);
                if (current?.EmpresaId == null || user.EmpresaId != current.EmpresaId)
                    return Forbid();
            }

            email = (email ?? "").Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Erro = "E-mail é obrigatório.";
                ViewBag.Roles = await _userManager.GetRolesAsync(user);
                ViewBag.IsAdminValoriza = User.IsInRole("AdminValoriza");
                return View(user);
            }

            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                var existente = await _userManager.FindByEmailAsync(email);
                if (existente != null && existente.Id != user.Id)
                {
                    ViewBag.Erro = "Já existe um usuário com este e-mail.";
                    ViewBag.Roles = await _userManager.GetRolesAsync(user);
                    ViewBag.IsAdminValoriza = User.IsInRole("AdminValoriza");
                    return View(user);
                }

                var setEmail = await _userManager.SetEmailAsync(user, email);
                if (!setEmail.Succeeded)
                {
                    ViewBag.Erro = string.Join(" ", setEmail.Errors.Select(e => e.Description));
                    ViewBag.Roles = await _userManager.GetRolesAsync(user);
                    ViewBag.IsAdminValoriza = User.IsInRole("AdminValoriza");
                    return View(user);
                }

                await _userManager.SetUserNameAsync(user, email);
            }

            user.NomeCompleto = nomeCompleto.Trim();
            user.NomeSocial = string.IsNullOrWhiteSpace(nomeSocial) ? null : nomeSocial.Trim();
            user.Cpf = cpf;
            user.Genero = genero;
            user.Etnia = etnia;
            user.Cargo = cargo;
            user.Salario = salario;

            // Atualiza foto se enviou nova
            user.FotoUrl = await SalvarFotoAsync(foto, user.FotoUrl);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ViewBag.Erro = string.Join(" ", result.Errors.Select(e => e.Description));
                ViewBag.Roles = await _userManager.GetRolesAsync(user);
                ViewBag.IsAdminValoriza = User.IsInRole("AdminValoriza");
                return View(user);
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                if (!User.IsInRole("AdminValoriza") && role == "AdminValoriza")
                {
                    TempData["Erro"] = "Sem permissão para definir Admin Valoriza.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                var atuais = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, atuais);
                await _userManager.AddToRoleAsync(user, role);
            }

            TempData["Sucesso"] = "Usuário atualizado.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ATIVAR / DESATIVAR
        [HttpPost]
        public async Task<IActionResult> AlternarStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Erro"] = "Você não pode alterar o próprio status por aqui.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var rolesAlvo = await _userManager.GetRolesAsync(user);
            var isAlvoAdmin = rolesAlvo.Contains("AdminValoriza") || rolesAlvo.Contains("AdminEmpresa");

            if (User.IsInRole("AdminEmpresa") && !User.IsInRole("AdminValoriza") && isAlvoAdmin)
            {
                TempData["Erro"] = "Admin Empresa não pode desativar/ativar outros administradores.";
                return RedirectToAction(nameof(Details), new { id });
            }

            user.Ativo = !user.Ativo;
            await _userManager.UpdateAsync(user);

            TempData["Sucesso"] = user.Ativo
                ? $"Usuário {user.NomeCompleto} ativado."
                : $"Usuário {user.NomeCompleto} desativado.";

            return RedirectToAction(nameof(Details), new { id });
        }

        // HELPER ViewBag cadastro
        private async Task PreencherViewBagsCreate(bool isAdminValoriza, ApplicationUser? current)
        {
            ViewBag.IsAdminValoriza = isAdminValoriza;
            if (!isAdminValoriza)
            {
                ViewBag.EmpresaIdFixa = current?.EmpresaId;
                if (current?.EmpresaId != null)
                {
                    var emp = await _context.Empresas.FindAsync(current.EmpresaId);
                    ViewBag.EmpresaNome = emp?.NomeFantasia ?? emp?.RazaoSocial;
                }
            }
            else
            {
                ViewBag.Empresas = await _context.Empresas
                    .Where(e => e.Ativa)
                    .OrderBy(e => e.NomeFantasia ?? e.RazaoSocial)
                    .ToListAsync();
            }
        }
    }
}