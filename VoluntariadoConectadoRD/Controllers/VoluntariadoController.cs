using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoluntariadoConectadoRD.Attributes;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;
using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class VoluntariadoController : ControllerBase
    {
        private readonly ILogger<VoluntariadoController> _logger;
        private readonly IOpportunityService _opportunityService;

        public VoluntariadoController(ILogger<VoluntariadoController> logger, IOpportunityService opportunityService)
        {
            _logger = logger;
            _opportunityService = opportunityService;
        }

        /// <summary>
        /// Endpoint disponible para todos los usuarios autenticados
        /// </summary>
        [HttpGet("opportunities")]
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
        /// Obtener detalles de una oportunidad específica
        /// </summary>
        [HttpGet("opportunities/{id}")]
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
                        Message = "Usuario no válido"
                    });
                }

                var success = await _opportunityService.ApplyToOpportunityAsync(opportunityId, userId, applyDto);
                
                if (!success)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "No se pudo enviar la aplicación. La oportunidad puede no existir o ya aplicaste anteriormente."
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Aplicación enviada exitosamente",
                    Data = new { userId, opportunityId, message = "Tu aplicación ha sido enviada" }
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
        public ActionResult<ApiResponseDto<object>> CreateOpportunity([FromBody] object opportunityData)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Oportunidad creada exitosamente",
                Data = new { userId, message = "La oportunidad ha sido creada" }
            });
        }

        /// <summary>
        /// Endpoint solo para organizaciones - Gestionar aplicaciones
        /// </summary>
        [HttpGet("applications")]
        [OrganizacionOnly]
        public ActionResult<ApiResponseDto<object>> GetApplications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Lista de aplicaciones",
                Data = new { userId, message = "Aquí estarían las aplicaciones a tus oportunidades" }
            });
        }

        /// <summary>
        /// Endpoint solo para administradores - Estadísticas generales
        /// </summary>
        [HttpGet("admin/stats")]
        [AdminOnly]
        public ActionResult<ApiResponseDto<object>> GetAdminStats()
        {
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Estadísticas del sistema",
                Data = new { 
                    totalUsers = 100, 
                    totalOrganizations = 25, 
                    totalOpportunities = 50,
                    message = "Estadísticas generales del sistema" 
                }
            });
        }

        /// <summary>
        /// Endpoint para voluntarios y administradores
        /// </summary>
        [HttpGet("my-applications")]
        [VoluntarioOrAdmin]
        public ActionResult<ApiResponseDto<object>> GetMyApplications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Mis aplicaciones",
                Data = new { userId, userRole, message = "Lista de aplicaciones del usuario" }
            });
        }

        /// <summary>
        /// Endpoint para organizaciones y administradores
        /// </summary>
        [HttpPut("opportunities/{opportunityId}")]
        [OrganizacionOrAdmin]
        public ActionResult<ApiResponseDto<object>> UpdateOpportunity(int opportunityId, [FromBody] object opportunityData)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Oportunidad actualizada",
                Data = new { userId, userRole, opportunityId, message = "La oportunidad ha sido actualizada" }
            });
        }
    }
}