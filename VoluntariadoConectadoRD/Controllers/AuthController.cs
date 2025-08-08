using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models.DTOs;
using Microsoft.AspNetCore.RateLimiting;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login de usuario (voluntario u organización)
        /// </summary>
        [HttpPost("login")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.LoginAsync(loginRequest);
                
                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login para email: {Email}", loginRequest.Email);
                return StatusCode(500, new ApiResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Registro de voluntario
        /// </summary>
        [HttpPost("register/voluntario")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<ApiResponseDto<UserInfoDto>>> RegisterVoluntario([FromBody] RegisterVoluntarioDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<UserInfoDto>
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.RegisterVoluntarioAsync(registerDto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetUserProfile), new { }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro de voluntario para email: {Email}", registerDto.Email);
                return StatusCode(500, new ApiResponseDto<UserInfoDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Registro de organización
        /// </summary>
        [HttpPost("register/organizacion")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<ApiResponseDto<UserInfoDto>>> RegisterOrganizacion([FromBody] RegisterOrganizacionDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<UserInfoDto>
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.RegisterOrganizacionAsync(registerDto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetUserProfile), new { }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro de organización para email: {Email}", registerDto.EmailAdmin);
                return StatusCode(500, new ApiResponseDto<UserInfoDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Obtener perfil del usuario autenticado
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<UserInfoDto>>> GetUserProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponseDto<UserInfoDto>
                    {
                        Success = false,
                        Message = "Token inválido"
                    });
                }

                var result = await _authService.GetUserByIdAsync(userId);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil de usuario");
                return StatusCode(500, new ApiResponseDto<UserInfoDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Validar disponibilidad de email
        /// </summary>
        [HttpGet("validate-email")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ValidateEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Email es requerido"
                    });
                }

                var result = await _authService.ValidateEmailAsync(email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar email: {Email}", email);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Cambiar contraseña del usuario autenticado
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<bool>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Token inválido"
                    });
                }

                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña para usuario");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Logout (opcional - principalmente para invalidar refresh tokens)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<bool>>> Logout()
        {
            try
            {
                // Aquí podrías implementar invalidación de refresh tokens
                // Por ahora, simplemente devolvemos éxito
                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Logout exitoso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en logout");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Solicitar recuperación de contraseña
        /// </summary>
        [HttpPost("forgot-password")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
                
                // Always return success for security reasons (don't reveal if email exists)
                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Si el correo existe en nuestro sistema, recibirás instrucciones para restablecer tu contraseña."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en forgot password para email: {Email}", forgotPasswordDto.Email);
                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Si el correo existe en nuestro sistema, recibirás instrucciones para restablecer tu contraseña."
                });
            }
        }

        /// <summary>
        /// Restablecer contraseña con token
        /// </summary>
        [HttpPost("reset-password")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.ResetPasswordAsync(resetPasswordDto.Email, resetPasswordDto.Token, resetPasswordDto.NewPassword);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en reset password para email: {Email}", resetPasswordDto.Email);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}