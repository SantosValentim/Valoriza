/* Isolamento multi-tenant por EmpresaId */
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Valoriza.API.Models;

namespace Valoriza.Web.Helpers
{
    public static class EmpresaAccessHelper
    {
        /// True se o usuário é Admin da plataforma Valoriza
        public static bool IsAdminValoriza(ClaimsPrincipal user) =>
            user.IsInRole("AdminValoriza");

        /// EmpresaId do usuário logado (claim ou perfil).
        public static async Task<int?> GetEmpresaIdAsync(
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.GetUserAsync(principal);
            return user?.EmpresaId;
        }

        /// AdminValoriza NÃO acessa dados operacionais de clientes (trilhas, denúncias, mentorias).
        public static bool PodeAcessarOperacional(ClaimsPrincipal user) =>
            !IsAdminValoriza(user);

        /// AdminValoriza acessa indicadores e visão de usuários admin das empresas.
        public static bool PodeAcessarIndicadoresGlobais(ClaimsPrincipal user) =>
            IsAdminValoriza(user);
    }
}