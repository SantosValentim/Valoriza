/* ============================================================
   DTOs de Resposta Padronizados
   Autor: Oliver Valentim Carvalho Santos - RA 2632071
   ============================================================ */

namespace Valoriza.API.DTOs
{
    public class ApiResponse<T>
    {
        public bool Sucesso { get; set; } = true;
        public string Mensagem { get; set; } = "Operação realizada com sucesso.";
        public T? Dados { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> Ok(T dados, string mensagem = "Operação realizada com sucesso.")
            => new() { Sucesso = true, Mensagem = mensagem, Dados = dados };

        public static ApiResponse<T> Ok(string mensagem = "Operação realizada com sucesso.")
            => new() { Sucesso = true, Mensagem = mensagem, Dados = default };
    }

    public class ApiErrorResponse
    {
        public bool Sucesso { get; set; } = false;
        public string Mensagem { get; set; } = "Ocorreu um erro ao processar a solicitação.";
        public string? CodigoErro { get; set; }
        public string? Detalhes { get; set; }
        public Dictionary<string, string[]>? ErrosValidacao { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiErrorResponse Criar(
            string mensagem,
            string? codigoErro = null,
            string? detalhes = null,
            Dictionary<string, string[]>? errosValidacao = null)
            => new()
            {
                Mensagem = mensagem,
                CodigoErro = codigoErro,
                Detalhes = detalhes,
                ErrosValidacao = errosValidacao
            };
    }

    public class PagedResponse<T>
    {
        public bool Sucesso { get; set; } = true;
        public string Mensagem { get; set; } = "Dados recuperados com sucesso.";
        public List<T> Dados { get; set; } = new();
        public int PaginaAtual { get; set; }
        public int ItensPorPagina { get; set; }
        public int TotalItens { get; set; }
        public int TotalPaginas => ItensPorPagina > 0
            ? (int)Math.Ceiling(TotalItens / (double)ItensPorPagina) : 0;
        public bool TemProximaPagina => PaginaAtual < TotalPaginas;
        public bool TemPaginaAnterior => PaginaAtual > 1;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static PagedResponse<T> Criar(
            List<T> dados, int paginaAtual, int itensPorPagina, int totalItens,
            string mensagem = "Dados recuperados com sucesso.")
            => new()
            {
                Dados = dados,
                PaginaAtual = paginaAtual,
                ItensPorPagina = itensPorPagina,
                TotalItens = totalItens,
                Mensagem = mensagem
            };
    }

    public class UsuarioResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? EmpresaId { get; set; }
        public string? EmpresaNome { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiracao { get; set; }
        public UsuarioResponseDTO Usuario { get; set; } = new();
    }

    public class DenunciaResponseDTO
    {
        public int Id { get; set; }
        public string? Protocolo { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Relato { get; set; } = string.Empty;
        public bool Anonima { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DataRegistro { get; set; }
        public DateTime? DataResolucao { get; set; }
        public string? EmpresaNome { get; set; }
        public string? UsuarioNome { get; set; }
    }

    public class TrilhaResponseDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int CargaHoraria { get; set; }
        public string Nivel { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public int QuantidadeConteudos { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    public class ConteudoResponseDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Texto { get; set; }
        public string? UrlVideo { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public int DuracaoMinutos { get; set; }
    }

    public class ProgressoResponseDTO
    {
        public int Id { get; set; }
        public int TrilhaId { get; set; }
        public string TrilhaTitulo { get; set; } = string.Empty;
        public int PercentualConcluido { get; set; }
        public bool Concluido { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string? CertificadoUrl { get; set; }
    }

    public class IndicadorResponseDTO
    {
        public int Id { get; set; }
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
    }

    public class MentoriaResponseDTO
    {
        public int Id { get; set; }
        public string MentorNome { get; set; } = string.Empty;
        public string MentoradoNome { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? Objetivos { get; set; }
    }
}
