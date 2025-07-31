using VoluntariadoApi.Models;

namespace VoluntariadoConectadoRD.Services
{
    public interface IOportunidadService
    {
        Task<IEnumerable<Oportunidad>> GetAllOportunidadesAsync();
        Task<Oportunidad?> GetOportunidadByIdAsync(int id);
    }
}
