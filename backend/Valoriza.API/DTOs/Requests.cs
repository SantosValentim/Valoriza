/* DTOs de ENTRADA (Request) com validação
   Validação: DataAnnotations + ValidationFilter */

using System.ComponentModel.DataAnnotations;

namespace Valoriza.API.DTOs
{
    // AUTH
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string Senha { get; set; } = string.Empty;
    }

    // DENÚNCIAS
    /// Criar denúncia (qualquer usuário autenticado).
    public class CriarDenunciaRequestDTO
    {
        [StringLength(200)]
        public string? Titulo { get; set; }

        [Required(ErrorMessage = "A descrição da denúncia é obrigatória.")]
        [StringLength(3000, MinimumLength = 20,
            ErrorMessage = "A descrição deve ter entre 20 e 3000 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        /// Discriminação | Assédio | Racismo | Outro
        [StringLength(80)]
        public string? Categoria { get; set; }

        /// Se true, gestores não veem o nome do autor. UsuarioId continua gravado para o autor acompanhar.
        public bool Anonima { get; set; } = false;
    }

    /// Atualizar status (Gestor/Admin da mesma empresa).
    public class AtualizarStatusDenunciaRequestDTO
    {
        [Required]
        [RegularExpression(
            "^(Aberta|Em análise|EmAnalise|Em investigação|Resolvida|Arquivada)$",
            ErrorMessage = "Status inválido.")]
        public string Status { get; set; } = string.Empty;

        /// Observações de andamento (visíveis ao autor).
        [StringLength(2000)]
        public string? Observacoes { get; set; }
    }

    // TREINAMENTOS
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

        /// Texto | Video | Quiz
        public string Tipo { get; set; } = "Texto";

        public int Ordem { get; set; }
        public int DuracaoMinutos { get; set; }

        public string? OpcaoA { get; set; }
        public string? OpcaoB { get; set; }
        public string? OpcaoC { get; set; }
        public string? OpcaoD { get; set; }

        /// A | B | C | D
        public string? RespostaCorreta { get; set; }
    }

    public class ResponderQuizRequestDTO
    {
        /// A | B | C | D
        [Required]
        public string Resposta { get; set; } = string.Empty;
    }

    // INDICADORES
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

    // MENTORIA
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

    public class CriarProgramaMentoriaDto
    {
        [Required, StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Objetivos { get; set; }

        [Required]
        public string MentorId { get; set; } = string.Empty;
    }

    public class CriarPostMentoriaDto
    {
        [Required, StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? Conteudo { get; set; }

        [StringLength(1000)]
        public string? UrlLink { get; set; }

        [StringLength(1000)]
        public string? UrlMidia { get; set; }

        /// Nenhuma | Imagem | Video
        public string? TipoMidia { get; set; }
    }

    // USUÁRIOS
    public class CriarUsuarioRequestDTO
    {
        [Required, StringLength(150)]
        public string NomeCompleto { get; set; } = string.Empty;

        [StringLength(150)]
        public string? NomeSocial { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Senha { get; set; } = string.Empty;

        [StringLength(14)]
        public string? Cpf { get; set; }

        [StringLength(40)]
        public string? Genero { get; set; }

        [StringLength(40)]
        public string? Etnia { get; set; }

        [StringLength(120)]
        public string? Cargo { get; set; }

        public decimal? Salario { get; set; }

        public int? EmpresaId { get; set; }

        [Required]
        [RegularExpression("^(AdminValoriza|AdminEmpresa|GestorDEI|Colaborador)$")]
        public string Role { get; set; } = "Colaborador";
    }

    public class AtualizarUsuarioRequestDTO
    {
        [StringLength(150)] public string? NomeCompleto { get; set; }
        [StringLength(150)] public string? NomeSocial { get; set; }
        [StringLength(14)] public string? Cpf { get; set; }
        [StringLength(40)] public string? Genero { get; set; }
        [StringLength(40)] public string? Etnia { get; set; }
        [StringLength(120)] public string? Cargo { get; set; }
        public decimal? Salario { get; set; }
        public bool? Ativo { get; set; }
        public int? EmpresaId { get; set; }
    }
}