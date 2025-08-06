using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly DbContextApplication _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(DbContextApplication context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                var totalUsers = await _context.Usuarios.CountAsync();
                var totalOrganizations = await _context.Organizaciones.CountAsync();
                var totalOpportunities = await _context.VolunteerOpportunities.CountAsync();
                var activeOpportunities = await _context.VolunteerOpportunities
                    .CountAsync(o => o.Estatus == OpportunityStatus.Activa);
                var totalApplications = await _context.VolunteerApplications.CountAsync();
                var completedOpportunities = await _context.VolunteerOpportunities
                    .CountAsync(o => o.Estatus == OpportunityStatus.Cerrada);

                return new DashboardStatsDto
                {
                    TotalUsers = totalUsers,
                    TotalOrganizations = totalOrganizations,
                    TotalOpportunities = totalOpportunities,
                    ActiveOpportunities = activeOpportunities,
                    TotalApplications = totalApplications,
                    CompletedOpportunities = completedOpportunities,
                    AverageRating = 4.5, // Placeholder until rating system is implemented
                    TotalVolunteerHours = totalApplications * 8 // Estimated based on applications
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return new DashboardStatsDto();
            }
        }

        public async Task<UserDashboardDto> GetUserDashboardDataAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null)
                    return new UserDashboardDto { UserId = userId };

                var applications = await _context.VolunteerApplications
                    .Where(a => a.UsuarioId == userId)
                    .ToListAsync();

                var recentActivities = await GetRecentActivitiesAsync(userId, 5);
                var suggestedOpportunities = await _context.VolunteerOpportunities
                    .Include(o => o.Organizacion)
                    .Where(o => o.Estatus == OpportunityStatus.Activa)
                    .OrderByDescending(o => o.FechaCreacion)
                    .Take(3)
                    .Select(o => new OpportunityListDto
                    {
                        Id = o.Id,
                        Titulo = o.Titulo,
                        Descripcion = o.Descripcion,
                        Ubicacion = o.Ubicacion,
                        FechaInicio = o.FechaInicio,
                        FechaFin = o.FechaFin,
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

                return new UserDashboardDto
                {
                    UserId = userId,
                    ApplicationsCount = applications.Count,
                    ApprovedApplications = applications.Count(a => a.Estatus == ApplicationStatus.Aceptada),
                    CompletedActivities = applications.Count(a => a.Estatus == ApplicationStatus.Aceptada), // Assuming approved = completed for now
                    VolunteerHours = applications.Count(a => a.Estatus == ApplicationStatus.Aceptada) * 8, // Estimated
                    AverageRating = 4.5, // Placeholder
                    BadgesCount = 0, // Placeholder until badge system is implemented
                    RecentActivities = recentActivities.ToList(),
                    SuggestedOpportunities = suggestedOpportunities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user dashboard data for user {UserId}", userId);
                return new UserDashboardDto { UserId = userId };
            }
        }

        public async Task<OrganizationDashboardDto> GetOrganizationDashboardDataAsync(int organizationId)
        {
            try
            {
                var opportunities = await _context.VolunteerOpportunities
                    .Where(o => o.OrganizacionId == organizationId)
                    .ToListAsync();

                var applications = await _context.VolunteerApplications
                    .Include(a => a.Opportunity)
                    .Include(a => a.Usuario)
                    .Where(a => a.Opportunity.OrganizacionId == organizationId)
                    .ToListAsync();

                var recentApplications = applications
                    .OrderByDescending(a => a.FechaAplicacion)
                    .Take(5)
                    .Select(a => new RecentApplicationDto
                    {
                        Id = a.Id,
                        VolunteerName = $"{a.Usuario.Nombre} {a.Usuario.Apellido}",
                        OpportunityTitle = a.Opportunity.Titulo,
                        ApplicationDate = a.FechaAplicacion,
                        Status = a.Estatus,
                        Message = a.Mensaje
                    })
                    .ToList();

                var recentOpportunities = opportunities
                    .OrderByDescending(o => o.FechaCreacion)
                    .Take(3)
                    .Select(o => new OpportunityListDto
                    {
                        Id = o.Id,
                        Titulo = o.Titulo,
                        Descripcion = o.Descripcion,
                        Ubicacion = o.Ubicacion,
                        FechaInicio = o.FechaInicio,
                        FechaFin = o.FechaFin,
                        DuracionHoras = o.DuracionHoras,
                        VoluntariosRequeridos = o.VoluntariosRequeridos,
                        VoluntariosInscritos = o.VoluntariosInscritos,
                        AreaInteres = o.AreaInteres,
                        NivelExperiencia = o.NivelExperiencia,
                        Estatus = o.Estatus,
                        Organizacion = new OrganizacionBasicDto
                        {
                            Id = organizationId,
                            Nombre = "", // Will be filled by the controller
                            Ubicacion = ""
                        }
                    })
                    .ToList();

                return new OrganizationDashboardDto
                {
                    OrganizationId = organizationId,
                    OpportunitiesCreated = opportunities.Count,
                    ActiveOpportunities = opportunities.Count(o => o.Estatus == OpportunityStatus.Activa),
                    TotalApplications = applications.Count,
                    ApprovedApplications = applications.Count(a => a.Estatus == ApplicationStatus.Aceptada),
                    TotalVolunteers = applications.Where(a => a.Estatus == ApplicationStatus.Aceptada).Select(a => a.UsuarioId).Distinct().Count(),
                    AverageRating = 4.5, // Placeholder
                    RecentApplications = recentApplications,
                    RecentOpportunities = recentOpportunities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization dashboard data for organization {OrganizationId}", organizationId);
                return new OrganizationDashboardDto { OrganizationId = organizationId };
            }
        }

        public async Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int userId, int limit = 10)
        {
            try
            {
                var activities = new List<RecentActivityDto>();

                // Get recent applications by the user
                var recentApplications = await _context.VolunteerApplications
                    .Include(a => a.Opportunity)
                    .ThenInclude(o => o.Organizacion)
                    .Where(a => a.UsuarioId == userId)
                    .OrderByDescending(a => a.FechaAplicacion)
                    .Take(limit)
                    .ToListAsync();

                foreach (var app in recentApplications)
                {
                    string activityType = app.Estatus switch
                    {
                        ApplicationStatus.Pendiente => "applied",
                        ApplicationStatus.Aceptada => "approved",
                        ApplicationStatus.Rechazada => "rejected",
                        _ => "applied"
                    };

                    string description = app.Estatus switch
                    {
                        ApplicationStatus.Pendiente => "Aplicaste a una oportunidad de voluntariado",
                        ApplicationStatus.Aceptada => "Tu aplicación fue aprobada",
                        ApplicationStatus.Rechazada => "Tu aplicación fue rechazada",
                        _ => "Actividad de aplicación"
                    };

                    activities.Add(new RecentActivityDto
                    {
                        Id = app.Id,
                        Title = $"Aplicación a {app.Opportunity.Titulo}",
                        Description = description,
                        ActivityType = activityType,
                        Date = app.FechaRespuesta ?? app.FechaAplicacion,
                        Status = app.Estatus.ToString(),
                        OrganizationName = app.Opportunity.Organizacion.Nombre,
                        OpportunityTitle = app.Opportunity.Titulo
                    });
                }

                return activities.OrderByDescending(a => a.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities for user {UserId}", userId);
                return new List<RecentActivityDto>();
            }
        }

        public async Task<IEnumerable<OpportunityListDto>> GetUserOpportunitiesAsync(int userId, int limit = 5)
        {
            try
            {
                // Get opportunities where user has applied and been approved
                var userOpportunities = await _context.VolunteerApplications
                    .Include(a => a.Opportunity)
                    .ThenInclude(o => o.Organizacion)
                    .Where(a => a.UsuarioId == userId && a.Estatus == ApplicationStatus.Aceptada)
                    .Select(a => a.Opportunity)
                    .Take(limit)
                    .Select(o => new OpportunityListDto
                    {
                        Id = o.Id,
                        Titulo = o.Titulo,
                        Descripcion = o.Descripcion,
                        Ubicacion = o.Ubicacion,
                        FechaInicio = o.FechaInicio,
                        FechaFin = o.FechaFin,
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

                return userOpportunities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user opportunities for user {UserId}", userId);
                return new List<OpportunityListDto>();
            }
        }
    }
}