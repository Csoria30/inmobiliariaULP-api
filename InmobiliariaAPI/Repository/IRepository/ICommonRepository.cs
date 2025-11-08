using InmobiliariaAPI.Models;

namespace InmobiliariaAPI.Repository.IRepository
{
    public interface ICommonRepository<TEntity>
    {
        Task<ICollection<TEntity>> GetAllAsync();
        Task<TEntity> GetByIdAsync(int id);
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(int id, TEntity entity);
        Task<TEntity> CambiarEstadoAsync(int id, bool estado);
        Task<TEntity> DeleteAsync(int id);
        Task Save();

    }
}
