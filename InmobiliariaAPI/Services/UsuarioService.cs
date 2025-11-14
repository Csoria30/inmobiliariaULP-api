using InmobiliariaAPI.Data;
using InmobiliariaAPI.Exceptions;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using InmobiliariaAPI.Repository.IRepository;
using InmobiliariaAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InmobiliariaAPI.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly DataContext _dataContext;
        private readonly IConfiguration _config;
        private readonly IPersonaService _personaService;

        public UsuarioService(
            IUsuarioRepository usuarioRepository, 
            IPersonaService personaService,
            DataContext dataContext, 
            IConfiguration config
            )
        {
            _usuarioRepository = usuarioRepository;
            _dataContext = dataContext;
            _config = config;
            _personaService = personaService;
        }


        public async Task<UsuarioObtenerDTO> CreateAsync(UsuarioCrearDTO dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));

            // Verificar que la persona exista y este activa
            var personaDto = await _personaService.GetByIdWithRolesAsync(dto.PersonaId);
            if (personaDto == null)
                throw new NotFoundException($"La persona {dto.PersonaId} no existe o está inactiva.");


            // Comprobar que el rol asignado sea 'Empleado' o 'Propietario'
            var tieneRolValido = await _personaService.HasValidRoleForUserAsync(dto.PersonaId);
            if (!tieneRolValido)
                throw new NotFoundException("La persona seleccionada no tiene rol válido para registrarse.");

            // Verificar que el empleado no este regitrado
            var existente = await _usuarioRepository.GetByPersonaIdAsync(dto.PersonaId);
            if (existente != null)
                throw new NotFoundException($"Ya existe un usuario para la persona {dto.PersonaId}.");                

            // Validar password no este vacio, ademas de FluetValidation
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidOperationException("Password obligatorio.");

            // Hashear password
            var hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var usuario = new Usuario
            {
                PersonaId = dto.PersonaId,
                Password = hashed,
                Avatar = null // Se modificara del perfil
            };

            var creado = await _usuarioRepository.AddAsync(usuario);

            return new UsuarioObtenerDTO
            {
                UsuarioId = creado.UsuarioId,
                PersonaId = creado.PersonaId,
                Avatar = creado.Avatar
            };
        }

        public async Task<AuthResponseDTO> AuthenticateAsync(UsuarioLoginDTO dto)
        {
            
            var personaDto = await _personaService.GetByEmailWithRolesAsync(dto.Email);
            if (personaDto == null)
                throw new NotFoundException($"Credenciales invalidas");

            //Recuperamos el usuario asociado a la persona y Contraseña Hasheada
            var usuario = await _usuarioRepository.GetByPersonaIdIncludingPersonaAsync(personaDto.PersonaId);

            // Si el password almacenado es null, no intentar Verify
            if (string.IsNullOrEmpty(usuario.Password) || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Password))
                throw new NotFoundException("Credenciales inválidas.");

            // Verificar que la contraseña enviada coincide con el hash almacenado
            var matches = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Password);
            if (!matches) 
                throw new NotFoundException("Credenciales inválidas.");

            // Obtiene roles activos, sino tiene roles, retorna array vacio
            var roles = await _personaService.GetActiveRoleNamesAsync(personaDto.PersonaId);

            // Generar JWT reutilizando el método BuildToken 
            var tokenString = BuildToken(usuario.UsuarioId, usuario.Persona.PersonaId, roles);

            // Caduca en 24hs
            var expiresAt = DateTime.UtcNow.AddHours(24);

            // Devolver DTO de autenticación con token
            return new AuthResponseDTO
            {
                Token = tokenString,
                ExpiresAt = expiresAt,
                UsuarioId = usuario.UsuarioId,
                Email = usuario.Persona.Email,
                Roles = roles
            };
        }

        public async Task<UsuarioObtenerDTO> GetByPersonaIdAsync(int personaId)
        {
            var usuario = await _usuarioRepository.GetByPersonaIdIncludingPersonaAsync(personaId);
            if (usuario == null)
                throw new NotFoundException("Usuario no existe");

            return new UsuarioObtenerDTO
            {
                UsuarioId = usuario.UsuarioId,
                PersonaId = usuario.PersonaId,
                Avatar = usuario.Avatar
            };
        }

        public async Task<UsuarioObtenerDTO> GetByIdAsync(int id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                throw new NotFoundException("Usuario no existe");

            return new UsuarioObtenerDTO
            {
                UsuarioId = usuario.UsuarioId,
                PersonaId = usuario.PersonaId,
                Avatar = usuario.Avatar
            };
        }

        private string BuildToken(int usuarioId, int personaId, string[] roles)
        {
            // Leer la clave secreta desde configuración, lanzar si no está configurada.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new NotFoundException("Jwt:Key no configurada")));

            // Crear credenciales de firma utilizando HMAC-SHA256
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Construir la lista inicial de claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()), // 
                new Claim("personaId", personaId.ToString()) // IDPersona
            };


            // Añadir un claim por cada rol 
            foreach (var r in roles.Distinct())
                claims.Add(new Claim(ClaimTypes.Role, r));

            // Expiración a 24 horas
            var expiresAt = DateTime.UtcNow.AddHours(24);

            // Crear el token JWT con issuer, audience, claims, expiración y firma
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"], // emisor del token 
                audience: _config["Jwt:Audience"], // audiencia del token 
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds // credenciales de firma
            );

            // Serializar y devolver el token en formato compacto (string)
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        
        public async Task<bool> ChangePasswordAsync(int personaId, string currentPassword, string newPassword)
        {
            var usuario = await _usuarioRepository.GetByPersonaIdIncludingPersonaAsync(personaId);
            if (usuario == null)
                throw new NotFoundException("Usuario no existe");

            // pass actual
            if (string.IsNullOrEmpty(usuario.Password) || !BCrypt.Net.BCrypt.Verify(currentPassword, usuario.Password))
                throw new NotFoundException("Contraseña inválida.");

            // hashear y guardar nueva pass
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _usuarioRepository.UpdateAsync(usuario.UsuarioId, usuario);
            return true;
        }

        // Actualiza Solo IRL
        public async Task<UsuarioObtenerDTO> UpdateAvatarAsync(int personaId, string avatarUrl)
        {
            var usuario = await _usuarioRepository.GetByPersonaIdIncludingPersonaAsync(personaId);
            if (usuario == null) 
                throw new NotFoundException("Usuario no existe");

            usuario.Avatar = avatarUrl;
            var actualizado = await _usuarioRepository.UpdateAsync(usuario.UsuarioId, usuario);

            return new UsuarioObtenerDTO
            {
                UsuarioId = actualizado.UsuarioId,
                PersonaId = actualizado.PersonaId,
                Avatar = actualizado.Avatar
            };
        }
    }
}
