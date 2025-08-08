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
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _skillService;
        private readonly ILogger<SkillController> _logger;

        public SkillController(ISkillService skillService, ILogger<SkillController> logger)
        {
            _skillService = skillService;
            _logger = logger;
        }

        /// <summary>
        /// Get all skills - Public
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<List<SkillDto>>>> GetAllSkills()
        {
            try
            {
                var skills = await _skillService.GetAllSkillsAsync();

                return Ok(new ApiResponseDto<List<SkillDto>>
                {
                    Success = true,
                    Message = "Habilidades obtenidas",
                    Data = skills
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all skills");
                return StatusCode(500, new ApiResponseDto<List<SkillDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Search skills - Public
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<List<SkillDto>>>> SearchSkills([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return BadRequest(new ApiResponseDto<List<SkillDto>>
                    {
                        Success = false,
                        Message = "Término de búsqueda requerido"
                    });
                }

                var skills = await _skillService.SearchSkillsAsync(searchTerm);

                return Ok(new ApiResponseDto<List<SkillDto>>
                {
                    Success = true,
                    Message = "Búsqueda de habilidades completada",
                    Data = skills
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching skills");
                return StatusCode(500, new ApiResponseDto<List<SkillDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get user's skills
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponseDto<List<SkillDto>>>> GetUserSkills(int userId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int currentUserId))
                {
                    // Users can only see their own skills unless they're admin
                    var userRole = User.FindFirst("Rol")?.Value;
                    if (userId != currentUserId && userRole != "3") // Not admin
                    {
                        return Forbid();
                    }
                }

                var skills = await _skillService.GetUserSkillsAsync(userId);

                return Ok(new ApiResponseDto<List<SkillDto>>
                {
                    Success = true,
                    Message = "Habilidades del usuario obtenidas",
                    Data = skills
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user skills for user {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<List<SkillDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get current user's skills
        /// </summary>
        [HttpGet("my-skills")]
        public async Task<ActionResult<ApiResponseDto<List<SkillDto>>>> GetMySkills()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<List<SkillDto>>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var skills = await _skillService.GetUserSkillsAsync(userId);

                return Ok(new ApiResponseDto<List<SkillDto>>
                {
                    Success = true,
                    Message = "Mis habilidades obtenidas",
                    Data = skills
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user skills");
                return StatusCode(500, new ApiResponseDto<List<SkillDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Create a new skill - Admin only
        /// </summary>
        [HttpPost]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<SkillDto>>> CreateSkill([FromBody] CreateSkillDto createSkillDto)
        {
            try
            {
                var skill = await _skillService.CreateSkillAsync(createSkillDto);

                return Ok(new ApiResponseDto<SkillDto>
                {
                    Success = true,
                    Message = "Habilidad creada exitosamente",
                    Data = skill
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating skill");
                return StatusCode(500, new ApiResponseDto<SkillDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Update a skill - Admin only
        /// </summary>
        [HttpPut("{skillId}")]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<SkillDto>>> UpdateSkill(int skillId, [FromBody] UpdateSkillDto updateSkillDto)
        {
            try
            {
                var skill = await _skillService.UpdateSkillAsync(skillId, updateSkillDto);

                return Ok(new ApiResponseDto<SkillDto>
                {
                    Success = true,
                    Message = "Habilidad actualizada exitosamente",
                    Data = skill
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponseDto<SkillDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skill {SkillId}", skillId);
                return StatusCode(500, new ApiResponseDto<SkillDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Delete a skill - Admin only
        /// </summary>
        [HttpDelete("{skillId}")]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteSkill(int skillId)
        {
            try
            {
                var result = await _skillService.DeleteSkillAsync(skillId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Habilidad no encontrada"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Habilidad eliminada exitosamente",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skill {SkillId}", skillId);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Add skill to current user
        /// </summary>
        [HttpPost("add-to-user")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AddSkillToUser([FromBody] AddUserSkillDto addSkillDto)
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

                var result = await _skillService.AddSkillToUserAsync(userId, addSkillDto.SkillId, addSkillDto.Nivel, addSkillDto.Certificacion);

                if (!result)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "No se pudo agregar la habilidad. Verifique que la habilidad exista y que no la tenga ya."
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Habilidad agregada exitosamente",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding skill to user");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Remove skill from current user
        /// </summary>
        [HttpDelete("remove-from-user/{skillId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> RemoveSkillFromUser(int skillId)
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

                var result = await _skillService.RemoveSkillFromUserAsync(userId, skillId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "El usuario no tiene esta habilidad"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Habilidad removida exitosamente",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing skill from user");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Update user skill details
        /// </summary>
        [HttpPut("update-user-skill/{skillId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateUserSkill(int skillId, [FromBody] UpdateUserSkillDto updateDto)
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

                var result = await _skillService.UpdateUserSkillAsync(userId, skillId, updateDto);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "El usuario no tiene esta habilidad"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Habilidad actualizada exitosamente",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user skill");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get skill statistics - Admin only
        /// </summary>
        [HttpGet("stats")]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<SkillStatsDto>>> GetSkillStats()
        {
            try
            {
                var stats = await _skillService.GetSkillStatsAsync();

                return Ok(new ApiResponseDto<SkillStatsDto>
                {
                    Success = true,
                    Message = "Estadísticas de habilidades obtenidas",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skill stats");
                return StatusCode(500, new ApiResponseDto<SkillStatsDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get users with specific skill
        /// </summary>
        [HttpGet("{skillId}/users")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<List<UserSkillDto>>>> GetUsersWithSkill(int skillId, int page = 1, int pageSize = 20)
        {
            try
            {
                var users = await _skillService.GetUsersWithSkillAsync(skillId, page, pageSize);

                return Ok(new ApiResponseDto<List<UserSkillDto>>
                {
                    Success = true,
                    Message = "Usuarios con la habilidad obtenidos",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with skill {SkillId}", skillId);
                return StatusCode(500, new ApiResponseDto<List<UserSkillDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get skill categories
        /// </summary>
        [HttpGet("categories")]
        [AllowAnonymous]
        public ActionResult<ApiResponseDto<List<string>>> GetSkillCategories()
        {
            try
            {
                var categories = SkillCategories.GetAllCategories();

                return Ok(new ApiResponseDto<List<string>>
                {
                    Success = true,
                    Message = "Categorías de habilidades obtenidas",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skill categories");
                return StatusCode(500, new ApiResponseDto<List<string>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get skill levels
        /// </summary>
        [HttpGet("levels")]
        [AllowAnonymous]
        public ActionResult<ApiResponseDto<List<string>>> GetSkillLevels()
        {
            try
            {
                var levels = SkillLevels.GetAllLevels();

                return Ok(new ApiResponseDto<List<string>>
                {
                    Success = true,
                    Message = "Niveles de habilidades obtenidos",
                    Data = levels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skill levels");
                return StatusCode(500, new ApiResponseDto<List<string>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Seed default skills - Admin only
        /// </summary>
        [HttpPost("seed-defaults")]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<bool>>> SeedDefaultSkills()
        {
            try
            {
                await _skillService.SeedDefaultSkillsAsync();

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Habilidades por defecto creadas exitosamente",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding default skills");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }
    }
}