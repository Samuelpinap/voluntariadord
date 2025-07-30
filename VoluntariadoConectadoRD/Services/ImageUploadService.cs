using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly ILogger<ImageUploadService> _logger;
        private readonly IProfileService _profileService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        // Configuration constants
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly int _maxImageWidth = 2048;
        private readonly int _maxImageHeight = 2048;

        public ImageUploadService(
            ILogger<ImageUploadService> logger,
            IProfileService profileService,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _logger = logger;
            _profileService = profileService;
            _environment = environment;
            _configuration = configuration;
        }

        #region Avatar Upload Methods

        public async Task<ApiResponseDto<ImageResponseDto>> UploadAvatarAsync(IFormFile file, int userId)
        {
            try
            {
                // Validate the image
                var validationResult = await ValidateImageAsync(file);
                if (!validationResult.Success)
                {
                    return new ApiResponseDto<ImageResponseDto>
                    {
                        Success = false,
                        Message = validationResult.Message,
                        Data = null
                    };
                }

                // Generate unique filename
                var fileName = GenerateUniqueFileName(file.FileName, $"avatar_{userId}");
                var folder = "avatars";

                // Save image to storage
                var saveResult = await SaveImageToStorageAsync(file, fileName, folder);
                if (!saveResult)
                {
                    return new ApiResponseDto<ImageResponseDto>
                    {
                        Success = false,
                        Message = "Error al guardar la imagen",
                        Data = null
                    };
                }

                // Get image URL
                var imageUrl = GetImageUrl(fileName, folder);

                // Update user avatar in database
                var updateResult = await _profileService.UpdateUserAvatarAsync(userId, imageUrl);
                if (!updateResult.Success)
                {
                    // If database update fails, delete the uploaded file
                    await DeleteImageFromStorageAsync(imageUrl);
                    return new ApiResponseDto<ImageResponseDto>
                    {
                        Success = false,
                        Message = "Error al actualizar el avatar en la base de datos",
                        Data = null
                    };
                }

                var imageResponse = new ImageResponseDto
                {
                    Success = true,
                    Message = "Avatar subido exitosamente",
                    ImageUrl = imageUrl,
                    FileName = fileName,
                    FileSize = file.Length
                };

                return new ApiResponseDto<ImageResponseDto>
                {
                    Success = true,
                    Message = "Avatar subido y actualizado exitosamente",
                    Data = imageResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar for userId: {UserId}", userId);
                return new ApiResponseDto<ImageResponseDto>
                {
                    Success = false,
                    Message = "Error interno del servidor al subir el avatar",
                    Data = null
                };
            }
        }

        #endregion

        #region Logo Upload Methods

        public async Task<ApiResponseDto<ImageResponseDto>> UploadLogoAsync(IFormFile file, int organizationId)
        {
            try
            {
                // Validate the image
                var validationResult = await ValidateImageAsync(file);
                if (!validationResult.Success)
                {
                    return new ApiResponseDto<ImageResponseDto>
                    {
                        Success = false,
                        Message = validationResult.Message,
                        Data = null
                    };
                }

                // Generate unique filename
                var fileName = GenerateUniqueFileName(file.FileName, $"logo_{organizationId}");
                var folder = "logos";

                // Save image to storage
                var saveResult = await SaveImageToStorageAsync(file, fileName, folder);
                if (!saveResult)
                {
                    return new ApiResponseDto<ImageResponseDto>
                    {
                        Success = false,
                        Message = "Error al guardar la imagen",
                        Data = null
                    };
                }

                // Get image URL
                var imageUrl = GetImageUrl(fileName, folder);

                // Update organization logo in database
                var updateResult = await _profileService.UpdateOrganizationLogoAsync(organizationId, imageUrl);
                if (!updateResult.Success)
                {
                    // If database update fails, delete the uploaded file
                    await DeleteImageFromStorageAsync(imageUrl);
                    return new ApiResponseDto<ImageResponseDto>
                    {
                        Success = false,
                        Message = "Error al actualizar el logo en la base de datos",
                        Data = null
                    };
                }

                var imageResponse = new ImageResponseDto
                {
                    Success = true,
                    Message = "Logo subido exitosamente",
                    ImageUrl = imageUrl,
                    FileName = fileName,
                    FileSize = file.Length
                };

                return new ApiResponseDto<ImageResponseDto>
                {
                    Success = true,
                    Message = "Logo subido y actualizado exitosamente",
                    Data = imageResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading logo for organizationId: {OrganizationId}", organizationId);
                return new ApiResponseDto<ImageResponseDto>
                {
                    Success = false,
                    Message = "Error interno del servidor al subir el logo",
                    Data = null
                };
            }
        }

        #endregion

        #region General Image Methods

        public async Task<ApiResponseDto<bool>> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "URL de imagen no válida",
                        Data = false
                    };
                }

                var deleteResult = await DeleteImageFromStorageAsync(imageUrl);

                return new ApiResponseDto<bool>
                {
                    Success = deleteResult,
                    Message = deleteResult ? "Imagen eliminada exitosamente" : "Error al eliminar la imagen",
                    Data = deleteResult
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor al eliminar la imagen",
                    Data = false
                };
            }
        }

        public async Task<ApiResponseDto<bool>> ValidateImageAsync(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "No se proporcionó ningún archivo",
                        Data = false
                    };
                }

                // Check file size
                if (!IsValidImageSize(file.Length))
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = $"El archivo es demasiado grande. Tamaño máximo permitido: {_maxFileSize / (1024 * 1024)}MB",
                        Data = false
                    };
                }

                // Check file extension
                if (!IsValidImageExtension(file.FileName))
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = $"Formato de archivo no válido. Formatos permitidos: {string.Join(", ", _allowedExtensions)}",
                        Data = false
                    };
                }

                // Check file content
                var isValidContent = await IsValidImageContentAsync(file);
                if (!isValidContent)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "El contenido del archivo no es una imagen válida",
                        Data = false
                    };
                }

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Imagen válida",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image: {FileName}", file?.FileName);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error al validar la imagen",
                    Data = false
                };
            }
        }

        #endregion

        #region Utility Methods

        public string GenerateUniqueFileName(string originalFileName, string prefix)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8]; // First 8 characters of GUID
            return $"{prefix}_{timestamp}_{guid}{extension}";
        }

        public async Task<bool> SaveImageToStorageAsync(IFormFile file, string fileName, string folder)
        {
            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Full file path
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image to storage: {FileName}", fileName);
                return false;
            }
        }

        public async Task<bool> DeleteImageFromStorageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                // Extract relative path from URL
                var uri = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
                var relativePath = uri.IsAbsoluteUri ? uri.AbsolutePath : imageUrl;
                
                // Remove leading slash if present
                if (relativePath.StartsWith("/"))
                    relativePath = relativePath[1..];

                // Full file path
                var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from storage: {ImageUrl}", imageUrl);
                return false;
            }
        }

        public string GetImageUrl(string fileName, string folder)
        {
            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:5161";
            return $"{baseUrl}/uploads/{folder}/{fileName}";
        }

        #endregion

        #region Validation Methods

        public bool IsValidImageExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        public bool IsValidImageSize(long fileSize)
        {
            return fileSize > 0 && fileSize <= _maxFileSize;
        }

        public async Task<bool> IsValidImageContentAsync(IFormFile file)
        {
            try
            {
                // Reset stream position
                file.OpenReadStream().Position = 0;

                // Read first few bytes to check for image signatures
                using var stream = file.OpenReadStream();
                var buffer = new byte[12];
                await stream.ReadAsync(buffer, 0, 12);

                // Check for common image file signatures
                // JPEG: FF D8 FF
                if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                    return true;

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47 &&
                    buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A)
                    return true;

                // GIF: 47 49 46 38 (GIF8)
                if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38)
                    return true;

                // WebP: 52 49 46 46 (RIFF) ... 57 45 42 50 (WEBP)
                if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                    buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image content for file: {FileName}", file.FileName);
                return false;
            }
        }

        #endregion
    }
}