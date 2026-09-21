/* Lista e cadastra indicadores de diversidade.
   Acesso: AdminValoriza, AdminEmpresa, GestorDEI (leitura)
   Cadastro: apenas Admins */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.Web.Controllers
{
    // Quem pode acessar este controller
    [Authorize(Roles = "AdminValoriza,AdminEmpresa,GestorDEI")]
    public class IndicadoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IndicadoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// Lista todos os indicadores, do mais recente para o mais antigo.
        public async Task<IActionResult> Index()
        {
            var lista = await _context.IndicadoresDiversidade
                .Include(i => i.Empresa) // carrega dados da empresa (se houver)
                .OrderByDescending(i => i.Ano)
                .ThenByDescending(i => i.Mes)
                .ToListAsync();

            return View(lista);
        }

        /// Formulário de novo indicador (somente Admin).
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// Salva o indicador no banco.
        [Authorize(Roles = "AdminValoriza,AdminEmpresa")]
        [HttpPost]
        public async Task<IActionResult> Create(IndicadorDiversidade model)
        {
            // Data de registro automática
            model.DataRegistro = DateTime.UtcNow;

            // Empresa padrão se não informado
            if (model.EmpresaId <= 0)
                model.EmpresaId = 1;

            _context.IndicadoresDiversidade.Add(model);
            await _context.SaveChangesAsync();

            // Volta para a lista
            return RedirectToAction(nameof(Index));
        }
    }
}