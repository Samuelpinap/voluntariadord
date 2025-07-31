using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRd.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly DbContextApplication _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImageUploadService> _logger;

        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
        private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        public ImageUploadService(
            DbContextApplication context, 
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<ImageUploadService> logger)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ImageUploadResponseDto> UploadAvatarAsync(IFormFile file, int userId)
        {
            if (!IsValidImageFile(file))
                throw new ArgumentException("Archivo de imagen no válido");

            try
            {
                // Delete existing avatar if exists
                await DeleteAvatarAsync(userId);

                // Save new avatar
                var fileName = await SaveImageAsync(file, "avatars");
                var imageUrl = GetImageUrl(fileName, "avatars");

                // Update user profile
                var user = await _context.Usuarios.FindAsync(userId);
                if (user != null)
                {
                    user.ProfileImageUrl = imageUrl;
                    await _context.SaveChangesAsync();
                }

                return new ImageUploadResponseDto
                {
                    FileName = fileName,
                    Url = imageUrl,
                    Size = file.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir avatar para usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<ImageUploadResponseDto> UploadLogoAsync(IFormFile file, int userId)
        {
            if (!IsValidImageFile(file))
                throw new ArgumentException("Archivo de imagen no válido");

            try
            {
                // Delete existing logo if exists
                await DeleteLogoAsync(userId);

                // Save new logo
                var fileName = await SaveImageAsync(file, "logos");
                var imageUrl = GetImageUrl(fileName, "logos");

                // Update organization profile
                var organization = await _context.Organizaciones
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.Usuario.Id == userId);

                if (organization != null)
                {
                    organization.LogoUrl = imageUrl;
                    await _context.SaveChangesAsync();
                }

                return new ImageUploadResponseDto
                {
                    FileName = fileName,
                    Url = imageUrl,
                    Size = file.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir logo para usuario {UserId}", userId);
                throw;
            }
        }

        public async Task DeleteAvatarAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user != null && !string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    await DeleteImageAsync(user.ProfileImageUrl);
                    user.ProfileImageUrl = null;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar avatar para usuario {UserId}", userId);
                throw;
            }
        }

        public async Task DeleteLogoAsync(int userId)
        {
            try
            {
                var organization = await _context.Organizaciones
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.Usuario.Id == userId);

                if (organization != null && !string.IsNullOrEmpty(organization.LogoUrl))
                {
                    await DeleteImageAsync(organization.LogoUrl);
                    organization.LogoUrl = null;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar logo para usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar imagen en carpeta {Folder}", folder);
                throw;
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                // Extract relative path from URL
                var uri = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
                var relativePath = uri.IsAbsoluteUri ? uri.AbsolutePath : imageUrl;

                // Remove leading slash if present
                // Fixed: Use Substring instead of range operator for compatibility
                if (relativePath.StartsWith("/"))
                    relativePath = relativePath.Substring(1);

                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen {ImageUrl}", imageUrl);
                // Don't throw here, as this is often called during cleanup
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check file size
            if (file.Length > MaxFileSizeBytes)
                return false;

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Check MIME type
            if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            // Fixed: Use Substring instead of range operator for compatibility
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8); // First 8 characters of GUID
            
            return $"{timestamp}_{guid}{extension}";
        }

        private string GetImageUrl(string fileName, string folder)
        {
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:6010";
            return $"{baseUrl}/uploads/{folder}/{fileName}";
        }
    }
}