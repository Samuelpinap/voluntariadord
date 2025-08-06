using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IOpportunityService _opportunityService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            IOpportunityService opportunityService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _opportunityService = opportunityService;
            _logger = logger;
        }

        /// <summary>
        /// Get general dashboard statistics (public)
        /// </summary>
        [HttpGet("stats")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<DashboardStatsDto>>> GetDashboardStats()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                return Ok(new ApiResponseDto<DashboardStatsDto>
                {
                    Success = true,
                    Message = "Estadísticas del dashboard obtenidas exitosamente",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new ApiResponseDto<DashboardStatsDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get user-specific dashboard data
        /// </summary>
        [HttpGet("user")]
        public async Task<ActionResult<ApiResponseDto<UserDashboardDto>>> GetUserDashboard()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<UserDashboardDto>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var dashboardData = await _dashboardService.GetUserDashboardDataAsync(userId);
                return Ok(new ApiResponseDto<UserDashboardDto>
                {
                    Success = true,
                    Message = "Dashboard del usuario obtenido exitosamente",
                    Data = dashboardData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user dashboard");
                return StatusCode(500, new ApiResponseDto<UserDashboardDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get organization-specific dashboard data
        /// </summary>
        [HttpGet("organization")]
        public async Task<ActionResult<ApiResponseDto<OrganizationDashboardDto>>> GetOrganizationDashboard()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<OrganizationDashboardDto>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var organizationId = await _opportunityService.GetOrganizacionIdByUserAsync(userId);
                if (!organizationId.HasValue)
                {
                    return BadRequest(new ApiResponseDto<OrganizationDashboardDto>
                    {
                        Success = false,
                        Message = "Usuario no asociado a una organización"
                    });
                }

                var dashboardData = await _dashboardService.GetOrganizationDashboardDataAsync(organizationId.Value);
                return Ok(new ApiResponseDto<OrganizationDashboardDto>
                {
                    Success = true,
                    Message = "Dashboard de la organización obtenido exitosamente",
                    Data = dashboardData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization dashboard");
                return StatusCode(500, new ApiResponseDto<OrganizationDashboardDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get recent activities for a user
        /// </summary>
        [HttpGet("activities")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<RecentActivityDto>>>> GetRecentActivities([FromQuery] int limit = 10)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<IEnumerable<RecentActivityDto>>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var activities = await _dashboardService.GetRecentActivitiesAsync(userId, limit);
                return Ok(new ApiResponseDto<IEnumerable<RecentActivityDto>>
                {
                    Success = true,
                    Message = "Actividades recientes obtenidas exitosamente",
                    Data = activities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
                return StatusCode(500, new ApiResponseDto<IEnumerable<RecentActivityDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get user's enrolled opportunities
        /// </summary>
        [HttpGet("my-opportunities")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<OpportunityListDto>>>> GetMyOpportunities([FromQuery] int limit = 5)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<IEnumerable<OpportunityListDto>>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var opportunities = await _dashboardService.GetUserOpportunitiesAsync(userId, limit);
                return Ok(new ApiResponseDto<IEnumerable<OpportunityListDto>>
                {
                    Success = true,
                    Message = "Oportunidades del usuario obtenidas exitosamente",
                    Data = opportunities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user opportunities");
                return StatusCode(500, new ApiResponseDto<IEnumerable<OpportunityListDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }
    }
}