using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public class OpportunityService : IOpportunityService
    {
        private readonly DbContextApplication _context;
        private readonly ILogger<OpportunityService> _logger;

        public OpportunityService(DbContextApplication context, ILogger<OpportunityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<OpportunityListDto>> GetAllOpportunitiesAsync()
        {
            return await _context.VolunteerOpportunities
                .Include(o => o.Organizacion)
                .Where(o => o.Estatus == OpportunityStatus.Activa)
                .OrderByDescending(o => o.FechaCreacion)
                .Select(o => new OpportunityListDto
                {
                    Id = o.Id,
                    Titulo = o.Titulo,
                    Descripcion = o.Descripcion,
                    Ubicacion = o.Ubicacion,
                    FechaInicio = o.FechaInicio,
                    FechaFin = o.FechaFin,
                    DuracionHoras = o.DuracionHoras,
                    VoluntariosRequeridos = o.VoluntariosRequeridos,
                    VoluntariosInscritos = o.VoluntariosInscritos,
                    AreaInteres = o.AreaInteres,
                    NivelExperiencia = o.NivelExperiencia,
                    Estatus = o.Estatus,
                    Organizacion = new OrganizacionBasicDto
                    {
                        Id = o.Organizacion.Id,
                        Nombre = o.Organizacion.Nombre,
                        Ubicacion = o.Organizacion.Direccion
                    }
                })
                .ToListAsync();
        }

        public async Task<OpportunityDetailDto?> GetOpportunityByIdAsync(int id)
        {
            return await _context.VolunteerOpportunities
                .Include(o => o.Organizacion)
                .Where(o => o.Id == id)
                .Select(o => new OpportunityDetailDto
                {
                    Id = o.Id,
                    Titulo = o.Titulo,
                    Descripcion = o.Descripcion,
                    Ubicacion = o.Ubicacion,
                    FechaInicio = o.FechaInicio,
                    FechaFin = o.FechaFin,
                    DuracionHoras = o.DuracionHoras,
                    VoluntariosRequeridos = o.VoluntariosRequeridos,
                    VoluntariosInscritos = o.VoluntariosInscritos,
                    AreaInteres = o.AreaInteres,
                    NivelExperiencia = o.NivelExperiencia,
                    Requisitos = o.Requisitos,
                    Beneficios = o.Beneficios,
                    Estatus = o.Estatus,
                    FechaCreacion = o.FechaCreacion,
                    Organizacion = new OrganizacionBasicDto
                    {
                        Id = o.Organizacion.Id,
                        Nombre = o.Organizacion.Nombre,
                        Ubicacion = o.Organizacion.Direccion
                    }
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<OpportunityListDto>> GetOpportunitiesByOrganizationAsync(int organizationId)
        {
            return await _context.VolunteerOpportunities
                .Include(o => o.Organizacion)
                .Where(o => o.OrganizacionId == organizationId)
                .OrderByDescending(o => o.FechaCreacion)
                .Select(o => new OpportunityListDto
                {
                    Id = o.Id,
                    Titulo = o.Titulo,
                    Descripcion = o.Descripcion,
                    Ubicacion = o.Ubicacion,
                    FechaInicio = o.FechaInicio,
                    FechaFin = o.FechaFin,
                    DuracionHoras = o.DuracionHoras,
                    VoluntariosRequeridos = o.VoluntariosRequeridos,
                    VoluntariosInscritos = o.VoluntariosInscritos,
                    AreaInteres = o.AreaInteres,
                    NivelExperiencia = o.NivelExperiencia,
                    Estatus = o.Estatus,
                    Organizacion = new OrganizacionBasicDto
                    {
                        Id = o.Organizacion.Id,
                        Nombre = o.Organizacion.Nombre,
                        Ubicacion = o.Organizacion.Direccion
                    }
                })
                .ToListAsync();
        }

        public async Task<OpportunityDetailDto> CreateOpportunityAsync(CreateOpportunityDto createDto, int organizationId)
        {
            var opportunity = new VolunteerOpportunity
            {
                Titulo = createDto.Titulo,
                Descripcion = createDto.Descripcion,
                Ubicacion = createDto.Ubicacion,
                FechaInicio = createDto.FechaInicio,
                FechaFin = createDto.FechaFin,
                DuracionHoras = createDto.DuracionHoras,
                VoluntariosRequeridos = createDto.VoluntariosRequeridos,
                AreaInteres = createDto.AreaInteres,
                NivelExperiencia = createDto.NivelExperiencia,
                Requisitos = createDto.Requisitos,
                Beneficios = createDto.Beneficios,
                OrganizacionId = organizationId,
                Estatus = OpportunityStatus.Activa,
                FechaCreacion = DateTime.UtcNow
            };

            _context.VolunteerOpportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            return await GetOpportunityByIdAsync(opportunity.Id) ?? throw new InvalidOperationException("Failed to create opportunity");
        }

        public async Task<OpportunityDetailDto?> UpdateOpportunityAsync(int id, UpdateOpportunityDto updateDto, int organizationId)
        {
            var opportunity = await _context.VolunteerOpportunities
                .FirstOrDefaultAsync(o => o.Id == id && o.OrganizacionId == organizationId);

            if (opportunity == null)
                return null;

            opportunity.Titulo = updateDto.Titulo;
            opportunity.Descripcion = updateDto.Descripcion;
            opportunity.Ubicacion = updateDto.Ubicacion;
            opportunity.FechaInicio = updateDto.FechaInicio;
            opportunity.FechaFin = updateDto.FechaFin;
            opportunity.DuracionHoras = updateDto.DuracionHoras;
            opportunity.VoluntariosRequeridos = updateDto.VoluntariosRequeridos;
            opportunity.AreaInteres = updateDto.AreaInteres;
            opportunity.NivelExperiencia = updateDto.NivelExperiencia;
            opportunity.Requisitos = updateDto.Requisitos;
            opportunity.Beneficios = updateDto.Beneficios;
            opportunity.FechaActualizacion = DateTime.UtcNow;

            if (updateDto.Estatus.HasValue)
                opportunity.Estatus = updateDto.Estatus.Value;

            await _context.SaveChangesAsync();

            return await GetOpportunityByIdAsync(id);
        }

        public async Task<bool> DeleteOpportunityAsync(int id, int organizationId)
        {
            var opportunity = await _context.VolunteerOpportunities
                .FirstOrDefaultAsync(o => o.Id == id && o.OrganizacionId == organizationId);

            if (opportunity == null)
                return false;

            opportunity.Estatus = OpportunityStatus.Cerrada;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ApplyToOpportunityAsync(int opportunityId, int userId, ApplyToOpportunityDto? applyDto = null)
        {
            var opportunity = await _context.VolunteerOpportunities
                .FirstOrDefaultAsync(o => o.Id == opportunityId && o.Estatus == OpportunityStatus.Activa);

            if (opportunity == null)
                return false;

            var existingApplication = await _context.VolunteerApplications
                .FirstOrDefaultAsync(a => a.OpportunityId == opportunityId && a.UsuarioId == userId);

            if (existingApplication != null)
                return false; // User already applied

            var application = new VolunteerApplication
            {
                UsuarioId = userId,
                OpportunityId = opportunityId,
                Mensaje = applyDto?.Mensaje,
                Estatus = ApplicationStatus.Pendiente,
                FechaAplicacion = DateTime.UtcNow
            };

            _context.VolunteerApplications.Add(application);

            // Update enrolled count
            opportunity.VoluntariosInscritos++;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ApplicationDto>> GetApplicationsForOpportunityAsync(int opportunityId, int organizationId)
        {
            return await _context.VolunteerApplications
                .Include(a => a.Usuario)
                .Include(a => a.Opportunity)
                .Where(a => a.OpportunityId == opportunityId && a.Opportunity.OrganizacionId == organizationId)
                .Select(a => new ApplicationDto
                {
                    Id = a.Id,
                    OpportunityId = a.OpportunityId,
                    OpportunityTitulo = a.Opportunity.Titulo,
                    UsuarioNombre = $"{a.Usuario.Nombre} {a.Usuario.Apellido}",
                    UsuarioEmail = a.Usuario.Email,
                    Mensaje = a.Mensaje,
                    Estatus = a.Estatus,
                    FechaAplicacion = a.FechaAplicacion,
                    FechaRespuesta = a.FechaRespuesta,
                    NotasOrganizacion = a.NotasOrganizacion
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationDto>> GetUserApplicationsAsync(int userId)
        {
            return await _context.VolunteerApplications
                .Include(a => a.Usuario)
                .Include(a => a.Opportunity)
                .Where(a => a.UsuarioId == userId)
                .Select(a => new ApplicationDto
                {
                    Id = a.Id,
                    OpportunityId = a.OpportunityId,
                    OpportunityTitulo = a.Opportunity.Titulo,
                    UsuarioNombre = $"{a.Usuario.Nombre} {a.Usuario.Apellido}",
                    UsuarioEmail = a.Usuario.Email,
                    Mensaje = a.Mensaje,
                    Estatus = a.Estatus,
                    FechaAplicacion = a.FechaAplicacion,
                    FechaRespuesta = a.FechaRespuesta,
                    NotasOrganizacion = a.NotasOrganizacion
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus status, int organizationId, string? notes = null)
        {
            var application = await _context.VolunteerApplications
                .Include(a => a.Opportunity)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.Opportunity.OrganizacionId == organizationId);

            if (application == null)
                return false;

            application.Estatus = status;
            application.FechaRespuesta = DateTime.UtcNow;
            application.NotasOrganizacion = notes;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}