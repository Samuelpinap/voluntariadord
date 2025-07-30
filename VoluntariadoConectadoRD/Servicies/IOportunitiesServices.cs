using VoluntariadoApi.Models;

namespace VoluntariadoApi.Interfaces.IServices
{
    public interface IOportunidadService
    {
        Task<IEnumerable<Oportunidad>> GetAllOportunidadesAsync();
        Task<Oportunidad?> GetOportunidadByIdAsync(int id);
    }
}
