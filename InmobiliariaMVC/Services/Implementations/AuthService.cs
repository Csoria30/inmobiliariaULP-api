using InmobiliariaDTO;
using InmobiliariaDTO.Models.DTO;
using InmobiliariaMVC.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace InmobiliariaMVC.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<(bool Success, string Error, IEnumerable<Claim> Claims)> AuthenticateAsync(UsuarioLoginDTO model)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            HttpResponseMessage resp;

            // Manejo de excepciones para errores de conexión
            try
            {
                resp = await client.PostAsJsonAsync("auth/login", model);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexión con la API: {ex.Message}", Enumerable.Empty<Claim>());
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}", Enumerable.Empty<Claim>());
            }

            // Si no es un 200...
            if (!resp.IsSuccessStatusCode)
            {
                var content = await resp.Content.ReadAsStringAsync();

                try
                {
                    // Intentar deserializar al contrato ApiResponse<T> común
                    var apiErr = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiErr != null)
                    {
                        if (apiErr.ErrorMessages != null && apiErr.ErrorMessages.Count > 0)
                            return (false, string.Join("; ", apiErr.ErrorMessages), Enumerable.Empty<Claim>());

                        if (!string.IsNullOrWhiteSpace(apiErr.Result?.ToString()))
                            return (false, apiErr.Result.ToString(), Enumerable.Empty<Claim>());
                    }
                }
                catch
                {
                    // ignorar parseo si no corresponde
                }

                // fallback: mostrar el texto plano o código http
                var shortContent = string.IsNullOrWhiteSpace(content) ? resp.ReasonPhrase ?? resp.StatusCode.ToString() : content;
                return (false, $"API {(int)resp.StatusCode}: {shortContent}", Enumerable.Empty<Claim>());

            }

            // Si es un 200
            ApiResponse<AuthResponseDTO>? apiResp;

            try
            {
                apiResp = await resp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDTO>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                return (false, $"Respuesta inválida de la API: {ex.Message}", Enumerable.Empty<Claim>());
            }

            if (apiResp == null || apiResp.Result == null)
                return (false, "La API no devolvió datos de autenticación.", Enumerable.Empty<Claim>());

            var token = apiResp.Result.Token;
            if (string.IsNullOrWhiteSpace(token))
                return (false, "Token vacío recibido desde la API.", Enumerable.Empty<Claim>());

            //Guardar el token en la sesión
            _httpContextAccessor.HttpContext?.Session.SetString("ApiToken", token);

            // Decodificar JWT y construir claims
            JwtSecurityToken jwt;

            try
            {
                jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            }
            catch
            {
                return (false, "Token JWT inválido.", Enumerable.Empty<Claim>());
            }

            var claims = jwt.Claims.ToList();

            // Preferir roles desde AuthResponseDTO si vienen
            if (apiResp.Result.Roles != null && apiResp.Result.Roles.Length > 0)
            {
                claims.RemoveAll(c => c.Type == ClaimTypes.Role || c.Type.EndsWith("role", StringComparison.OrdinalIgnoreCase));
                foreach (var r in apiResp.Result.Roles.Where(r => !string.IsNullOrWhiteSpace(r)))
                    claims.Add(new Claim(ClaimTypes.Role, r.Trim()));
            }
            else
            {
                // Mapear posibles claim names de roles a ClaimTypes.Role
                var roleClaims = jwt.Claims.Where(c =>
                    string.Equals(c.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) ||
                    c.Type.EndsWith("role", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase));

                foreach (var rc in roleClaims)
                    claims.Add(new Claim(ClaimTypes.Role, rc.Value));
            }

            // Asegurar identificadores básicos
            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier) && apiResp.Result != null)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, apiResp.Result.UsuarioId.ToString()));

            if (!claims.Any(c => c.Type == ClaimTypes.Email) && apiResp.Result != null)
                claims.Add(new Claim(ClaimTypes.Email, apiResp.Result.Email ?? string.Empty));

            return (true, string.Empty, claims);

        }

        public Task LogoutAsync()
        {
            try
            {
                _httpContextAccessor.HttpContext?.Session.Remove("ApiToken");
            }
            catch
            {
                // ignore
            }
            return Task.CompletedTask;
        }
    }
}
