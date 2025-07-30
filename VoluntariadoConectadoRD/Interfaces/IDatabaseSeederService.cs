namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IDatabaseSeederService
    {
        Task SeedAsync();
        Task<bool> IsDatabaseSeededAsync();
    }
}