using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoluntariadoConectadoRD.Attributes;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Data;
using Microsoft.EntityFrameworkCore;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticaci�n para todos los endpoints
    public class VoluntariadoController : ControllerBase
{
    private readonly ILogger<VoluntariadoController> _logger;
    private readonly IOpportunityService _opportunityService;
    private readonly DbContextApplication _context;

    public VoluntariadoController(ILogger<VoluntariadoController> logger, IOpportunityService opportunityService, DbContextApplication context)
    {
        _logger = logger;
        _opportunityService = opportunityService;
        _context = context;
    }

    /// <summary>
    /// Endpoint disponible para todos los usuarios (público)
    /// </summary>
    [HttpGet("opportunities")]
    [AllowAnonymous] // Permitir acceso sin autenticación
    public async Task<ActionResult<ApiResponseDto<IEnumerable<OpportunityListDto>>>> GetVolunteerOpportunities()
    {
        try
        {
            var opportunities = await _opportunityService.GetAllOpportunitiesAsync();

                return Ok(new ApiResponseDto<IEnumerable<OpportunityListDto>>
            {
                Success = true,
                Message = "Lista de oportunidades de voluntariado",
                Data = opportunities
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving volunteer opportunities");
            return StatusCode(500, new ApiResponseDto<IEnumerable<OpportunityListDto>>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Obtener detalles de una oportunidad específica (público)
    /// </summary>
    [HttpGet("opportunities/{id}")]
    [AllowAnonymous] // Permitir acceso sin autenticación
    public async Task<ActionResult<ApiResponseDto<OpportunityDetailDto>>> GetOpportunityDetails(int id)
    {
        try
        {
            var opportunity = await _opportunityService.GetOpportunityByIdAsync(id);

            if (opportunity == null)
            {
                return NotFound(new ApiResponseDto<OpportunityDetailDto>
                {
                    Success = false,
                    Message = "Oportunidad no encontrada"
                });
            }

            return Ok(new ApiResponseDto<OpportunityDetailDto>
            {
                Success = true,
                Message = "Detalles de la oportunidad",
                Data = opportunity
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving opportunity details for ID {Id}", id);
            return StatusCode(500, new ApiResponseDto<OpportunityDetailDto>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Endpoint solo para voluntarios - Aplicar a oportunidades
    /// </summary>
    [HttpPost("apply/{opportunityId}")]
    [VoluntarioOnly]
    public async Task<ActionResult<ApiResponseDto<object>>> ApplyToOpportunity(int opportunityId, [FromBody] ApplyToOpportunityDto? applyDto = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Usuario no v�lido"
                });
            }

            var success = await _opportunityService.ApplyToOpportunityAsync(opportunityId, userId, applyDto);

            if (!success)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "No se pudo enviar la aplicaci�n. La oportunidad puede no existir o ya aplicaste anteriormente."
                });
            }

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Aplicaci�n enviada exitosamente",
                Data = new { userId, opportunityId, message = "Tu aplicaci�n ha sido enviada" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying to opportunity {OpportunityId}", opportunityId);
            return StatusCode(500, new ApiResponseDto<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Endpoint solo para organizaciones - Crear oportunidades
    /// </summary>
    [HttpPost("opportunities")]
    [OrganizacionOnly]
    public async Task<ActionResult<ApiResponseDto<OpportunityDetailDto>>> CreateOpportunity([FromBody] CreateOpportunityDto opportunityDto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new ApiResponseDto<OpportunityDetailDto>
                {
                    Success = false,
                    Message = "Usuario no válido"
                });
            }

            var organizacionId = await _opportunityService.GetOrganizacionIdByUserAsync(userId);
            if (!organizacionId.HasValue)
            {
                return BadRequest(new ApiResponseDto<OpportunityDetailDto>
                {
                    Success = false,
                    Message = "Usuario no asociado a una organización"
                });
            }

            var result = await _opportunityService.CreateOpportunityAsync(opportunityDto, organizacionId.Value);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating opportunity");
            return StatusCode(500, new ApiResponseDto<OpportunityDetailDto>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Endpoint solo para organizaciones - Gestionar aplicaciones
    /// </summary>
    [HttpGet("applications")]
    [OrganizacionOnly]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<ApplicationDto>>>> GetApplications()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new ApiResponseDto<IEnumerable<ApplicationDto>>
                {
                    Success = false,
                    Message = "Usuario no válido"
                });
            }

            var organizacionId = await _opportunityService.GetOrganizacionIdByUserAsync(userId);
            if (!organizacionId.HasValue)
            {
                return BadRequest(new ApiResponseDto<IEnumerable<ApplicationDto>>
                {
                    Success = false,
                    Message = "Usuario no asociado a una organización"
                });
            }

            var applications = await _opportunityService.GetOrganizationApplicationsAsync(organizacionId.Value);

            return Ok(new ApiResponseDto<IEnumerable<ApplicationDto>>
            {
                Success = true,
                Message = "Aplicaciones obtenidas exitosamente",
                Data = applications
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization applications");
            return StatusCode(500, new ApiResponseDto<IEnumerable<ApplicationDto>>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Endpoint para voluntarios y administradores
    /// </summary>
    [HttpGet("my-applications")]
    [VoluntarioOrAdmin]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<ApplicationDto>>>> GetMyApplications()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new ApiResponseDto<IEnumerable<ApplicationDto>>
                {
                    Success = false,
                    Message = "Usuario no válido"
                });
            }

            var applications = await _opportunityService.GetVolunteerApplicationsAsync(userId);

            return Ok(new ApiResponseDto<IEnumerable<ApplicationDto>>
            {
                Success = true,
                Message = "Aplicaciones obtenidas exitosamente",
                Data = applications
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user applications");
            return StatusCode(500, new ApiResponseDto<IEnumerable<ApplicationDto>>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Update application status (approve/reject) - Organizations only
    /// </summary>
    [HttpPut("applications/{applicationId}/status")]
    [OrganizacionOnly]
    public async Task<ActionResult<ApiResponseDto<object>>> UpdateApplicationStatus(int applicationId, [FromBody] UpdateApplicationStatusDto statusDto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Usuario no válido"
                });
            }

            var organizacionId = await _opportunityService.GetOrganizacionIdByUserAsync(userId);
            if (!organizacionId.HasValue)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Usuario no asociado a una organización"
                });
            }

            var success = await _opportunityService.UpdateApplicationStatusAsync(applicationId, statusDto.Status, statusDto.Notes);
            
            if (!success)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "No se pudo actualizar la aplicación. Verifica que sea una aplicación válida de tu organización."
                });
            }

            var statusText = statusDto.Status switch
            {
                ApplicationStatus.Aceptada => "aceptada",
                ApplicationStatus.Rechazada => "rechazada",
                _ => "actualizada"
            };

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = $"Aplicación {statusText} exitosamente",
                Data = new { applicationId, status = statusDto.Status, notes = statusDto.Notes }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application status for applicationId {ApplicationId}", applicationId);
            return StatusCode(500, new ApiResponseDto<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Cancel an application - Volunteers only
    /// </summary>
    [HttpDelete("applications/{applicationId}")]
    [VoluntarioOnly]
    public async Task<ActionResult<ApiResponseDto<object>>> CancelApplication(int applicationId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Usuario no válido"
                });
            }

            // Check if the application belongs to the current user
            var userApplications = await _opportunityService.GetVolunteerApplicationsAsync(userId);
            var application = userApplications.FirstOrDefault(a => a.Id == applicationId);

            if (application == null)
            {
                return NotFound(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Aplicación no encontrada o no pertenece al usuario"
                });
            }

            if (application.Estatus == ApplicationStatus.Aceptada)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "No se puede cancelar una aplicación ya aceptada"
                });
            }

            var success = await _opportunityService.UpdateApplicationStatusAsync(applicationId, ApplicationStatus.Rechazada, "Cancelada por el voluntario");
            
            if (!success)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "No se pudo cancelar la aplicación"
                });
            }

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Aplicación cancelada exitosamente",
                Data = new { applicationId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling application {ApplicationId}", applicationId);
            return StatusCode(500, new ApiResponseDto<object>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Endpoint para organizaciones y administradores
    /// </summary>
    [HttpPut("opportunities/{opportunityId}")]
    [OrganizacionOrAdmin]
    public async Task<ActionResult<ApiResponseDto<OpportunityDetailDto>>> UpdateOpportunity(int opportunityId, [FromBody] UpdateOpportunityDto opportunityData)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new ApiResponseDto<OpportunityDetailDto>
                {
                    Success = false,
                    Message = "Usuario no válido"
                });
            }

            var organizacionId = await _opportunityService.GetOrganizacionIdByUserAsync(userId);
            if (!organizacionId.HasValue)
            {
                return BadRequest(new ApiResponseDto<OpportunityDetailDto>
                {
                    Success = false,
                    Message = "Usuario no asociado a una organización"
                });
            }

            var result = await _opportunityService.UpdateOpportunityAsync(opportunityId, opportunityData, organizacionId.Value);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating opportunity {OpportunityId}", opportunityId);
            return StatusCode(500, new ApiResponseDto<OpportunityDetailDto>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Get organization statistics for dashboard
    /// </summary>
    [HttpGet("organization/stats")]
    public async Task<ActionResult<ApiResponseDto<OrganizationStatsDto>>> GetOrganizationStats()
    {
        try
        {
            // Get current user ID to filter by organization if needed
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Calculate real statistics from database
            var activeVolunteers = await _context.Usuarios
                .CountAsync(u => u.Rol == UserRole.Voluntario && u.Estatus == UserStatus.Activo);
            
            var completedActivities = await _context.VolunteerActivities
                .CountAsync(va => va.Estado == ActivityStatus.Completada);
            
            var totalProjects = await _context.VolunteerOpportunities
                .CountAsync(vo => vo.Estatus == OpportunityStatus.Activa);
            
            var totalHours = await _context.VolunteerActivities
                .Where(va => va.Estado == ActivityStatus.Completada)
                .SumAsync(va => va.HorasCompletadas);
            
            var totalVolunteers = await _context.Usuarios
                .CountAsync(u => u.Rol == UserRole.Voluntario);
            
            var activeOrganizations = await _context.Organizaciones
                .CountAsync(o => o.Estatus == OrganizacionStatus.Activa);

            // Calculate recurrent donors percentage from applications
            var totalApplicants = await _context.VolunteerApplications
                .Select(va => va.UsuarioId)
                .Distinct()
                .CountAsync();
            
            var recurrentApplicants = await _context.VolunteerApplications
                .GroupBy(va => va.UsuarioId)
                .Where(g => g.Count() > 1)
                .CountAsync();
            
            var recurrentPercentage = totalApplicants > 0 ? (int)Math.Round((double)recurrentApplicants / totalApplicants * 100) : 0;

            // Calculate growth percentage based on recent vs older applications
            var threeMonthsAgo = DateTime.Now.AddMonths(-3);
            var recentApplications = await _context.VolunteerApplications
                .CountAsync(va => va.FechaAplicacion >= threeMonthsAgo);
            var olderApplications = await _context.VolunteerApplications
                .CountAsync(va => va.FechaAplicacion < threeMonthsAgo);
            
            var growthPercentage = olderApplications > 0 ? 
                (int)Math.Round((double)(recentApplications - olderApplications) / olderApplications * 100) : 0;

            // Calculate new communities from recent organizations
            var newCommunities = await _context.Organizaciones
                .CountAsync(o => o.FechaRegistro >= threeMonthsAgo && o.Estatus == OrganizacionStatus.Activa);

            // Calculate real monthly donation data from completed volunteer hours
            var monthlyDonations = new List<MonthlyDonationDto>();
            var months = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun" };
            
            for (int i = 0; i < months.Length; i++)
            {
                var monthStart = DateTime.Now.AddMonths(-(months.Length - 1 - i)).Date.AddDays(-(DateTime.Now.Day - 1));
                var monthEnd = monthStart.AddMonths(1);
                
                var monthlyHours = await _context.VolunteerActivities
                    .Where(va => va.Estado == ActivityStatus.Completada && 
                                va.FechaCompletada >= monthStart && 
                                va.FechaCompletada < monthEnd)
                    .SumAsync(va => (decimal?)va.HorasCompletadas) ?? 0;
                
                monthlyDonations.Add(new MonthlyDonationDto 
                { 
                    Month = months[i], 
                    Amount = Math.Round(monthlyHours * 15.5m, 0) // Estimated value per hour
                });
            }

            var stats = new OrganizationStatsDto
            {
                ActiveDonors = activeVolunteers,
                EventsRealized = completedActivities,
                TotalDonations = Math.Round(totalHours * 15.5m, 0), // Estimated value per hour
                ActiveProjects = totalProjects,
                AverageDonation = totalVolunteers > 0 ? Math.Round((totalHours * 15.5m) / totalVolunteers, 0) : 0,
                NewDonors = Math.Max(1, activeVolunteers / 10), // 10% are estimated as new
                RecurrentDonorsPercentage = recurrentPercentage,
                PeopleBenefited = totalHours * 2, // Estimate 2 people benefited per hour
                CommunitiesReached = activeOrganizations,
                GrowthPercentage = Math.Max(-50, Math.Min(growthPercentage, 200)), // Cap between -50% and 200%
                NewCommunities = newCommunities,
                MonthlyDonations = monthlyDonations,
                ImpactDistribution = new List<ImpactDistributionDto>
                {
                    new ImpactDistributionDto { Category = "Educación", Percentage = 45, Color = "#87CEEB" },
                    new ImpactDistributionDto { Category = "Salud", Percentage = 25, Color = "#4682B4" },
                    new ImpactDistributionDto { Category = "Medio ambiente", Percentage = 20, Color = "#2E4A6B" },
                    new ImpactDistributionDto { Category = "Otros", Percentage = 10, Color = "#B0C4DE" }
                }
            };

            return Ok(new ApiResponseDto<OrganizationStatsDto>
            {
                Success = true,
                Message = "Estadísticas de organización obtenidas exitosamente",
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization stats");
            return StatusCode(500, new ApiResponseDto<OrganizationStatsDto>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Get user's volunteer events (opportunities they applied to)
    /// </summary>
    [HttpGet("user/events")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<UserEventDto>>>> GetUserEvents()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new ApiResponseDto<IEnumerable<UserEventDto>>
                {
                    Success = false,
                    Message = "Usuario no válido"
                });
            }

            // Get user's applications with related opportunities
            var userEvents = await _context.VolunteerApplications
                .Where(va => va.UsuarioId == userId && 
                    (va.Estatus == ApplicationStatus.Aceptada || va.Estatus == ApplicationStatus.Pendiente))
                .Include(va => va.Opportunity)
                    .ThenInclude(o => o.Organizacion)
                .OrderBy(va => va.Opportunity.FechaInicio)
                .Select(va => new UserEventDto
                {
                    Id = va.Opportunity.Id,
                    ApplicationId = va.Id,
                    Title = va.Opportunity.Titulo,
                    OrganizationName = va.Opportunity.Organizacion.Nombre,
                    Date = va.Opportunity.FechaInicio,
                    EndDate = va.Opportunity.FechaFin,
                    Location = va.Opportunity.Ubicacion ?? "Ubicación por definir",
                    Description = va.Opportunity.Descripcion,
                    Requirements = va.Opportunity.Requisitos,
                    DurationHours = va.Opportunity.DuracionHoras,
                    ApplicationStatus = va.Estatus == ApplicationStatus.Aceptada ? "Confirmado" : "Pendiente",
                    EventStatus = va.Opportunity.FechaInicio > DateTime.Now ? "Próximo" :
                                 (va.Opportunity.FechaFin.HasValue && va.Opportunity.FechaFin > DateTime.Now ? "En progreso" : "Completado"),
                    ImageUrl = null // You can add image URLs if stored in database
                })
                .ToListAsync();

            // Set image URLs based on organization logos or category-specific defaults
            var categoryImageMap = new Dictionary<string, string>
            {
                { "educación", "/images/categories/educacion.jpg" },
                { "salud", "/images/categories/salud.jpg" },
                { "medio ambiente", "/images/categories/medio-ambiente.jpg" },
                { "comunitario", "/images/categories/comunitario.jpg" },
                { "emergencia", "/images/categories/emergencia.jpg" },
                { "default", "/images/categories/voluntariado-default.jpg" }
            };

            foreach (var evt in userEvents)
            {
                // First try to use organization logo if available
                if (!string.IsNullOrEmpty(evt.OrganizationName))
                {
                    // Check if organization has a logo stored in database
                    var orgLogo = await _context.Organizaciones
                        .Where(o => o.Nombre == evt.OrganizationName && !string.IsNullOrEmpty(o.LogoUrl))
                        .Select(o => o.LogoUrl)
                        .FirstOrDefaultAsync();
                    
                    if (!string.IsNullOrEmpty(orgLogo))
                    {
                        evt.ImageUrl = orgLogo;
                        continue;
                    }
                }
                
                // Fallback to category-based image from opportunity's area of interest
                var opportunity = await _context.VolunteerOpportunities
                    .Where(vo => vo.Id == evt.Id)
                    .Select(vo => vo.AreaInteres)
                    .FirstOrDefaultAsync();
                
                var categoryKey = opportunity?.ToLower() ?? "default";
                if (!categoryImageMap.ContainsKey(categoryKey))
                    categoryKey = "default";
                
                evt.ImageUrl = categoryImageMap[categoryKey];
            }

            return Ok(new ApiResponseDto<IEnumerable<UserEventDto>>
            {
                Success = true,
                Message = "Eventos del usuario obtenidos exitosamente",
                Data = userEvents
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user events");
            return StatusCode(500, new ApiResponseDto<IEnumerable<UserEventDto>>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }

    /// <summary>
    /// Get organization events for dashboard
    /// </summary>
    [HttpGet("organization/events")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<OrganizationEventDto>>>> GetOrganizationEvents()
    {
        try
        {
            // Get volunteer opportunities as "events" from the database
            var opportunities = await _context.VolunteerOpportunities
                .OrderByDescending(vo => vo.FechaCreacion)
                .Take(6) // Limit to recent events
                .ToListAsync();

            var events = new List<OrganizationEventDto>();
            
            // Define icons based on opportunity category or type
            var categoryIconMap = new Dictionary<string, (string Icon, string Color)>
            {
                { "educación", ("bi-mortarboard", "text-info") },
                { "salud", ("bi-heart-pulse", "text-danger") },
                { "medio ambiente", ("bi-tree", "text-success") },
                { "comunitario", ("bi-people", "text-primary") },
                { "emergencia", ("bi-umbrella", "text-warning") },
                { "default", ("bi-house-heart", "text-secondary") }
            };

            foreach (var opportunity in opportunities)
            {
                // Select icon based on category or use default
                var categoryKey = opportunity.AreaInteres?.ToLower() ?? "default";
                if (!categoryIconMap.ContainsKey(categoryKey))
                    categoryKey = "default";
                
                var (icon, color) = categoryIconMap[categoryKey];
                
                // Get actual applications count for this opportunity
                var participants = await _context.VolunteerApplications
                    .CountAsync(va => va.OpportunityId == opportunity.Id && va.Estatus == ApplicationStatus.Aceptada);
                
                // Determine status based on dates
                var status = "Completado";
                if (opportunity.FechaInicio > DateTime.Now)
                {
                    status = "Próximo";
                }
                else if (opportunity.FechaFin > DateTime.Now)
                {
                    status = "En progreso";
                }

                events.Add(new OrganizationEventDto
                {
                    Id = opportunity.Id,
                    Name = opportunity.Titulo,
                    Date = opportunity.FechaInicio,
                    Location = opportunity.Ubicacion ?? "Ubicación por definir",
                    Participants = participants,
                    Status = status,
                    Icon = icon,
                    IconColor = color
                });
            }


            return Ok(new ApiResponseDto<IEnumerable<OrganizationEventDto>>
            {
                Success = true,
                Message = "Eventos de organización obtenidos exitosamente",
                Data = events
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization events");
            return StatusCode(500, new ApiResponseDto<IEnumerable<OrganizationEventDto>>
            {
                Success = false,
                Message = "Error interno del servidor"
            });
        }
    }
}
}