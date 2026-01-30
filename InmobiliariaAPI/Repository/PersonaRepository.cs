using InmobiliariaAPI.Data;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Models.DTO;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace InmobiliariaAPI.Repository.IRepository
{
    public class PersonaRepository : IPersonaRepository
    {
        private DataContext _dataContext;
        private readonly string _connectionString;

        public PersonaRepository(DataContext dataContext, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        //Metodos Base
        public async Task<ICollection<Persona>> GetAllAsync()
        {
            return await _dataContext.Personas
                .AsNoTracking()
                .Where(p => p.Estado)
                .Include(p => p.PersonaRoles)
                     .ThenInclude(pr => pr.Role)
                .ToListAsync();
        }
        public async Task<Persona> GetByIdAsync(int id)
        {
            return await _dataContext.Personas
                .Include(p => p.PersonaRoles)
                    .ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(p => p.PersonaId == id && p.Estado);
        }
        public async Task<Persona> AddAsync(Persona entity)
        {
            await _dataContext.Personas.AddAsync(entity);
            await _dataContext.SaveChangesAsync();
            return entity;
        }
        public async Task<Persona> UpdateAsync(int id, Persona entity)
        {
            var persona = await _dataContext.Personas
            .Include(p => p.PersonaRoles)
                .ThenInclude(pr => pr.Role)
            .FirstOrDefaultAsync(p => p.PersonaId == id && p.Estado);


            // Actualizar propiedades
            persona.Dni = entity.Dni;
            persona.Apellido = entity.Apellido;
            persona.Nombre = entity.Nombre;
            persona.Telefono = entity.Telefono;
            persona.Email = entity.Email;
            persona.Estado = entity.Estado;

            _dataContext.Personas.Update(persona);
            await Save();

            // Vuelve a cargar los roles actualizados
            await _dataContext.Entry(persona)
                .Collection(p => p.PersonaRoles)
                .Query()
                .Include(pr => pr.Role)
                .LoadAsync();

            return persona;
        }
        public async Task<Persona> CambiarEstadoAsync(int id, bool estado)
        {
            var persona = await _dataContext.Personas
                .Include(p => p.PersonaRoles)
                .FirstOrDefaultAsync(p => p.PersonaId == id);

            // Cambia el estado de la persona
            persona.Estado = estado;

            // Cambia el estado de los roles asociados
            foreach (var personaRol in persona.PersonaRoles)
            {
                personaRol.Estado = estado;
                personaRol.FechaBaja = estado ? null : DateTime.Now;
                personaRol.FechaAlta = estado ? DateTime.Now : personaRol.FechaAlta;
            }

            _dataContext.Personas.Update(persona);
            await Save();
            return persona;
        }
        public Task<Persona> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task Save()
        {
            await _dataContext.SaveChangesAsync();
        }

        //Metodos Adicionales

        public async Task AddPersonaRoleAsync(PersonaRole personaRole)
        {
            await _dataContext.PersonaRoles.AddAsync(personaRole);
            await _dataContext.SaveChangesAsync();
        }
        public async Task<ICollection<PersonaRole>> GetActiveRolesAsync(int personaId)
        {
            return await _dataContext.PersonaRoles
                .Include(pr => pr.Role)
                .Where(pr => pr.PersonaId == personaId && pr.Estado)
                .ToListAsync();
        }
        public async Task UpdatePersonaRolesAsync(int personaId, int rolId)
        {
            var personaRole = await _dataContext.PersonaRoles
                .FirstOrDefaultAsync(pr => pr.PersonaId == personaId && pr.RolId == rolId);

            if (personaRole == null)
            {
                personaRole = new PersonaRole
                {
                    PersonaId = personaId,
                    RolId = rolId,
                    FechaAlta = DateTime.Now,
                    Estado = true
                };
                await _dataContext.PersonaRoles.AddAsync(personaRole);
            }
            else
            {
                personaRole.Estado = !personaRole.Estado;
                personaRole.FechaBaja = personaRole.Estado ? null : DateTime.Now;
                _dataContext.PersonaRoles.Update(personaRole);
            }

            await Save();
        }
        public async Task<bool> ExistsByDniAsync(string dni)
        {
            return await _dataContext.Personas
                .AsNoTracking()
                .AnyAsync(p => p.Dni == dni && p.Estado);
        }
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dataContext.Personas
                .AsNoTracking()
                .AnyAsync(p => p.Email == email && p.Estado);
        }
        public async Task<Persona> GetByDniAsync(string dni)
        {
            return await _dataContext.Personas
                .Include(p => p.PersonaRoles)
                .FirstOrDefaultAsync(p => p.Dni == dni && p.Estado);
        }
        public async Task<Persona> GetByEmailAsync(string email)
        {
            return await _dataContext.Personas
                .Include(p => p.PersonaRoles)
                .FirstOrDefaultAsync(p => p.Email == email && p.Estado);
        }

        public async Task<(ICollection<Persona> Items, int Total)> GetPagedAsync(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _dataContext.Personas
                .AsNoTracking()
                .Where(p => p.Estado)
                .Include(p => p.PersonaRoles)
                    .ThenInclude(pr => pr.Role)
                .OrderBy(p => p.PersonaId);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<PagedResult<PersonaObtenerDTO>> GetAllPagedAsync(int page, int pageSize, string? search = null, string? orderBy = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // 1) total sin filtro
            int total = 0;
            using (var countAllCmd = connection.CreateCommand())
            {
                countAllCmd.CommandText = "SELECT COUNT(*) FROM personas";
                total = Convert.ToInt32(await countAllCmd.ExecuteScalarAsync());
            }

            // 2) preparar where para búsqueda (si existe)
            string where = "";
            if (!string.IsNullOrWhiteSpace(search))
            {
                where = @"WHERE p.dni LIKE @search OR p.apellido LIKE @search OR p.nombre LIKE @search OR p.email LIKE @search";
            }

            // 3) total filtrado
            int totalFiltered = total;
            using (var countFilteredCmd = connection.CreateCommand())
            {
                countFilteredCmd.CommandText = $"SELECT COUNT(*) FROM personas p {where}";
                if (!string.IsNullOrWhiteSpace(search))
                    countFilteredCmd.Parameters.AddWithValue("@search", $"%{search}%");
                totalFiltered = Convert.ToInt32(await countFilteredCmd.ExecuteScalarAsync());
            }

            // 4) consulta paginada: seleccionar solo columnas de persona y aplicar LIMIT/OFFSET
            var items = new List<PersonaObtenerDTO>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $@"
                    SELECT p.id_persona, p.dni, p.apellido, p.nombre, p.telefono, p.email, p.estado
                    FROM personas p
                    {where}
                    {(!string.IsNullOrWhiteSpace(orderBy) ? $"ORDER BY {orderBy}" : "ORDER BY p.id_persona DESC")}
                    LIMIT @limit OFFSET @offset;
                ";

                if (!string.IsNullOrWhiteSpace(search))
                    cmd.Parameters.AddWithValue("@search", $"%{search}%");

                cmd.Parameters.AddWithValue("@limit", pageSize);
                cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);

                using var reader = await cmd.ExecuteReaderAsync();
                var ids = new List<int>();
                while (await reader.ReadAsync())
                {
                    var p = new PersonaObtenerDTO
                    {
                        PersonaId = reader.GetInt32("id_persona"),
                        Dni = reader.GetString("dni"),
                        Apellido = reader.GetString("apellido"),
                        Nombre = reader.GetString("nombre"),
                        Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? null! : reader.GetString("telefono"),
                        Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null! : reader.GetString("email"),
                        Estado = reader.GetBoolean("estado"),
                        Roles = new List<RoleObtenerDTO>()
                    };
                    items.Add(p);
                    ids.Add(p.PersonaId);
                }

                // 5) Cargar roles para los ids de la página (evita duplicados en la paginación)
                if (ids.Any())
                {
                    // Obtengo roles activos por persona
                    using var rolesCmd = connection.CreateCommand();
                    rolesCmd.CommandText = $@"
                        SELECT pr.id_persona, r.id_rol AS rolId, r.nombre, r.descripcion
                        FROM persona_roles pr
                        INNER JOIN roles r ON pr.id_rol = r.id_rol AND r.estado = 1
                        WHERE pr.id_persona IN ({string.Join(",", ids)})
                    ";
                    using var rolesReader = await rolesCmd.ExecuteReaderAsync();
                    var rolesByPersona = new Dictionary<int, List<RoleObtenerDTO>>();
                    while (await rolesReader.ReadAsync())
                    {
                        int personaId = rolesReader.GetInt32("id_persona");
                        var role = new RoleObtenerDTO
                        {
                            RolId = rolesReader.GetInt32("rolId"),
                            Nombre = rolesReader.GetString("nombre"),
                            Descripcion = rolesReader.IsDBNull(rolesReader.GetOrdinal("descripcion")) ? null! : rolesReader.GetString("descripcion")
                        };
                        if (!rolesByPersona.TryGetValue(personaId, out var list))
                        {
                            list = new List<RoleObtenerDTO>();
                            rolesByPersona[personaId] = list;
                        }
                        list.Add(role);
                    }

                    // Mapear roles a items
                    foreach (var it in items)
                    {
                        if (rolesByPersona.TryGetValue(it.PersonaId, out var rlist))
                            it.Roles = rlist;
                    }
                }
            }

            return new PagedResult<PersonaObtenerDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total,
                TotalFiltered = totalFiltered
            };
        }

    }
}
