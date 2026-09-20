/* ============================================================
   DTOs de Entrada com Validação
   ============================================================ */

using System.ComponentModel.DataAnnotations;

namespace Valoriza.API.DTOs
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class CriarDenunciaRequestDTO
    {
        [Required(ErrorMessage = "O tipo da denúncia é obrigatório.")]
        [StringLength(100)]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O relato é obrigatório.")]
        [StringLength(3000, MinimumLength = 20, ErrorMessage = "O relato deve ter entre 20 e 3000 caracteres.")]
        public string Relato { get; set; } = string.Empty;

        public bool Anonima { get; set; } = true;
    }

    public class AtualizarStatusDenunciaRequestDTO
    {
        [Required]
        [RegularExpression("^(Aberta|EmAnalise|Resolvida|Arquivada)$",
            ErrorMessage = "Status inválido. Use: Aberta, EmAnalise, Resolvida ou Arquivada.")]
        public string Status { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? ObservacoesInternas { get; set; }
    }

    public class CriarTrilhaRequestDTO
    {
        [Required, StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descricao { get; set; }

        [Range(1, 1000)]
        public int CargaHoraria { get; set; }

        [Required]
        [RegularExpression("^(Básico|Intermediário|Avançado)$")]
        public string Nivel { get; set; } = "Básico";

        [Required]
        public int EmpresaId { get; set; }
    }

    public class CriarConteudoRequestDTO
    {
        [Required, StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        public string? Texto { get; set; }
        public string? UrlVideo { get; set; }

        [Required]
        [RegularExpression("^(Texto|Video|Quiz)$")]
        public string Tipo { get; set; } = "Texto";

        [Range(1, 100)]
        public int Ordem { get; set; }

        [Range(1, 300)]
        public int DuracaoMinutos { get; set; }
    }

    public class CriarIndicadorRequestDTO
    {
        [Required]
        public int EmpresaId { get; set; }

        [Range(2020, 2035)]
        public int Ano { get; set; }

        [Range(1, 12)]
        public int Mes { get; set; }

        [Range(0, int.MaxValue)] public int TotalColaboradores { get; set; }
        [Range(0, int.MaxValue)] public int ColaboradoresNegros { get; set; }
        [Range(0, int.MaxValue)] public int ColaboradoresIndigenas { get; set; }
        [Range(0, int.MaxValue)] public int ColaboradoresPcd { get; set; }
        [Range(0, int.MaxValue)] public int MulheresLideranca { get; set; }
        [Range(0, int.MaxValue)] public int NegrosLideranca { get; set; }
        [Range(0, 100)] public decimal PercentualAdesaoTreinamentos { get; set; }
        [Range(0, int.MaxValue)] public int TotalDenunciasPeriodo { get; set; }
        [Range(0, int.MaxValue)] public int DenunciasResolvidas { get; set; }
    }

    public class CriarMentoriaRequestDTO
    {
        [Required] public string MentorId { get; set; } = string.Empty;
        [Required] public string MentoradoId { get; set; } = string.Empty;
        [Required] public int EmpresaId { get; set; }
        [StringLength(1000)] public string? Objetivos { get; set; }
        [StringLength(2000)] public string? Observacoes { get; set; }
    }

    public class AtualizarMentoriaRequestDTO
    {
        [RegularExpression("^(Ativa|Concluida|Cancelada)$")]
        public string Status { get; set; } = "Ativa";

        [StringLength(1000)] public string? Objetivos { get; set; }
        [StringLength(2000)] public string? Observacoes { get; set; }
    }

    public class CriarUsuarioRequestDTO
    {
        [Required, StringLength(150)]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Senha { get; set; } = string.Empty;

        [StringLength(14)]
        public string? Cpf { get; set; }

        public int? EmpresaId { get; set; }

        [Required]
        [RegularExpression("^(AdminValoriza|AdminEmpresa|GestorDEI|Colaborador)$")]
        public string Role { get; set; } = "Colaborador";
    }

    public class AtualizarUsuarioRequestDTO
    {
        [StringLength(150)] public string? NomeCompleto { get; set; }
        [StringLength(14)] public string? Cpf { get; set; }
        public bool? Ativo { get; set; }
        public int? EmpresaId { get; set; }
    }
}
