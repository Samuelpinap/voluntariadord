using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IOpportunityService
    {
        Task<IEnumerable<OpportunityListDto>> GetAllOpportunitiesAsync();
        Task<OpportunityDetailDto?> GetOpportunityByIdAsync(int id);
        Task<IEnumerable<OpportunityListDto>> GetOpportunitiesByOrganizationAsync(int organizationId);
        Task<OpportunityDetailDto> CreateOpportunityAsync(CreateOpportunityDto createDto, int organizationId);
        Task<OpportunityDetailDto?> UpdateOpportunityAsync(int id, UpdateOpportunityDto updateDto, int organizationId);
        Task<bool> DeleteOpportunityAsync(int id, int organizationId);
        Task<bool> ApplyToOpportunityAsync(int opportunityId, int userId, ApplyToOpportunityDto? applyDto = null);
        Task<IEnumerable<ApplicationDto>> GetApplicationsForOpportunityAsync(int opportunityId, int organizationId);
        Task<IEnumerable<ApplicationDto>> GetUserApplicationsAsync(int userId);
        Task<bool> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus status, int organizationId, string? notes = null);
    }
}