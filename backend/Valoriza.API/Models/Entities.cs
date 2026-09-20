using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Valoriza.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(150)]
        public string NomeCompleto { get; set; } = string.Empty;
        [MaxLength(14)]
        public string? Cpf { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;
        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public ICollection<Denuncia> Denuncias { get; set; } = new List<Denuncia>();
        public ICollection<ProgressoTreinamento> Progressos { get; set; } = new List<ProgressoTreinamento>();
        public ICollection<Mentoria> MentoriasComoMentor { get; set; } = new List<Mentoria>();
        public ICollection<Mentoria> MentoriasComoMentorado { get; set; } = new List<Mentoria>();
    }

    public class Empresa
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string RazaoSocial { get; set; } = string.Empty;
        [MaxLength(200)] public string? NomeFantasia { get; set; }
        [Required, MaxLength(18)] public string Cnpj { get; set; } = string.Empty;
        [MaxLength(100)] public string? Segmento { get; set; }
        public int QuantidadeColaboradores { get; set; }
        public string Plano { get; set; } = "Basic";
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativa { get; set; } = true;
        public ICollection<ApplicationUser> Usuarios { get; set; } = new List<ApplicationUser>();
        public ICollection<TrilhaTreinamento> Trilhas { get; set; } = new List<TrilhaTreinamento>();
        public ICollection<Denuncia> Denuncias { get; set; } = new List<Denuncia>();
        public ICollection<IndicadorDiversidade> Indicadores { get; set; } = new List<IndicadorDiversidade>();
    }

    public class TrilhaTreinamento
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string Titulo { get; set; } = string.Empty;
        [MaxLength(2000)] public string? Descricao { get; set; }
        public int CargaHoraria { get; set; }
        public string Nivel { get; set; } = "Básico";
        public bool Ativa { get; set; } = true;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public ICollection<Conteudo> Conteudos { get; set; } = new List<Conteudo>();
        public ICollection<ProgressoTreinamento> Progressos { get; set; } = new List<ProgressoTreinamento>();
    }

    public class Conteudo
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string Titulo { get; set; } = string.Empty;
        public string? Texto { get; set; }
        public string? UrlVideo { get; set; }
        public string Tipo { get; set; } = "Texto";
        public int Ordem { get; set; }
        public int DuracaoMinutos { get; set; }
        public int TrilhaId { get; set; }
        public TrilhaTreinamento? Trilha { get; set; }
    }

    public class ProgressoTreinamento
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }
        public int TrilhaId { get; set; }
        public TrilhaTreinamento? Trilha { get; set; }
        public int PercentualConcluido { get; set; }
        public bool Concluido { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string? CertificadoUrl { get; set; }
    }

    public class Denuncia
    {
        public int Id { get; set; }
        public string? Protocolo { get; set; }
        [Required, MaxLength(100)] public string Tipo { get; set; } = string.Empty;
        [Required, MaxLength(3000)] public string Relato { get; set; } = string.Empty;
        public bool Anonima { get; set; } = true;
        public string Status { get; set; } = "Aberta";
        public string? ObservacoesInternas { get; set; }
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
        public DateTime? DataResolucao { get; set; }
        public string? UsuarioId { get; set; }
        public ApplicationUser? Usuario { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
    }

    public class IndicadorDiversidade
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public int Ano { get; set; }
        public int Mes { get; set; }
        public int TotalColaboradores { get; set; }
        public int ColaboradoresNegros { get; set; }
        public int ColaboradoresIndigenas { get; set; }
        public int ColaboradoresPcd { get; set; }
        public int MulheresLideranca { get; set; }
        public int NegrosLideranca { get; set; }
        public decimal PercentualAdesaoTreinamentos { get; set; }
        public int TotalDenunciasPeriodo { get; set; }
        public int DenunciasResolvidas { get; set; }
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    }

    public class Mentoria
    {
        public int Id { get; set; }
        public string MentorId { get; set; } = string.Empty;
        public ApplicationUser? Mentor { get; set; }
        public string MentoradoId { get; set; } = string.Empty;
        public ApplicationUser? Mentorado { get; set; }
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
        public string Status { get; set; } = "Ativa";
        public DateTime DataInicio { get; set; } = DateTime.UtcNow;
        public DateTime? DataFim { get; set; }
        [MaxLength(1000)] public string? Objetivos { get; set; }
        [MaxLength(2000)] public string? Observacoes { get; set; }
    }
}
