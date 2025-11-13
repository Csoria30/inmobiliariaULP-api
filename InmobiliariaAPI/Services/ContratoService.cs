using FluentValidation;
using InmobiliariaAPI.Data;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;
using InmobiliariaAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InmobiliariaAPI.Services
{
    public class ContratoService : IContratoService
    {
        private readonly IContratoRepository _contratoRepository;
        private readonly IInmuebleRepository _inmuebleRepository;
        private readonly IPersonaRepository _personaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IValidator<ContratoCrearDTO> _crearValidator;

        public ContratoService(
            IContratoRepository contratoRepository,
            IInmuebleRepository inmuebleRepository,
            IPersonaRepository personaRepository,
            IUsuarioRepository usuarioRepository,
            IValidator<ContratoCrearDTO> crearValidator
            )
        {
            _contratoRepository = contratoRepository;
            _inmuebleRepository = inmuebleRepository;
            _personaRepository = personaRepository;
            _usuarioRepository = usuarioRepository;
            _crearValidator = crearValidator;
        }

        public async Task<ContratoObtenerDTO> CreateAsync(ContratoCrearDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Validar: inmueble Existe y Habilitado
            var inmueble = await _inmuebleRepository.GetByIdAsync(dto.InmuebleId);
            if (inmueble == null)
                throw new InvalidOperationException("El inmueble no existe.");
            if (!inmueble.Estado)
                throw new InvalidOperationException("El inmueble no está habilitado para generar contratos.");


            // Validar: inquilino Existe y Habilitado
            var inquilino = await _personaRepository.GetByIdAsync(dto.InquilinoId);
            if (inquilino == null || !inquilino.Estado)
                throw new InvalidOperationException("El inquilino no existe o está inactivo.");

            // Fechas coherentes
            var inicio = dto.FechaInicio.Date;
            var fin = dto.FechaFin.Date;
            if (fin < inicio)
                throw new InvalidOperationException("La fecha de fin debe ser igual o posterior a la fecha de inicio.");

            // Obtener inmuebles y filtrar vigentes para el inmueble
            var contratos = await _contratoRepository.GetAllAsync(); 
            var contratosVigentes = contratos
                .Where(c => c.InmuebleId == dto.InmuebleId && string.Equals(c.Estado, "vigente", StringComparison.OrdinalIgnoreCase))
                .ToList();


            //Contratos vigentes solapados
            bool haySolapamiento = contratosVigentes.Any(c =>
                !(fin < c.FechaInicio.Date || inicio > c.FechaFin.Date)
            );

            if (haySolapamiento)
                throw new InvalidOperationException("Existe un contrato vigente que solapa las fechas solicitadas.");

            // Si dto.UsuarioId no fue provisto, intentar asignar desde persona -> usuario
            if (dto.UsuarioId == 0)
            {
                var usuario = await _usuarioRepository.GetByPersonaIdAsync(inquilino.PersonaId);
                if (usuario != null)
                    dto.UsuarioId = usuario.UsuarioId;
            }

            // Forzar estado a 'vigente' al crear
            dto.Estado = "vigente";

            // Mapear a entidad y persistir vía repositorio
            var entidad = new Contrato
            {
                InmuebleId = dto.InmuebleId,
                InquilinoId = dto.InquilinoId,
                UsuarioId = dto.UsuarioId,
                UsuarioFinalizaId = dto.UsuarioFinalizaId,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                MontoMensual = dto.MontoMensual,
                FechaFinalizacionAnticipada = dto.FechaFinalizacionAnticipada,
                Multa = dto.Multa,
                Estado = dto.Estado
            };

            var creado = await _contratoRepository.AddAsync(entidad);

            return new ContratoObtenerDTO
            {
                ContratoId = creado.ContratoId,
                InmuebleId = creado.InmuebleId,
                InquilinoId = creado.InquilinoId,
                UsuarioId = creado.UsuarioId,
                UsuarioFinalizaId = creado.UsuarioFinalizaId,
                FechaInicio = creado.FechaInicio,
                FechaFin = creado.FechaFin,
                MontoMensual = creado.MontoMensual,
                FechaFinalizacionAnticipada = creado.FechaFinalizacionAnticipada,
                Multa = creado.Multa,
                Estado = creado.Estado
            };

        }

        public Task<ContratoObtenerDTO> DeleteAsync(int id, bool estado)
        {
            throw new NotImplementedException();
        }

        public Task<ContratoObtenerDTO> ExistsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ContratoObtenerDTO>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ContratoObtenerDTO> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ContratoObtenerDTO> UpdateAsync(int id, ContratoActualizarDTO entity)
        {
            throw new NotImplementedException();
        }
    }
}
