using InmobiliariaAPI.InmobiliariaMappers;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Mappers
{
    public class RoleMapeo : BaseMapeo<Role, RoleObtenerDTO, RoleCrearDTO, RoleActualizarDTO>
    {
        public override RoleActualizarDTO MapToActualizarDTO(Role entidad)
        {
            if(entidad == null) return null;
            return new RoleActualizarDTO
            {
                RolId = entidad.RolId,
                Nombre = entidad.Nombre,
                Descripcion = entidad.Descripcion
            };
        }

        public override RoleCrearDTO MapToCrearDTO(Role entidad)
        {
            if(entidad == null) return null;
            return new RoleCrearDTO
            {
                Nombre = entidad.Nombre,
                Descripcion = entidad.Descripcion
            };
        }

        public override void MapToEntidadDesdeActualizar(RoleActualizarDTO dto, Role entidad)
        {
            if(dto == null || entidad == null) return;
            entidad.Nombre = dto.Nombre;
            entidad.Descripcion = dto.Descripcion;
        }

        public override Role MapToEntidadDesdeCrear(RoleCrearDTO dto)
        {
            if(dto == null) return null;
            return new Role
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };
        }

        public override Role MapToEntidadDesdeObtener(RoleObtenerDTO dto)
        {
            if(dto == null) return null;
            return new Role
            {
                RolId = dto.RolId,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };
        }

        public override RoleObtenerDTO MapToObtenerDTO(Role entidad)
        {
            if(entidad == null) return null;
            return new RoleObtenerDTO
            {
                RolId = entidad.RolId,
                Nombre = entidad.Nombre,
                Descripcion = entidad.Descripcion
            };
        }
    }
}
