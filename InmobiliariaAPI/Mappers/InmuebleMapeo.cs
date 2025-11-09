using InmobiliariaAPI.InmobiliariaMappers;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Mappers
{
    public class InmuebleMapeo : BaseMapeo<Inmueble, InmuebleObtenerDTO, InmuebleCrearDTO, InmuebleActualizarDTO>
    {
        public override InmuebleActualizarDTO MapToActualizarDTO(Inmueble entidad)
        {
            if (entidad == null) return null;
            return new InmuebleActualizarDTO
            {
                InmuebleId = entidad.InmuebleId,
                Direccion = entidad.Direccion,
                Uso = entidad.Uso,
                Ambientes = entidad.Ambientes,
                Coordenadas = entidad.Coordenadas,
                PrecioBase = entidad.PrecioBase,
                PropietarioId = entidad.PropietarioId,
                TipoId = entidad.TipoId,
                Estado = entidad.Estado
            };
        }

        public override InmuebleCrearDTO MapToCrearDTO(Inmueble entidad)
        {
            if (entidad == null) return null;
            return new InmuebleCrearDTO
            {
                Direccion = entidad.Direccion,
                Uso = entidad.Uso,
                Ambientes = entidad.Ambientes,
                Coordenadas = entidad.Coordenadas,
                PrecioBase = entidad.PrecioBase,
                PropietarioId = entidad.PropietarioId,
                TipoId = entidad.TipoId
            };
        }

        public override void MapToEntidadDesdeActualizar(InmuebleActualizarDTO dto, Inmueble entidad)
        {
            if (dto == null || entidad == null) return;
            entidad.Direccion = dto.Direccion?.Trim();
            entidad.Uso = dto.Uso?.Trim();
            entidad.Ambientes = dto.Ambientes;
            entidad.Coordenadas = dto.Coordenadas?.Trim();
            entidad.PrecioBase = dto.PrecioBase;
            entidad.PropietarioId = dto.PropietarioId;
            entidad.TipoId = dto.TipoId;
            entidad.Estado = dto.Estado;
        }

        public override Inmueble MapToEntidadDesdeCrear(InmuebleCrearDTO dto)
        {
            if (dto == null) return null;
            return new Inmueble
            {
                Direccion = dto.Direccion?.Trim(),
                Uso = dto.Uso?.Trim(),
                Ambientes = dto.Ambientes,
                Coordenadas = dto.Coordenadas?.Trim(),
                PrecioBase = dto.PrecioBase,
                PropietarioId = dto.PropietarioId,
                TipoId = dto.TipoId,
                Estado = false
            };
        }

        public override Inmueble MapToEntidadDesdeObtener(InmuebleObtenerDTO dto)
        {
            if (dto == null) return null;
            return new Inmueble
            {
                InmuebleId = dto.InmuebleId,
                Direccion = dto.Direccion?.Trim(),
                Uso = dto.Uso?.Trim(),
                Ambientes = dto.Ambientes,
                Coordenadas = dto.Coordenadas?.Trim(),
                PrecioBase = dto.PrecioBase,
                PropietarioId = dto.PropietarioId,
                TipoId = dto.TipoId,
                Estado = dto.Estado
            };
        }

        public override InmuebleObtenerDTO MapToObtenerDTO(Inmueble entidad)
        {
            if (entidad == null) return null;
            return new InmuebleObtenerDTO
            {
                InmuebleId = entidad.InmuebleId,
                Direccion = entidad.Direccion,
                Uso = entidad.Uso,
                Ambientes = entidad.Ambientes,
                Coordenadas = entidad.Coordenadas,
                PrecioBase = entidad.PrecioBase,
                Estado = entidad.Estado,
                PropietarioId = entidad.PropietarioId,
                TipoId = entidad.TipoId
            };
        }

    }
}
