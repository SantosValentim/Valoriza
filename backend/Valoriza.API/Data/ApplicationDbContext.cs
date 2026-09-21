/* Contexto EF Core com Identity + Cascade na Empresa (exceto Denúncias para retenção judicial) */

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Models;

namespace Valoriza.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets do domínio Valoriza
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<TrilhaTreinamento> TrilhasTreinamento { get; set; }
        public DbSet<Conteudo> Conteudos { get; set; }
        public DbSet<ProgressoTreinamento> ProgressosTreinamento { get; set; }
        public DbSet<Denuncia> Denuncias { get; set; }
        public DbSet<IndicadorDiversidade> IndicadoresDiversidade { get; set; }
        public DbSet<Mentoria> Mentorias { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apaga usuários ao excluir empresa.
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Empresa)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trilhas
            builder.Entity<TrilhaTreinamento>()
                .HasOne(t => t.Empresa)
                .WithMany(e => e.Trilhas)
                .HasForeignKey(t => t.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Denúncias (retenção judicial, NÃO apaga)
            builder.Entity<Denuncia>()
                .HasOne(d => d.Empresa)
                .WithMany(e => e.Denuncias)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indicadores
            builder.Entity<IndicadorDiversidade>()
                .HasOne(i => i.Empresa)
                .WithMany(e => e.Indicadores)
                .HasForeignKey(i => i.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mentorias da empresa
            builder.Entity<Mentoria>()
                .HasOne(m => m.Empresa)
                .WithMany()
                .HasForeignKey(m => m.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Conteudo>()
                .HasOne(c => c.Trilha)
                .WithMany(t => t.Conteudos)
                .HasForeignKey(c => c.TrilhaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Progresso sobe com a trilha
            builder.Entity<ProgressoTreinamento>()
                .HasOne(p => p.Trilha)
                .WithMany(t => t.Progressos)
                .HasForeignKey(p => p.TrilhaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Denúncia permanece se o usuário for apagado
            builder.Entity<Denuncia>()
                .HasOne(d => d.Usuario)
                .WithMany(u => u.Denuncias)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            // Restrict
            builder.Entity<ProgressoTreinamento>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Progressos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Mentoria>()
                .HasOne(m => m.Mentor)
                .WithMany(u => u.MentoriasComoMentor)
                .HasForeignKey(m => m.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Mentoria>()
                .HasOne(m => m.Mentorado)
                .WithMany(u => u.MentoriasComoMentorado)
                .HasForeignKey(m => m.MentoradoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ÍNDICES / PRECISÃO
            builder.Entity<Denuncia>()
                .HasIndex(d => d.Protocolo)
                .IsUnique();

            builder.Entity<IndicadorDiversidade>()
                .Property(i => i.PercentualAdesaoTreinamentos)
                .HasPrecision(5, 2);
        }
    }
}