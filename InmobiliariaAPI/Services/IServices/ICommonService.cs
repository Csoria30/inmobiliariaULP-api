using InmobiliariaAPI.Models.DTO;

namespace InmobiliariaAPI.Repository.IRepository
{
    public interface ICommonService<TReturn, TInsert, TUpdate>
    {
        Task<ICollection<TReturn>> GetAllAsync();
        Task<TReturn> GetByIdAsync(int id);
        Task<TReturn> ExistsAsync(int id);
        Task<TReturn> CreateAsync(TInsert entity);
        Task<TReturn> UpdateAsync(int id, TUpdate entity);
        Task<TReturn> DeleteAsync(int id, bool estado);
    }
}
