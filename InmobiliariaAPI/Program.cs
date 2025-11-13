using FluentValidation;
using FluentValidation.AspNetCore;
using InmobiliariaAPI.Data;
using InmobiliariaAPI.InmobiliariaMappers;
using InmobiliariaAPI.Mappers;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository;
using InmobiliariaAPI.Repository.IRepository;
using InmobiliariaAPI.Services;
using InmobiliariaAPI.Services.IServices;
using InmobiliariaAPI.Swagger;
using InmobiliariaAPI.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Contexto - BD
builder.Services.AddDbContext<DataContext>(opciones =>
    opciones.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        new MySqlServerVersion(new Version(8, 0, 41))
    )
);

// Servicios (services)
builder.Services.AddScoped<IPersonaService, PersonaService>();
builder.Services.AddScoped<IInmuebleService, InmuebleService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Repositorios (repositories)
builder.Services.AddScoped<ICommonRepository<Persona>, PersonaRepository>();
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IInmuebleRepository, InmuebleRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Validators (FluentValidation)
builder.Services.AddScoped<IValidator<PersonaCrearDTO>, personaCrearDTOValidator>();
builder.Services.AddScoped<IValidator<PersonaActualizarDTO>, PersonaActualizarDTOValidator>();
builder.Services.AddScoped<IValidator<InmuebleCrearDTO>, InmuebleCrearDTOValidator>();
builder.Services.AddScoped<IValidator<InmuebleActualizarDTO>, InmuebleActualizarDTOValidator>();
builder.Services.AddScoped<IValidator<RoleCrearDTO>, RoleCrearDTOValidator>();
builder.Services.AddScoped<IValidator<RoleActualizarDTO>, RoleActualizarDTOValidator>();
builder.Services.AddScoped<IValidator<UsuarioCrearDTO>, UsuarioCrearDTOValidator>();
builder.Services.AddScoped<IValidator<CambiarPasswordDTO>, CambiarPasswordDTOValidator>();
builder.Services.AddScoped<IValidator<AvatarUploadDTO>, AvatarUploadDTOValidator>();
builder.Services.AddScoped<IValidator<PersonaRoleAsignarDTO>, PersonaRolAsignarDTOValidator>();

//Mappers
builder.Services.AddScoped<PersonaMapeo>();
builder.Services.AddScoped<InmuebleMapeo>();
builder.Services.AddScoped<RoleMapeo>();

//Middleware Errores centralizados
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value.Errors.Select(e => e.ErrorMessage))
            .ToList();

        var response = new InmobiliariaAPI.Models.DTO.ApiResponse<object>
        {
            StatusCode = System.Net.HttpStatusCode.BadRequest,
            IsSuccess = false,
            ErrorMessages = errors,
            Result = null
        };

        return new BadRequestObjectResult(response);
    };
});

// Controllers 
builder.Services.AddControllers();

// HttpContextAccessor (útil para obtener user/claims desde servicios)
builder.Services.AddHttpContextAccessor();

// CORS básico 
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});


// Autenticación JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })

    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
}


// Autorización: política por roles (Administrador y Propietario)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("Propietario", policy => policy.RequireRole("Propietario"));
    
    // Fallback: exigir autenticación por defecto
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autenticación con JWT (Bearer). Introduce: 'Bearer {tu_token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Inmobiliaria API",
        Description = "API de la inmobiliaria"
    });

    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

// Build the app
var app = builder.Build();

// Use Middleware de manejo centralizado de errores
app.UseMiddleware<InmobiliariaAPI.Middleware.ErrorHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inmobiliaria API v1");
        // c.RoutePrefix = string.Empty; // opcional: sirve para exponer swagger en la raíz
    });
}

// Aplicar CORS
app.UseCors();

// Habilitar archivos estaticos (wwwroot)
//app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
