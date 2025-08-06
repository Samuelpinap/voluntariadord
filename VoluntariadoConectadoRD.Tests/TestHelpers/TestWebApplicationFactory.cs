using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VoluntariadoConectadoRD.Data;

namespace VoluntariadoConectadoRD.Tests.TestHelpers;

public class TestWebApplicationFactory&lt;TStartup&gt; : WebApplicationFactory&lt;TStartup&gt; where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =&gt;
        {
            var descriptor = services.SingleOrDefault(d =&gt; d.ServiceType == typeof(DbContextOptions&lt;DbContextApplication&gt;));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext&lt;DbContextApplication&gt;(options =&gt;
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService&lt;DbContextApplication&gt;();
            var logger = scopedServices.GetRequiredService&lt;ILogger&lt;TestWebApplicationFactory&lt;TStartup&gt;&gt;&gt;();

            db.Database.EnsureCreated();

            try
            {
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the test database");
            }
        });
    }

    private static void SeedTestData(DbContextApplication context)
    {
        context.Usuarios.RemoveRange(context.Usuarios);
        context.Organizaciones.RemoveRange(context.Organizaciones);
        context.VolunteerOpportunities.RemoveRange(context.VolunteerOpportunities);
        context.SaveChanges();

        var testVolunteer = new VoluntariadoConectadoRD.Models.Usuario
        {
            Id = 1,
            Nombre = "Test",
            Apellido = "Volunteer",
            Email = "test.volunteer@test.com",
            Password = BCrypt.Net.BCrypt.HashString("TestPassword123!"),
            Role = 1,
            Status = 1,
            FechaRegistro = DateTime.UtcNow,
            Telefono = "809-555-0001",
            FechaNacimiento = new DateTime(1995, 1, 1),
            Avatar = null
        };

        var testOrg = new VoluntariadoConectadoRD.Models.Usuario
        {
            Id = 2,
            Nombre = "Test",
            Apellido = "Organization Admin",
            Email = "test.org@test.com",
            Password = BCrypt.Net.BCrypt.HashString("TestPassword123!"),
            Role = 2,
            Status = 1,
            FechaRegistro = DateTime.UtcNow,
            Telefono = "809-555-0002",
            FechaNacimiento = new DateTime(1990, 1, 1),
            Avatar = null
        };

        var testAdmin = new VoluntariadoConectadoRD.Models.Usuario
        {
            Id = 3,
            Nombre = "Test",
            Apellido = "Admin",
            Email = "test.admin@test.com",
            Password = BCrypt.Net.BCrypt.HashString("TestPassword123!"),
            Role = 3,
            Status = 1,
            FechaRegistro = DateTime.UtcNow,
            Telefono = "809-555-0003",
            FechaNacimiento = new DateTime(1985, 1, 1),
            Avatar = null
        };

        context.Usuarios.AddRange(testVolunteer, testOrg, testAdmin);

        var testOrganization = new VoluntariadoConectadoRD.Models.Organizacion
        {
            Id = 1,
            IdUsuario = 2,
            NombreOrganizacion = "Test Organization",
            Descripcion = "Test organization description",
            Direccion = "Test Address",
            Telefono = "809-555-0100",
            SitioWeb = "https://test.org",
            FechaFundacion = new DateTime(2010, 1, 1),
            Status = 1,
            FechaRegistro = DateTime.UtcNow
        };

        context.Organizaciones.Add(testOrganization);

        var testOpportunity = new VoluntariadoConectadoRD.Models.VolunteerOpportunity
        {
            Id = 1,
            Title = "Test Volunteer Opportunity",
            Description = "Test opportunity description",
            RequiredSkills = "Test skills",
            Location = "Test Location",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(14),
            MaxVolunteers = 10,
            Status = "Activa",
            CreatedBy = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.VolunteerOpportunities.Add(testOpportunity);
        context.SaveChanges();
    }
}