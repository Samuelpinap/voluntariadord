using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public interface IImageUploadService
    {
        // Avatar Upload Methods
        Task<ApiResponseDto<ImageResponseDto>> UploadAvatarAsync(IFormFile file, int userId);
        
        // Logo Upload Methods
        Task<ApiResponseDto<ImageResponseDto>> UploadLogoAsync(IFormFile file, int organizationId);
        
        // General Image Methods
        Task<ApiResponseDto<bool>> DeleteImageAsync(string imageUrl);
        Task<ApiResponseDto<bool>> ValidateImageAsync(IFormFile file);
        
        // Utility Methods
        string GenerateUniqueFileName(string originalFileName, string prefix);
        Task<bool> SaveImageToStorageAsync(IFormFile file, string fileName, string folder);
        Task<bool> DeleteImageFromStorageAsync(string imageUrl);
        string GetImageUrl(string fileName, string folder);
        
        // Validation Methods
        bool IsValidImageExtension(string fileName);
        bool IsValidImageSize(long fileSize);
        Task<bool> IsValidImageContentAsync(IFormFile file);
    }
}