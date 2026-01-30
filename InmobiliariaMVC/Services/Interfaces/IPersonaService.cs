using InmobiliariaDTO;

namespace InmobiliariaMVC.Services.Interfaces
{
    public interface IPersonaService
    {
        Task<PersonaObtenerDTO> CreateAsync(PersonaCrearDTO dto);
    }
}
