using VoluntariadoApi.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IOportunidadService
    {
        Task<IEnumerable<OpportunityListDto>> GetAllOportunidadesAsync();
        Task<OpportunityDetailDto?> GetOpportunidadByIdAsync(int id);
    }
}
