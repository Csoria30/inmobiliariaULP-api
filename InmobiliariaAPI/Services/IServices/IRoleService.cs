using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;

namespace InmobiliariaAPI.Services.IServices
{
    public interface IRoleService : ICommonService<RoleObtenerDTO, RoleCrearDTO, RoleActualizarDTO>
    {
    }
}
