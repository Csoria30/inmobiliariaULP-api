using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.InmobiliariaMappers
{
    public class PersonaMapeo : BaseMapeo<Persona, PersonaObtenerDTO, PersonaCrearDTO, PersonaActualizarDTO>
    {
        public override PersonaActualizarDTO MapToActualizarDTO(Persona entidad)
        {
            if (entidad == null) return null;
            return new PersonaActualizarDTO
            {
                PersonaId = entidad.PersonaId,
                Dni = entidad.Dni,
                Apellido = entidad.Apellido,
                Nombre = entidad.Nombre,
                Telefono = entidad.Telefono,
                Email = entidad.Email,
                Estado = entidad.Estado
            };
        }

        public override PersonaCrearDTO MapToCrearDTO(Persona entidad)
        {
            if (entidad == null) return null;
            return new PersonaCrearDTO
            {
                Dni = entidad.Dni,
                Apellido = entidad.Apellido,
                Nombre = entidad.Nombre,
                Telefono = entidad.Telefono,
                Email = entidad.Email
            };
        }

        public override void MapToEntidadDesdeActualizar(PersonaActualizarDTO dto, Persona entidad)
        {
            if (dto == null || entidad == null) return;
            entidad.Dni = dto.Dni?.Trim();
            entidad.Apellido = dto.Apellido?.Trim();
            entidad.Nombre = dto.Nombre?.Trim();
            entidad.Telefono = dto.Telefono?.Trim();
            entidad.Email = dto.Email?.Trim();
            entidad.Estado = dto.Estado;
        }

        public override Persona MapToEntidadDesdeCrear(PersonaCrearDTO dto)
        {
            if (dto == null) return null;
            return new Persona
            {
                Dni = dto.Dni?.Trim(),
                Apellido = dto.Apellido?.Trim(),
                Nombre = dto.Nombre?.Trim(),
                Telefono = dto.Telefono?.Trim(),
                Email = dto.Email?.Trim(),
                Estado = true
            };
        }

        public override Persona MapToEntidadDesdeObtener(PersonaObtenerDTO dto)
        {
            if (dto == null) return null;
            return new Persona
            {
                PersonaId = dto.PersonaId,
                Dni = dto.Dni,
                Apellido = dto.Apellido,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Estado = dto.Estado
            };
        }

        public override PersonaObtenerDTO MapToObtenerDTO(Persona entidad)
        {
            if (entidad == null) return null;
            return new PersonaObtenerDTO
            {
                PersonaId = entidad.PersonaId,
                Dni = entidad.Dni,
                Apellido = entidad.Apellido,
                Nombre = entidad.Nombre,
                Telefono = entidad.Telefono,
                Email = entidad.Email,
                Estado = entidad.Estado,
                Roles = entidad.PersonaRoles?
                    .Where(pr => pr.Estado)
                    .Select(pr => new RolObtenerDTO
                    {
                        RolId = pr.RolId,
                        Nombre = pr.Role?.Nombre,
                        Descripcion = pr.Role?.Descripcion
                    }).ToList()
            };
        }
    }
}
