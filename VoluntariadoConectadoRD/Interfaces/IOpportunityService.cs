using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IOpportunityService
    {
        Task<IEnumerable<OpportunityListDto>> GetAllOpportunitiesAsync(
            string? searchTerm = null,
            string? areaInteres = null,
            string? ubicacion = null,
            OpportunityStatus? status = null);

        Task<OpportunityDetailDto?> GetOpportunityByIdAsync(int id);

        Task<IEnumerable<OpportunityListDto>> GetOrganizationOpportunitiesAsync(int organizacionId);

        Task<ApiResponseDto<OpportunityDetailDto>> CreateOpportunityAsync(CreateOpportunityDto dto, int organizacionId);

        Task<ApiResponseDto<OpportunityDetailDto>> UpdateOpportunityAsync(int id, UpdateOpportunityDto dto, int organizacionId);

        Task<bool> DeleteOpportunityAsync(int id, int organizacionId);

        Task<bool> ApplyToOpportunityAsync(int opportunityId, int volunteerId, ApplyToOpportunityDto? applicationDto = null);

        Task<IEnumerable<ApplicationDto>> GetVolunteerApplicationsAsync(int volunteerId);

        Task<IEnumerable<ApplicationDto>> GetOrganizationApplicationsAsync(int organizacionId);

        Task<bool> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus status, string? notes = null);

        Task<int?> GetOrganizacionIdByUserAsync(int userId);
    }
}