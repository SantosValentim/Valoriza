/* ============================================================
   Filtro Global de Exceções
   ============================================================ */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Valoriza.API.DTOs;

namespace Valoriza.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Erro não tratado: {Message}", context.Exception.Message);

            var statusCode = context.Exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            var response = ApiErrorResponse.Criar(
                mensagem: statusCode == 500
                    ? "Ocorreu um erro interno no servidor."
                    : context.Exception.Message,
                codigoErro: $"ERR_{statusCode}",
                detalhes: _env.IsDevelopment() ? context.Exception.ToString() : null
            );

            context.Result = new ObjectResult(response) { StatusCode = statusCode };
            context.ExceptionHandled = true;
        }
    }
}
