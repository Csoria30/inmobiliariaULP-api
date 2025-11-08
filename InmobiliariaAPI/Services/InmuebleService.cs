using InmobiliariaAPI.Data;
using InmobiliariaAPI.InmobiliariaMappers;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;
using InmobiliariaAPI.Services.IServices;

namespace InmobiliariaAPI.Services
{
    public class InmuebleService : IInmuebleService
    {
        private readonly IInmuebleRepository _inmuebleRepository;
        private readonly InmuebleMapeo _inmuebleMapeo;
        private readonly DataContext _dataContext;

        public InmuebleService(
            IInmuebleRepository inmuebleRepository,
            InmuebleMapeo inmuebleMapeo,
            DataContext dataContext
            )
        {
            _inmuebleRepository = inmuebleRepository;
            _inmuebleMapeo = inmuebleMapeo;
            _dataContext = dataContext;
        }

        public async Task<InmuebleObtenerDTO> CreateAsync(InmuebleCrearDTO entity)
        {
            throw new NotImplementedException();
        }

        public async Task<InmuebleObtenerDTO> DeleteAsync(int id, bool estado)
        {
            throw new NotImplementedException();
        }

        public async Task<InmuebleObtenerDTO> ExistsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<InmuebleObtenerDTO>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<InmuebleObtenerDTO> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<InmuebleObtenerDTO> UpdateAsync(int id, InmuebleActualizarDTO entity)
        {
            throw new NotImplementedException();
        }
    }
}
