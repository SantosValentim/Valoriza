using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.API.Services
{
    public interface IAuditoriaService
    {
        Task RegistrarAsync(
            string usuarioId,
            string? email,
            string acao,
            string? entidade,
            string? entidadeId,
            string? detalhes,
            string? ip);
    }

    public class AuditoriaService : IAuditoriaService
    {
        private readonly ApplicationDbContext _context;

        public AuditoriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAsync(
            string usuarioId,
            string? email,
            string acao,
            string? entidade,
            string? entidadeId,
            string? detalhes,
            string? ip)
        {
            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                UsuarioId = usuarioId ?? "",
                UsuarioEmail = email,
                Acao = acao,
                Entidade = entidade,
                EntidadeId = entidadeId,
                Detalhes = detalhes,
                Ip = ip,
                DataUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}