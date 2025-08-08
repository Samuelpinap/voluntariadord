using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoluntariadoConectadoRD.Attributes;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BadgeController : ControllerBase
    {
        private readonly IBadgeService _badgeService;
        private readonly ILogger<BadgeController> _logger;

        public BadgeController(IBadgeService badgeService, ILogger<BadgeController> logger)
        {
            _badgeService = badgeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all badges - Public
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<List<BadgeDto>>>> GetAllBadges()
        {
            try
            {
                var badges = await _badgeService.GetAllBadgesAsync();

                return Ok(new ApiResponseDto<List<BadgeDto>>
                {
                    Success = true,
                    Message = "Insignias obtenidas",
                    Data = badges
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all badges");
                return StatusCode(500, new ApiResponseDto<List<BadgeDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get user's badges
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponseDto<List<BadgeDto>>>> GetUserBadges(int userId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int currentUserId))
                {
                    // Users can only see their own badges unless they're admin
                    var userRole = User.FindFirst("Rol")?.Value;
                    if (userId != currentUserId && userRole != "3") // Not admin
                    {
                        return Forbid();
                    }
                }

                var badges = await _badgeService.GetUserBadgesAsync(userId);

                return Ok(new ApiResponseDto<List<BadgeDto>>
                {
                    Success = true,
                    Message = "Insignias del usuario obtenidas",
                    Data = badges
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user badges for user {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<List<BadgeDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get current user's badges
        /// </summary>
        [HttpGet("my-badges")]
        public async Task<ActionResult<ApiResponseDto<List<BadgeDto>>>> GetMyBadges()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<List<BadgeDto>>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var badges = await _badgeService.GetUserBadgesAsync(userId);

                return Ok(new ApiResponseDto<List<BadgeDto>>
                {
                    Success = true,
                    Message = "Mis insignias obtenidas",
                    Data = badges
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user badges");
                return StatusCode(500, new ApiResponseDto<List<BadgeDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Create a new badge - Admin only
        /// </summary>
        [HttpPost]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<BadgeDto>>> CreateBadge([FromBody] CreateBadgeDto createBadgeDto)
        {
            try
            {
                var badge = await _badgeService.CreateBadgeAsync(createBadgeDto);

                return Ok(new ApiResponseDto<BadgeDto>
                {
                    Success = true,
                    Message = "Insignia creada exitosamente",
                    Data = badge
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating badge");
                return StatusCode(500, new ApiResponseDto<BadgeDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Update a badge - Admin only
        /// </summary>
        [HttpPut("{badgeId}")]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<BadgeDto>>> UpdateBadge(int badgeId, [FromBody] UpdateBadgeDto updateBadgeDto)
        {
            try
            {
                var badge = await _badgeService.UpdateBadgeAsync(badgeId, updateBadgeDto);

                return Ok(new ApiResponseDto<BadgeDto>
                {
                    Success = true,
                    Message = "Insignia actualizada exitosamente",
                    Data = badge
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponseDto<BadgeDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating badge {BadgeId}", badgeId);
                return StatusCode(500, new ApiResponseDto<BadgeDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Delete a badge - Admin only
        /// </summary>
        [HttpDelete("{badgeId}")]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteBadge(int badgeId)
        {
            try
            {
                var result = await _badgeService.DeleteBadgeAsync(badgeId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Insignia no encontrada"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Insignia eliminada exitosamente",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting badge {BadgeId}", badgeId);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Award badge to user - Admin only
        /// </summary>
        [HttpPost("award")]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<bool>>> AwardBadge([FromBody] AwardBadgeDto awardBadgeDto)
        {
            try
            {
                var result = await _badgeService.AwardBadgeToUserAsync(
                    awardBadgeDto.UserId, 
                    awardBadgeDto.BadgeId, 
                    awardBadgeDto.Reason);

                if (!result)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "No se pudo otorgar la insignia. Verifique que el usuario y la insignia existan y que el usuario no tenga ya esta insignia."
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Insignia otorgada exitosamente",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding badge {BadgeId} to user {UserId}", awardBadgeDto.BadgeId, awardBadgeDto.UserId);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Revoke badge from user - Admin only
        /// </summary>
        [HttpDelete("revoke/{userId}/{badgeId}")]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<bool>>> RevokeBadge(int userId, int badgeId)
        {
            try
            {
                var result = await _badgeService.RevokeBadgeFromUserAsync(userId, badgeId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "El usuario no tiene esta insignia"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Insignia revocada exitosamente",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking badge {BadgeId} from user {UserId}", badgeId, userId);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get available badges for user
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<ApiResponseDto<List<BadgeDto>>>> GetAvailableBadges()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<List<BadgeDto>>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var badges = await _badgeService.GetAvailableBadgesForUserAsync(userId);

                return Ok(new ApiResponseDto<List<BadgeDto>>
                {
                    Success = true,
                    Message = "Insignias disponibles obtenidas",
                    Data = badges
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available badges");
                return StatusCode(500, new ApiResponseDto<List<BadgeDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Check and award automatic badges for user
        /// </summary>
        [HttpPost("check-automatic")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CheckAutomaticBadges()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                await _badgeService.CheckAndAwardAutomaticBadgesAsync(userId);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Verificación de insignias automáticas completada",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking automatic badges");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get badge statistics - Admin only
        /// </summary>
        [HttpGet("stats")]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<BadgeStatsDto>>> GetBadgeStats()
        {
            try
            {
                var stats = await _badgeService.GetBadgeStatsAsync();

                return Ok(new ApiResponseDto<BadgeStatsDto>
                {
                    Success = true,
                    Message = "Estadísticas de insignias obtenidas",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting badge stats");
                return StatusCode(500, new ApiResponseDto<BadgeStatsDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get badge holders
        /// </summary>
        [HttpGet("{badgeId}/holders")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<List<UserBadgeDto>>>> GetBadgeHolders(int badgeId, int page = 1, int pageSize = 20)
        {
            try
            {
                var holders = await _badgeService.GetBadgeHoldersAsync(badgeId, page, pageSize);

                return Ok(new ApiResponseDto<List<UserBadgeDto>>
                {
                    Success = true,
                    Message = "Portadores de la insignia obtenidos",
                    Data = holders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting badge holders for badge {BadgeId}", badgeId);
                return StatusCode(500, new ApiResponseDto<List<UserBadgeDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }
    }
}