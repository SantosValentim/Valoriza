/* Relatórios gerenciais com extração em CSV (Excel).
   AdminValoriza: todos os relatórios da própria empresa + clientes (churn).
   AdminEmpresa / GestorDEI: só dados da própria empresa. */

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
    public class RelatoriosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RelatoriosController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsAdminValoriza => User.IsInRole("AdminValoriza");

        ///Página com botões de download
        public IActionResult Index()
        {
            ViewBag.IsAdminValoriza = IsAdminValoriza;
            return View();
        }

        // USUÁRIOS / REPRESENTATIVIDADE
        [HttpGet]
        public async Task<IActionResult> Usuarios()
        {
            var user = await _userManager.GetUserAsync(User);
            var q = _context.Users.Include(u => u.Empresa).Where(u => u.Ativo);

            if (!IsAdminValoriza && user?.EmpresaId != null)
                q = q.Where(u => u.EmpresaId == user.EmpresaId);
            else if (!IsAdminValoriza)
                q = q.Where(u => false);

            var lista = await q.OrderBy(u => u.NomeCompleto).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Nome;NomeSocial;Email;Genero;Etnia;Cargo;Salario;Empresa;Ativo;DataCadastro");

            foreach (var u in lista)
            {
                sb.AppendLine(string.Join(";",
                    Csv(u.NomeCompleto),
                    Csv(u.NomeSocial),
                    Csv(u.Email),
                    Csv(u.Genero),
                    Csv(u.Etnia),
                    Csv(u.Cargo),
                    u.Salario?.ToString("0.00") ?? "",
                    Csv(u.Empresa?.NomeFantasia ?? u.Empresa?.RazaoSocial),
                    u.Ativo ? "Sim" : "Nao",
                    u.DataCadastro.ToString("dd/MM/yyyy")));
            }

            return ArquivoCsv(sb.ToString(), "relatorio-usuarios");
        }

        // DENÚNCIAS
        [HttpGet]
        public async Task<IActionResult> Denuncias()
        {
            var user = await _userManager.GetUserAsync(User);
            var q = _context.Denuncias.Include(d => d.Empresa).AsQueryable();

            if (user?.EmpresaId != null)
                q = q.Where(d => d.EmpresaId == user.EmpresaId);
            else
                q = q.Where(d => false);

            var lista = await q.OrderByDescending(d => d.DataRegistro).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Protocolo;Titulo;Categoria;Status;Anonima;Empresa;DataRegistro;DataResolucao");

            foreach (var d in lista)
            {
                sb.AppendLine(string.Join(";",
                    Csv(d.Protocolo),
                    Csv(d.Titulo),
                    Csv(d.Categoria),
                    Csv(d.Status),
                    d.Anonima ? "Sim" : "Nao",
                    Csv(d.Empresa?.NomeFantasia ?? d.Empresa?.RazaoSocial),
                    d.DataRegistro.ToString("dd/MM/yyyy HH:mm"),
                    d.DataResolucao?.ToString("dd/MM/yyyy") ?? ""));
            }

            return ArquivoCsv(sb.ToString(), "relatorio-denuncias");
        }

        // TREINAMENTOS / ADESÃO
        [HttpGet]
        public async Task<IActionResult> Treinamentos()
        {
            var user = await _userManager.GetUserAsync(User);
            var q = _context.ProgressosTreinamento
                .Include(p => p.Usuario)
                .Include(p => p.Trilha)
                .AsQueryable();

            if (user?.EmpresaId != null)
            {
                q = q.Where(p =>
                    p.Usuario != null &&
                    p.Usuario.EmpresaId == user.EmpresaId);
            }
            else
            {
                q = q.Where(p => false);
            }

            var lista = await q.OrderByDescending(p => p.PercentualConcluido).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Usuario;Trilha;Percentual;Concluido;DataConclusao");

            foreach (var p in lista)
            {
                var nome = p.Usuario != null
                    ? (p.Usuario.NomeSocial ?? p.Usuario.NomeCompleto)
                    : "";
                sb.AppendLine(string.Join(";",
                    Csv(nome),
                    Csv(p.Trilha?.Titulo),
                    p.PercentualConcluido.ToString(),
                    p.Concluido ? "Sim" : "Nao",
                    p.DataConclusao?.ToString("dd/MM/yyyy") ?? ""));
            }

            return ArquivoCsv(sb.ToString(), "relatorio-treinamentos");
        }

        // INDICADORES
        [HttpGet]
        public async Task<IActionResult> Indicadores()
        {
            var user = await _userManager.GetUserAsync(User);
            var q = _context.IndicadoresDiversidade.Include(i => i.Empresa).AsQueryable();

            if (user?.EmpresaId != null)
                q = q.Where(i => i.EmpresaId == user.EmpresaId);
            else
                q = q.Where(i => false);

            var lista = await q
                .OrderByDescending(i => i.Ano)
                .ThenByDescending(i => i.Mes)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Empresa;Ano;Mes;TotalColaboradores;Negros;Indigenas;PcD;MulheresLideranca;NegrosLideranca;AdesaoTreinamentos;DenunciasPeriodo;DenunciasResolvidas");

            foreach (var i in lista)
            {
                sb.AppendLine(string.Join(";",
                    Csv(i.Empresa?.NomeFantasia ?? i.Empresa?.RazaoSocial),
                    i.Ano,
                    i.Mes,
                    i.TotalColaboradores,
                    i.ColaboradoresNegros,
                    i.ColaboradoresIndigenas,
                    i.ColaboradoresPcd,
                    i.MulheresLideranca,
                    i.NegrosLideranca,
                    i.PercentualAdesaoTreinamentos.ToString("0.00"),
                    i.TotalDenunciasPeriodo,
                    i.DenunciasResolvidas));
            }

            return ArquivoCsv(sb.ToString(), "relatorio-indicadores");
        }

        // EMPRESAS / CHURN (só AdminValoriza)
        [HttpGet]
        [Authorize(Roles = "AdminValoriza")]
        public async Task<IActionResult> EmpresasChurn()
        {
            var lista = await _context.Empresas
                .OrderBy(e => e.RazaoSocial)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("NomeFantasia;RazaoSocial;CNPJ;Plano;ValorAssinatura;Ciclo;StatusAssinatura;Ativa;DataChurn;MotivoChurn");

            foreach (var e in lista)
            {
                sb.AppendLine(string.Join(";",
                    Csv(e.NomeFantasia),
                    Csv(e.RazaoSocial),
                    Csv(e.Cnpj),
                    Csv(e.Plano),
                    e.ValorAssinatura.ToString("0.00"),
                    Csv(e.CicloCobranca),
                    Csv(e.StatusAssinatura),
                    e.Ativa ? "Sim" : "Nao",
                    e.DataChurn?.ToString("dd/MM/yyyy") ?? "",
                    Csv(e.MotivoChurn)));
            }

            return ArquivoCsv(sb.ToString(), "relatorio-empresas-churn");
        }

        // Helpers
        private static string Csv(string? valor)
        {
            if (string.IsNullOrEmpty(valor)) return "";
            var v = valor.Replace("\"", "\"\"");
            if (v.Contains(';') || v.Contains('"') || v.Contains('\n'))
                return $"\"{v}\"";
            return v;
        }

        private FileContentResult ArquivoCsv(string conteudo, string nomeBase)
        {
            // BOM UTF-8 para o Excel abrir acentos corretamente
            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(conteudo))
                .ToArray();

            var nome = $"{nomeBase}-{DateTime.Now:yyyyMMdd-HHmm}.csv";
            return File(bytes, "text/csv; charset=utf-8", nome);
        }
    }
}