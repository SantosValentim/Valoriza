/* Dashboard
   Métricas:
   - Representatividade (etnia / gênero na base de usuários)
   - Progressão de carreira (proxy: cargos de liderança x total)
   - Equidade salarial (média salarial por grupo, quando houver salário)
   - Adesão a programas de inclusão (trilhas concluídas / progresso)
   Regras de acesso:
   - Colaborador: sem dashboard
   - Gestor/Admin: dados da própria empresa
   - Cards de “pessoas” conforme perfil (já definido antes)
   ============================================================ */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            var isAdminValoriza = roles.Contains("AdminValoriza");
            var isAdminEmpresa = roles.Contains("AdminEmpresa");
            var isGestorDEI = roles.Contains("GestorDEI");

            // Colaborador não acessa dashboard
            if (!isAdminValoriza && !isAdminEmpresa && !isGestorDEI)
                return RedirectToAction("Index", "Treinamentos");

            ViewBag.NomeUsuario = !string.IsNullOrWhiteSpace(user.NomeSocial)
                ? user.NomeSocial
                : user.NomeCompleto;
            ViewBag.IsAdminValoriza = isAdminValoriza;
            ViewBag.IsAdminEmpresa = isAdminEmpresa;
            ViewBag.IsGestorDEI = isGestorDEI;

            // Escopo: própria empresa (todos os gestores/admins, inclusive AdminValoriza)
            int? empresaId = user.EmpresaId;

            //  Base de usuários ativos no escopo
            var usuariosQuery = _context.Users.Where(u => u.Ativo);
            if (empresaId != null)
                usuariosQuery = usuariosQuery.Where(u => u.EmpresaId == empresaId);
            else
                usuariosQuery = usuariosQuery.Where(u => false); // sem empresa = sem dados de cliente

            var usuarios = await usuariosQuery.ToListAsync();
            var total = usuarios.Count;

            // REPRESENTATIVIDADE
            // % por etnia e gênero (sobre quem informou)
            ViewBag.RepTotal = total;
            ViewBag.RepNegros = usuarios.Count(u =>
                string.Equals(u.Etnia, "Preta", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.Etnia, "Parda", StringComparison.OrdinalIgnoreCase));
            ViewBag.RepIndigenas = usuarios.Count(u =>
                string.Equals(u.Etnia, "Indígena", StringComparison.OrdinalIgnoreCase));
            ViewBag.RepMulheres = usuarios.Count(u =>
                string.Equals(u.Genero, "Feminino", StringComparison.OrdinalIgnoreCase));
            ViewBag.RepPctNegros = Pct(ViewBag.RepNegros, total);
            ViewBag.RepPctIndigenas = Pct(ViewBag.RepIndigenas, total);
            ViewBag.RepPctMulheres = Pct(ViewBag.RepMulheres, total);

            // PROGRESSÃO DE CARREIRA
            // Proxy: cargos que sugerem liderança vs total
            static bool IsLideranca(string? cargo)
            {
                if (string.IsNullOrWhiteSpace(cargo)) return false;
                var c = cargo.ToLowerInvariant();
                return c.Contains("diretor") || c.Contains("gerente") ||
                       c.Contains("coordenador") || c.Contains("supervisor") ||
                       c.Contains("líder") || c.Contains("lider") ||
                       c.Contains("head") || c.Contains("ceo") || c.Contains("cto");
            }

            var lideranca = usuarios.Where(u => IsLideranca(u.Cargo)).ToList();
            var negrosEmLideranca = lideranca.Count(u =>
                string.Equals(u.Etnia, "Preta", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.Etnia, "Parda", StringComparison.OrdinalIgnoreCase));
            var mulheresEmLideranca = lideranca.Count(u =>
                string.Equals(u.Genero, "Feminino", StringComparison.OrdinalIgnoreCase));

            ViewBag.CarreiraTotalLideranca = lideranca.Count;
            ViewBag.CarreiraNegrosLideranca = negrosEmLideranca;
            ViewBag.CarreiraMulheresLideranca = mulheresEmLideranca;
            ViewBag.CarreiraPctNegrosNaLideranca = Pct(negrosEmLideranca, lideranca.Count);
            ViewBag.CarreiraPctMulheresNaLideranca = Pct(mulheresEmLideranca, lideranca.Count);

            // EQUIDADE SALARIAL
            // Média salarial geral e por grupos (só quem tem salário informado)
            var comSalario = usuarios.Where(u => u.Salario != null && u.Salario > 0).ToList();
            decimal Media(IEnumerable<ApplicationUser> list) =>
                list.Any() ? list.Average(u => u.Salario!.Value) : 0m;

            var mediaGeral = Media(comSalario);
            var mediaNegros = Media(comSalario.Where(u =>
                string.Equals(u.Etnia, "Preta", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.Etnia, "Parda", StringComparison.OrdinalIgnoreCase)));
            var mediaBrancos = Media(comSalario.Where(u =>
                string.Equals(u.Etnia, "Branca", StringComparison.OrdinalIgnoreCase)));
            var mediaMulheres = Media(comSalario.Where(u =>
                string.Equals(u.Genero, "Feminino", StringComparison.OrdinalIgnoreCase)));
            var mediaHomens = Media(comSalario.Where(u =>
                string.Equals(u.Genero, "Masculino", StringComparison.OrdinalIgnoreCase)));

            ViewBag.EqMediaGeral = mediaGeral;
            ViewBag.EqMediaNegros = mediaNegros;
            ViewBag.EqMediaBrancos = mediaBrancos;
            ViewBag.EqMediaMulheres = mediaMulheres;
            ViewBag.EqMediaHomens = mediaHomens;
            // Razão (1 = paridade; < 1 grupo ganha menos)
            ViewBag.EqRazaoNegrosBrancos = mediaBrancos > 0 ? mediaNegros / mediaBrancos : 0m;
            ViewBag.EqRazaoMulheresHomens = mediaHomens > 0 ? mediaMulheres / mediaHomens : 0m;
            ViewBag.EqComSalario = comSalario.Count;

            // ADESÃO A PROGRAMAS DE INCLUSÃO
            // Trilhas ativas da empresa + progresso dos usuários
            var trilhasQuery = _context.TrilhasTreinamento.Where(t => t.Ativa);
            if (empresaId != null)
                trilhasQuery = trilhasQuery.Where(t => t.EmpresaId == empresaId);

            var trilhasIds = await trilhasQuery.Select(t => t.Id).ToListAsync();
            var totalTrilhas = trilhasIds.Count;

            var userIds = usuarios.Select(u => u.Id).ToList();
            var progressos = await _context.ProgressosTreinamento
                .Where(p => userIds.Contains(p.UsuarioId) && trilhasIds.Contains(p.TrilhaId))
                .ToListAsync();

            var concluidas = progressos.Count(p => p.Concluido);
            var emAndamento = progressos.Count(p => !p.Concluido && p.PercentualConcluido > 0);
            // Adesão: usuários com pelo menos 1 progresso / total usuários
            var usuariosComProgresso = progressos.Select(p => p.UsuarioId).Distinct().Count();
            var mediaPercentual = progressos.Any()
                ? (int)Math.Round(progressos.Average(p => p.PercentualConcluido))
                : 0;

            ViewBag.AdesaoTrilhasAtivas = totalTrilhas;
            ViewBag.AdesaoConcluidas = concluidas;
            ViewBag.AdesaoEmAndamento = emAndamento;
            ViewBag.AdesaoUsuariosEngajados = usuariosComProgresso;
            ViewBag.AdesaoPctEngajamento = Pct(usuariosComProgresso, total);
            ViewBag.AdesaoMediaPercentual = mediaPercentual;

            // Também do último indicador cadastrado (se existir)
            var qInd = _context.IndicadoresDiversidade.AsQueryable();
            if (empresaId != null)
                qInd = qInd.Where(i => i.EmpresaId == empresaId);
            var indicador = await qInd
                .Include(i => i.Empresa)
                .OrderByDescending(i => i.Ano).ThenByDescending(i => i.Mes)
                .FirstOrDefaultAsync();
            ViewBag.Indicador = indicador;

            // Denúncias ao vivo (própria empresa)
            var qDen = _context.Denuncias.AsQueryable();
            if (empresaId != null)
                qDen = qDen.Where(d => d.EmpresaId == empresaId);
            ViewBag.DenunciasTotalLive = await qDen.CountAsync();
            ViewBag.DenunciasResolvidasLive = await qDen.CountAsync(d => d.Status == "Resolvida");
            ViewBag.TotalTrilhas = totalTrilhas;

            // Cards extras por perfil
            if (isAdminValoriza)
            {
                var admins = await _userManager.GetUsersInRoleAsync("AdminValoriza");
                ViewBag.AdminsValorizaAtivos = admins.Count(u => u.Ativo);
                ViewBag.EmpresasClientesAtivas = await _context.Empresas.CountAsync(e => e.Ativa);
                ViewBag.TipoCardPessoas = "AdminValoriza";
            }
            else if (isAdminEmpresa)
            {
                ViewBag.FuncionariosAtivosEmpresa = total;
                ViewBag.TipoCardPessoas = "AdminEmpresa";
            }
            else
            {
                ViewBag.TipoCardPessoas = "GestorDEI"; // sem card de usuários
            }

            return View();
        }

        private static int Pct(int parte, int total) =>
            total <= 0 ? 0 : (int)Math.Round(100.0 * parte / total);

        [AllowAnonymous]
        public IActionResult Error() => View();
    }
}