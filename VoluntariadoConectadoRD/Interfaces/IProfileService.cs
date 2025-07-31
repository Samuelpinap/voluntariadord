using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<OrganizationProfileDto?> GetOrganizationProfileAsync(int orgId);
        Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto);
        Task<OrganizationProfileDto> UpdateOrganizationProfileAsync(int userId, UpdateOrganizationProfileDto updateDto);
        Task<ProfileCompletionDto> GetUserProfileCompletionAsync(int userId);
        Task<ProfileCompletionDto> GetOrganizationProfileCompletionAsync(int orgId);
    }
}