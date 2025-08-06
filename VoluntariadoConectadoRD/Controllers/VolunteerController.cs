using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoluntariadoConectadoRD.Attributes;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VolunteerController : ControllerBase
    {
        private readonly IVolunteerService _volunteerService;
        private readonly ILogger<VolunteerController> _logger;

        public VolunteerController(IVolunteerService volunteerService, ILogger<VolunteerController> logger)
        {
            _volunteerService = volunteerService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Get enhanced user profile by ID
        /// </summary>
        [HttpGet("profile/{id}")]
        public async Task<ActionResult<ApiResponseDto<EnhancedUserProfileDto>>> GetUserProfile(int id)
        {
            try
            {
                var profile = await _volunteerService.GetUserProfileByIdAsync(id);
                
                if (profile == null)
                {
                    return NotFound(new ApiResponseDto<EnhancedUserProfileDto>
                    {
                        Success = false,
                        Message = "Perfil de usuario no encontrado"
                    });
                }

                return Ok(new ApiResponseDto<EnhancedUserProfileDto>
                {
                    Success = true,
                    Message = "Perfil de usuario obtenido exitosamente",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile {UserId}", id);
                return StatusCode(500, new ApiResponseDto<EnhancedUserProfileDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get current user's profile
        /// </summary>
        [HttpGet("profile/me")]
        public async Task<ActionResult<ApiResponseDto<EnhancedUserProfileDto>>> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiResponseDto<EnhancedUserProfileDto>
                {
                    Success = false,
                    Message = "Usuario no autenticado"
                });
            }

            return await GetUserProfile(userId);
        }

        /// <summary>
        /// Get volunteer statistics
        /// </summary>
        [HttpGet("stats/{id}")]
        public async Task<ActionResult<ApiResponseDto<VolunteerStatsDto>>> GetVolunteerStats(int id)
        {
            try
            {
                var stats = await _volunteerService.GetVolunteerStatsAsync(id);
                
                if (stats == null)
                {
                    return NotFound(new ApiResponseDto<VolunteerStatsDto>
                    {
                        Success = false,
                        Message = "Estadísticas del voluntario no encontradas"
                    });
                }

                return Ok(new ApiResponseDto<VolunteerStatsDto>
                {
                    Success = true,
                    Message = "Estadísticas obtenidas exitosamente",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting volunteer stats {UserId}", id);
                return StatusCode(500, new ApiResponseDto<VolunteerStatsDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get current user's applications
        /// </summary>
        [HttpGet("applications/me")]
        [RoleAuthorization(UserRole.Voluntario)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<VolunteerApplicationDetailDto>>>> GetMyApplications()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<IEnumerable<VolunteerApplicationDetailDto>>
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                var applications = await _volunteerService.GetMyApplicationsAsync(userId);

                return Ok(new ApiResponseDto<IEnumerable<VolunteerApplicationDetailDto>>
                {
                    Success = true,
                    Message = "Aplicaciones obtenidas exitosamente",
                    Data = applications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user applications");
                return StatusCode(500, new ApiResponseDto<IEnumerable<VolunteerApplicationDetailDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get applications for a specific volunteer (Admin/Organization only)
        /// </summary>
        [HttpGet("applications/{volunteerId}")]
        [RoleAuthorization(UserRole.Administrador, UserRole.Organizacion)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<VolunteerApplicationDetailDto>>>> GetVolunteerApplications(int volunteerId)
        {
            try
            {
                var applications = await _volunteerService.GetMyApplicationsAsync(volunteerId);

                return Ok(new ApiResponseDto<IEnumerable<VolunteerApplicationDetailDto>>
                {
                    Success = true,
                    Message = "Aplicaciones del voluntario obtenidas exitosamente",
                    Data = applications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting volunteer applications for user {VolunteerId}", volunteerId);
                return StatusCode(500, new ApiResponseDto<IEnumerable<VolunteerApplicationDetailDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get all available skills
        /// </summary>
        [HttpGet("skills")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<SkillDto>>>> GetAllSkills()
        {
            try
            {
                var skills = await _volunteerService.GetAllSkillsAsync();

                return Ok(new ApiResponseDto<IEnumerable<SkillDto>>
                {
                    Success = true,
                    Message = "Habilidades obtenidas exitosamente",
                    Data = skills
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all skills");
                return StatusCode(500, new ApiResponseDto<IEnumerable<SkillDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get user's skills
        /// </summary>
        [HttpGet("skills/me")]
        [RoleAuthorization(UserRole.Voluntario)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<UserSkillDto>>>> GetMySkills()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<IEnumerable<UserSkillDto>>
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                var skills = await _volunteerService.GetUserSkillsAsync(userId);

                return Ok(new ApiResponseDto<IEnumerable<UserSkillDto>>
                {
                    Success = true,
                    Message = "Habilidades del usuario obtenidas exitosamente",
                    Data = skills
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user skills");
                return StatusCode(500, new ApiResponseDto<IEnumerable<UserSkillDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Add skill to current user
        /// </summary>
        [HttpPost("skills")]
        [RoleAuthorization(UserRole.Voluntario)]
        public async Task<ActionResult<ApiResponseDto<object>>> AddSkill([FromBody] CreateUserSkillDto skillDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                var success = await _volunteerService.AddUserSkillAsync(userId, skillDto);

                if (!success)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "No se pudo agregar la habilidad. Puede que ya exista o la habilidad no sea válida."
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Habilidad agregada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding skill to user");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Update skill level
        /// </summary>
        [HttpPut("skills/{skillId}")]
        [RoleAuthorization(UserRole.Voluntario)]
        public async Task<ActionResult<ApiResponseDto<object>>> UpdateSkill(int skillId, [FromBody] int nivel)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                if (nivel < 1 || nivel > 100)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "El nivel debe estar entre 1 y 100"
                    });
                }

                var success = await _volunteerService.UpdateUserSkillAsync(userId, skillId, nivel);

                if (!success)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Habilidad no encontrada"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Nivel de habilidad actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skill level");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Remove skill from current user
        /// </summary>
        [HttpDelete("skills/{skillId}")]
        [RoleAuthorization(UserRole.Voluntario)]
        public async Task<ActionResult<ApiResponseDto<object>>> RemoveSkill(int skillId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                var success = await _volunteerService.RemoveUserSkillAsync(userId, skillId);

                if (!success)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Habilidad no encontrada"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Habilidad eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing skill from user");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get user activities
        /// </summary>
        [HttpGet("activities/me")]
        [RoleAuthorization(UserRole.Voluntario)]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<VolunteerActivityDto>>>> GetMyActivities()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<IEnumerable<VolunteerActivityDto>>
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                var activities = await _volunteerService.GetUserActivitiesAsync(userId);

                return Ok(new ApiResponseDto<IEnumerable<VolunteerActivityDto>>
                {
                    Success = true,
                    Message = "Actividades obtenidas exitosamente",
                    Data = activities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user activities");
                return StatusCode(500, new ApiResponseDto<IEnumerable<VolunteerActivityDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get platform statistics (public endpoint)
        /// </summary>
        [HttpGet("platform/stats")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<PlatformStatsDto>>> GetPlatformStats()
        {
            try
            {
                var stats = await _volunteerService.GetPlatformStatsAsync();

                if (stats == null)
                {
                    return NotFound(new ApiResponseDto<PlatformStatsDto>
                    {
                        Success = false,
                        Message = "Estadísticas de la plataforma no disponibles"
                    });
                }

                return Ok(new ApiResponseDto<PlatformStatsDto>
                {
                    Success = true,
                    Message = "Estadísticas de la plataforma obtenidas exitosamente",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting platform stats");
                return StatusCode(500, new ApiResponseDto<PlatformStatsDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get featured opportunities (public endpoint)
        /// </summary>
        [HttpGet("opportunities/featured")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<OpportunityListDto>>>> GetFeaturedOpportunities([FromQuery] int count = 3)
        {
            try
            {
                if (count < 1 || count > 10)
                {
                    count = 3;
                }

                var opportunities = await _volunteerService.GetFeaturedOpportunitiesAsync(count);

                return Ok(new ApiResponseDto<IEnumerable<OpportunityListDto>>
                {
                    Success = true,
                    Message = "Oportunidades destacadas obtenidas exitosamente",
                    Data = opportunities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured opportunities");
                return StatusCode(500, new ApiResponseDto<IEnumerable<OpportunityListDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Update platform statistics (admin only)
        /// </summary>
        [HttpPost("platform/stats/update")]
        [RoleAuthorization(UserRole.Administrador)]
        public async Task<ActionResult<ApiResponseDto<object>>> UpdatePlatformStats()
        {
            try
            {
                var success = await _volunteerService.UpdatePlatformStatsAsync();

                if (!success)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "No se pudieron actualizar las estadísticas"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Estadísticas actualizadas exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating platform stats");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get all volunteers for admin (paginated)
        /// </summary>
        [HttpGet("admin/volunteers")]
        [RoleAuthorization(UserRole.Administrador)]
        public async Task<ActionResult<ApiResponseDto<PaginatedResult<AdminVolunteerDto>>>> GetAllVolunteersForAdmin(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _volunteerService.GetAllVolunteersForAdminAsync(page, pageSize, search);

                return Ok(new ApiResponseDto<PaginatedResult<AdminVolunteerDto>>
                {
                    Success = true,
                    Message = "Voluntarios obtenidos exitosamente",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting volunteers for admin");
                return StatusCode(500, new ApiResponseDto<PaginatedResult<AdminVolunteerDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get admin statistics
        /// </summary>
        [HttpGet("admin/stats")]
        [RoleAuthorization(UserRole.Administrador)]
        public async Task<ActionResult<ApiResponseDto<AdminStatsDto>>> GetAdminStats()
        {
            try
            {
                var stats = await _volunteerService.GetAdminStatsAsync();

                return Ok(new ApiResponseDto<AdminStatsDto>
                {
                    Success = true,
                    Message = "Estadísticas administrativas obtenidas exitosamente",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin stats");
                return StatusCode(500, new ApiResponseDto<AdminStatsDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Update user status (block/unblock/activate/suspend)
        /// </summary>
        [HttpPut("admin/users/{userId}/status")]
        [RoleAuthorization(UserRole.Administrador)]
        public async Task<ActionResult<ApiResponseDto<object>>> UpdateUserStatus(int userId, [FromBody] UpdateUserStatusDto statusDto)
        {
            try
            {
                var success = await _volunteerService.UpdateUserStatusAsync(userId, statusDto.Status);

                if (!success)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "No se pudo actualizar el estado del usuario"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = $"Estado del usuario actualizado a {statusDto.Status} exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Edit user profile (admin)
        /// </summary>
        [HttpPut("admin/users/{userId}")]
        [RoleAuthorization(UserRole.Administrador)]
        public async Task<ActionResult<ApiResponseDto<object>>> EditUserProfile(int userId, [FromBody] AdminEditUserDto editDto)
        {
            try
            {
                var success = await _volunteerService.EditUserProfileAsAdminAsync(userId, editDto);

                if (!success)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "No se pudo actualizar el perfil del usuario"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Perfil del usuario actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing user profile {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Delete user (admin only - soft delete)
        /// </summary>
        [HttpDelete("admin/users/{userId}")]
        [RoleAuthorization(UserRole.Administrador)]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteUser(int userId)
        {
            try
            {
                var success = await _volunteerService.DeleteUserAsync(userId);

                if (!success)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "No se pudo eliminar el usuario"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Usuario eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get all organizations for admin (paginated)
        /// </summary>
        [HttpGet("admin/organizations")]
        [RoleAuthorization(UserRole.Administrador)]
        public async Task<ActionResult<ApiResponseDto<PaginatedResult<AdminOrganizationDto>>>> GetAllOrganizationsForAdmin(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _volunteerService.GetAllOrganizationsForAdminAsync(page, pageSize, search);

                return Ok(new ApiResponseDto<PaginatedResult<AdminOrganizationDto>>
                {
                    Success = true,
                    Message = "Organizaciones obtenidas exitosamente",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizations for admin");
                return StatusCode(500, new ApiResponseDto<PaginatedResult<AdminOrganizationDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Update organization status (verify/approve/suspend)
        /// </summary>
        [HttpPut("admin/organizations/{orgId}/status")]
        [RoleAuthorization(UserRole.Administrador)]
        public async Task<ActionResult<ApiResponseDto<object>>> UpdateOrganizationStatus(int orgId, [FromBody] UpdateOrganizationStatusDto statusDto)
        {
            try
            {
                var success = await _volunteerService.UpdateOrganizationStatusAsync(orgId, statusDto.Status, statusDto.Verificada);

                if (!success)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "No se pudo actualizar el estado de la organización"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Estado de la organización actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization status {OrgId}", orgId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Edit organization (admin)
        /// </summary>
        [HttpPut("admin/organizations/{orgId}")]
        [RoleAuthorization(UserRole.Administrador)]
        public async Task<ActionResult<ApiResponseDto<object>>> EditOrganization(int orgId, [FromBody] AdminEditOrganizationDto editDto)
        {
            try
            {
                var success = await _volunteerService.EditOrganizationAsAdminAsync(orgId, editDto);

                if (!success)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "No se pudo actualizar la organización"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Organización actualizada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing organization {OrgId}", orgId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Delete organization (admin only - soft delete)
        /// </summary>
        [HttpDelete("admin/organizations/{orgId}")]
        [RoleAuthorization(UserRole.Administrador)]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteOrganization(int orgId)
        {
            try
            {
                var success = await _volunteerService.DeleteOrganizationAsync(orgId);

                if (!success)
                {
                    return BadRequest(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "No se pudo eliminar la organización"
                    });
                }

                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Organización eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting organization {OrgId}", orgId);
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }
    }
}