using InmobiliariaDTO;

namespace InmobiliariaMVC.Services.Interfaces
{
    public interface IRoleService
    {
        Task<List<RoleObtenerDTO>> GetAllAsync();
    }
}
