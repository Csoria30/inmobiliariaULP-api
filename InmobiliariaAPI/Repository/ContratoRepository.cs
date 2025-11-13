using InmobiliariaAPI.Data;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Repository
{
    public class ContratoRepository : IContratoRepository
    {
        private readonly DataContext _dataContext;

        public ContratoRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<Contrato> AddAsync(Contrato entity)
        {
            await _dataContext.Contratos.AddAsync(entity);
            await Save();
            return entity;
        }

        public async Task<Contrato> CambiarEstadoAsync(int id, bool estado)
        {
            var contrato = await _dataContext.Contratos
               .FirstOrDefaultAsync(c => c.ContratoId == id);

            if (contrato == null) return null;

            // Mapear bool a estado semántico de contrato
            contrato.Estado = estado ? "vigente" : "finalizado";

            _dataContext.Contratos.Update(contrato);
            await Save();

            return contrato;
        }

        public async Task<Contrato> DeleteAsync(int id)
        {
            var contrato = await _dataContext.Contratos
                .FirstOrDefaultAsync(c => c.ContratoId == id);

            if (contrato == null) return null;

            _dataContext.Contratos.Remove(contrato);
            await Save();

            return contrato;
        }

        public async Task<ICollection<Contrato>> GetAllAsync()
        {
            return await _dataContext.Contratos
                .AsNoTracking()
                .Include(c => c.Inmueble)
                .Include(c => c.Inquilino)
                .Include(c => c.Usuario)
                .Include(c => c.UsuarioFinaliza)
                .ToListAsync();
        }

        public async Task<Contrato> GetByIdAsync(int id)
        {
            return await _dataContext.Contratos
                .AsNoTracking()
                .Include(c => c.Inmueble)
                .Include(c => c.Inquilino)
                .Include(c => c.Usuario)
                .Include(c => c.UsuarioFinaliza)
                .FirstOrDefaultAsync(c => c.ContratoId == id);
        }

        public async Task Save()
        {
            await _dataContext.SaveChangesAsync();
        }

        public async Task<Contrato> UpdateAsync(int id, Contrato entity)
        {
            var existente = await _dataContext.Contratos
                .FirstOrDefaultAsync(c => c.ContratoId == id);

            if (existente == null) return null;

            // Actualizar campos relevantes
            existente.InmuebleId = entity.InmuebleId;
            existente.InquilinoId = entity.InquilinoId;
            existente.UsuarioId = entity.UsuarioId;
            existente.UsuarioFinalizaId = entity.UsuarioFinalizaId;
            existente.FechaInicio = entity.FechaInicio;
            existente.FechaFin = entity.FechaFin;
            existente.MontoMensual = entity.MontoMensual;
            existente.FechaFinalizacionAnticipada = entity.FechaFinalizacionAnticipada;
            existente.Multa = entity.Multa;
            existente.Estado = entity.Estado;

            _dataContext.Contratos.Update(existente);
            await Save();

            return existente;
        }

    }
}
