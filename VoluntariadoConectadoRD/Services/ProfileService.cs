using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRd.Services
{
    public class ProfileService : IProfileService
    {
        private readonly DbContextApplication _context;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(DbContextApplication context, ILogger<ProfileService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios
                    .Include(u => u.ResenasRecibidas)
                        .ThenInclude(ur => ur.Organizacion)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return null;

                return new UserProfileDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    Email = user.Email,
                    Telefono = user.Telefono,
                    FechaNacimiento = user.FechaNacimiento,
                    Biografia = user.Biografia,
                    Habilidades = user.Habilidades?.Split(',').ToList() ?? new List<string>(),
                    ExperienciaAnios = user.ExperienciaAnios ?? 0,
                    Disponibilidad = user.Disponibilidad,
                    ProfileImageUrl = user.ProfileImageUrl,
                    PerfilCompleto = user.PerfilCompleto,
                    TotalResenas = user.ResenasRecibidas?.Count ?? 0,
                    CalificacionPromedio = user.ResenasRecibidas?.Any() == true 
                        ? user.ResenasRecibidas.Average(r => r.Calificacion) 
                        : 0,
                    UltimasResenas = user.ResenasRecibidas?
                        .OrderByDescending(r => r.FechaCreacion)
                        .Take(5)
                        .Select(r => new UserReviewDto
                        {
                            OrganizacionNombre = r.Organizacion?.Nombre ?? "Organización desconocida",
                            Calificacion = r.Calificacion,
                            Comentario = r.Comentario,
                            FechaCreacion = r.FechaCreacion
                        }).ToList() ?? new List<UserReviewDto>(),
                    Badges = new List<BadgeDto>() // TODO: Implement badges system
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil del usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<OrganizationProfileDto?> GetOrganizationProfileAsync(int orgId)
        {
            try
            {
                var organization = await _context.Organizaciones
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.Id == orgId);

                if (organization == null) return null;

                return new OrganizationProfileDto
                {
                    Id = organization.Id,
                    Nombre = organization.Nombre,
                    Descripcion = organization.Descripcion,
                    Direccion = organization.Direccion,
                    Telefono = organization.Telefono,
                    Email = organization.Email,
                    SitioWeb = organization.SitioWeb,
                    TipoOrganizacion = organization.TipoOrganizacion,
                    NumeroRegistro = organization.NumeroRegistro,
                    FechaFundacion = organization.FechaFundacion,
                    Mision = organization.Mision,
                    Vision = organization.Vision,
                    AreasInteres = organization.AreasInteres?.Split(',').ToList() ?? new List<string>(),
                    LogoUrl = organization.LogoUrl,
                    PerfilCompleto = organization.PerfilCompleto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil de la organización {OrgId}", orgId);
                throw;
            }
        }

        public async Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado");

                // Update fields
                if (!string.IsNullOrEmpty(updateDto.Nombre))
                    user.Nombre = updateDto.Nombre;
                
                if (!string.IsNullOrEmpty(updateDto.Apellido))
                    user.Apellido = updateDto.Apellido;
                
                if (!string.IsNullOrEmpty(updateDto.Telefono))
                    user.Telefono = updateDto.Telefono;
                
                if (updateDto.FechaNacimiento.HasValue)
                    user.FechaNacimiento = updateDto.FechaNacimiento.Value;
                
                if (!string.IsNullOrEmpty(updateDto.Biografia))
                    user.Biografia = updateDto.Biografia;
                
                if (updateDto.Habilidades?.Any() == true)
                    user.Habilidades = string.Join(",", updateDto.Habilidades);
                
                if (updateDto.ExperienciaAnios.HasValue)
                    user.ExperienciaAnios = updateDto.ExperienciaAnios.Value;
                
                if (!string.IsNullOrEmpty(updateDto.Disponibilidad))
                    user.Disponibilidad = updateDto.Disponibilidad;

                // Update profile completion
                UpdateUserProfileCompletion(user);

                await _context.SaveChangesAsync();

                return await GetUserProfileAsync(userId) ?? throw new InvalidOperationException("Error al obtener el perfil actualizado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil del usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<OrganizationProfileDto> UpdateOrganizationProfileAsync(int userId, UpdateOrganizationProfileDto updateDto)
        {
            try
            {
                var organization = await _context.Organizaciones
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.Usuario.Id == userId);

                if (organization == null)
                    throw new ArgumentException("Organización no encontrada para este usuario");

                // Update fields
                if (!string.IsNullOrEmpty(updateDto.Nombre))
                    organization.Nombre = updateDto.Nombre;
                
                if (!string.IsNullOrEmpty(updateDto.Descripcion))
                    organization.Descripcion = updateDto.Descripcion;
                
                if (!string.IsNullOrEmpty(updateDto.Direccion))
                    organization.Direccion = updateDto.Direccion;
                
                if (!string.IsNullOrEmpty(updateDto.Telefono))
                    organization.Telefono = updateDto.Telefono;
                
                if (!string.IsNullOrEmpty(updateDto.SitioWeb))
                    organization.SitioWeb = updateDto.SitioWeb;
                
                if (!string.IsNullOrEmpty(updateDto.TipoOrganizacion))
                    organization.TipoOrganizacion = updateDto.TipoOrganizacion;
                
                if (updateDto.FechaFundacion.HasValue)
                    organization.FechaFundacion = updateDto.FechaFundacion;
                
                if (!string.IsNullOrEmpty(updateDto.Mision))
                    organization.Mision = updateDto.Mision;
                
                if (!string.IsNullOrEmpty(updateDto.Vision))
                    organization.Vision = updateDto.Vision;
                
                if (updateDto.AreasInteres?.Any() == true)
                    organization.AreasInteres = string.Join(",", updateDto.AreasInteres);

                // Update profile completion
                UpdateOrganizationProfileCompletion(organization);

                await _context.SaveChangesAsync();

                return await GetOrganizationProfileAsync(organization.Id) ?? throw new InvalidOperationException("Error al obtener el perfil actualizado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil de la organización para el usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<ProfileCompletionDto> GetUserProfileCompletionAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado");

                return CalculateUserProfileCompletion(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular completitud del perfil del usuario {UserId}", userId);
                throw;
            }
        }

        public async Task<ProfileCompletionDto> GetOrganizationProfileCompletionAsync(int orgId)
        {
            try
            {
                var organization = await _context.Organizaciones.FindAsync(orgId);
                if (organization == null)
                    throw new ArgumentException("Organización no encontrada");

                return CalculateOrganizationProfileCompletion(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular completitud del perfil de la organización {OrgId}", orgId);
                throw;
            }
        }

        private ProfileCompletionDto CalculateUserProfileCompletion(Usuario user)
        {
            var fields = new List<(string name, bool isComplete)>
            {
                ("Nombre", !string.IsNullOrEmpty(user.Nombre)),
                ("Apellido", !string.IsNullOrEmpty(user.Apellido)),
                ("Email", !string.IsNullOrEmpty(user.Email)),
                ("Teléfono", !string.IsNullOrEmpty(user.Telefono)),
                ("Fecha de Nacimiento", user.FechaNacimiento.HasValue),
                ("Biografía", !string.IsNullOrEmpty(user.Biografia)),
                ("Habilidades", !string.IsNullOrEmpty(user.Habilidades)),
                ("Experiencia", user.ExperienciaAnios.HasValue && user.ExperienciaAnios > 0),
                ("Disponibilidad", !string.IsNullOrEmpty(user.Disponibilidad)),
                ("Foto de Perfil", !string.IsNullOrEmpty(user.ProfileImageUrl))
            };

            var completedFields = fields.Count(f => f.isComplete);
            var totalFields = fields.Count;
            var percentage = (int)Math.Round((double)completedFields / totalFields * 100);

            return new ProfileCompletionDto
            {
                Percentage = percentage,
                IsComplete = percentage >= 80, // Consider complete if 80% or more fields are filled
                CompletedFields = completedFields,
                TotalFields = totalFields,
                MissingFields = fields.Where(f => !f.isComplete).Select(f => f.name).ToList()
            };
        }

        private ProfileCompletionDto CalculateOrganizationProfileCompletion(Organizacion organization)
        {
            var fields = new List<(string name, bool isComplete)>
            {
                ("Nombre", !string.IsNullOrEmpty(organization.Nombre)),
                ("Descripción", !string.IsNullOrEmpty(organization.Descripcion)),
                ("Dirección", !string.IsNullOrEmpty(organization.Direccion)),
                ("Teléfono", !string.IsNullOrEmpty(organization.Telefono)),
                ("Email", !string.IsNullOrEmpty(organization.Email)),
                ("Sitio Web", !string.IsNullOrEmpty(organization.SitioWeb)),
                ("Tipo de Organización", !string.IsNullOrEmpty(organization.TipoOrganizacion)),
                ("Número de Registro", !string.IsNullOrEmpty(organization.NumeroRegistro)),
                ("Fecha de Fundación", organization.FechaFundacion.HasValue),
                ("Misión", !string.IsNullOrEmpty(organization.Mision)),
                ("Visión", !string.IsNullOrEmpty(organization.Vision)),
                ("Áreas de Interés", !string.IsNullOrEmpty(organization.AreasInteres)),
                ("Logo", !string.IsNullOrEmpty(organization.LogoUrl))
            };

            var completedFields = fields.Count(f => f.isComplete);
            var totalFields = fields.Count;
            var percentage = (int)Math.Round((double)completedFields / totalFields * 100);

            return new ProfileCompletionDto
            {
                Percentage = percentage,
                IsComplete = percentage >= 80, // Consider complete if 80% or more fields are filled
                CompletedFields = completedFields,
                TotalFields = totalFields,
                MissingFields = fields.Where(f => !f.isComplete).Select(f => f.name).ToList()
            };
        }

        // Fixed: Removed async/await as suggested by Copilot AI
        private void UpdateUserProfileCompletion(Usuario user)
        {
            var completion = CalculateUserProfileCompletion(user);
            user.PerfilCompleto = completion.IsComplete;
        }

        // Fixed: Removed async/await as suggested by Copilot AI
        private void UpdateOrganizationProfileCompletion(Organizacion organization)
        {
            var completion = CalculateOrganizationProfileCompletion(organization);
            organization.PerfilCompleto = completion.IsComplete;
        }
    }
}