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
        public DbSet<ConteudoProgresso> ConteudosProgresso { get; set; }
        public DbSet<Denuncia> Denuncias { get; set; }
        public DbSet<IndicadorDiversidade> IndicadoresDiversidade { get; set; }
        public DbSet<Mentoria> Mentorias { get; set; }
        public DbSet<MentoriaPrograma> MentoriaProgramas { get; set; }
        public DbSet<MentoriaInscricao> MentoriaInscricoes { get; set; }
        public DbSet<MentoriaPost> MentoriaPosts { get; set; }
        public DbSet<MentoriaResposta> MentoriaRespostas { get; set; }
        public DbSet<OrientacaoInclusiva> OrientacoesInclusivas { get; set; }
        public DbSet<ChecklistInclusivoItem> ChecklistInclusivoItens { get; set; }
        public DbSet<ChecklistInclusivoConclusao> ChecklistInclusivoConclusoes { get; set; }
        public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // EMPRESA / USUÁRIO
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Empresa)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .Property(u => u.Salario)
                .HasPrecision(18, 2);

            builder.Entity<Empresa>()
                .Property(e => e.ValorAssinatura)
                .HasPrecision(18, 2);

            // TRILHAS
            builder.Entity<TrilhaTreinamento>()
                .HasOne(t => t.Empresa)
                .WithMany(e => e.Trilhas)
                .HasForeignKey(t => t.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Conteudo>()
                .HasOne(c => c.Trilha)
                .WithMany(t => t.Conteudos)
                .HasForeignKey(c => c.TrilhaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProgressoTreinamento>()
                .HasOne(p => p.Trilha)
                .WithMany(t => t.Progressos)
                .HasForeignKey(p => p.TrilhaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProgressoTreinamento>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Progressos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ConteudoProgresso>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ConteudoProgresso>()
                .HasOne(c => c.Conteudo)
                .WithMany()
                .HasForeignKey(c => c.ConteudoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Um usuário só conclui o mesmo conteúdo uma vez
            builder.Entity<ConteudoProgresso>()
                .HasIndex(c => new { c.UsuarioId, c.ConteudoId })
                .IsUnique();

            // DENÚNCIAS (retenção judicial)
            builder.Entity<Denuncia>()
                .HasOne(d => d.Empresa)
                .WithMany(e => e.Denuncias)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Denuncia>()
                .HasOne(d => d.Usuario)
                .WithMany(u => u.Denuncias)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Denuncia>()
                .HasIndex(d => d.Protocolo)
                .IsUnique();

            // INDICADORES
            builder.Entity<IndicadorDiversidade>()
                .HasOne(i => i.Empresa)
                .WithMany(e => e.Indicadores)
                .HasForeignKey(i => i.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<IndicadorDiversidade>()
                .Property(i => i.PercentualAdesaoTreinamentos)
                .HasPrecision(5, 2);

            // MENTORIA 
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

            builder.Entity<Mentoria>()
                .HasOne(m => m.Empresa)
                .WithMany()
                .HasForeignKey(m => m.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MentoriaPrograma>()
                .HasOne(m => m.Empresa)
                .WithMany()
                .HasForeignKey(m => m.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MentoriaPrograma>()
                .HasOne(m => m.Mentor)
                .WithMany() // qualquer usuário pode ser mentor
                .HasForeignKey(m => m.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MentoriaInscricao>()
                .HasOne(i => i.Programa)
                .WithMany(p => p.Inscricoes)
                .HasForeignKey(i => i.MentoriaProgramaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MentoriaInscricao>()
                .HasOne(i => i.Mentorado)
                .WithMany()
                .HasForeignKey(i => i.MentoradoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MentoriaInscricao>()
                .HasIndex(i => new { i.MentoriaProgramaId, i.MentoradoId })
                .IsUnique();

            builder.Entity<MentoriaPost>()
                .HasOne(p => p.Programa)
                .WithMany(m => m.Posts)
                .HasForeignKey(p => p.MentoriaProgramaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MentoriaPost>()
                .HasOne(p => p.Autor)
                .WithMany()
                .HasForeignKey(p => p.AutorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MentoriaResposta>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Respostas)
                .HasForeignKey(r => r.MentoriaPostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MentoriaResposta>()
                .HasOne(r => r.Autor)
                .WithMany()
                .HasForeignKey(r => r.AutorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChecklistInclusivoConclusao>()
                .HasIndex(c => new { c.ItemId, c.UsuarioId, c.DataReferencia })
                .IsUnique();
        }
    }
}