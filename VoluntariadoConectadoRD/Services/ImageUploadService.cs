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

        public async Task<ImageUploadResponseDto> UploadFileAsync(IFormFile file, string folder)
        {
            if (!IsValidImageFile(file))
                throw new ArgumentException("Archivo de imagen no válido");

            try
            {
                var fileName = await SaveImageAsync(file, folder);
                var imageUrl = GetImageUrl(fileName, folder);

                return new ImageUploadResponseDto
                {
                    FileName = fileName,
                    Url = imageUrl,
                    Size = file.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir archivo a carpeta {Folder}", folder);
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
            {
                _logger.LogWarning("File is null or empty");
                return false;
            }

            // Check file size
            if (file.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning("File size {FileSize} exceeds maximum allowed size {MaxSize}", file.Length, MaxFileSizeBytes);
                return false;
            }

            // Check minimum file size (avoid 1-byte malicious files)
            if (file.Length < 100)
            {
                _logger.LogWarning("File size {FileSize} is too small to be a valid image", file.Length);
                return false;
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("File extension {Extension} is not allowed", extension);
                return false;
            }

            // Check MIME type
            if (string.IsNullOrEmpty(file.ContentType) || !_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("MIME type {ContentType} is not allowed", file.ContentType);
                return false;
            }

            // Check for potentially malicious filenames
            var fileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            {
                _logger.LogWarning("Potentially malicious filename: {FileName}", fileName);
                return false;
            }

            // Additional security: Check file signature (magic bytes)
            if (!IsValidImageFileSignature(file))
            {
                _logger.LogWarning("File signature validation failed for {FileName}", fileName);
                return false;
            }

            return true;
        }

        private bool IsValidImageFileSignature(IFormFile file)
        {
            try
            {
                using var reader = new BinaryReader(file.OpenReadStream());
                var bytes = reader.ReadBytes(8);
                
                // Check for common image file signatures
                // JPEG: FF D8 FF
                if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                    return true;
                
                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                    return true;
                
                // GIF: GIF87a or GIF89a
                if (bytes.Length >= 6)
                {
                    var header = System.Text.Encoding.ASCII.GetString(bytes, 0, 6);
                    if (header == "GIF87a" || header == "GIF89a")
                        return true;
                }
                
                // WebP: RIFF....WEBP
                if (bytes.Length >= 8)
                {
                    var riff = System.Text.Encoding.ASCII.GetString(bytes, 0, 4);
                    if (riff == "RIFF")
                        return true; // Simplified WebP check
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file signature");
                return false;
            }
            finally
            {
                // Reset stream position
                file.OpenReadStream().Position = 0;
            }
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