using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public class OpportunitiesService : IOportunidadService
    {
        private readonly DbContextApplication _context;
        private readonly ILogger<OpportunitiesService> _logger;

        public OpportunitiesService(DbContextApplication context, ILogger<OpportunitiesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<OpportunityListDto>> GetAllOportunidadesAsync()
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

        public async Task<OpportunityDetailDto?> GetOpportunidadByIdAsync(int id)
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
    }
}