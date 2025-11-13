using InmobiliariaAPI.Data;
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

        public UsuarioService(IUsuarioRepository usuarioRepository, DataContext dataContext, IConfiguration config)
        {
            _usuarioRepository = usuarioRepository;
            _dataContext = dataContext;
            _config = config;
        }


        public async Task<UsuarioObtenerDTO> CreateAsync(UsuarioCrearDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Verificar que la persona exista y esté activa, recuperando sus roles
            var persona = await _dataContext.Personas
                .Include(p => p.PersonaRoles).ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(p => p.PersonaId == dto.PersonaId && p.Estado);

            if (persona == null)
                throw new InvalidOperationException($"La persona {dto.PersonaId} no existe o está inactiva.");

            // Comprobar que la persona tiene el rol 'Empleado' con id = 2 y que la relación está activa
            var tieneRolValido = persona.PersonaRoles?
                .Any(pr => pr.Estado
                           && pr.Role != null
                           && (string.Equals(pr.Role.Nombre, "EMPLEADO", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(pr.Role.Nombre, "PROPIETARIO", StringComparison.OrdinalIgnoreCase)
                               ))
                == true;

            if (!tieneRolValido)
                throw new InvalidOperationException("La persona seleccionada no tiene rol válido para registrarse.");


            // Verificar que el empleado no este regitrado
            var existente = await _usuarioRepository.GetByPersonaIdAsync(dto.PersonaId);
            if (existente != null)
                throw new InvalidOperationException($"Ya existe un usuario para la persona {dto.PersonaId}.");


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
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Buscar usuario que coincida con el email y este activo, incluyendo Persona y sus Roles
            var usuario = await _dataContext.Usuarios
                .Include(u => u.Persona)
                    .ThenInclude(p => p.PersonaRoles)
                        .ThenInclude(pr => pr.Role)
                .AsNoTracking() // Lectura: no necesitamos seguimiento de cambios
                .FirstOrDefaultAsync(u => u.Persona.Email == dto.Email && u.Persona.Estado);

            if (usuario == null)
                throw new InvalidOperationException("Credenciales inválidas.");

            // Si el password almacenado es null, no intentar Verify
            if (string.IsNullOrEmpty(usuario.Password) || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Password))
                throw new InvalidOperationException("Credenciales inválidas.");

            // Verificar que la contraseña enviada coincide con el hash almacenado
            var matches = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Password);
            if (!matches) throw new InvalidOperationException("Credenciales inválidas.");

            // Obtiene roles activos, sino tiene roles, retorna array vacio
            var roles = usuario.Persona.PersonaRoles?
                .Where(pr => pr.Estado && pr.Role != null) 
                .Select(pr => pr.Role.Nombre)              
                .ToArray() ?? Array.Empty<string>();

            
            // Generar JWT reutilizando el método BuildToken 
            var tokenString = BuildToken(usuario.UsuarioId, usuario.Persona.PersonaId, roles);

            // Calcular fecha de expiración para devolver en la respuesta
            //var expiresMinutes = int.Parse(_config["Jwt:ExpiresMinutes"] ?? "60");
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
            if (usuario == null) return null;
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
            if (usuario == null) return null;
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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurada")));

            // Crear credenciales de firma utilizando HMAC-SHA256
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Construir la lista inicial de claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()), // 'sub' (subject) estándar: identificador del usuario
                new Claim("personaId", personaId.ToString()) // Claim: Id Persona
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

        // Cambia pass identificado por personaId
        public async Task<bool> ChangePasswordAsync(int personaId, string currentPassword, string newPassword)
        {
            var usuario = await _usuarioRepository.GetByPersonaIdIncludingPersonaAsync(personaId);
            if (usuario == null) return false;

            // pass actual
            if (string.IsNullOrEmpty(usuario.Password) || !BCrypt.Net.BCrypt.Verify(currentPassword, usuario.Password))
                return false;

            // hashear y guardar nueva pass
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _usuarioRepository.UpdateAsync(usuario.UsuarioId, usuario);
            return true;
        }

        // Actualiza solo la URL/avatar del usuario asociado a personaId
        public async Task<UsuarioObtenerDTO> UpdateAvatarAsync(int personaId, string avatarUrl)
        {
            var usuario = await _usuarioRepository.GetByPersonaIdIncludingPersonaAsync(personaId);
            if (usuario == null) return null;

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
