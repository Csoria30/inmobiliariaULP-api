using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;

namespace InmobiliariaAPI.Services.IServices
{
    public interface IInmuebleService : ICommonService<InmuebleObtenerDTO, InmuebleCrearDTO, InmuebleActualizarDTO>
    {
    }
}
