using InmobiliariaAPI.Models.DTO;
using System.Net;
using System.Security;
using System.Text;
using System.Text.Json;

namespace InmobiliariaAPI.Middleware
{
    // Middleware centralizado para capturar excepciones no controladas HTTP.
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next; // invocar el siguiente middleware
        private readonly ILogger<ErrorHandlerMiddleware> _logger; 

        // Constructor
        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;         // guardar referencia al siguiente componente
            _logger = logger;     // guardar logger
        }
        
        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Delegar la ejecución al siguiente middleware en la pipeline.
                await _next(context);

                // Si code 401 o 403 y aun no se ha iniciado la respuesta, se necesita autenticación/permiso
                if (!context.Response.HasStarted &&
                    (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized ||
                     context.Response.StatusCode == (int)HttpStatusCode.Forbidden))
                {
                    var code = (HttpStatusCode)context.Response.StatusCode;
                    var message = code == HttpStatusCode.Unauthorized
                        ? "No autorizado. Es necesario iniciar sesión."
                        : "Acceso prohibido. No tienes permisos para esta operación.";

                    var response = new ApiResponse<object>
                    {
                        StatusCode = code,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { message },
                        Result = null
                    };

                    var payload = JsonSerializer.Serialize(response);

                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength = Encoding.UTF8.GetByteCount(payload);
                    context.Response.StatusCode = (int)code;

                    await context.Response.WriteAsync(payload);
                }


            }
            catch (Exception ex)
            {
                // Eexcepcion no capturada
                _logger.LogError(ex, "Unhandled exception"); // registrar detalle en logs
                await HandleExceptionAsync(context, ex);     // construir y enviar respuesta uniforme
            }
        }

        // Metodo estático que construye la respuesta JSON uniforme para errores.
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Por defecto, consideramos 500 Internal Server Error.
            var code = HttpStatusCode.InternalServerError;
            var userMessages = new List<string>();

            // Mapear excepciones especificas
            switch (exception)
            {
                case ArgumentNullException:
                case ArgumentException:
                case FormatException:
                    code = HttpStatusCode.BadRequest;
                    userMessages.Add("Entrada inválida.");
                    break;

                case System.ComponentModel.DataAnnotations.ValidationException:
                case FluentValidation.ValidationException:
                    code = HttpStatusCode.BadRequest;
                    userMessages.Add("Errores de validación.");
                    break;

                case KeyNotFoundException:
                case FileNotFoundException:
                    code = HttpStatusCode.NotFound;
                    userMessages.Add("Recurso no encontrado.");
                    break;

                case UnauthorizedAccessException:
                case System.Security.Authentication.AuthenticationException:
                    code = HttpStatusCode.Unauthorized;
                    userMessages.Add("No autorizado.");
                    break;

                case SecurityException:
                    code = HttpStatusCode.Forbidden;
                    userMessages.Add("Acceso prohibido.");
                    break;

                case TimeoutException:
                case OperationCanceledException:
                    code = HttpStatusCode.RequestTimeout;
                    userMessages.Add("La operación ha expirado.");
                    break;

                case NotImplementedException:
                    code = HttpStatusCode.NotImplemented;
                    userMessages.Add("Funcionalidad no implementada.");
                    break;

                // Si usas EF Core, considera mapear DbUpdateConcurrencyException a 409
                case Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException:
                    code = HttpStatusCode.Conflict;
                    userMessages.Add("Conflicto de concurrencia.");
                    break;

                case InvalidOperationException:
                    code = HttpStatusCode.BadRequest;
                    userMessages.Add("Operación inválida.");
                    break;

                default:
                    // Para excepciones no mapeadas, mantener 500 y mensaje genérico.
                    userMessages.Add("Error interno del servidor.");
                    break;
            }


            // Construir el objeto de respuesta estandar (ApiResponse<object>)
            var response = new ApiResponse<object>
            {
                StatusCode = code,                        
                IsSuccess = false,                        
                ErrorMessages = new List<string> { exception.Message }, // mensaje para cliente (simple)
                Result = null                             // no hay resultado válido
            };

            // Serializar la respuesta a JSON 
            var payload = JsonSerializer.Serialize(response);

            // Preparar la respuesta HTTP
            context.Response.ContentType = "application/json";    // tipo de contenido
            context.Response.StatusCode = (int)code;             // código HTTP numérico

            
            return context.Response.WriteAsync(payload);
        }
    }
}
