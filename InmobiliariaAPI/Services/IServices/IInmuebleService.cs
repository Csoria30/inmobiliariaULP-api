using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;

namespace InmobiliariaAPI.Services.IServices
{
    public interface IInmuebleService : ICommonService<InmuebleObtenerDTO, InmuebleCrearDTO, InmuebleActualizarDTO>
    {
        Task<ICollection<InmuebleObtenerDTO>> GetByPropietarioAsync(int propietarioId);
    }
}
