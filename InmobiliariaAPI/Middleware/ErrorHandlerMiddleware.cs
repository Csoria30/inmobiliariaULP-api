using InmobiliariaAPI.Exceptions;
using InmobiliariaAPI.Models.DTO;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Data.Common;
using System.Net;
using System.Security;
using System.Text;
using System.Text.Json;

csharp Middleware/ErrorHandlerMiddleware.cs
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Data.Common;
using System.Net;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InmobiliariaAPI.Middleware
{
    // Middleware centralizado para capturar excepciones no controladas HTTP.
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next; // invocar el siguiente middleware
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        // Constructor
        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;         // guardar referencia al siguiente componente
            _logger = logger;     // guardar logger
            _env = env;
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
                // Excepción no capturada
                _logger.LogError(ex, "Unhandled exception"); // registrar detalle en logs
                await HandleExceptionAsync(context, ex, _env.IsDevelopment());     // construir y enviar respuesta uniforme
            }
        }

        // Metodo estático que construye la respuesta JSON uniforme para errores.
        private static Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
        {
            // Por defecto, consideramos 500 Internal Server Error.
            var code = HttpStatusCode.InternalServerError;
            var userMessages = new List<string>();

            // Mapear excepciones especificas mediante comprobaciones explícitas
            if (exception is ArgumentNullException || exception is ArgumentException || exception is FormatException || exception is ArgumentOutOfRangeException)
            {
                code = HttpStatusCode.BadRequest;
                userMessages.Add("Entrada inválida.");
            }
            else if (exception is System.ComponentModel.DataAnnotations.ValidationException || exception is FluentValidation.ValidationException)
            {
                code = HttpStatusCode.BadRequest;
                userMessages.Add("Errores de validación.");
            }
            else if (exception is KeyNotFoundException || exception is FileNotFoundException)
            {
                code = HttpStatusCode.NotFound;
                userMessages.Add("Recurso no encontrado.");
            }
            else if (exception is UnauthorizedAccessException || exception is SecurityTokenException || exception is System.Security.Authentication.AuthenticationException)
            {
                code = HttpStatusCode.Unauthorized;
                userMessages.Add("No autorizado.");
            }
            else if (exception is SecurityException)
            {
                code = HttpStatusCode.Forbidden;
                userMessages.Add("Acceso prohibido.");
            }
            else if (exception is TimeoutException || exception is OperationCanceledException || exception is TaskCanceledException)
            {
                code = HttpStatusCode.RequestTimeout;
                userMessages.Add("La operación ha expirado.");
            }
            else if (exception is NotImplementedException)
            {
                code = HttpStatusCode.NotImplemented;
                userMessages.Add("Funcionalidad no implementada.");
            }
            else if (exception is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                code = HttpStatusCode.Conflict;
                userMessages.Add("Conflicto de concurrencia.");
            }
            else if (exception is Microsoft.EntityFrameworkCore.DbUpdateException || exception is DbException || exception is MySqlException)
            {
                code = HttpStatusCode.Conflict;
                userMessages.Add("Error en la base de datos.");
            }
            else if (exception is InvalidOperationException)
            {
                code = HttpStatusCode.BadRequest;
                userMessages.Add("Operación inválida.");
            }
            else if (exception is NotFoundException)
            {
                code = HttpStatusCode.NotFound;
                userMessages.Add(exception.Message); // se mostrará el mensaje (y en Development aparecerá primero si lo tienes configurado)
            }
            else
            {
                // Para excepciones no mapeadas, mantener 500 y mensaje genérico.
                userMessages.Add("Error interno del servidor.");
            }

            // Si estamos en Development, añadir el message original (útil para debugging)
            if (isDevelopment)
            {
                userMessages.Insert(0, exception.Message);
            }

            // Usar mensajes controlados para devolver al cliente; si no hay mensajes, exponer genérico.
            var responseMessages = userMessages.Count > 0
                ? userMessages
                : new List<string> { "Error interno del servidor." };

            // Construir el objeto de respuesta estandar (ApiResponse<object>)
            var response = new ApiResponse<object>
            {
                StatusCode = code,
                IsSuccess = false,
                ErrorMessages = responseMessages,
                Result = null
            };

            var payload = JsonSerializer.Serialize(response);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(payload);
        }
    }
}