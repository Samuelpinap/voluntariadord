using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public interface ISkillService
    {
        Task<List<SkillDto>> GetAllSkillsAsync();
        Task<List<SkillDto>> GetUserSkillsAsync(int userId);
        Task<SkillDto> CreateSkillAsync(CreateSkillDto createSkillDto);
        Task<SkillDto> UpdateSkillAsync(int skillId, UpdateSkillDto updateSkillDto);
        Task<bool> DeleteSkillAsync(int skillId);
        Task<bool> AddSkillToUserAsync(int userId, int skillId, string? nivel = null, string? certificacion = null);
        Task<bool> RemoveSkillFromUserAsync(int userId, int skillId);
        Task<bool> UpdateUserSkillAsync(int userId, int skillId, UpdateUserSkillDto updateDto);
        Task<List<SkillDto>> SearchSkillsAsync(string searchTerm);
        Task<SkillStatsDto> GetSkillStatsAsync();
        Task<List<UserSkillDto>> GetUsersWithSkillAsync(int skillId, int page = 1, int pageSize = 20);
        Task<List<SkillDto>> GetSkillsForOpportunityMatchingAsync(int userId);
        Task SeedDefaultSkillsAsync();
    }

    public class SkillService : ISkillService
    {
        private readonly DbContextApplication _context;
        private readonly ILogger<SkillService> _logger;

        public SkillService(DbContextApplication context, ILogger<SkillService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SkillDto>> GetAllSkillsAsync()
        {
            try
            {
                var skills = await _context.Skills
                    .Include(s => s.UsuarioSkills)
                    .Where(s => s.EsActivo)
                    .OrderBy(s => s.Categoria)
                    .ThenBy(s => s.Nombre)
                    .ToListAsync();

                return skills.Select(MapToSkillDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all skills");
                throw;
            }
        }

        public async Task<List<SkillDto>> GetUserSkillsAsync(int userId)
        {
            try
            {
                var userSkills = await _context.UsuarioSkills
                    .Include(us => us.Skill)
                    .Where(us => us.UsuarioId == userId && us.Skill.EsActivo)
                    .OrderBy(us => us.Skill.Categoria)
                    .ThenBy(us => us.Skill.Nombre)
                    .ToListAsync();

                return userSkills.Select(us => new SkillDto
                {
                    Id = us.Skill.Id,
                    Nombre = us.Skill.Nombre,
                    Descripcion = us.Skill.Descripcion,
                    Categoria = us.Skill.Categoria,
                    IconoUrl = us.Skill.IconoUrl,
                    Color = us.Skill.Color,
                    EsActivo = us.Skill.EsActivo,
                    FechaCreacion = us.Skill.FechaCreacion,
                    TotalUsuarios = 0, // Will be calculated separately if needed
                    Nivel = us.Nivel,
                    Certificacion = us.Certificacion,
                    FechaAdquisicion = us.FechaAdquisicion
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user skills for user {UserId}", userId);
                throw;
            }
        }

        public async Task<SkillDto> CreateSkillAsync(CreateSkillDto createSkillDto)
        {
            try
            {
                var skill = new Skill
                {
                    Nombre = createSkillDto.Nombre,
                    Descripcion = createSkillDto.Descripcion,
                    Categoria = createSkillDto.Categoria,
                    IconoUrl = createSkillDto.IconoUrl,
                    Color = createSkillDto.Color,
                    EsActivo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();

                return MapToSkillDto(skill);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating skill");
                throw;
            }
        }

        public async Task<SkillDto> UpdateSkillAsync(int skillId, UpdateSkillDto updateSkillDto)
        {
            try
            {
                var skill = await _context.Skills.FindAsync(skillId);
                if (skill == null)
                {
                    throw new ArgumentException("Skill not found");
                }

                skill.Nombre = updateSkillDto.Nombre;
                skill.Descripcion = updateSkillDto.Descripcion;
                skill.Categoria = updateSkillDto.Categoria;
                skill.IconoUrl = updateSkillDto.IconoUrl;
                skill.Color = updateSkillDto.Color;
                skill.EsActivo = updateSkillDto.EsActivo;

                await _context.SaveChangesAsync();

                return MapToSkillDto(skill);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skill {SkillId}", skillId);
                throw;
            }
        }

        public async Task<bool> DeleteSkillAsync(int skillId)
        {
            try
            {
                var skill = await _context.Skills
                    .Include(s => s.UsuarioSkills)
                    .FirstOrDefaultAsync(s => s.Id == skillId);

                if (skill == null)
                {
                    return false;
                }

                // Remove all user skills first
                _context.UsuarioSkills.RemoveRange(skill.UsuarioSkills);
                
                // Remove the skill
                _context.Skills.Remove(skill);
                
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skill {SkillId}", skillId);
                throw;
            }
        }

        public async Task<bool> AddSkillToUserAsync(int userId, int skillId, string? nivel = null, string? certificacion = null)
        {
            try
            {
                // Check if user already has this skill
                var existingSkill = await _context.UsuarioSkills
                    .FirstOrDefaultAsync(us => us.UsuarioId == userId && us.SkillId == skillId);

                if (existingSkill != null)
                {
                    return false; // User already has this skill
                }

                // Verify skill and user exist
                var skill = await _context.Skills.FindAsync(skillId);
                var user = await _context.Usuarios.FindAsync(userId);

                if (skill == null || user == null)
                {
                    return false;
                }

                // Add the skill
                var userSkill = new UsuarioSkill
                {
                    UsuarioId = userId,
                    SkillId = skillId,
                    Nivel = nivel,
                    Certificacion = certificacion,
                    FechaAdquisicion = DateTime.UtcNow
                };

                _context.UsuarioSkills.Add(userSkill);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Skill {SkillId} added to user {UserId}", skillId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding skill {SkillId} to user {UserId}", skillId, userId);
                throw;
            }
        }

        public async Task<bool> RemoveSkillFromUserAsync(int userId, int skillId)
        {
            try
            {
                var userSkill = await _context.UsuarioSkills
                    .FirstOrDefaultAsync(us => us.UsuarioId == userId && us.SkillId == skillId);

                if (userSkill == null)
                {
                    return false;
                }

                _context.UsuarioSkills.Remove(userSkill);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Skill {SkillId} removed from user {UserId}", skillId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing skill {SkillId} from user {UserId}", skillId, userId);
                throw;
            }
        }

        public async Task<bool> UpdateUserSkillAsync(int userId, int skillId, UpdateUserSkillDto updateDto)
        {
            try
            {
                var userSkill = await _context.UsuarioSkills
                    .FirstOrDefaultAsync(us => us.UsuarioId == userId && us.SkillId == skillId);

                if (userSkill == null)
                {
                    return false;
                }

                userSkill.Nivel = updateDto.Nivel;
                userSkill.Certificacion = updateDto.Certificacion;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user skill for user {UserId}, skill {SkillId}", userId, skillId);
                throw;
            }
        }

        public async Task<List<SkillDto>> SearchSkillsAsync(string searchTerm)
        {
            try
            {
                var term = searchTerm.ToLower();
                var skills = await _context.Skills
                    .Include(s => s.UsuarioSkills)
                    .Where(s => s.EsActivo && 
                               (s.Nombre.ToLower().Contains(term) || 
                                s.Descripcion.ToLower().Contains(term) ||
                                s.Categoria.ToLower().Contains(term)))
                    .OrderBy(s => s.Categoria)
                    .ThenBy(s => s.Nombre)
                    .ToListAsync();

                return skills.Select(MapToSkillDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching skills with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<SkillStatsDto> GetSkillStatsAsync()
        {
            try
            {
                var totalSkills = await _context.Skills.CountAsync(s => s.EsActivo);
                var totalUserSkills = await _context.UsuarioSkills.CountAsync();
                var uniqueSkillHolders = await _context.UsuarioSkills
                    .Select(us => us.UsuarioId)
                    .Distinct()
                    .CountAsync();

                var categoryStats = await _context.Skills
                    .Where(s => s.EsActivo)
                    .GroupBy(s => s.Categoria)
                    .Select(g => new SkillCategoryStatsDto
                    {
                        Categoria = g.Key,
                        TotalSkills = g.Count(),
                        TotalUserSkills = g.Sum(s => s.UsuarioSkills.Count)
                    })
                    .ToListAsync();

                var topSkills = await _context.Skills
                    .Where(s => s.EsActivo)
                    .Select(s => new SkillPopularityDto
                    {
                        SkillId = s.Id,
                        Nombre = s.Nombre,
                        IconoUrl = s.IconoUrl,
                        UserCount = s.UsuarioSkills.Count,
                        Categoria = s.Categoria
                    })
                    .OrderByDescending(s => s.UserCount)
                    .Take(10)
                    .ToListAsync();

                return new SkillStatsDto
                {
                    TotalSkills = totalSkills,
                    TotalUserSkills = totalUserSkills,
                    UniqueSkillHolders = uniqueSkillHolders,
                    CategoryStats = categoryStats,
                    TopSkills = topSkills
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skill stats");
                throw;
            }
        }

        public async Task<List<UserSkillDto>> GetUsersWithSkillAsync(int skillId, int page = 1, int pageSize = 20)
        {
            try
            {
                var userSkills = await _context.UsuarioSkills
                    .Include(us => us.Usuario)
                    .Include(us => us.Skill)
                    .Where(us => us.SkillId == skillId)
                    .OrderByDescending(us => us.FechaAdquisicion)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return userSkills.Select(us => new UserSkillDto
                {
                    UsuarioId = us.UsuarioId,
                    SkillId = us.SkillId,
                    Usuario = new UserBasicDto
                    {
                        Id = us.Usuario.Id,
                        Nombre = us.Usuario.Nombre,
                        Apellido = us.Usuario.Apellido,
                        ImagenUrl = us.Usuario.ImagenUrl
                    },
                    Skill = MapToSkillDto(us.Skill),
                    Nivel = us.Nivel,
                    Certificacion = us.Certificacion,
                    FechaAdquisicion = us.FechaAdquisicion
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with skill {SkillId}", skillId);
                throw;
            }
        }

        public async Task<List<SkillDto>> GetSkillsForOpportunityMatchingAsync(int userId)
        {
            try
            {
                // Get user's skills that could match with opportunities
                var userSkills = await _context.UsuarioSkills
                    .Include(us => us.Skill)
                    .Where(us => us.UsuarioId == userId && us.Skill.EsActivo)
                    .Select(us => us.Skill)
                    .ToListAsync();

                return userSkills.Select(MapToSkillDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skills for opportunity matching for user {UserId}", userId);
                throw;
            }
        }

        public async Task SeedDefaultSkillsAsync()
        {
            try
            {
                if (await _context.Skills.AnyAsync())
                {
                    return; // Skills already exist
                }

                var defaultSkills = SkillTemplates.GetDefaultSkills();
                var skills = defaultSkills.Select(dto => new Skill
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Categoria = dto.Categoria,
                    IconoUrl = dto.IconoUrl,
                    Color = dto.Color,
                    EsActivo = true,
                    FechaCreacion = DateTime.UtcNow
                }).ToList();

                _context.Skills.AddRange(skills);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Seeded {Count} default skills", skills.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default skills");
                throw;
            }
        }

        private SkillDto MapToSkillDto(Skill skill)
        {
            return new SkillDto
            {
                Id = skill.Id,
                Nombre = skill.Nombre,
                Descripcion = skill.Descripcion,
                Categoria = skill.Categoria,
                IconoUrl = skill.IconoUrl,
                Color = skill.Color,
                EsActivo = skill.EsActivo,
                FechaCreacion = skill.FechaCreacion,
                TotalUsuarios = skill.UsuarioSkills?.Count ?? 0
            };
        }
    }
}