using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Models;
using Microsoft.EntityFrameworkCore;

namespace VoluntariadoConectadoRD.Services
{
    public interface ISearchService
    {
        Task<SearchResultDto<VolunteerOpportunity>> SearchOpportunitiesAsync(OpportunitySearchDto searchDto);
        Task<SearchResultDto<Usuario>> SearchVolunteersAsync(VolunteerSearchDto searchDto);
        Task<SearchResultDto<Organizacion>> SearchOrganizationsAsync(OrganizationSearchDto searchDto);
        Task<QuickSearchResultDto> QuickSearchAsync(QuickSearchDto searchDto);
        Task<SearchFilters> GetSearchFiltersAsync(string type);
    }

    public class SearchService : ISearchService
    {
        private readonly DbContextApplication _context;
        private readonly ILogger<SearchService> _logger;

        public SearchService(DbContextApplication context, ILogger<SearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SearchResultDto<VolunteerOpportunity>> SearchOpportunitiesAsync(OpportunitySearchDto searchDto)
        {
            try
            {
                var query = _context.VolunteerOpportunities
                    .Include(o => o.Organizacion)
                    .Include(o => o.VolunteerApplications)
                    .Where(o => o.Estado == "Activo");

                // Apply search filters
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    var term = searchDto.SearchTerm.ToLower();
                    query = query.Where(o => 
                        o.Titulo.ToLower().Contains(term) || 
                        o.Descripcion.ToLower().Contains(term) ||
                        o.Organizacion.Nombre.ToLower().Contains(term));
                }

                if (!string.IsNullOrEmpty(searchDto.Category))
                {
                    query = query.Where(o => o.Categoria == searchDto.Category);
                }

                if (!string.IsNullOrEmpty(searchDto.Location))
                {
                    query = query.Where(o => o.Ubicacion.ToLower().Contains(searchDto.Location.ToLower()));
                }

                if (searchDto.StartDate.HasValue)
                {
                    query = query.Where(o => o.FechaInicio >= searchDto.StartDate.Value);
                }

                if (searchDto.EndDate.HasValue)
                {
                    query = query.Where(o => o.FechaInicio <= searchDto.EndDate.Value);
                }

                if (searchDto.MinDuration.HasValue)
                {
                    query = query.Where(o => o.DuracionHoras >= searchDto.MinDuration.Value);
                }

                if (searchDto.MaxDuration.HasValue)
                {
                    query = query.Where(o => o.DuracionHoras <= searchDto.MaxDuration.Value);
                }

                // Apply sorting
                query = searchDto.SortBy?.ToLower() switch
                {
                    "title" => searchDto.SortOrder == "Asc" ? 
                        query.OrderBy(o => o.Titulo) : 
                        query.OrderByDescending(o => o.Titulo),
                    "duration" => searchDto.SortOrder == "Asc" ? 
                        query.OrderBy(o => o.DuracionHoras) : 
                        query.OrderByDescending(o => o.DuracionHoras),
                    "volunteers" => searchDto.SortOrder == "Asc" ? 
                        query.OrderBy(o => o.VoluntariosRequeridos) : 
                        query.OrderByDescending(o => o.VoluntariosRequeridos),
                    _ => searchDto.SortOrder == "Asc" ? 
                        query.OrderBy(o => o.FechaInicio) : 
                        query.OrderByDescending(o => o.FechaInicio)
                };

                var totalCount = await query.CountAsync();
                var results = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return new SearchResultDto<VolunteerOpportunity>
                {
                    Results = results,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching opportunities");
                throw;
            }
        }

        public async Task<SearchResultDto<Usuario>> SearchVolunteersAsync(VolunteerSearchDto searchDto)
        {
            try
            {
                var query = _context.Usuarios
                    .Where(u => u.Rol == 1 && u.Estado == 1); // Voluntarios activos

                // Apply search filters
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    var term = searchDto.SearchTerm.ToLower();
                    query = query.Where(u => 
                        u.Nombre.ToLower().Contains(term) || 
                        u.Apellido.ToLower().Contains(term) ||
                        u.Email.ToLower().Contains(term));
                }

                if (!string.IsNullOrEmpty(searchDto.Location))
                {
                    query = query.Where(u => u.Ubicacion != null && u.Ubicacion.ToLower().Contains(searchDto.Location.ToLower()));
                }

                if (!string.IsNullOrEmpty(searchDto.Availability))
                {
                    query = query.Where(u => u.Disponibilidad == searchDto.Availability);
                }

                if (searchDto.MinExperience.HasValue)
                {
                    query = query.Where(u => u.ExperienciaAnios >= searchDto.MinExperience.Value);
                }

                if (searchDto.MaxExperience.HasValue)
                {
                    query = query.Where(u => u.ExperienciaAnios <= searchDto.MaxExperience.Value);
                }

                if (searchDto.IsActive.HasValue)
                {
                    var statusFilter = searchDto.IsActive.Value ? 1 : 2;
                    query = query.Where(u => u.Estado == statusFilter);
                }

                // Apply sorting
                query = searchDto.SortBy?.ToLower() switch
                {
                    "name" => searchDto.SortOrder == "Asc" ? 
                        query.OrderBy(u => u.Nombre).ThenBy(u => u.Apellido) : 
                        query.OrderByDescending(u => u.Nombre).ThenByDescending(u => u.Apellido),
                    "experience" => searchDto.SortOrder == "Asc" ? 
                        query.OrderBy(u => u.ExperienciaAnios) : 
                        query.OrderByDescending(u => u.ExperienciaAnios),
                    "joindate" => searchDto.SortOrder == "Asc" ? 
                        query.OrderBy(u => u.FechaCreacion) : 
                        query.OrderByDescending(u => u.FechaCreacion),
                    _ => searchDto.SortOrder == "Asc" ? 
                        query.OrderBy(u => u.Nombre) : 
                        query.OrderByDescending(u => u.Nombre)
                };

                var totalCount = await query.CountAsync();
                var results = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return new SearchResultDto<Usuario>
                {
                    Results = results,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching volunteers");
                throw;
            }
        }

        public async Task<SearchResultDto<Organizacion>> SearchOrganizationsAsync(OrganizationSearchDto searchDto)
        {
            try
            {
                var query = _context.Organizaciones
                    .Include(o => o.Usuario)
                    .Where(o => o.Usuario.Estado == 1); // Organizaciones activas

                // Apply search filters
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    var term = searchDto.SearchTerm.ToLower();
                    query = query.Where(o => 
                        o.Nombre.ToLower().Contains(term) || 
                        o.Descripcion.ToLower().Contains(term));
                }

                if (!string.IsNullOrEmpty(searchDto.Location))
                {
                    query = query.Where(o => o.Direccion.ToLower().Contains(searchDto.Location.ToLower()));
                }

                if (!string.IsNullOrEmpty(searchDto.Category))
                {
                    query = query.Where(o => o.TipoOrganizacion == searchDto.Category);
                }

                if (searchDto.IsVerified.HasValue)
                {
                    query = query.Where(o => o.EsVerificada == searchDto.IsVerified.Value);
                }

                // Apply sorting
                query = searchDto.SortBy?.ToLower() switch
                {
                    "createddate" => searchDto.SortOrder == "Asc" ? 
                        query.OrderBy(o => o.FechaCreacion) : 
                        query.OrderByDescending(o => o.FechaCreacion),
                    _ => searchDto.SortOrder == "Asc" ? 
                        query.OrderBy(o => o.Nombre) : 
                        query.OrderByDescending(o => o.Nombre)
                };

                var totalCount = await query.CountAsync();
                var results = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return new SearchResultDto<Organizacion>
                {
                    Results = results,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching organizations");
                throw;
            }
        }

        public async Task<QuickSearchResultDto> QuickSearchAsync(QuickSearchDto searchDto)
        {
            try
            {
                var result = new QuickSearchResultDto();
                var term = searchDto.Query.ToLower();

                if (searchDto.Type == "all" || searchDto.Type == "opportunities")
                {
                    var opportunities = await _context.VolunteerOpportunities
                        .Include(o => o.Organizacion)
                        .Where(o => o.Estado == "Activo" && 
                               (o.Titulo.ToLower().Contains(term) || 
                                o.Descripcion.ToLower().Contains(term)))
                        .Take(searchDto.Limit)
                        .Select(o => new SearchSuggestion
                        {
                            Id = o.Id,
                            Title = o.Titulo,
                            Subtitle = o.Organizacion.Nombre,
                            Type = "opportunity",
                            Url = $"/Events/Details/{o.Id}",
                            Location = o.Ubicacion,
                            Date = o.FechaInicio
                        })
                        .ToListAsync();

                    result.Opportunities = opportunities;
                }

                if (searchDto.Type == "all" || searchDto.Type == "volunteers")
                {
                    var volunteers = await _context.Usuarios
                        .Where(u => u.Rol == 1 && u.Estado == 1 &&
                               (u.Nombre.ToLower().Contains(term) || 
                                u.Apellido.ToLower().Contains(term)))
                        .Take(searchDto.Limit)
                        .Select(u => new SearchSuggestion
                        {
                            Id = u.Id,
                            Title = $"{u.Nombre} {u.Apellido}",
                            Subtitle = $"{u.ExperienciaAnios} años de experiencia",
                            ImageUrl = u.ImagenUrl,
                            Type = "volunteer",
                            Url = $"/Dashboard/VolunteerDetails/{u.Id}",
                            Location = u.Ubicacion
                        })
                        .ToListAsync();

                    result.Volunteers = volunteers;
                }

                if (searchDto.Type == "all" || searchDto.Type == "organizations")
                {
                    var organizations = await _context.Organizaciones
                        .Include(o => o.Usuario)
                        .Where(o => o.Usuario.Estado == 1 &&
                               (o.Nombre.ToLower().Contains(term) || 
                                o.Descripcion.ToLower().Contains(term)))
                        .Take(searchDto.Limit)
                        .Select(o => new SearchSuggestion
                        {
                            Id = o.Id,
                            Title = o.Nombre,
                            Subtitle = o.TipoOrganizacion,
                            ImageUrl = o.LogoUrl,
                            Type = "organization",
                            Url = $"/Organizations/Details/{o.Id}",
                            Location = o.Direccion
                        })
                        .ToListAsync();

                    result.Organizations = organizations;
                }

                result.TotalResults = result.Opportunities.Count + result.Volunteers.Count + result.Organizations.Count;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing quick search");
                throw;
            }
        }

        public async Task<SearchFilters> GetSearchFiltersAsync(string type)
        {
            try
            {
                var filters = new SearchFilters();

                switch (type.ToLower())
                {
                    case "opportunities":
                        // Categories
                        filters.Categories = await _context.VolunteerOpportunities
                            .Where(o => o.Estado == "Activo" && !string.IsNullOrEmpty(o.Categoria))
                            .GroupBy(o => o.Categoria)
                            .Select(g => new FilterOption
                            {
                                Value = g.Key,
                                Label = g.Key,
                                Count = g.Count()
                            })
                            .OrderBy(f => f.Label)
                            .ToListAsync();

                        // Locations
                        filters.Locations = await _context.VolunteerOpportunities
                            .Where(o => o.Estado == "Activo" && !string.IsNullOrEmpty(o.Ubicacion))
                            .GroupBy(o => o.Ubicacion)
                            .Select(g => new FilterOption
                            {
                                Value = g.Key,
                                Label = g.Key,
                                Count = g.Count()
                            })
                            .OrderBy(f => f.Label)
                            .ToListAsync();

                        // Organizations
                        filters.Organizations = await _context.VolunteerOpportunities
                            .Include(o => o.Organizacion)
                            .Where(o => o.Estado == "Activo")
                            .GroupBy(o => new { o.Organizacion.Id, o.Organizacion.Nombre })
                            .Select(g => new FilterOption
                            {
                                Value = g.Key.Id.ToString(),
                                Label = g.Key.Nombre,
                                Count = g.Count()
                            })
                            .OrderBy(f => f.Label)
                            .ToListAsync();

                        // Duration range
                        var durationStats = await _context.VolunteerOpportunities
                            .Where(o => o.Estado == "Activo")
                            .GroupBy(o => 1)
                            .Select(g => new { 
                                Min = g.Min(o => o.DuracionHoras), 
                                Max = g.Max(o => o.DuracionHoras) 
                            })
                            .FirstOrDefaultAsync();

                        if (durationStats != null)
                        {
                            filters.DurationRange = new NumberRange
                            {
                                Min = durationStats.Min,
                                Max = durationStats.Max
                            };
                        }

                        break;

                    case "volunteers":
                        // Experience range
                        var experienceStats = await _context.Usuarios
                            .Where(u => u.Rol == 1 && u.Estado == 1)
                            .GroupBy(u => 1)
                            .Select(g => new { 
                                Min = g.Min(u => u.ExperienciaAnios), 
                                Max = g.Max(u => u.ExperienciaAnios) 
                            })
                            .FirstOrDefaultAsync();

                        if (experienceStats != null)
                        {
                            filters.ExperienceRange = new NumberRange
                            {
                                Min = experienceStats.Min,
                                Max = experienceStats.Max
                            };
                        }

                        // Locations
                        filters.Locations = await _context.Usuarios
                            .Where(u => u.Rol == 1 && u.Estado == 1 && !string.IsNullOrEmpty(u.Ubicacion))
                            .GroupBy(u => u.Ubicacion)
                            .Select(g => new FilterOption
                            {
                                Value = g.Key,
                                Label = g.Key,
                                Count = g.Count()
                            })
                            .OrderBy(f => f.Label)
                            .ToListAsync();

                        break;

                    case "organizations":
                        // Categories
                        filters.Categories = await _context.Organizaciones
                            .Include(o => o.Usuario)
                            .Where(o => o.Usuario.Estado == 1 && !string.IsNullOrEmpty(o.TipoOrganizacion))
                            .GroupBy(o => o.TipoOrganizacion)
                            .Select(g => new FilterOption
                            {
                                Value = g.Key,
                                Label = g.Key,
                                Count = g.Count()
                            })
                            .OrderBy(f => f.Label)
                            .ToListAsync();

                        // Locations
                        filters.Locations = await _context.Organizaciones
                            .Include(o => o.Usuario)
                            .Where(o => o.Usuario.Estado == 1 && !string.IsNullOrEmpty(o.Direccion))
                            .GroupBy(o => o.Direccion)
                            .Select(g => new FilterOption
                            {
                                Value = g.Key,
                                Label = g.Key,
                                Count = g.Count()
                            })
                            .OrderBy(f => f.Label)
                            .ToListAsync();

                        break;
                }

                return filters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search filters for type: {Type}", type);
                throw;
            }
        }
    }
}