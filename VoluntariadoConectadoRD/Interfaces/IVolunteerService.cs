using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IVolunteerService
    {
        // Profile and User Management
        Task<EnhancedUserProfileDto?> GetUserProfileByIdAsync(int userId);
        Task<VolunteerStatsDto?> GetVolunteerStatsAsync(int volunteerId);
        Task<IEnumerable<VolunteerApplicationDetailDto>> GetMyApplicationsAsync(int userId);
        
        // Skills Management
        Task<IEnumerable<SkillDto>> GetAllSkillsAsync();
        Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(int userId);
        Task<bool> AddUserSkillAsync(int userId, CreateUserSkillDto skillDto);
        Task<bool> UpdateUserSkillAsync(int userId, int skillId, int nivel);
        Task<bool> RemoveUserSkillAsync(int userId, int skillId);
        
        // Activities Management
        Task<IEnumerable<VolunteerActivityDto>> GetUserActivitiesAsync(int userId);
        Task<VolunteerActivity?> CreateActivityAsync(int userId, int opportunityId, string titulo);
        Task<bool> UpdateActivityStatusAsync(int activityId, ActivityStatus status, int? horas = null);
        
        // Platform Statistics
        Task<PlatformStatsDto?> GetPlatformStatsAsync();
        Task<bool> UpdatePlatformStatsAsync();
        
        // Featured Opportunities
        Task<IEnumerable<OpportunityListDto>> GetFeaturedOpportunitiesAsync(int count = 3);
        
        // Admin Methods
        Task<PaginatedResult<AdminVolunteerDto>> GetAllVolunteersForAdminAsync(int page, int pageSize, string? search = null);
        Task<AdminStatsDto> GetAdminStatsAsync();
        
        // Admin User Management
        Task<bool> UpdateUserStatusAsync(int userId, UserStatus status);
        Task<bool> EditUserProfileAsAdminAsync(int userId, AdminEditUserDto editDto);
        Task<bool> DeleteUserAsync(int userId);
        
        // Admin Organization Management
        Task<PaginatedResult<AdminOrganizationDto>> GetAllOrganizationsForAdminAsync(int page, int pageSize, string? search = null);
        Task<bool> UpdateOrganizationStatusAsync(int orgId, UserStatus status, bool verificada);
        Task<bool> EditOrganizationAsAdminAsync(int orgId, AdminEditOrganizationDto editDto);
        Task<bool> DeleteOrganizationAsync(int orgId);
    }
}