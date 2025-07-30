using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IProfileService profileService, ILogger<ProfileController> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        #region User Profile Endpoints

        /// <summary>
        /// Obtiene el perfil de un usuario específico
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Perfil del usuario</returns>
        [HttpGet("user/{id}")]
        public async Task<ActionResult<ApiResponseDto<UserProfileDto>>> GetUserProfile(int id)
        {
            try
            {
                // Verificar que el usuario solo pueda acceder a su propio perfil o sea admin
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                if (currentUserId != id && currentUserRole != "Administrador")
                {
                    return Forbid("No tienes permisos para acceder a este perfil");
                }

                var result = await _profileService.GetUserProfileAsync(id);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for ID: {UserId}", id);
                return StatusCode(500, new ApiResponseDto<UserProfileDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Actualiza el perfil del usuario autenticado
        /// </summary>
        /// <param name="dto">Datos de actualización del perfil</param>
        /// <returns>Perfil actualizado</returns>
        [HttpPut("user")]
        public async Task<ActionResult<ApiResponseDto<UserProfileDto>>> UpdateUserProfile([FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<UserProfileDto>
                    {
                        Success = false,
                        Message = "Datos de entrada no válidos",
                        Data = null
                    });
                }

                var currentUserId = GetCurrentUserId();
                var result = await _profileService.UpdateUserProfileAsync(currentUserId, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponseDto<UserProfileDto>
                {
                    Success = false,
                    Message = "Error al actualizar el perfil",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Verifica la completitud del perfil del usuario autenticado
        /// </summary>
        /// <returns>Estado de completitud del perfil</returns>
        [HttpGet("user/completion")]
        public async Task<ActionResult<ApiResponseDto<ProfileCompletionDto>>> CheckUserProfileCompletion()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _profileService.CheckUserProfileCompletionAsync(currentUserId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user profile completion for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponseDto<ProfileCompletionDto>
                {
                    Success = false,
                    Message = "Error al verificar completitud del perfil",
                    Data = null
                });
            }
        }

        #endregion

        #region Organization Profile Endpoints

        /// <summary>
        /// Obtiene el perfil de una organización específica
        /// </summary>
        /// <param name="id">ID de la organización</param>
        /// <returns>Perfil de la organización</returns>
        [HttpGet("organization/{id}")]
        public async Task<ActionResult<ApiResponseDto<OrganizationProfileDto>>> GetOrganizationProfile(int id)
        {
            try
            {
                // Verificar permisos: solo admins de la organización o administradores del sistema
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                if (currentUserRole != "Administrador")
                {
                    // Verificar si el usuario es administrador de esta organización
                    var orgProfile = await _profileService.GetOrganizationProfileAsync(id);
                    if (orgProfile.Success && orgProfile.Data?.UsuarioAdministrador?.Id != currentUserId)
                    {
                        return Forbid("No tienes permisos para acceder a este perfil de organización");
                    }
                }

                var result = await _profileService.GetOrganizationProfileAsync(id);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization profile for ID: {OrganizationId}", id);
                return StatusCode(500, new ApiResponseDto<OrganizationProfileDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Actualiza el perfil de la organización del usuario autenticado
        /// </summary>
        /// <param name="dto">Datos de actualización del perfil de organización</param>
        /// <returns>Perfil de organización actualizado</returns>
        [HttpPut("organization")]
        [Authorize(Roles = "Organizacion")]
        public async Task<ActionResult<ApiResponseDto<OrganizationProfileDto>>> UpdateOrganizationProfile([FromBody] UpdateOrganizationProfileDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<OrganizationProfileDto>
                    {
                        Success = false,
                        Message = "Datos de entrada no válidos",
                        Data = null
                    });
                }

                // Obtener ID de la organización del usuario autenticado
                var organizationId = GetCurrentUserOrganizationId();
                if (organizationId == 0)
                {
                    return BadRequest(new ApiResponseDto<OrganizationProfileDto>
                    {
                        Success = false,
                        Message = "Usuario no está asociado con ninguna organización",
                        Data = null
                    });
                }

                var result = await _profileService.UpdateOrganizationProfileAsync(organizationId, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization profile for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponseDto<OrganizationProfileDto>
                {
                    Success = false,
                    Message = "Error al actualizar el perfil de organización",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Verifica la completitud del perfil de la organización del usuario autenticado
        /// </summary>
        /// <returns>Estado de completitud del perfil de organización</returns>
        [HttpGet("organization/completion")]
        [Authorize(Roles = "Organizacion")]
        public async Task<ActionResult<ApiResponseDto<ProfileCompletionDto>>> CheckOrganizationProfileCompletion()
        {
            try
            {
                // Obtener ID de la organización del usuario autenticado
                var organizationId = GetCurrentUserOrganizationId();
                if (organizationId == 0)
                {
                    return BadRequest(new ApiResponseDto<ProfileCompletionDto>
                    {
                        Success = false,
                        Message = "Usuario no está asociado con ninguna organización",
                        Data = null
                    });
                }

                var result = await _profileService.CheckOrganizationProfileCompletionAsync(organizationId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking organization profile completion for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponseDto<ProfileCompletionDto>
                {
                    Success = false,
                    Message = "Error al verificar completitud del perfil de organización",
                    Data = null
                });
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Obtiene el ID del usuario autenticado
        /// </summary>
        /// <returns>ID del usuario</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Usuario no autenticado correctamente");
        }

        /// <summary>
        /// Obtiene el rol del usuario autenticado
        /// </summary>
        /// <returns>Rol del usuario</returns>
        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        /// <summary>
        /// Obtiene el ID de la organización del usuario autenticado (si aplica)
        /// </summary>
        /// <returns>ID de la organización o 0 si no aplica</returns>
        private int GetCurrentUserOrganizationId()
        {
            var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
            if (int.TryParse(orgIdClaim, out int organizationId))
            {
                return organizationId;
            }
            return 0;
        }

        #endregion
    }
}