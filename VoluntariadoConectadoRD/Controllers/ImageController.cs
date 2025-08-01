using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IImageUploadService imageUploadService, ILogger<ImageController> logger)
        {
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        [HttpPost("upload/avatar")]
        public async Task<ActionResult<ApiResponseDto<ImageUploadResponseDto>>> UploadAvatar([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponseDto<ImageUploadResponseDto>
                    {
                        Success = false,
                        Message = "No se ha seleccionado ningún archivo",
                        Data = null
                    });
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<ImageUploadResponseDto>
                    {
                        Success = false,
                        Message = "Usuario no autenticado",
                        Data = null
                    });
                }

                var result = await _imageUploadService.UploadAvatarAsync(file, userId);
                
                return Ok(new ApiResponseDto<ImageUploadResponseDto>
                {
                    Success = true,
                    Message = "Avatar subido exitosamente",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDto<ImageUploadResponseDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir avatar");
                return StatusCode(500, new ApiResponseDto<ImageUploadResponseDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }

        [HttpPost("upload/logo")]
        public async Task<ActionResult<ApiResponseDto<ImageUploadResponseDto>>> UploadLogo([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponseDto<ImageUploadResponseDto>
                    {
                        Success = false,
                        Message = "No se ha seleccionado ningún archivo",
                        Data = null
                    });
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<ImageUploadResponseDto>
                    {
                        Success = false,
                        Message = "Usuario no autenticado",
                        Data = null
                    });
                }

                var result = await _imageUploadService.UploadLogoAsync(file, userId);
                
                return Ok(new ApiResponseDto<ImageUploadResponseDto>
                {
                    Success = true,
                    Message = "Logo subido exitosamente",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDto<ImageUploadResponseDto>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir logo");
                return StatusCode(500, new ApiResponseDto<ImageUploadResponseDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }

        [HttpDelete("avatar")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteAvatar()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no autenticado",
                        Data = null
                    });
                }

                await _imageUploadService.DeleteAvatarAsync(userId);
                
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Avatar eliminado exitosamente",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar avatar");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }

        [HttpDelete("logo")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteLogo()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new ApiResponseDto<object>
                    {
                        Success = false,
                        Message = "Usuario no autenticado",
                        Data = null
                    });
                }

                await _imageUploadService.DeleteLogoAsync(userId);
                
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Logo eliminado exitosamente",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar logo");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                });
            }
        }
    }
}