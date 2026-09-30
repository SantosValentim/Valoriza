/* VALORIZA – Modelos de domínio
   Autor: Oliver Valentim Carvalho Santos - RA 2632071 */

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Valoriza.API.Models
{
    // Usuário do sistema (estende IdentityUser para login/senha)
    public class ApplicationUser : IdentityUser
    {
        /// URL ou caminho da foto de perfil
        [MaxLength(500)]
        public string? FotoUrl { get; set; }

        [Required, MaxLength(150)]
        public string NomeCompleto { get; set; } = string.Empty;

        /// Nome social (opcional)
        [MaxLength(150)]
        public string? NomeSocial { get; set; }

        [MaxLength(14)]
        public string? Cpf { get; set; }

        /// Feminino, Masculino, Não-binário, Prefere não informar, Outro
        [MaxLength(40)]
        public string? Genero { get; set; }

        /// Preta, Parda, Branca, Indígena, Amarela, Prefere não informar
        [MaxLength(40)]
        public string? Etnia { get; set; }

        ///Cargo na empresa
        [MaxLength(120)]
        public string? Cargo { get; set; }

        /// Salário (uso interno RH / equidade salarial)
        public decimal? Salario { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;

        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        public ICollection<Denuncia> Denuncias { get; set; } = new List<Denuncia>();
        public ICollection<ProgressoTreinamento> Progressos { get; set; } = new List<ProgressoTreinamento>();
        public ICollection<Mentoria> MentoriasComoMentor { get; set; } = new List<Mentoria>();
        public ICollection<Mentoria> MentoriasComoMentorado { get; set; } = new List<Mentoria>();
    }

    // Empresa cliente da plataforma SaaS
    public class Empresa
    {
        public int Id { get; set; } // gerado pelo banco de dados

        [Required, MaxLength(200)]
        public string RazaoSocial { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? NomeFantasia { get; set; }

        [Required, MaxLength(18)]
        public string Cnpj { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Segmento { get; set; } // setor

        public int QuantidadeColaboradores { get; set; }
        public string Plano { get; set; } = "Basico"; // Basico | Profissional | Empresarial
        public DateTime? DataInicioAssinatura { get; set; }
        public bool Ativa { get; set; } = true;

        ///Mensal | Anual
        [MaxLength(20)]
        public string CicloCobranca { get; set; } = "Mensal";

        /// Valor da assinatura (R$)
        public decimal ValorAssinatura { get; set; }

        /// Quando a empresa cancelou / churnou (null = ativa)
        public DateTime? DataChurn { get; set; }

        /// Motivo do cancelamento
        [MaxLength(500)]
        public string? MotivoChurn { get; set; }

        /// Ativa | Inadimplente | Churn
        [MaxLength(30)]
        public string StatusAssinatura { get; set; } = "Ativa";

        public ICollection<ApplicationUser> Usuarios { get; set; } = new List<ApplicationUser>();
        public ICollection<TrilhaTreinamento> Trilhas { get; set; } = new List<TrilhaTreinamento>();
        public ICollection<Denuncia> Denuncias { get; set; } = new List<Denuncia>();
        public ICollection<IndicadorDiversidade> Indicadores { get; set; } = new List<IndicadorDiversidade>();
    }

    // Trilha de capacitação DEI
    public class TrilhaTreinamento
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Descricao { get; set; }

        public int CargaHoraria { get; set; }
        public string Nivel { get; set; } = "Básico"; // Básico | Intermediário | Avançado
        public bool Ativa { get; set; } = true;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        public ICollection<Conteudo> Conteudos { get; set; } = new List<Conteudo>();
        public ICollection<ProgressoTreinamento> Progressos { get; set; } = new List<ProgressoTreinamento>();
    }

    // Conteúdo dentro de uma trilha (texto, vídeo ou quiz)
    public class Conteudo
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;
        public string? Texto { get; set; }
        public string? UrlVideo { get; set; }
        public string Tipo { get; set; } = "Texto"; // Texto | Video | Quiz
        public int Ordem { get; set; }
        public int DuracaoMinutos { get; set; }

        public int TrilhaId { get; set; }
        public TrilhaTreinamento? Trilha { get; set; }

        //CAMPO DO QUIZ
        [MaxLength(500)]
        public string? OpcaoA { get; set; }
        [MaxLength(500)]
        public string? OpcaoB { get; set; }
        [MaxLength(500)]
        public string? OpcaoC { get; set; }
        [MaxLength(500)]
        public string? OpcaoD { get; set; }
        [MaxLength(1)]
        public string? RespostaCorreta { get; set; }
    }

    public class ProgressoTreinamento
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }
        public int TrilhaId { get; set; }
        public TrilhaTreinamento? Trilha { get; set; }

        ///0 a 100
        public int PercentualConcluido { get; set; }

        public bool Concluido { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string? CertificadoUrl { get; set; }
    }

    /* Progresso do colaborador em cada conteúdo da trilha */
    public class ConteudoProgresso
    {
        public int Id { get; set; }

        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        public int ConteudoId { get; set; }
        public Conteudo? Conteudo { get; set; }

        public DateTime DataConclusao { get; set; } = DateTime.UtcNow;

        /// Para Quiz: resposta escolhida (A/B/C/D)
        [MaxLength(1)]
        public string? RespostaQuiz { get; set; }

        ///True se acertou o quiz (ou se não é quiz)
        public bool Acertou { get; set; } = true;
    }

    // Canal de denúncias confidencial
    public class Denuncia
    {
        public int Id { get; set; }

        [Required, MaxLength(40)]
        public string Protocolo { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        ///Texto do relato
        [Required]
        public string Descricao { get; set; } = string.Empty;

        ///Categoria / tipo (Discriminação, Assédio, Racismo, Outro)
        [MaxLength(80)]
        public string? Categoria { get; set; }

        [Required, MaxLength(40)]
        public string Status { get; set; } = "Aberta";

        public bool Anonima { get; set; }

        ///Observações de andamento (visíveis ao autor)
        public string? Observacoes { get; set; }

        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;

        /// Preenchida quando Status = Resolvida
        public DateTime? DataResolucao { get; set; }

        public string? UsuarioId { get; set; }
        public ApplicationUser? Usuario { get; set; }

        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
    }

    // Indicadores mensais de diversidade
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

    // Programa de mentoria
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

        [MaxLength(1000)]
        public string? Objetivos { get; set; }

        [MaxLength(2000)]
        public string? Observacoes { get; set; }
    }

    /* Comunicação inclusiva – orientações e checklists para gestores */
    /// Texto de orientação (consulta)
    public class OrientacaoInclusiva
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        /// Linguagem | Reuniao | Feedback | Recrutamento | Geral
        [MaxLength(40)]
        public string Categoria { get; set; } = "Geral";

        [Required]
        public string Conteudo { get; set; } = string.Empty;

        public int Ordem { get; set; }
        public bool Ativa { get; set; } = true;
    }

    /// Item de checklist acionável
    public class ChecklistInclusivoItem
    {
        public int Id { get; set; }

        [Required, MaxLength(300)]
        public string Texto { get; set; } = string.Empty;

        /// AntesReuniao | Email | Feedback | Recrutamento | Semanal
        [MaxLength(40)]
        public string Contexto { get; set; } = "Semanal";

        public int Ordem { get; set; }
        public bool Ativo { get; set; } = true;
    }

    /// Gestor marcou o item como feito (em uma data)
    public class ChecklistInclusivoConclusao
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ChecklistInclusivoItem? Item { get; set; }

        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        /// Dia em que foi marcado (para acompanhamento semanal)
        public DateTime DataReferencia { get; set; }
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    }

    /* Registro de auditoria – operações críticas */
    public class AuditoriaLog
    {
        public long Id { get; set; }

        public DateTime DataUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string UsuarioId { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? UsuarioEmail { get; set; }

        /// Denuncia.Criar | Denuncia.Status | Usuario.Desativar | Empresa.Churn...
        [MaxLength(80)]
        public string Acao { get; set; } = string.Empty;

        [MaxLength(80)]
        public string? Entidade { get; set; }

        [MaxLength(80)]
        public string? EntidadeId { get; set; }

        /// Detalhes em texto (sem dados sensíveis desnecessários)
        public string? Detalhes { get; set; }

        [MaxLength(45)]
        public string? Ip { get; set; }
    }
}