using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public class VolunteerService : IVolunteerService
    {
        private readonly DbContextApplication _context;
        private readonly ILogger<VolunteerService> _logger;

        public VolunteerService(DbContextApplication context, ILogger<VolunteerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EnhancedUserProfileDto?> GetUserProfileByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios
                    .Include(u => u.Badges)
                        .ThenInclude(ub => ub.Badge)
                    .Include(u => u.ResenasRecibidas)
                        .ThenInclude(r => r.Organizacion)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return null;

                // Get user skills
                var skills = await _context.UsuarioSkills
                    .Include(us => us.Skill)
                    .Where(us => us.UsuarioId == userId)
                    .Select(us => new UserSkillDto
                    {
                        Id = us.Skill.Id,
                        Nombre = us.Skill.Nombre,
                        Nivel = us.Nivel,
                        Categoria = us.Skill.Categoria,
                        FechaCreacion = us.FechaCreacion
                    })
                    .ToListAsync();

                // Get recent activities
                var activities = await _context.VolunteerActivities
                    .Include(va => va.Opportunity)
                        .ThenInclude(o => o.Organizacion)
                    .Where(va => va.UsuarioId == userId)
                    .OrderByDescending(va => va.FechaCreacion)
                    .Take(5)
                    .Select(va => new VolunteerActivityDto
                    {
                        Id = va.Id,
                        Titulo = va.Titulo,
                        Descripcion = va.Descripcion,
                        FechaInicio = va.FechaInicio,
                        FechaFin = va.FechaFin,
                        HorasCompletadas = va.HorasCompletadas,
                        Estado = va.Estado,
                        OpportunityTitle = va.Opportunity.Titulo,
                        OrganizacionNombre = va.Opportunity.Organizacion.Nombre,
                        FechaCreacion = va.FechaCreacion
                    })
                    .ToListAsync();

                // Calculate stats
                var totalActivities = await _context.VolunteerActivities
                    .Where(va => va.UsuarioId == userId && va.Estado == ActivityStatus.Completada)
                    .CountAsync();

                var projectsCount = await _context.VolunteerActivities
                    .Where(va => va.UsuarioId == userId)
                    .Select(va => va.OpportunityId)
                    .Distinct()
                    .CountAsync();

                return new EnhancedUserProfileDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    Email = user.Email,
                    Telefono = user.Telefono,
                    FechaNacimiento = user.FechaNacimiento,
                    Biografia = user.Biografia,
                    ExperienciaAnios = user.ExperienciaAnios ?? 0,
                    Disponibilidad = user.Disponibilidad,
                    ImagenUrl = user.ProfileImageUrl,
                    Ubicacion = user.Direccion,
                    PerfilCompleto = user.PerfilCompleto,
                    TotalResenas = user.TotalResenas,
                    CalificacionPromedio = user.CalificacionPromedio,
                    HorasVoluntariado = user.HorasVoluntariado,
                    EventosParticipados = totalActivities,
                    ProyectosParticipados = projectsCount,
                    Estado = user.Estatus,
                    FechaRegistro = user.FechaCreacion,
                    Habilidades = skills,
                    Badges = user.Badges.Select(ub => new BadgeDto
                    {
                        Id = ub.Badge.Id,
                        Nombre = ub.Badge.Nombre,
                        Descripcion = ub.Badge.Descripcion,
                        IconoUrl = ub.Badge.IconoUrl,
                        Color = ub.Badge.Color,
                        FechaObtenido = ub.FechaObtenido
                    }).ToList(),
                    UltimasResenas = user.ResenasRecibidas
                        .OrderByDescending(r => r.FechaCreacion)
                        .Take(3)
                        .Select(r => new UserReviewDto
                        {
                            OrganizacionNombre = r.Organizacion.Nombre,
                            Calificacion = r.Calificacion,
                            Comentario = r.Comentario,
                            FechaCreacion = r.FechaCreacion
                        }).ToList(),
                    ActividadesRecientes = activities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for user {UserId}", userId);
                return null;
            }
        }

        public async Task<VolunteerStatsDto?> GetVolunteerStatsAsync(int volunteerId)
        {
            try
            {
                var user = await _context.Usuarios
                    .Include(u => u.Badges)
                        .ThenInclude(ub => ub.Badge)
                    .FirstOrDefaultAsync(u => u.Id == volunteerId);

                if (user == null) return null;

                var activities = await _context.VolunteerActivities
                    .Include(va => va.Opportunity)
                        .ThenInclude(o => o.Organizacion)
                    .Where(va => va.UsuarioId == volunteerId)
                    .OrderByDescending(va => va.FechaCreacion)
                    .Take(10)
                    .Select(va => new VolunteerActivityDto
                    {
                        Id = va.Id,
                        Titulo = va.Titulo,
                        Descripcion = va.Descripcion,
                        FechaInicio = va.FechaInicio,
                        FechaFin = va.FechaFin,
                        HorasCompletadas = va.HorasCompletadas,
                        Estado = va.Estado,
                        OpportunityTitle = va.Opportunity.Titulo,
                        OrganizacionNombre = va.Opportunity.Organizacion.Nombre,
                        FechaCreacion = va.FechaCreacion
                    })
                    .ToListAsync();

                var projectsCount = await _context.VolunteerActivities
                    .Where(va => va.UsuarioId == volunteerId)
                    .Select(va => va.OpportunityId)
                    .Distinct()
                    .CountAsync();

                // Monthly stats for the last 6 months
                var monthlyStats = new Dictionary<string, int>();
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                
                var monthlyData = await _context.VolunteerActivities
                    .Where(va => va.UsuarioId == volunteerId && va.FechaCreacion >= sixMonthsAgo)
                    .GroupBy(va => new { va.FechaCreacion.Year, va.FechaCreacion.Month })
                    .Select(g => new { 
                        Year = g.Key.Year, 
                        Month = g.Key.Month, 
                        Count = g.Count() 
                    })
                    .ToListAsync();

                foreach (var data in monthlyData)
                {
                    var monthKey = new DateTime(data.Year, data.Month, 1).ToString("MMM yyyy");
                    monthlyStats[monthKey] = data.Count;
                }

                return new VolunteerStatsDto
                {
                    Id = user.Id,
                    NombreCompleto = $"{user.Nombre} {user.Apellido}",
                    HorasVoluntariado = user.HorasVoluntariado,
                    EventosParticipados = activities.Count(a => a.Estado == ActivityStatus.Completada),
                    ProyectosParticipados = projectsCount,
                    CalificacionPromedio = user.CalificacionPromedio,
                    TotalResenas = user.TotalResenas,
                    FechaRegistro = user.FechaCreacion,
                    Estado = user.Estatus,
                    ActividadesRecientes = activities,
                    Badges = user.Badges.Select(ub => new BadgeDto
                    {
                        Id = ub.Badge.Id,
                        Nombre = ub.Badge.Nombre,
                        Descripcion = ub.Badge.Descripcion,
                        IconoUrl = ub.Badge.IconoUrl,
                        Color = ub.Badge.Color,
                        FechaObtenido = ub.FechaObtenido
                    }).ToList(),
                    EstadisticasPorMes = monthlyStats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting volunteer stats for user {UserId}", volunteerId);
                return null;
            }
        }

        public async Task<IEnumerable<VolunteerApplicationDetailDto>> GetMyApplicationsAsync(int userId)
        {
            try
            {
                return await _context.VolunteerApplications
                    .Include(va => va.Opportunity)
                        .ThenInclude(o => o.Organizacion)
                    .Where(va => va.UsuarioId == userId)
                    .OrderByDescending(va => va.FechaAplicacion)
                    .Select(va => new VolunteerApplicationDetailDto
                    {
                        Id = va.Id,
                        OpportunityTitle = va.Opportunity.Titulo,
                        OrganizacionNombre = va.Opportunity.Organizacion.Nombre,
                        FechaInicio = va.Opportunity.FechaInicio,
                        FechaFin = va.Opportunity.FechaFin,
                        Ubicacion = va.Opportunity.Ubicacion,
                        Estado = va.Estatus,
                        FechaAplicacion = va.FechaAplicacion,
                        FechaRespuesta = va.FechaRespuesta,
                        Mensaje = va.Mensaje,
                        NotasOrganizacion = va.NotasOrganizacion,
                        HorasEstimadas = va.Opportunity.DuracionHoras
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applications for user {UserId}", userId);
                return new List<VolunteerApplicationDetailDto>();
            }
        }

        public async Task<IEnumerable<SkillDto>> GetAllSkillsAsync()
        {
            try
            {
                return await _context.Skills
                    .Where(s => s.EsActivo)
                    .OrderBy(s => s.Categoria)
                    .ThenBy(s => s.Nombre)
                    .Select(s => new SkillDto
                    {
                        Id = s.Id,
                        Nombre = s.Nombre,
                        Descripcion = s.Descripcion,
                        Categoria = s.Categoria
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all skills");
                return new List<SkillDto>();
            }
        }

        public async Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(int userId)
        {
            try
            {
                return await _context.UsuarioSkills
                    .Include(us => us.Skill)
                    .Where(us => us.UsuarioId == userId)
                    .Select(us => new UserSkillDto
                    {
                        Id = us.Skill.Id,
                        Nombre = us.Skill.Nombre,
                        Nivel = us.Nivel,
                        Categoria = us.Skill.Categoria,
                        FechaCreacion = us.FechaCreacion
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skills for user {UserId}", userId);
                return new List<UserSkillDto>();
            }
        }

        public async Task<bool> AddUserSkillAsync(int userId, CreateUserSkillDto skillDto)
        {
            try
            {
                // Check if skill exists
                var skill = await _context.Skills.FindAsync(skillDto.SkillId);
                if (skill == null) return false;

                // Check if user already has this skill
                var existingSkill = await _context.UsuarioSkills
                    .FirstOrDefaultAsync(us => us.UsuarioId == userId && us.SkillId == skillDto.SkillId);

                if (existingSkill != null) return false;

                var userSkill = new UsuarioSkill
                {
                    UsuarioId = userId,
                    SkillId = skillDto.SkillId,
                    Nivel = skillDto.Nivel
                };

                _context.UsuarioSkills.Add(userSkill);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding skill to user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdateUserSkillAsync(int userId, int skillId, int nivel)
        {
            try
            {
                var userSkill = await _context.UsuarioSkills
                    .FirstOrDefaultAsync(us => us.UsuarioId == userId && us.SkillId == skillId);

                if (userSkill == null) return false;

                userSkill.Nivel = nivel;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skill for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RemoveUserSkillAsync(int userId, int skillId)
        {
            try
            {
                var userSkill = await _context.UsuarioSkills
                    .FirstOrDefaultAsync(us => us.UsuarioId == userId && us.SkillId == skillId);

                if (userSkill == null) return false;

                _context.UsuarioSkills.Remove(userSkill);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing skill from user {UserId}", userId);
                return false;
            }
        }

        public async Task<IEnumerable<VolunteerActivityDto>> GetUserActivitiesAsync(int userId)
        {
            try
            {
                return await _context.VolunteerActivities
                    .Include(va => va.Opportunity)
                        .ThenInclude(o => o.Organizacion)
                    .Where(va => va.UsuarioId == userId)
                    .OrderByDescending(va => va.FechaCreacion)
                    .Select(va => new VolunteerActivityDto
                    {
                        Id = va.Id,
                        Titulo = va.Titulo,
                        Descripcion = va.Descripcion,
                        FechaInicio = va.FechaInicio,
                        FechaFin = va.FechaFin,
                        HorasCompletadas = va.HorasCompletadas,
                        Estado = va.Estado,
                        Notas = va.Notas,
                        CalificacionVoluntario = va.CalificacionVoluntario,
                        CalificacionOrganizacion = va.CalificacionOrganizacion,
                        ComentarioVoluntario = va.ComentarioVoluntario,
                        ComentarioOrganizacion = va.ComentarioOrganizacion,
                        OpportunityTitle = va.Opportunity.Titulo,
                        OrganizacionNombre = va.Opportunity.Organizacion.Nombre,
                        FechaCreacion = va.FechaCreacion
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activities for user {UserId}", userId);
                return new List<VolunteerActivityDto>();
            }
        }

        public async Task<VolunteerActivity?> CreateActivityAsync(int userId, int opportunityId, string titulo)
        {
            try
            {
                var opportunity = await _context.VolunteerOpportunities.FindAsync(opportunityId);
                if (opportunity == null) return null;

                var activity = new VolunteerActivity
                {
                    UsuarioId = userId,
                    OpportunityId = opportunityId,
                    Titulo = titulo,
                    FechaInicio = opportunity.FechaInicio,
                    FechaFin = opportunity.FechaFin,
                    Estado = ActivityStatus.Programada
                };

                _context.VolunteerActivities.Add(activity);
                await _context.SaveChangesAsync();
                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UpdateActivityStatusAsync(int activityId, ActivityStatus status, int? horas = null)
        {
            try
            {
                var activity = await _context.VolunteerActivities.FindAsync(activityId);
                if (activity == null) return false;

                activity.Estado = status;
                if (horas.HasValue)
                {
                    activity.HorasCompletadas = horas.Value;
                }
                activity.FechaActualizacion = DateTime.UtcNow;

                // Update user's total hours if activity is completed
                if (status == ActivityStatus.Completada && horas.HasValue)
                {
                    var user = await _context.Usuarios.FindAsync(activity.UsuarioId);
                    if (user != null)
                    {
                        user.HorasVoluntariado += horas.Value;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity status {ActivityId}", activityId);
                return false;
            }
        }

        public async Task<PlatformStatsDto?> GetPlatformStatsAsync()
        {
            try
            {
                var stats = await _context.PlatformStats.FirstOrDefaultAsync();
                
                if (stats == null)
                {
                    // Calculate stats if none exist
                    await UpdatePlatformStatsAsync();
                    stats = await _context.PlatformStats.FirstOrDefaultAsync();
                }

                return stats == null ? null : new PlatformStatsDto
                {
                    VoluntariosActivos = stats.VoluntariosActivos,
                    OrganizacionesActivas = stats.OrganizacionesActivas,
                    ProyectosActivos = stats.ProyectosActivos,
                    HorasTotalesDonadas = stats.HorasTotalesDonadas,
                    PersonasBeneficiadas = stats.PersonasBeneficiadas,
                    FondosRecaudados = stats.FondosRecaudados,
                    FechaActualizacion = stats.FechaActualizacion
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting platform stats");
                return null;
            }
        }

        public async Task<bool> UpdatePlatformStatsAsync()
        {
            try
            {
                var voluntariosActivos = await _context.Usuarios
                    .Where(u => u.Rol == UserRole.Voluntario && u.Estatus == UserStatus.Activo)
                    .CountAsync();

                var organizacionesActivas = await _context.Organizaciones
                    .Where(o => o.Estatus == OrganizacionStatus.Activa)
                    .CountAsync();

                var proyectosActivos = await _context.VolunteerOpportunities
                    .Where(o => o.Estatus == OpportunityStatus.Activa)
                    .CountAsync();

                var horasTotales = await _context.VolunteerActivities
                    .Where(va => va.Estado == ActivityStatus.Completada)
                    .SumAsync(va => va.HorasCompletadas);

                var stats = await _context.PlatformStats.FirstOrDefaultAsync();
                
                if (stats == null)
                {
                    stats = new PlatformStats();
                    _context.PlatformStats.Add(stats);
                }

                stats.VoluntariosActivos = voluntariosActivos;
                stats.OrganizacionesActivas = organizacionesActivas;
                stats.ProyectosActivos = proyectosActivos;
                stats.HorasTotalesDonadas = horasTotales;
                stats.PersonasBeneficiadas = horasTotales * 2; // Estimation
                stats.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating platform stats");
                return false;
            }
        }

        public async Task<IEnumerable<OpportunityListDto>> GetFeaturedOpportunitiesAsync(int count = 3)
        {
            try
            {
                return await _context.VolunteerOpportunities
                    .Include(o => o.Organizacion)
                    .Where(o => o.Estatus == OpportunityStatus.Activa)
                    .OrderByDescending(o => o.FechaCreacion)
                    .Take(count)
                    .Select(o => new OpportunityListDto
                    {
                        Id = o.Id,
                        Titulo = o.Titulo,
                        Descripcion = o.Descripcion,
                        FechaInicio = o.FechaInicio,
                        FechaFin = o.FechaFin ?? o.FechaInicio.AddDays(1),
                        Ubicacion = o.Ubicacion ?? "Por definir",
                        DuracionHoras = o.DuracionHoras,
                        VoluntariosRequeridos = o.VoluntariosRequeridos,
                        VoluntariosInscritos = o.VoluntariosInscritos,
                        AreaInteres = o.AreaInteres,
                        NivelExperiencia = o.NivelExperiencia,
                        Estatus = o.Estatus,
                        Organizacion = new OrganizacionBasicDto
                        {
                            Id = o.Organizacion.Id,
                            Nombre = o.Organizacion.Nombre,
                            Ubicacion = o.Organizacion.Direccion
                        }
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured opportunities");
                return new List<OpportunityListDto>();
            }
        }

        public async Task<PaginatedResult<AdminVolunteerDto>> GetAllVolunteersForAdminAsync(int page, int pageSize, string? search = null)
        {
            try
            {
                var query = _context.Usuarios
                    .Include(u => u.Organizacion)
                    .Where(u => u.Rol == UserRole.Voluntario);

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(u => 
                        u.Nombre.ToLower().Contains(searchLower) ||
                        u.Apellido.ToLower().Contains(searchLower) ||
                        u.Email.ToLower().Contains(searchLower));
                }

                var totalCount = await query.CountAsync();
                
                var volunteers = await query
                    .OrderByDescending(u => u.FechaCreacion)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new AdminVolunteerDto
                    {
                        Id = u.Id,
                        Nombre = u.Nombre,
                        Apellido = u.Apellido,
                        Email = u.Email,
                        Telefono = u.Telefono,
                        ImagenUrl = u.ProfileImageUrl,
                        Estado = u.Estatus,
                        FechaRegistro = u.FechaCreacion,
                        OrganizacionActual = u.Organizacion != null ? u.Organizacion.Nombre : null,
                        HorasVoluntariado = u.HorasVoluntariado,
                        EventosParticipados = 0, // Will need to be calculated differently
                        CalificacionPromedio = u.CalificacionPromedio,
                        UltimaActividad = u.FechaActualizacion,
                        Pais = "República Dominicana"
                    })
                    .ToListAsync();

                return new PaginatedResult<AdminVolunteerDto>
                {
                    Items = volunteers,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting volunteers for admin");
                return new PaginatedResult<AdminVolunteerDto>
                {
                    Items = new List<AdminVolunteerDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
        }

        public async Task<AdminStatsDto> GetAdminStatsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var startOfYear = new DateTime(now.Year, 1, 1);

                var totalVoluntarios = await _context.Usuarios.CountAsync(u => u.Rol == UserRole.Voluntario);
                var voluntariosActivos = await _context.Usuarios.CountAsync(u => u.Rol == UserRole.Voluntario && u.Estatus == UserStatus.Activo);
                var voluntariosInactivos = await _context.Usuarios.CountAsync(u => u.Rol == UserRole.Voluntario && u.Estatus == UserStatus.Inactivo);
                var voluntariosSuspendidos = await _context.Usuarios.CountAsync(u => u.Rol == UserRole.Voluntario && u.Estatus == UserStatus.Suspendido);

                var totalOrganizaciones = await _context.Organizaciones.CountAsync();
                var organizacionesActivas = await _context.Organizaciones.CountAsync(o => o.Estatus == OrganizacionStatus.Activa);

                var totalOportunidades = await _context.VolunteerOpportunities.CountAsync();
                var oportunidadesActivas = await _context.VolunteerOpportunities.CountAsync(o => o.Estatus == OpportunityStatus.Activa);

                var totalAplicaciones = await _context.VolunteerApplications.CountAsync(); 
                var aplicacionesPendientes = await _context.VolunteerApplications.CountAsync(a => a.Estatus == ApplicationStatus.Pendiente);
                var aplicacionesAprobadas = await _context.VolunteerApplications.CountAsync(a => a.Estatus == ApplicationStatus.Aceptada);

                var totalHorasVoluntariado = await _context.VolunteerActivities.SumAsync(va => va.HorasCompletadas);

                // Get monthly statistics
                var voluntariosPorMes = await _context.Usuarios
                    .Where(u => u.Rol == UserRole.Voluntario && u.FechaCreacion >= startOfYear)
                    .GroupBy(u => new { u.FechaCreacion.Year, u.FechaCreacion.Month })
                    .Select(g => new { 
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                        Count = g.Count() 
                    })
                    .ToDictionaryAsync(x => x.Month, x => x.Count);

                var aplicacionesPorMes = await _context.VolunteerApplications
                    .Where(a => a.FechaAplicacion >= startOfYear)
                    .GroupBy(a => new { a.FechaAplicacion.Year, a.FechaAplicacion.Month })
                    .Select(g => new { 
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}", 
                        Count = g.Count() 
                    })
                    .ToDictionaryAsync(x => x.Month, x => x.Count);

                return new AdminStatsDto
                {
                    TotalVoluntarios = totalVoluntarios,
                    VoluntariosActivos = voluntariosActivos,
                    VoluntariosInactivos = voluntariosInactivos,
                    VoluntariosSuspendidos = voluntariosSuspendidos,
                    TotalOrganizaciones = totalOrganizaciones,
                    OrganizacionesActivas = organizacionesActivas,
                    TotalOportunidades = totalOportunidades,
                    OportunidadesActivas = oportunidadesActivas,
                    TotalAplicaciones = totalAplicaciones,
                    AplicacionesPendientes = aplicacionesPendientes,
                    AplicacionesAprobadas = aplicacionesAprobadas,
                    TotalHorasVoluntariado = totalHorasVoluntariado,
                    FechaActualizacion = DateTime.UtcNow,
                    VoluntariosPorMes = voluntariosPorMes,
                    AplicacionesPorMes = aplicacionesPorMes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin stats");
                return new AdminStatsDto
                {
                    FechaActualizacion = DateTime.UtcNow
                };
            }
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, UserStatus status)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null) return false;

                user.Estatus = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> EditUserProfileAsAdminAsync(int userId, AdminEditUserDto editDto)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null) return false;

                user.Nombre = editDto.Nombre;
                user.Apellido = editDto.Apellido;
                user.Email = editDto.Email;
                user.Telefono = editDto.Telefono;
                user.FechaNacimiento = editDto.FechaNacimiento;
                user.Biografia = editDto.Biografia;
                user.Disponibilidad = editDto.Disponibilidad;
                user.Estatus = editDto.Status;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing user profile {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null) return false;

                // Soft delete by setting status to inactive
                user.Estatus = UserStatus.Inactivo;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return false;
            }
        }

        public async Task<PaginatedResult<AdminOrganizationDto>> GetAllOrganizationsForAdminAsync(int page, int pageSize, string? search = null)
        {
            try
            {
                var query = _context.Organizaciones
                    .Include(o => o.Usuario)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(o => 
                        o.Nombre.ToLower().Contains(searchLower) ||
                        o.Email.ToLower().Contains(searchLower) ||
                        (o.NumeroRegistro != null && o.NumeroRegistro.ToLower().Contains(searchLower)));
                }

                var totalCount = await query.CountAsync();
                
                var organizations = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(o => new AdminOrganizationDto
                    {
                        Id = o.Id,
                        Nombre = o.Nombre,
                        Email = o.Email,
                        Telefono = o.Telefono,
                        Direccion = o.Direccion,
                        SitioWeb = o.SitioWeb,
                        NumeroRegistro = o.NumeroRegistro,
                        Verificada = o.Verificada,
                        Estado = o.Usuario != null ? o.Usuario.Estatus : UserStatus.Inactivo,
                        FechaRegistro = o.FechaCreacion,
                        FechaVerificacion = o.FechaVerificacion,
                        LogoUrl = o.LogoUrl,
                        TipoOrganizacion = o.TipoOrganizacion,
                        TotalOportunidades = o.Opportunities.Count(),
                        OportunidadesActivas = o.Opportunities.Count(op => op.Estatus == OpportunityStatus.Activa),
                        TotalVoluntarios = o.Opportunities
                            .SelectMany(op => op.Aplicaciones)
                            .Where(app => app.Estatus == ApplicationStatus.Aceptada)
                            .Select(app => app.UsuarioId)
                            .Distinct()
                            .Count(),
                        UltimaActividad = o.Opportunities.Any() ? o.Opportunities.Max(op => op.FechaCreacion) : o.FechaCreacion
                    })
                    .ToListAsync();

                return new PaginatedResult<AdminOrganizationDto>
                {
                    Items = organizations,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizations for admin");
                return new PaginatedResult<AdminOrganizationDto>
                {
                    Items = new List<AdminOrganizationDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
        }

        public async Task<bool> UpdateOrganizationStatusAsync(int orgId, UserStatus status, bool verificada)
        {
            try
            {
                var organizacion = await _context.Organizaciones
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.Id == orgId);
                    
                if (organizacion == null) return false;

                organizacion.Verificada = verificada;
                if (verificada && organizacion.FechaVerificacion == null)
                {
                    organizacion.FechaVerificacion = DateTime.UtcNow;
                }

                if (organizacion.Usuario != null)
                {
                    organizacion.Usuario.Estatus = status;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization status {OrgId}", orgId);
                return false;
            }
        }

        public async Task<bool> EditOrganizationAsAdminAsync(int orgId, AdminEditOrganizationDto editDto)
        {
            try
            {
                var organizacion = await _context.Organizaciones
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.Id == orgId);
                    
                if (organizacion == null) return false;

                organizacion.Nombre = editDto.Nombre;
                organizacion.Descripcion = editDto.Descripcion;
                organizacion.Email = editDto.Email;
                organizacion.Telefono = editDto.Telefono;
                organizacion.Direccion = editDto.Direccion;
                organizacion.SitioWeb = editDto.SitioWeb;
                organizacion.NumeroRegistro = editDto.NumeroRegistro;
                organizacion.TipoOrganizacion = editDto.TipoOrganizacion;
                organizacion.Verificada = editDto.Verificada;

                if (organizacion.Usuario != null)
                {
                    organizacion.Usuario.Estatus = editDto.Status;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing organization {OrgId}", orgId);
                return false;
            }
        }

        public async Task<bool> DeleteOrganizationAsync(int orgId)
        {
            try
            {
                var organizacion = await _context.Organizaciones
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.Id == orgId);
                    
                if (organizacion == null) return false;

                // Soft delete by setting admin user status to inactive
                if (organizacion.Usuario != null)
                {
                    organizacion.Usuario.Estatus = UserStatus.Inactivo;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting organization {OrgId}", orgId);
                return false;
            }
        }
    }
}