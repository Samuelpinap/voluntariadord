using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<UserDashboardDto> GetUserDashboardDataAsync(int userId);
        Task<OrganizationDashboardDto> GetOrganizationDashboardDataAsync(int organizationId);
        Task<IEnumerable<RecentActivityDto>> GetRecentActivitiesAsync(int userId, int limit = 10);
        Task<IEnumerable<OpportunityListDto>> GetUserOpportunitiesAsync(int userId, int limit = 5);
    }
}