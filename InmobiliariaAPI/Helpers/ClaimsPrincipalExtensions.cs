using System.Security.Claims;

namespace InmobiliariaAPI.Helpers
{
    /*Metodo de extenxion para ClaimsPrincipal
     * declarado con el primer parámetro precedido por this
     * Eso permite invocarlo usando la sintaxis de método de instancia sobre cualquier objeto del tipo especificado
     */
    public static class ClaimsPrincipalExtensions
    {
        // Devuelve personaId 
        public static int? GetPersonaId(this ClaimsPrincipal user)
        {
            if (user == null) 
                return null;
            
            var claim = user.FindFirst("personaId")?.Value;
            return int.TryParse(claim, out var id) ? id : (int?)null;
        }

        // Devuelve roles 
        public static string[] GetRoles(this ClaimsPrincipal user)
        {
            if (user == null) return Array.Empty<string>();
            return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        }
    }
}
