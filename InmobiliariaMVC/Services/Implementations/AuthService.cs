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

            // 1) Realizar petición a la API y capturar errores de red
            try
            {
                resp = await client.PostAsJsonAsync("auth/login", model);
            }
            catch (HttpRequestException ex)
            {
                // Error de conexión (DNS, rechazada, timeout...)
                return (false, $"Error de conexión con la API: {ex.Message}", Enumerable.Empty<Claim>());
            }
            catch (Exception ex)
            {
                // Cualquier otro error 
                return (false, $"Error inesperado: {ex.Message}", Enumerable.Empty<Claim>());
            }

            // 2) Si la API responde con error (4xx/5xx) intentar extraer mensaje útil
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
                    // Si el body no coincide con ApiResponse, usamos fallback
                }

                // fallback generico: mostrar el texto plano o cod http
                var shortContent = string.IsNullOrWhiteSpace(content) ? resp.ReasonPhrase ?? resp.StatusCode.ToString() : content;
                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return (false, "Credenciales inválidas.", Enumerable.Empty<Claim>());

                return (false, $"API {(int)resp.StatusCode}: {shortContent}", Enumerable.Empty<Claim>());

            }

            // 3) Si es un 200
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

            // 4) Guardar token en Session para que TokenHandler lo use en futuras peticiones.

            _httpContextAccessor.HttpContext?.Session.SetString("ApiToken", token);

            
            // 5) Decodificar JWT para obtener claims 
            JwtSecurityToken jwt;

            try
            {
                jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            }
            catch
            {
                return (false, "Token JWT inválido.", Enumerable.Empty<Claim>());
            }

            // Tomamos las claims que vienen en el JWT
            var claims = jwt.Claims.ToList();

            // 6) Normalizar roles tal como los entregue la API en AuthResponseDTO
            if (apiResp.Result.Roles != null && apiResp.Result.Roles.Length > 0)
            {
                claims.RemoveAll( c => string.Equals(
                    c.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) || 
                    c.Type.EndsWith("role", StringComparison.OrdinalIgnoreCase)
                );

                foreach (var r in apiResp.Result.Roles.Where(r => !string.IsNullOrWhiteSpace(r)))
                    claims.Add(new Claim(ClaimTypes.Role, r.Trim().ToUpperInvariant())
                );
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

            // 7) Asegurar identificadores básicos (NameIdentifier y Email) usando AuthResponseDTO si faltan
            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier) && apiResp.Result != null)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, apiResp.Result.UsuarioId.ToString()));

            if (!claims.Any(c => c.Type == ClaimTypes.Email) && apiResp.Result != null)
                claims.Add(new Claim(ClaimTypes.Email, apiResp.Result.Email ?? string.Empty));

            // 8) Devolver éxito y las claims listas para crear la identidad en el controlador
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
