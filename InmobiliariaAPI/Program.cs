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
using InmobiliariaAPI.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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

//Mappers
builder.Services.AddScoped<PersonaMapeo>();
builder.Services.AddScoped<InmuebleMapeo>();
builder.Services.AddScoped<RoleMapeo>();

// Controllers 
builder.Services.AddControllers();

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
}



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
