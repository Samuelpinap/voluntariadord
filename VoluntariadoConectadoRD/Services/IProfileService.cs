using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public interface IProfileService
    {
        // User Profile Methods
        Task<ApiResponseDto<UserProfileDto>> GetUserProfileAsync(int userId);
        Task<ApiResponseDto<UserProfileDto>> UpdateUserProfileAsync(int userId, UpdateUserProfileDto dto);
        
        // Organization Profile Methods
        Task<ApiResponseDto<OrganizationProfileDto>> GetOrganizationProfileAsync(int organizationId);
        Task<ApiResponseDto<OrganizationProfileDto>> UpdateOrganizationProfileAsync(int organizationId, UpdateOrganizationProfileDto dto);
        
        // Profile Completion Methods
        Task<ApiResponseDto<ProfileCompletionDto>> CheckUserProfileCompletionAsync(int userId);
        Task<ApiResponseDto<ProfileCompletionDto>> CheckOrganizationProfileCompletionAsync(int organizationId);
        
        // Utility Methods
        Task<ApiResponseDto<bool>> UpdateUserAvatarAsync(int userId, string avatarUrl);
        Task<ApiResponseDto<bool>> UpdateOrganizationLogoAsync(int organizationId, string logoUrl);
    }
}