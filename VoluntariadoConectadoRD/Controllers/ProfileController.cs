using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Attributes;
using VoluntariadoConectadoRD.Models;

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

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponseDto<UserProfileDto>>> GetUserProfile(int userId)
        {
            try
            {
                var profile = await _profileService.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    return NotFound(new ApiResponseDto<UserProfileDto>
                    {
                        Success = false,
                        Message = "Perfil de usuario no encontrado",
                        Data = null
                    });
                }

                return Ok(new ApiResponseDto<UserProfileDto>
                {
                    Success = true,
                    Message = "Perfil obtenido exitosamente",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil del usuario {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<UserProfileDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }

        [HttpGet("organization/{orgId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<OrganizationProfileDto>>> GetOrganizationProfile(int orgId)
        {
            try
            {
                var profile = await _profileService.GetOrganizationProfileAsync(orgId);
                if (profile == null)
                {
                    return NotFound(new ApiResponseDto<OrganizationProfileDto>
                    {
                        Success = false,
                        Message = "Perfil de organización no encontrado",
                        Data = null
                    });
                }

                return Ok(new ApiResponseDto<OrganizationProfileDto>
                {
                    Success = true,
                    Message = "Perfil obtenido exitosamente",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil de la organización {OrgId}", orgId);
                return StatusCode(500, new ApiResponseDto<OrganizationProfileDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }

        [HttpPut("user")]
        public async Task<ActionResult<ApiResponseDto<UserProfileDto>>> UpdateUserProfile([FromBody] UpdateUserProfileDto updateDto)
        {
            try
            {
                // Debug logging
                var allClaims = User.Claims.ToList();
                _logger.LogInformation("User claims: {Claims}", string.Join(", ", allClaims.Select(c => $"{c.Type}={c.Value}")));
                
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    _logger.LogWarning("UserId not found in token claims");
                    return Unauthorized(new ApiResponseDto<UserProfileDto>
                    {
                        Success = false,
                        Message = "Usuario no autenticado",
                        Data = null
                    });
                }

                var profile = await _profileService.UpdateUserProfileAsync(userId, updateDto);
                return Ok(new ApiResponseDto<UserProfileDto>
                {
                    Success = true,
                    Message = "Perfil actualizado exitosamente",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil del usuario");
                return StatusCode(500, new ApiResponseDto<UserProfileDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }

        [HttpPut("organization")]
        public async Task<ActionResult<ApiResponseDto<OrganizationProfileDto>>> UpdateOrganizationProfile([FromBody] UpdateOrganizationProfileDto updateDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<OrganizationProfileDto>
                    {
                        Success = false,
                        Message = "Usuario no autenticado",
                        Data = null
                    });
                }

                var profile = await _profileService.UpdateOrganizationProfileAsync(userId, updateDto);
                return Ok(new ApiResponseDto<OrganizationProfileDto>
                {
                    Success = true,
                    Message = "Perfil actualizado exitosamente",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil de la organización");
                return StatusCode(500, new ApiResponseDto<OrganizationProfileDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }

        [HttpGet("user/completion/{userId}")]
        public async Task<ActionResult<ApiResponseDto<ProfileCompletionDto>>> GetUserProfileCompletion(int userId)
        {
            try
            {
                var completion = await _profileService.GetUserProfileCompletionAsync(userId);
                return Ok(new ApiResponseDto<ProfileCompletionDto>
                {
                    Success = true,
                    Message = "Completitud del perfil obtenida exitosamente",
                    Data = completion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener completitud del perfil del usuario {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<ProfileCompletionDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }

        [HttpGet("organization/completion/{orgId}")]
        public async Task<ActionResult<ApiResponseDto<ProfileCompletionDto>>> GetOrganizationProfileCompletion(int orgId)
        {
            try
            {
                var completion = await _profileService.GetOrganizationProfileCompletionAsync(orgId);
                return Ok(new ApiResponseDto<ProfileCompletionDto>
                {
                    Success = true,
                    Message = "Completitud del perfil obtenida exitosamente",
                    Data = completion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener completitud del perfil de la organización {OrgId}", orgId);
                return StatusCode(500, new ApiResponseDto<ProfileCompletionDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }
    }
}