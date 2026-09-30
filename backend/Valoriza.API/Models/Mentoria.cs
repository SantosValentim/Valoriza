/* Mentoria + Fórum onde cada empresa só vê os próprios dados e o mentor escolhido por AdminEmpresa ou GestorDEI */

using System.ComponentModel.DataAnnotations;

namespace Valoriza.API.Models
{
    /// Programa de mentoria de UMA empresa com um mentor (colaborador) e vários mentorados inscritos.
    public class MentoriaPrograma
    {
        public int Id { get; set; }

        /// Empresa dona do programa
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        /// Colaborador escolhido como mentor
        [Required]
        public string MentorId { get; set; } = string.Empty;
        public ApplicationUser? Mentor { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Objetivos { get; set; }

        /// Ativa, encerrada ou pausada
        public string Status { get; set; } = "Ativa";

        public DateTime DataInicio { get; set; } = DateTime.UtcNow;
        public DateTime? DataFim { get; set; }

        public ICollection<MentoriaInscricao> Inscricoes { get; set; } = new List<MentoriaInscricao>();
        public ICollection<MentoriaPost> Posts { get; set; } = new List<MentoriaPost>();
    }

    /// Inscrição de um mentorado (colaborador) em um programa.
    public class MentoriaInscricao
    {
        public int Id { get; set; }

        public int MentoriaProgramaId { get; set; }
        public MentoriaPrograma? Programa { get; set; }

        [Required]
        public string MentoradoId { get; set; } = string.Empty;
        public ApplicationUser? Mentorado { get; set; }
        public DateTime DataInscricao { get; set; } = DateTime.UtcNow;

        /// Ativa, cancelada
        public string Status { get; set; } = "Ativa";
    }

    /// Post do fórum (mentor publica conteúdo, notícia, link, vídeo, foto).
    public class MentoriaPost
    {
        public int Id { get; set; }

        public int MentoriaProgramaId { get; set; }
        public MentoriaPrograma? Programa { get; set; }

        /// Quem publicou (em geral o mentor)
        [Required]
        public string AutorId { get; set; } = string.Empty;
        public ApplicationUser? Autor { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        ///Texto do post
        [MaxLength(5000)]
        public string? Conteudo { get; set; }

        ///Link externo (notícia, palestra, etc.)
        [MaxLength(1000)]
        public string? UrlLink { get; set; }

        ///Caminho ou URL de foto/vídeo anexado
        [MaxLength(1000)]
        public string? UrlMidia { get; set; }

        ///Nenhuma, imagem, video
        public string TipoMidia { get; set; } = "Nenhuma";
        public DateTime DataPublicacao { get; set; } = DateTime.UtcNow;
        public ICollection<MentoriaResposta> Respostas { get; set; } = new List<MentoriaResposta>();
    }

    /// Resposta de mentorado (ou mentor) a um post do fórum.
    public class MentoriaResposta
    {
        public int Id { get; set; }

        public int MentoriaPostId { get; set; }
        public MentoriaPost? Post { get; set; }

        [Required]
        public string AutorId { get; set; } = string.Empty;
        public ApplicationUser? Autor { get; set; }

        [Required, MaxLength(3000)]
        public string Texto { get; set; } = string.Empty;

        public DateTime Data { get; set; } = DateTime.UtcNow;
    }
}