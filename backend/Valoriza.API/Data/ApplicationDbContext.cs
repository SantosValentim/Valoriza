using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Models;

namespace Valoriza.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

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

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Empresa)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EmpresaId)
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

            builder.Entity<Denuncia>()
                .HasIndex(d => d.Protocolo)
                .IsUnique();

            builder.Entity<IndicadorDiversidade>()
                .Property(i => i.PercentualAdesaoTreinamentos)
                .HasPrecision(5, 2);
        }
    }
}
