using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;
using Xunit;

namespace VoluntariadoConectadoRD.Tests
{
    public class ApplicationServiceTests : IDisposable
    {
        private readonly DbContextApplication _context;
        private readonly Mock<ILogger<OpportunityService>> _loggerMock;
        private readonly OpportunityService _service;

        public ApplicationServiceTests()
        {
            var options = new DbContextOptionsBuilder<DbContextApplication>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DbContextApplication(options);
            _loggerMock = new Mock<ILogger<OpportunityService>>();
            _service = new OpportunityService(_context, _loggerMock.Object);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Crear usuarios de prueba
            var usuario1 = new Usuario
            {
                Id = 1,
                Nombre = "Juan",
                Apellido = "Pérez",
                Email = "juan.perez@test.com",
                PasswordHash = "hash123",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo,
                FechaCreacion = DateTime.UtcNow
            };

            var usuario2 = new Usuario
            {
                Id = 2,
                Nombre = "María",
                Apellido = "García",
                Email = "maria.garcia@test.com",
                PasswordHash = "hash456",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo,
                FechaCreacion = DateTime.UtcNow
            };

            // Crear organización de prueba
            var organizacion = new Organizacion
            {
                Id = 1,
                Nombre = "Fundación Test",
                Email = "fundacion@test.com",
                Descripcion = "Organización de prueba",
                UsuarioId = 3,
                Estatus = OrganizacionStatus.Activa,
                FechaCreacion = DateTime.UtcNow
            };

            // Crear oportunidades de prueba
            var oportunidad1 = new VolunteerOpportunity
            {
                Id = 1,
                Titulo = "Ayuda en Hospital",
                Descripcion = "Necesitamos voluntarios para ayudar en el hospital",
                Ubicacion = "Santo Domingo",
                FechaInicio = DateTime.UtcNow.AddDays(7),
                DuracionHoras = 4,
                VoluntariosRequeridos = 5,
                VoluntariosInscritos = 0,
                AreaInteres = "Salud",
                NivelExperiencia = "Principiante",
                Estatus = OpportunityStatus.Activa,
                OrganizacionId = 1,
                FechaCreacion = DateTime.UtcNow
            };

            var oportunidad2 = new VolunteerOpportunity
            {
                Id = 2,
                Titulo = "Limpieza de Playa",
                Descripcion = "Ayuda a limpiar las playas de la ciudad",
                Ubicacion = "Boca Chica",
                FechaInicio = DateTime.UtcNow.AddDays(14),
                DuracionHoras = 6,
                VoluntariosRequeridos = 10,
                VoluntariosInscritos = 0,
                AreaInteres = "Medio Ambiente",
                NivelExperiencia = "Principiante",
                Estatus = OpportunityStatus.Activa,
                OrganizacionId = 1,
                FechaCreacion = DateTime.UtcNow
            };

            // Crear postulaciones de prueba
            var postulacion1 = new VolunteerApplication
            {
                Id = 1,
                UsuarioId = 1,
                OpportunityId = 1,
                Mensaje = "Me interesa mucho ayudar en el hospital",
                Estatus = ApplicationStatus.Pendiente,
                FechaAplicacion = DateTime.UtcNow.AddDays(-5)
            };

            var postulacion2 = new VolunteerApplication
            {
                Id = 2,
                UsuarioId = 1,
                OpportunityId = 2,
                Mensaje = "Quiero contribuir al medio ambiente",
                Estatus = ApplicationStatus.Aceptada,
                FechaAplicacion = DateTime.UtcNow.AddDays(-3),
                FechaRespuesta = DateTime.UtcNow.AddDays(-1)
            };

            var postulacion3 = new VolunteerApplication
            {
                Id = 3,
                UsuarioId = 2,
                OpportunityId = 1,
                Mensaje = "Tengo experiencia en el área de salud",
                Estatus = ApplicationStatus.Pendiente,
                FechaAplicacion = DateTime.UtcNow.AddDays(-2)
            };

            // Agregar datos al contexto
            _context.Usuarios.AddRange(usuario1, usuario2);
            _context.Organizaciones.Add(organizacion);
            _context.VolunteerOpportunities.AddRange(oportunidad1, oportunidad2);
            _context.VolunteerApplications.AddRange(postulacion1, postulacion2, postulacion3);

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetUserApplicationsAsync_ShouldReturnUserApplications()
        {
            // Arrange
            var userId = 1;

            // Act
            var result = await _service.GetUserApplicationsAsync(userId);

            // Assert
            Assert.NotNull(result);
            var applications = result.ToList();
            Assert.Equal(2, applications.Count);

            var application1 = applications.FirstOrDefault(a => a.OpportunityId == 1);
            Assert.NotNull(application1);
            Assert.Equal("Juan Pérez", application1.UsuarioNombre);
            Assert.Equal("juan.perez@test.com", application1.UsuarioEmail);
            Assert.Equal("Ayuda en Hospital", application1.OpportunityTitulo);
            Assert.Equal(ApplicationStatus.Pendiente, application1.Estatus);

            var application2 = applications.FirstOrDefault(a => a.OpportunityId == 2);
            Assert.NotNull(application2);
            Assert.Equal("Juan Pérez", application2.UsuarioNombre);
            Assert.Equal("Limpieza de Playa", application2.OpportunityTitulo);
            Assert.Equal(ApplicationStatus.Aceptada, application2.Estatus);
        }

        [Fact]
        public async Task GetUserApplicationsAsync_ShouldReturnEmptyList_WhenUserHasNoApplications()
        {
            // Arrange
            var userId = 999; // Usuario que no existe

            // Act
            var result = await _service.GetUserApplicationsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ApplyToOpportunityAsync_ShouldCreateNewApplication()
        {
            // Arrange
            var opportunityId = 2; // Oportunidad a la que el usuario 2 no ha aplicado
            var userId = 2;
            var applyDto = new ApplyToOpportunityDto
            {
                Mensaje = "Me interesa mucho esta oportunidad"
            };

            // Act
            var result = await _service.ApplyToOpportunityAsync(opportunityId, userId, applyDto);

            // Assert
            Assert.True(result);

            // Verificar que la aplicación se creó en la base de datos
            var application = await _context.VolunteerApplications
                .FirstOrDefaultAsync(a => a.OpportunityId == opportunityId && a.UsuarioId == userId);

            Assert.NotNull(application);
            Assert.Equal(ApplicationStatus.Pendiente, application.Estatus);
            Assert.Equal("Me interesa mucho esta oportunidad", application.Mensaje);
            Assert.Equal(DateTime.UtcNow.Date, application.FechaAplicacion.Date);

            // Verificar que el contador de voluntarios inscritos se incrementó
            var opportunity = await _context.VolunteerOpportunities.FindAsync(opportunityId);
            Assert.NotNull(opportunity);
            Assert.Equal(1, opportunity.VoluntariosInscritos);
        }

        [Fact]
        public async Task ApplyToOpportunityAsync_ShouldReturnFalse_WhenOpportunityDoesNotExist()
        {
            // Arrange
            var opportunityId = 999; // Oportunidad que no existe
            var userId = 1;

            // Act
            var result = await _service.ApplyToOpportunityAsync(opportunityId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ApplyToOpportunityAsync_ShouldReturnFalse_WhenUserAlreadyApplied()
        {
            // Arrange
            var opportunityId = 1;
            var userId = 1; // Usuario que ya aplicó a esta oportunidad

            // Act
            var result = await _service.ApplyToOpportunityAsync(opportunityId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ApplyToOpportunityAsync_ShouldReturnFalse_WhenOpportunityIsNotActive()
        {
            // Arrange
            var opportunityId = 1;
            var userId = 2;

            // Cambiar el estado de la oportunidad a no activa
            var opportunity = await _context.VolunteerOpportunities.FindAsync(opportunityId);
            opportunity!.Estatus = OpportunityStatus.Cerrada;
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ApplyToOpportunityAsync(opportunityId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_ShouldUpdateApplicationStatus()
        {
            // Arrange
            var applicationId = 1;
            var organizationId = 1;
            var newStatus = ApplicationStatus.Aceptada;
            var notes = "Excelente candidato, aceptado";

            // Act
            var result = await _service.UpdateApplicationStatusAsync(applicationId, newStatus, organizationId, notes);

            // Assert
            Assert.True(result);

            // Verificar que la aplicación se actualizó
            var application = await _context.VolunteerApplications.FindAsync(applicationId);
            Assert.NotNull(application);
            Assert.Equal(newStatus, application.Estatus);
            Assert.Equal(notes, application.NotasOrganizacion);
            Assert.NotNull(application.FechaRespuesta);
            Assert.Equal(DateTime.UtcNow.Date, application.FechaRespuesta.Value.Date);
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_ShouldReturnFalse_WhenApplicationDoesNotExist()
        {
            // Arrange
            var applicationId = 999; // Aplicación que no existe
            var organizationId = 1;
            var newStatus = ApplicationStatus.Aceptada;

            // Act
            var result = await _service.UpdateApplicationStatusAsync(applicationId, newStatus, organizationId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateApplicationStatusAsync_ShouldReturnFalse_WhenOrganizationDoesNotOwnOpportunity()
        {
            // Arrange
            var applicationId = 1;
            var organizationId = 999; // Organización que no es dueña de la oportunidad
            var newStatus = ApplicationStatus.Aceptada;

            // Act
            var result = await _service.UpdateApplicationStatusAsync(applicationId, newStatus, organizationId);

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
        }
    }
} 