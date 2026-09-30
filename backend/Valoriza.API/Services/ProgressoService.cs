/* Calcula e atualiza o progresso do usuário em uma trilha */

using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;

namespace Valoriza.API.Services
{
    public class ProgressoService
    {
        private readonly ApplicationDbContext _context;

        public ProgressoService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// Marca um conteúdo como concluído e recalcula o % da trilha.
        public async Task<ProgressoTreinamento> ConcluirConteudoAsync(
            string usuarioId,
            int conteudoId,
            string? respostaQuiz = null)
        {
            var conteudo = await _context.Conteudos
                .Include(c => c.Trilha)
                .FirstOrDefaultAsync(c => c.Id == conteudoId)
                ?? throw new KeyNotFoundException("Conteúdo não encontrado.");

            // Já concluído? Não duplica
            var existente = await _context.ConteudosProgresso
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.ConteudoId == conteudoId);

            bool acertou = true;
            if (conteudo.Tipo == "Quiz" && !string.IsNullOrEmpty(conteudo.RespostaCorreta))
            {
                acertou = string.Equals(
                    respostaQuiz?.Trim(),
                    conteudo.RespostaCorreta,
                    StringComparison.OrdinalIgnoreCase);
            }

            if (existente == null)
            {
                _context.ConteudosProgresso.Add(new ConteudoProgresso
                {
                    UsuarioId = usuarioId,
                    ConteudoId = conteudoId,
                    DataConclusao = DateTime.UtcNow,
                    RespostaQuiz = respostaQuiz?.ToUpperInvariant(),
                    Acertou = acertou
                });
                await _context.SaveChangesAsync();
            }

            // Recalcula progresso da trilha
            return await RecalcularTrilhaAsync(usuarioId, conteudo.TrilhaId);
        }

        /// % = conteúdos concluídos / total de conteúdos da trilha
        public async Task<ProgressoTreinamento> RecalcularTrilhaAsync(string usuarioId, int trilhaId)
        {
            var total = await _context.Conteudos.CountAsync(c => c.TrilhaId == trilhaId);
            if (total == 0)
                total = 1; // evita divisão por zero

            var concluidos = await _context.ConteudosProgresso
                .CountAsync(p => p.UsuarioId == usuarioId
                              && p.Conteudo.TrilhaId == trilhaId);

            var percentual = (int)Math.Round(100.0 * concluidos / total);
            if (percentual > 100) percentual = 100;

            var progresso = await _context.ProgressosTreinamento
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.TrilhaId == trilhaId);

            if (progresso == null)
            {
                progresso = new ProgressoTreinamento
                {
                    UsuarioId = usuarioId,
                    TrilhaId = trilhaId
                };
                _context.ProgressosTreinamento.Add(progresso);
            }

            progresso.PercentualConcluido = percentual;
            progresso.Concluido = percentual >= 100;
            progresso.DataConclusao = progresso.Concluido ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();
            return progresso;
        }

        public async Task<ProgressoTreinamento?> ObterProgressoAsync(string usuarioId, int trilhaId)
        {
            return await _context.ProgressosTreinamento
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId && p.TrilhaId == trilhaId);
        }

        public async Task<HashSet<int>> ConteudosConcluidosIdsAsync(string usuarioId, int trilhaId)
        {
            return (await _context.ConteudosProgresso
                .Where(p => p.UsuarioId == usuarioId && p.Conteudo.TrilhaId == trilhaId)
                .Select(p => p.ConteudoId)
                .ToListAsync())
                .ToHashSet();
        }
    }
}