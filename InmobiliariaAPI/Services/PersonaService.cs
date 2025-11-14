using FluentValidation;
using InmobiliariaAPI.Data;
using InmobiliariaAPI.Exceptions;
using InmobiliariaAPI.InmobiliariaMappers;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;
using InmobiliariaAPI.Services.IServices;
using InmobiliariaAPI.Validators;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Net;

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

        // Metodos Base - Persona
        public async Task<PersonaObtenerDTO> CreateAsync(PersonaCrearDTO entity)
        {
            // Validaciones
            if (await _personaRepository.ExistsByDniAsync(entity.Dni))
                throw new InvalidOperationException($"Ya existe una persona con el DNI {entity.Dni}");

            if (await _personaRepository.ExistsByEmailAsync(entity.Email))
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
                throw new NotFoundException($"La persona con ID {id} no existe.");


            return _personaMapeo.MapToObtenerDTO(persona);
        }

        public async Task<PersonaObtenerDTO> ExistsAsync(int id)
        {
            var persona = await _personaRepository.GetByIdAsync(id);
            if (persona == null || !persona.Estado)
                throw new NotFoundException($"La persona con ID {id} no existe.");

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
                throw new NotFoundException($"La persona con ID {id} no existe.");

            return _personaMapeo.MapToObtenerDTO(perpsona);
        }

        public async Task<PersonaObtenerDTO> UpdateAsync(int id, PersonaActualizarDTO entity)
        {
            var persona = await _personaRepository.GetByIdAsync(id);
            if (persona == null)
                throw new NotFoundException($"La persona con ID {id} no existe.");

            // Verificar duplicados solo si cambio el DNI o email
            if (!string.Equals(persona.Dni, entity.Dni, StringComparison.OrdinalIgnoreCase) && await _personaRepository.ExistsByDniAsync(entity.Dni))
                throw new DuplicateResourceException($"Ya existe una persona con el DNI {entity.Dni}");

            if (!string.Equals(persona.Email, entity.Email, StringComparison.OrdinalIgnoreCase) && await _personaRepository.ExistsByEmailAsync(entity.Email))
                throw new DuplicateResourceException($"Ya existe una persona con el email {entity.Email}");

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
            var persona = await _personaRepository.GetByDniAsync(dni);
            if (persona == null)
                throw new NotFoundException($"La persona con DNI {dni} no existe.");

            return _personaMapeo.MapToObtenerDTO(persona);
        }

        public async Task<PersonaObtenerDTO> GetByEmailAsync(string email)
        {
            var persona = await _personaRepository.GetByEmailAsync(email);
            if (persona == null)
                throw new NotFoundException($"La persona con EMAIL {email} no existe.");

            return _personaMapeo.MapToObtenerDTO(persona);
        }

        public async Task<PersonaObtenerDTO> GetByIdWithRolesAsync(int id)
        {
            var persona = await _personaRepository.GetByIdAsync(id);
            if (persona == null || !persona.Estado)
                throw new NotFoundException($"La persona con ID {id} no existe o no esta habilitada.");

            var dto = _personaMapeo.MapToObtenerDTO(persona);

            // Aseguramos que Roles en el DTO estén poblados con roles activos
            dto.Roles = persona.PersonaRoles?
                .Where(pr => pr.Estado && pr.Role != null)
                .Select(pr => new RoleObtenerDTO
                {
                    RolId = pr.RolId,
                    Nombre = pr.Role?.Nombre,
                    Descripcion = pr.Role?.Descripcion
                })
                .ToList() ?? new List<RoleObtenerDTO>();

            return dto;
        }

        public async Task<bool> HasValidRoleForUserAsync(int personaId)
        {
            var roles = await _personaRepository.GetActiveRolesAsync(personaId);
            if (roles == null || roles.Count == 0)
                throw new NotFoundException($"No tiene roles asignados");


            return roles.Any(pr =>
                pr.Estado
                && pr.Role != null
                && (string.Equals(pr.Role.Nombre, "EMPLEADO", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(pr.Role.Nombre, "PROPIETARIO", StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<PersonaObtenerDTO> GetByEmailWithRolesAsync(string email)
        {
            var usuario = await _dataContext.Usuarios
                .Include(u => u.Persona)
                    .ThenInclude(p => p.PersonaRoles)
                        .ThenInclude(pr => pr.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Persona.Email == email && u.Persona.Estado);

            if (usuario == null || usuario.Persona == null)
                throw new NotFoundException($"El usuario no existe.");

            var persona = usuario.Persona;
            var dto = _personaMapeo.MapToObtenerDTO(persona);

            dto.Roles = persona.PersonaRoles?
                .Where(pr => pr.Estado && pr.Role != null)
                .Select(pr => new RoleObtenerDTO
                {
                    RolId = pr.RolId,
                    Nombre = pr.Role?.Nombre,
                    Descripcion = pr.Role?.Descripcion
                })
                .ToList() ?? new List<RoleObtenerDTO>();

            return dto;

        }

        public async Task<string[]> GetActiveRoleNamesAsync(int personaId)
        {
            if (personaId <= 0) return Array.Empty<string>();

            var personaRoles = await _personaRepository.GetActiveRolesAsync(personaId);
            if (personaRoles == null || personaRoles.Count == 0) return Array.Empty<string>();

            return personaRoles
                .Where(pr => pr.Estado && pr.Role != null)
                .Select(pr => pr.Role!.Nombre?.Trim())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray()!;
        }


    }
}
