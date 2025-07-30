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
    public class ImageController : ControllerBase
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IImageUploadService imageUploadService, ILogger<ImageController> logger)
        {
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        #region Avatar Upload Endpoints

        /// <summary>
        /// Sube un avatar para el usuario autenticado
        /// </summary>
        /// <param name="file">Archivo de imagen para el avatar</param>
        /// <returns>Información del avatar subido</returns>
        [HttpPost("upload/avatar")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponseDto<ImageResponseDto>>> UploadAvatar(IFormFile file)
        {
            try
            {
                // Validar que se proporcionó un archivo
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponseDto<ImageResponseDto>
                    {
                        Success = false,
                        Message = "No se proporcionó ningún archivo",
                        Data = null
                    });
                }

                // Obtener ID del usuario autenticado
                var currentUserId = GetCurrentUserId();

                // Subir avatar usando el servicio
                var result = await _imageUploadService.UploadAvatarAsync(file, currentUserId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponseDto<ImageResponseDto>
                {
                    Success = false,
                    Message = "Error interno del servidor al subir el avatar",
                    Data = null
                });
            }
        }

        #endregion

        #region Logo Upload Endpoints

        /// <summary>
        /// Sube un logo para la organización del usuario autenticado
        /// </summary>
        /// <param name="file">Archivo de imagen para el logo</param>
        /// <returns>Información del logo subido</returns>
        [HttpPost("upload/logo")]
        [Authorize(Roles = "Organizacion")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponseDto<ImageResponseDto>>> UploadLogo(IFormFile file)
        {
            try
            {
                // Validar que se proporcionó un archivo
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponseDto<ImageResponseDto>
                    {
                        Success = false,
                        Message = "No se proporcionó ningún archivo",
                        Data = null
                    });
                }

                // Obtener ID de la organización del usuario autenticado
                var organizationId = GetCurrentUserOrganizationId();
                if (organizationId == 0)
                {
                    return BadRequest(new ApiResponseDto<ImageResponseDto>
                    {
                        Success = false,
                        Message = "Usuario no está asociado con ninguna organización",
                        Data = null
                    });
                }

                // Subir logo usando el servicio
                var result = await _imageUploadService.UploadLogoAsync(file, organizationId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading logo for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, new ApiResponseDto<ImageResponseDto>
                {
                    Success = false,
                    Message = "Error interno del servidor al subir el logo",
                    Data = null
                });
            }
        }

        #endregion

        #region Image Management Endpoints

        /// <summary>
        /// Elimina una imagen del sistema
        /// </summary>
        /// <param name="imageUrl">URL codificada de la imagen a eliminar</param>
        /// <returns>Resultado de la eliminación</returns>
        [HttpDelete("{*imageUrl}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteImage(string imageUrl)
        {
            try
            {
                // Validar que se proporcionó una URL
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "URL de imagen no proporcionada",
                        Data = false
                    });
                }

                // Decodificar la URL (en caso de que esté codificada)
                var decodedUrl = Uri.UnescapeDataString(imageUrl);

                // Validar permisos: solo el propietario de la imagen o administradores pueden eliminarla
                var currentUserRole = GetCurrentUserRole();
                if (currentUserRole != "Administrador")
                {
                    // Verificar que la imagen pertenece al usuario actual
                    var currentUserId = GetCurrentUserId();
                    var organizationId = GetCurrentUserOrganizationId();

                    // Verificar si es avatar del usuario actual o logo de su organización
                    if (!IsUserAuthorizedToDeleteImage(decodedUrl, currentUserId, organizationId))
                    {
                        return Forbid("No tienes permisos para eliminar esta imagen");
                    }
                }

                // Eliminar imagen usando el servicio
                var result = await _imageUploadService.DeleteImageAsync(decodedUrl);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor al eliminar la imagen",
                    Data = false
                });
            }
        }

        /// <summary>
        /// Valida un archivo de imagen antes de subirlo
        /// </summary>
        /// <param name="file">Archivo a validar</param>
        /// <returns>Resultado de la validación</returns>
        [HttpPost("validate")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ValidateImage(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "No se proporcionó ningún archivo",
                        Data = false
                    });
                }

                var result = await _imageUploadService.ValidateImageAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image: {FileName}", file?.FileName);
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error al validar la imagen",
                    Data = false
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

        /// <summary>
        /// Verifica si el usuario actual está autorizado para eliminar una imagen específica
        /// </summary>
        /// <param name="imageUrl">URL de la imagen</param>
        /// <param name="userId">ID del usuario actual</param>
        /// <param name="organizationId">ID de la organización del usuario (si aplica)</param>
        /// <returns>True si está autorizado, false en caso contrario</returns>
        private bool IsUserAuthorizedToDeleteImage(string imageUrl, int userId, int organizationId)
        {
            try
            {
                // Extraer información de la URL para determinar el tipo y propietario
                var uri = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
                var path = uri.IsAbsoluteUri ? uri.AbsolutePath : imageUrl;

                // Verificar si es un avatar del usuario actual
                if (path.Contains("/uploads/avatars/") && path.Contains($"avatar_{userId}_"))
                {
                    return true;
                }

                // Verificar si es un logo de la organización del usuario actual
                if (organizationId > 0 && path.Contains("/uploads/logos/") && path.Contains($"logo_{organizationId}_"))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying image ownership for URL: {ImageUrl}", imageUrl);
                return false;
            }
        }

        #endregion
    }
}