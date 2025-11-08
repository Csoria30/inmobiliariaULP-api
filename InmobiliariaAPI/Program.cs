using FluentValidation;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Contexto - BD
builder.Services.AddDbContext<DataContext>(opciones =>
    opciones.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        new MySqlServerVersion(new Version(8, 0, 41))
    )
);

//Servicios
builder.Services.AddScoped<IPersonaService, PersonaService>();
builder.Services.AddScoped<IInmuebleService, InmuebleService>();
builder.Services.AddScoped<IRoleService, RoleService>();

//Repositorios
builder.Services.AddScoped<ICommonRepository<Persona>, PersonaRepository>();
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IInmuebleRepository, InmuebleRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Validators
builder.Services.AddScoped<IValidator<PersonaCrearDTO>, personaCrearDTOValidator>();
builder.Services.AddScoped<IValidator<PersonaActualizarDTO>, PersonaActualizarDTOValidator>();
builder.Services.AddScoped<IValidator<InmuebleCrearDTO>, InmuebleCrearDTOValidator>();
builder.Services.AddScoped<IValidator<InmuebleActualizarDTO>, InmuebleActualizarDTOValidator>();
builder.Services.AddScoped<IValidator<RoleCrearDTO>, RoleCrearDTOValidator>();
builder.Services.AddScoped<IValidator<RoleActualizarDTO>, RoleActualizarDTOValidator>();

//Mappers
builder.Services.AddScoped<PersonaMapeo>();
builder.Services.AddScoped<InmuebleMapeo>();
builder.Services.AddScoped<RoleMapeo>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

app.UseAuthorization();

app.MapControllers();

app.Run();
