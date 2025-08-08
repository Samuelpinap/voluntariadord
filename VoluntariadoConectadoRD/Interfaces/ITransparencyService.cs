using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface ITransparencyService
    {
        Task<List<OrganizationTransparencyDto>> GetAllOrganizationsFinancialSummaryAsync(TransparencyFiltersDto? filters = null);
        Task<OrganizationFinancialDetailsDto?> GetOrganizationFinancialDetailsAsync(int organizationId);
        Task<FinancialReportDetailDto?> GetFinancialReportDetailsAsync(int reportId);
        Task<List<int>> GetAvailableYearsAsync();
        Task<List<string>> GetOrganizationTypesAsync();
        Task<ChartDataDto> GetPlatformFinancialOverviewAsync();
        Task<bool> OrganizationHasPublicReportsAsync(int organizationId);
    }
}