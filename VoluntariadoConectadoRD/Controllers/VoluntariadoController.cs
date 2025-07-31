using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoluntariadoConectadoRD.Attributes;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Interfaces;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticaciÛn para todos los endpoints
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

<<<<<<< HEAD
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
=======
        /// <summary>
        /// Endpoint disponible para todos los usuarios autenticados - GET opportunities with filters
        /// </summary>
        [HttpGet("opportunities")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<OpportunityListDto>>>> GetVolunteerOpportunities(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? areaInteres = null,
            [FromQuery] string? ubicacion = null,
            [FromQuery] OpportunityStatus? status = null)
        {
            try
            {
                var opportunities = await _opportunityService.GetAllOpportunitiesAsync(searchTerm, areaInteres, ubicacion, status);
                
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
        /// Obtener detalles de una oportunidad espec√≠fica
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
                        Message = "Usuario no v√°lido"
                    });
                }

                var success = await _opportunityService.ApplyToOpportunityAsync(opportunityId, userId, applyDto);
                
                if (!success)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "No se pudo enviar la aplicaci√≥n. La oportunidad puede no existir o ya aplicaste anteriormente."
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Aplicaci√≥n enviada exitosamente",
                    Data = new { userId, opportunityId, message = "Tu aplicaci√≥n ha sido enviada" }
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
        /// Create new opportunity (Organizations only) - COMPLETE IMPLEMENTATION
        /// </summary>
        [HttpPost("opportunities")]
        [OrganizacionOnly]
        public async Task<ActionResult<ApiResponseDto<OpportunityDetailDto>>> CreateOpportunity([FromBody] CreateOpportunityDto createDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<OpportunityDetailDto>
                    {
                        Success = false,
                        Message = "Usuario no v√°lido"
                    });
                }

                // Get organization ID from user
                var organizacionId = await _opportunityService.GetOrganizacionIdByUserAsync(userId);
                if (!organizacionId.HasValue)
                {
                    return BadRequest(new ApiResponseDto<OpportunityDetailDto>
                    {
                        Success = false,
                        Message = "Usuario no asociado a ninguna organizaci√≥n"
                    });
                }

                var result = await _opportunityService.CreateOpportunityAsync(createDto, organizacionId.Value);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetOpportunityDetails), new { id = result.Data?.Id }, result);
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
        /// Get organization applications - COMPLETE IMPLEMENTATION
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
                        Message = "Usuario no v√°lido"
                    });
                }

                var organizacionId = await _opportunityService.GetOrganizacionIdByUserAsync(userId);
                if (!organizacionId.HasValue)
                {
                    return BadRequest(new ApiResponseDto<IEnumerable<ApplicationDto>>
                    {
                        Success = false,
                        Message = "Usuario no asociado a ninguna organizaci√≥n"
                    });
                }

                var applications = await _opportunityService.GetOrganizationApplicationsAsync(organizacionId.Value);

                return Ok(new ApiResponseDto<IEnumerable<ApplicationDto>>
                {
                    Success = true,
                    Message = "Lista de aplicaciones",
                    Data = applications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applications");
                return StatusCode(500, new ApiResponseDto<IEnumerable<ApplicationDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Endpoint solo para administradores - Estad√≠sticas generales
        /// </summary>
        [HttpGet("admin/stats")]
        [AdminOnly]
        public ActionResult<ApiResponseDto<object>> GetAdminStats()
        {
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Estad√≠sticas del sistema",
                Data = new { 
                    totalUsers = 100, 
                    totalOrganizations = 25, 
                    totalOpportunities = 50,
                    message = "Estad√≠sticas generales del sistema" 
                }
            });
        }

        /// <summary>
        /// Get volunteer applications - COMPLETE IMPLEMENTATION
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
                        Message = "Usuario no v√°lido"
                    });
                }

                var applications = await _opportunityService.GetVolunteerApplicationsAsync(userId);

                return Ok(new ApiResponseDto<IEnumerable<ApplicationDto>>
                {
                    Success = true,
                    Message = "Mis aplicaciones",
                    Data = applications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user applications");
                return StatusCode(500, new ApiResponseDto<IEnumerable<ApplicationDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Update opportunity - COMPLETE IMPLEMENTATION
        /// </summary>
        [HttpPut("opportunities/{opportunityId}")]
        [OrganizacionOrAdmin]
        public async Task<ActionResult<ApiResponseDto<OpportunityDetailDto>>> UpdateOpportunity(int opportunityId, [FromBody] UpdateOpportunityDto updateDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<OpportunityDetailDto>
                    {
                        Success = false,
                        Message = "Usuario no v√°lido"
                    });
                }

                var organizacionId = await _opportunityService.GetOrganizacionIdByUserAsync(userId);
                if (!organizacionId.HasValue)
                {
                    return BadRequest(new ApiResponseDto<OpportunityDetailDto>
                    {
                        Success = false,
                        Message = "Usuario no asociado a ninguna organizaci√≥n"
                    });
                }

                var result = await _opportunityService.UpdateOpportunityAsync(opportunityId, updateDto, organizacionId.Value);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating opportunity");
                return StatusCode(500, new ApiResponseDto<OpportunityDetailDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Update application status - NEW ENDPOINT
        /// </summary>
        [HttpPut("applications/{applicationId}/status")]
        [OrganizacionOrAdmin]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateApplicationStatus(
            int applicationId,
            [FromBody] UpdateApplicationStatusDto statusDto)
        {
            try
            {
                var success = await _opportunityService.UpdateApplicationStatusAsync(
                    applicationId,
                    statusDto.Status,
                    statusDto.Notes);

                if (!success)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "No se pudo actualizar el estado de la aplicaci√≥n"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Estado de aplicaci√≥n actualizado exitosamente",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating application status");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
>>>>>>> 0c725e62c64b9718dd5b40ef3c2e9767373ae7d3
        }
    }

    /// <summary>
    /// Obtener detalles de una oportunidad especÌfica
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
                    Message = "Usuario no v·lido"
                });
            }

            var success = await _opportunityService.ApplyToOpportunityAsync(opportunityId, userId, applyDto);

            if (!success)
            {
                return BadRequest(new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "No se pudo enviar la aplicaciÛn. La oportunidad puede no existir o ya aplicaste anteriormente."
                });
            }

            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "AplicaciÛn enviada exitosamente",
                Data = new { userId, opportunityId, message = "Tu aplicaciÛn ha sido enviada" }
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
            Data = new { userId, message = "AquÌ estarÌan las aplicaciones a tus oportunidades" }
        });
    }

    /// <summary>
    /// Endpoint solo para administradores - EstadÌsticas generales
    /// </summary>
    [HttpGet("admin/stats")]
    [AdminOnly]
    public ActionResult<ApiResponseDto<object>> GetAdminStats()
    {
        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "EstadÌsticas del sistema",
            Data = new
            {
                totalUsers = 100,
                totalOrganizations = 25,
                totalOpportunities = 50,
                message = "EstadÌsticas generales del sistema"
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