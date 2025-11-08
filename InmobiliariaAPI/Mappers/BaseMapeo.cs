namespace InmobiliariaAPI.InmobiliariaMappers
{
    public abstract class BaseMapeo<TEntidad, TObtenerDTO, TCrearDTO, TActualizarDTO>
    {
        // Convierte una entidad a su DTO de obtención (lectura).
        public abstract TObtenerDTO MapToObtenerDTO(TEntidad entidad);       
        // Convierte una entidad a su DTO de creación (datos recién creados).
        public abstract TCrearDTO MapToCrearDTO(TEntidad entidad);
        // Convierte una entidad a su DTO de actualización (editar datos existentes).
        public abstract TActualizarDTO MapToActualizarDTO(TEntidad entidad);



        // Convierte un DTO de obtención en una entidad 
        public abstract TEntidad MapToEntidadDesdeObtener(TObtenerDTO dto);
        // Convierte un DTO de creación en una nueva entidad.
        public abstract TEntidad MapToEntidadDesdeCrear(TCrearDTO dto);
        // Actualiza una entidad existente con los datos de un DTO de actualización.
        public abstract void MapToEntidadDesdeActualizar(TActualizarDTO dto, TEntidad entidad);
    }
}
