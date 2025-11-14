using InmobiliariaAPI.Models;

namespace InmobiliariaAPI.Repository.IRepository
{
    public interface IInmuebleRepository : ICommonRepository<Inmueble>
    {
        // Devuelve inmuebles activos
        Task<ICollection<Inmueble>> GetActiveAsync();

        
        // Comprueba si existe el tipo de inmueble
        Task<bool> TipoExistsAsync(int tipoId);


        // Comprueba si existe una dirección activa
        Task<bool> DireccionExistsAsync(string direccion);


        // Comprueba si existe una dirección activa excluyendo un id 
        Task<bool> DireccionExistsExcludingIdAsync(string direccion, int excludeInmuebleId);

        // obtener inmuebles por propietario 
        Task<ICollection<Inmueble>> GetByPropietarioAsync(int propietarioId);
    }
}
