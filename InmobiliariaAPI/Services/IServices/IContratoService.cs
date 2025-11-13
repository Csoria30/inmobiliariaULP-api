using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;

namespace InmobiliariaAPI.Services.IServices
{
    public interface IContratoService : ICommonService<ContratoObtenerDTO, ContratoCrearDTO, ContratoActualizarDTO>
    {
        
    }
}
