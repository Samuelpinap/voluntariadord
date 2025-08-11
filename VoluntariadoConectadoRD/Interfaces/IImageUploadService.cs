using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IImageUploadService
    {
        Task<ImageUploadResponseDto> UploadAvatarAsync(IFormFile file, int userId);
        Task<ImageUploadResponseDto> UploadLogoAsync(IFormFile file, int userId);
        Task<ImageUploadResponseDto> UploadFileAsync(IFormFile file, string folder);
        Task DeleteAvatarAsync(int userId);
        Task DeleteLogoAsync(int userId);
        Task<string> SaveImageAsync(IFormFile file, string folder);
        Task DeleteImageAsync(string filePath);
        bool IsValidImageFile(IFormFile file);
    }
}