using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Interfaces;
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

        public async Task<IEnumerable<OpportunityListDto>> GetAllOpportunitiesAsync(
            string? searchTerm = null,
            string? areaInteres = null,
            string? ubicacion = null,
            OpportunityStatus? status = null)
        {
            var query = _context.VolunteerOpportunities
                .Include(o => o.Organizacion)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o => o.Titulo.Contains(searchTerm) || o.Descripcion.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(areaInteres))
            {
                query = query.Where(o => o.AreaInteres == areaInteres);
            }

            if (!string.IsNullOrEmpty(ubicacion))
            {
                query = query.Where(o => o.Ubicacion != null && o.Ubicacion.Contains(ubicacion));
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Estatus == status.Value);
            }
            else
            {
                query = query.Where(o => o.Estatus == OpportunityStatus.Activa);
            }

            return await query
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

        public async Task<IEnumerable<OpportunityListDto>> GetOrganizationOpportunitiesAsync(int organizacionId)
        {
            return await _context.VolunteerOpportunities
                .Include(o => o.Organizacion)
                .Where(o => o.OrganizacionId == organizacionId)
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

        public async Task<ApiResponseDto<OpportunityDetailDto>> CreateOpportunityAsync(CreateOpportunityDto dto, int organizacionId)
        {
            try
            {
                var opportunity = new VolunteerOpportunity
                {
                    Titulo = dto.Titulo,
                    Descripcion = dto.Descripcion,
                    Ubicacion = dto.Ubicacion,
                    FechaInicio = dto.FechaInicio,
                    FechaFin = dto.FechaFin,
                    DuracionHoras = dto.DuracionHoras,
                    VoluntariosRequeridos = dto.VoluntariosRequeridos,
                    AreaInteres = dto.AreaInteres,
                    NivelExperiencia = dto.NivelExperiencia,
                    Requisitos = dto.Requisitos,
                    Beneficios = dto.Beneficios,
                    OrganizacionId = organizacionId,
                    Estatus = OpportunityStatus.Activa,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.VolunteerOpportunities.Add(opportunity);
                await _context.SaveChangesAsync();

                var createdOpportunity = await GetOpportunityByIdAsync(opportunity.Id);
                
                return new ApiResponseDto<OpportunityDetailDto>
                {
                    Success = true,
                    Message = "Oportunidad creada exitosamente",
                    Data = createdOpportunity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating opportunity");
                return new ApiResponseDto<OpportunityDetailDto>
                {
                    Success = false,
                    Message = "Error al crear la oportunidad"
                };
            }
        }

        public async Task<ApiResponseDto<OpportunityDetailDto>> UpdateOpportunityAsync(int id, UpdateOpportunityDto dto, int organizacionId)
        {
            try
            {
                var opportunity = await _context.VolunteerOpportunities
                    .FirstOrDefaultAsync(o => o.Id == id && o.OrganizacionId == organizacionId);

                if (opportunity == null)
                {
                    return new ApiResponseDto<OpportunityDetailDto>
                    {
                        Success = false,
                        Message = "Oportunidad no encontrada o no pertenece a la organización"
                    };
                }

                opportunity.Titulo = dto.Titulo;
                opportunity.Descripcion = dto.Descripcion;
                opportunity.Ubicacion = dto.Ubicacion;
                opportunity.FechaInicio = dto.FechaInicio;
                opportunity.FechaFin = dto.FechaFin;
                opportunity.DuracionHoras = dto.DuracionHoras;
                opportunity.VoluntariosRequeridos = dto.VoluntariosRequeridos;
                opportunity.AreaInteres = dto.AreaInteres;
                opportunity.NivelExperiencia = dto.NivelExperiencia;
                opportunity.Requisitos = dto.Requisitos;
                opportunity.Beneficios = dto.Beneficios;
                opportunity.FechaActualizacion = DateTime.UtcNow;

                if (dto.Estatus.HasValue)
                    opportunity.Estatus = dto.Estatus.Value;

                await _context.SaveChangesAsync();

                var updatedOpportunity = await GetOpportunityByIdAsync(id);
                
                return new ApiResponseDto<OpportunityDetailDto>
                {
                    Success = true,
                    Message = "Oportunidad actualizada exitosamente",
                    Data = updatedOpportunity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating opportunity {Id}", id);
                return new ApiResponseDto<OpportunityDetailDto>
                {
                    Success = false,
                    Message = "Error al actualizar la oportunidad"
                };
            }
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

        public async Task<bool> ApplyToOpportunityAsync(int opportunityId, int volunteerId, ApplyToOpportunityDto? applicationDto = null)
        {
            var opportunity = await _context.VolunteerOpportunities
                .FirstOrDefaultAsync(o => o.Id == opportunityId && o.Estatus == OpportunityStatus.Activa);

            if (opportunity == null)
                return false;

            var existingApplication = await _context.VolunteerApplications
                .FirstOrDefaultAsync(a => a.OpportunityId == opportunityId && a.UsuarioId == volunteerId);

            if (existingApplication != null)
                return false; // User already applied

            var application = new VolunteerApplication
            {
                UsuarioId = volunteerId,
                OpportunityId = opportunityId,
                Mensaje = applicationDto?.Mensaje,
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

        public async Task<IEnumerable<ApplicationDto>> GetVolunteerApplicationsAsync(int volunteerId)
        {
            return await _context.VolunteerApplications
                .Include(a => a.Usuario)
                .Include(a => a.Opportunity)
                .Where(a => a.UsuarioId == volunteerId)
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

        public async Task<IEnumerable<ApplicationDto>> GetOrganizationApplicationsAsync(int organizacionId)
        {
            return await _context.VolunteerApplications
                .Include(a => a.Usuario)
                .Include(a => a.Opportunity)
                .Where(a => a.Opportunity.OrganizacionId == organizacionId)
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

        public async Task<bool> UpdateApplicationStatusAsync(int applicationId, ApplicationStatus status, string? notes = null)
        {
            var application = await _context.VolunteerApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                return false;

            application.Estatus = status;
            application.FechaRespuesta = DateTime.UtcNow;
            application.NotasOrganizacion = notes;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int?> GetOrganizacionIdByUserAsync(int userId)
        {
            var organizacion = await _context.Organizaciones
                .Where(o => o.UsuarioId == userId)
                .Select(o => o.Id)
                .FirstOrDefaultAsync();

            return organizacion == 0 ? null : organizacion;
        }
    }
}