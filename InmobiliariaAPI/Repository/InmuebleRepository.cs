using InmobiliariaAPI.Data;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Repository
{
    public class InmuebleRepository : IInmuebleRepository
    {
        private DataContext _dataContext;

        public InmuebleRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        //Metodos Base
        public async Task<ICollection<Inmueble>> GetAllAsync()
        {
            return await _dataContext.Inmuebles
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Inmueble> GetByIdAsync(int id)
        {
            return await _dataContext.Inmuebles
                .FirstOrDefaultAsync(i => i.InmuebleId == id);
        }
        public async Task<Inmueble> AddAsync(Inmueble entity)
        {
            await _dataContext.Inmuebles.AddAsync(entity);
            await _dataContext.SaveChangesAsync();
            return entity;
        }
        public async Task<Inmueble> UpdateAsync(int id, Inmueble entity)
        {
            var inmueble =  _dataContext.Inmuebles
                .FirstOrDefault(i => i.InmuebleId == id);

            // Actualizar propiedades
            inmueble.Direccion = entity.Direccion;
            inmueble.Uso = entity.Uso;
            inmueble.Ambientes = entity.Ambientes;
            inmueble.Coordenadas = entity.Coordenadas;
            inmueble.PrecioBase = entity.PrecioBase;
            inmueble.Estado = entity.Estado;
            inmueble.PropietarioId = entity.PropietarioId;
            inmueble.TipoId = entity.TipoId;
            inmueble.Imagen = entity.Imagen;

            _dataContext.Inmuebles.Update(inmueble);
            await Save();

            return inmueble;
        }
        public async Task<Inmueble> CambiarEstadoAsync(int id, bool estado)
        {
            var inmueble =  _dataContext.Inmuebles
                .FirstOrDefault(i => i.InmuebleId == id);

            inmueble.Estado = estado;

            _dataContext.Inmuebles.Update(inmueble);
            await Save();
            return inmueble;
        }

        public Task<Inmueble> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task Save()
        {
            await _dataContext.SaveChangesAsync();
        }

        



        //Metodos Adicionales
    }
}
