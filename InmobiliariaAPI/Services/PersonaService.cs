using FluentValidation;
using InmobiliariaAPI.Data;
using InmobiliariaAPI.InmobiliariaMappers;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;
using InmobiliariaAPI.Services.IServices;
using InmobiliariaAPI.Validators;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Repository
{
    public class PersonaService : IPersonaService
    {
        private readonly IPersonaRepository _personaRepository;
        private readonly PersonaMapeo _personaMapeo;
        private readonly DataContext _dataContext;


        public PersonaService(
            IPersonaRepository personaRepository,
            PersonaMapeo personaMapeo,
            DataContext dataContext
            )
        {
            _personaRepository = personaRepository;
            _personaMapeo = personaMapeo;
            _dataContext = dataContext;
        }

        // Metodos Base
        public async Task<PersonaObtenerDTO> CreateAsync(PersonaCrearDTO entity)
        {
            // Validaciones
            var personaRepo = _personaRepository as PersonaRepository;
            if (await personaRepo.ExistsByDniAsync(entity.Dni))
                throw new InvalidOperationException($"Ya existe una persona con el DNI {entity.Dni}");

            if (await personaRepo.ExistsByEmailAsync(entity.Email))
                throw new InvalidOperationException($"Ya existe una persona con el email {entity.Email}");

            // Verificar si existe el rol
            var rolExiste = await _dataContext.Roles.AnyAsync(r => r.RolId == entity.IdRol);
            if (!rolExiste)
                throw new InvalidOperationException($"El rol con ID {entity.IdRol} no existe");

            // Crear persona
            var persona = new Persona
            {
                Dni = entity.Dni?.Trim(),
                Apellido = entity.Apellido?.Trim(),
                Nombre = entity.Nombre?.Trim(),
                Telefono = entity.Telefono?.Trim(),
                Email = entity.Email?.Trim(),
                Estado = true
            };

            // Usar transaccion para asegurar guardado de persona y rol
            using var transaction = await _dataContext.Database.BeginTransactionAsync();
            try
            {
                // Guardar persona
                var personaCreada = await _personaRepository.AddAsync(persona);

                // Crear y guardar relación persona-rol
                var personaRol = new PersonaRole
                {
                    PersonaId = personaCreada.PersonaId,
                    RolId = entity.IdRol,
                    FechaAlta = DateTime.Now,
                    Estado = true
                };

                await _dataContext.PersonaRoles.AddAsync(personaRol);
                await _dataContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return _personaMapeo.MapToObtenerDTO(personaCreada);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        public async Task<PersonaObtenerDTO> DeleteAsync(int id, bool estado)
        {
            var persona = await _personaRepository.CambiarEstadoAsync(id, estado);
            if (persona == null)
                return null;

            return _personaMapeo.MapToObtenerDTO(persona);
        }

        public async Task<PersonaObtenerDTO> ExistsAsync(int id)
        {
            var persona = await _personaRepository.GetByIdAsync(id);
            if (persona == null || !persona.Estado)
                return null;

            return _personaMapeo.MapToObtenerDTO(persona);
        }

        public async Task<ICollection<PersonaObtenerDTO>> GetAllAsync()
        {
            var personas = await _personaRepository.GetAllAsync();
            return personas.Select(p => _personaMapeo.MapToObtenerDTO(p)).ToList();
        }

        public async Task<PersonaObtenerDTO> GetByIdAsync(int id)
        {
            var perpsona = await _personaRepository.GetByIdAsync(id);
            if (perpsona == null || !perpsona.Estado)
                return null;

            return _personaMapeo.MapToObtenerDTO(perpsona);
        }

        public async Task<PersonaObtenerDTO> UpdateAsync(int id, PersonaActualizarDTO entity)
        {
            var persona = await _personaRepository.GetByIdAsync(id);
            if (persona == null)
                return null;

            // Verificar duplicados solo si cambió el DNI o email
            var personaRepo = _personaRepository as PersonaRepository;
            if (persona.Dni != entity.Dni && await personaRepo.ExistsByDniAsync(entity.Dni))
                throw new InvalidOperationException($"Ya existe una persona con el DNI {entity.Dni}");

            if (persona.Email != entity.Email && await personaRepo.ExistsByEmailAsync(entity.Email))
                throw new InvalidOperationException($"Ya existe una persona con el email {entity.Email}");

            // Actualizar datos de la persona
            persona.Dni = entity.Dni?.Trim();
            persona.Apellido = entity.Apellido?.Trim();
            persona.Nombre = entity.Nombre?.Trim();
            persona.Telefono = entity.Telefono?.Trim();
            persona.Email = entity.Email?.Trim();
            persona.Estado = entity.Estado;

            // Actualizar la persona
            var personaActualizada = await _personaRepository.UpdateAsync(id, persona);

            return _personaMapeo.MapToObtenerDTO(personaActualizada);


        }

        // Metodos Extras

        public async Task<PersonaObtenerDTO> GetByDniAsync(string dni)
        {
            var personaRepo = _personaRepository as PersonaRepository;
            var persona = await personaRepo.GetByDniAsync(dni);
            return persona != null ? _personaMapeo.MapToObtenerDTO(persona) : null;
        }

        public async Task<PersonaObtenerDTO> GetByEmailAsync(string email)
        {
            var personaRepo = _personaRepository as PersonaRepository;
            var persona = await personaRepo.GetByEmailAsync(email);
            return persona != null ? _personaMapeo.MapToObtenerDTO(persona) : null;
        }


    }
}
