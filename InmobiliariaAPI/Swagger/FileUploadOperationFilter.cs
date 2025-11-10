using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InmobiliariaAPI.Swagger
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Detectar si el método tiene IFormFile como parámetro directo
            var hasFormFile = context.MethodInfo.GetParameters()
                .Any(p => typeof(IFormFile).IsAssignableFrom(p.ParameterType));

            // Si no, detectar IFormFile dentro de DTOs marcados [FromForm]
            if (!hasFormFile)
            {
                hasFormFile = context.MethodInfo.GetParameters()
                    .Where(p => p.GetCustomAttributes().Any(a => a.GetType().Name == "FromFormAttribute"))
                    .SelectMany(p => p.ParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    .Any(pr => typeof(IFormFile).IsAssignableFrom(pr.PropertyType));
            }

            if (!hasFormFile) return;

            // Construir schema multipart/form-data combinando propiedades simples y archivos
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>()
            };

            foreach (var param in context.MethodInfo.GetParameters())
            {
                // Parámetro IFormFile directo
                if (typeof(IFormFile).IsAssignableFrom(param.ParameterType))
                {
                    schema.Properties[param.Name] = new OpenApiSchema { Type = "string", Format = "binary" };
                    continue;
                }

                // DTO marcado [FromForm] o tipo complejo: mapear propiedades públicas
                if (param.GetCustomAttributes().Any(a => a.GetType().Name == "FromFormAttribute") ||
                    (!param.ParameterType.IsPrimitive && param.ParameterType != typeof(string)))
                {
                    foreach (var prop in param.ParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (typeof(IFormFile).IsAssignableFrom(prop.PropertyType))
                        {
                            schema.Properties[prop.Name] = new OpenApiSchema { Type = "string", Format = "binary" };
                        }
                        else if (!schema.Properties.ContainsKey(prop.Name))
                        {
                            schema.Properties[prop.Name] = MapSimpleType(prop.PropertyType);
                        }
                    }
                }
            }

            // Reemplazar RequestBody con multipart/form-data
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = {
                    ["multipart/form-data"] = new OpenApiMediaType { Schema = schema }
                }
            };

            // Eliminar parámetros duplicados generados por Swashbuckle
            if (operation.Parameters != null && operation.Parameters.Count > 0)
            {
                var namesToRemove = schema.Properties.Keys;
                var toRemove = operation.Parameters.Where(p => namesToRemove.Contains(p.Name)).ToList();
                foreach (var p in toRemove)
                {
                    operation.Parameters.Remove(p);
                }
            }
        }

        private static OpenApiSchema MapSimpleType(Type type)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            if (t == typeof(string)) return new OpenApiSchema { Type = "string" };
            if (t == typeof(int) || t == typeof(long)) return new OpenApiSchema { Type = "integer", Format = "int32" };
            if (t == typeof(bool)) return new OpenApiSchema { Type = "boolean" };
            if (t == typeof(decimal) || t == typeof(double) || t == typeof(float)) return new OpenApiSchema { Type = "number", Format = "double" };
            if (t == typeof(DateTime)) return new OpenApiSchema { Type = "string", Format = "date-time" };
            return new OpenApiSchema { Type = "string" };
        }
    }
}