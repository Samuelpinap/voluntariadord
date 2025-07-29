namespace VoluntariadoConectadoRD.Services
{
    public interface IDatabaseSeederService
    {
        Task SeedAsync();
        Task<bool> IsDatabaseSeededAsync();
    }
}