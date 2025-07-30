using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
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

        #region User Profile Methods

        public async Task<ApiResponseDto<UserProfileDto>> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new ApiResponseDto<UserProfileDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado",
                        Data = null
                    };
                }

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    Email = user.Email,
                    Telefono = user.Telefono,
                    Direccion = user.Direccion,
                    FechaNacimiento = user.FechaNacimiento,
                    Avatar = user.Avatar,
                    Biografia = user.Biografia,
                    Intereses = user.Intereses,
                    Habilidades = user.Habilidades,
                    Disponibilidad = user.Disponibilidad,
                    ExperienciaPrevia = user.ExperienciaPrevia,
                    Ubicacion = user.Ubicacion,
                    PerfilCompleto = user.PerfilCompleto,
                    FechaCreacion = user.FechaCreacion,
                    FechaActualizacion = user.FechaActualizacion
                };

                return new ApiResponseDto<UserProfileDto>
                {
                    Success = true,
                    Message = "Perfil de usuario obtenido exitosamente",
                    Data = userProfile
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for userId: {UserId}", userId);
                return new ApiResponseDto<UserProfileDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<UserProfileDto>> UpdateUserProfileAsync(int userId, UpdateUserProfileDto dto)
        {
            try
            {
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new ApiResponseDto<UserProfileDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado",
                        Data = null
                    };
                }

                // Update profile fields
                user.Telefono = dto.Telefono ?? user.Telefono;
                user.Direccion = dto.Direccion ?? user.Direccion;
                user.Biografia = dto.Biografia ?? user.Biografia;
                user.Intereses = dto.Intereses ?? user.Intereses;
                user.Habilidades = dto.Habilidades ?? user.Habilidades;
                user.Disponibilidad = dto.Disponibilidad ?? user.Disponibilidad;
                user.ExperienciaPrevia = dto.ExperienciaPrevia ?? user.ExperienciaPrevia;
                user.Ubicacion = dto.Ubicacion ?? user.Ubicacion;
                user.FechaActualizacion = DateTime.UtcNow;

                // Check and update profile completion
                await UpdateUserProfileCompletionAsync(user);

                await _context.SaveChangesAsync();

                // Return updated profile
                return await GetUserProfileAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile for userId: {UserId}", userId);
                return new ApiResponseDto<UserProfileDto>
                {
                    Success = false,
                    Message = "Error al actualizar el perfil",
                    Data = null
                };
            }
        }

        #endregion

        #region Organization Profile Methods

        public async Task<ApiResponseDto<OrganizationProfileDto>> GetOrganizationProfileAsync(int organizationId)
        {
            try
            {
                var organization = await _context.Organizaciones
                    .Include(o => o.Usuario)
                    .FirstOrDefaultAsync(o => o.Id == organizationId);

                if (organization == null)
                {
                    return new ApiResponseDto<OrganizationProfileDto>
                    {
                        Success = false,
                        Message = "Organización no encontrada",
                        Data = null
                    };
                }

                var orgProfile = new OrganizationProfileDto
                {
                    Id = organization.Id,
                    Nombre = organization.Nombre,
                    Descripcion = organization.Descripcion,
                    Email = organization.Email,
                    Telefono = organization.Telefono,
                    Direccion = organization.Direccion,
                    SitioWeb = organization.SitioWeb,
                    NumeroRegistro = organization.NumeroRegistro,
                    Logo = organization.Logo,
                    Mision = organization.Mision,
                    Vision = organization.Vision,
                    AreasEnfoque = organization.AreasEnfoque,
                    PersonaContacto = organization.PersonaContacto,
                    CargoContacto = organization.CargoContacto,
                    TelefonoContacto = organization.TelefonoContacto,
                    PerfilCompleto = organization.PerfilCompleto,
                    Estatus = organization.Estatus,
                    FechaCreacion = organization.FechaCreacion,
                    FechaActualizacion = organization.FechaActualizacion,
                    UsuarioAdministrador = organization.Usuario != null ? new UserInfoDto
                    {
                        Id = organization.Usuario.Id,
                        Nombre = organization.Usuario.Nombre,
                        Apellido = organization.Usuario.Apellido,
                        Email = organization.Usuario.Email,
                        Telefono = organization.Usuario.Telefono,
                        Rol = organization.Usuario.Rol,
                        Estatus = organization.Usuario.Estatus
                    } : null
                };

                return new ApiResponseDto<OrganizationProfileDto>
                {
                    Success = true,
                    Message = "Perfil de organización obtenido exitosamente",
                    Data = orgProfile
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization profile for organizationId: {OrganizationId}", organizationId);
                return new ApiResponseDto<OrganizationProfileDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<OrganizationProfileDto>> UpdateOrganizationProfileAsync(int organizationId, UpdateOrganizationProfileDto dto)
        {
            try
            {
                var organization = await _context.Organizaciones
                    .FirstOrDefaultAsync(o => o.Id == organizationId);

                if (organization == null)
                {
                    return new ApiResponseDto<OrganizationProfileDto>
                    {
                        Success = false,
                        Message = "Organización no encontrada",
                        Data = null
                    };
                }

                // Update profile fields
                organization.Descripcion = dto.Descripcion ?? organization.Descripcion;
                organization.Telefono = dto.Telefono ?? organization.Telefono;
                organization.Direccion = dto.Direccion ?? organization.Direccion;
                organization.SitioWeb = dto.SitioWeb ?? organization.SitioWeb;
                organization.NumeroRegistro = dto.NumeroRegistro ?? organization.NumeroRegistro;
                organization.Mision = dto.Mision ?? organization.Mision;
                organization.Vision = dto.Vision ?? organization.Vision;
                organization.AreasEnfoque = dto.AreasEnfoque ?? organization.AreasEnfoque;
                organization.PersonaContacto = dto.PersonaContacto ?? organization.PersonaContacto;
                organization.CargoContacto = dto.CargoContacto ?? organization.CargoContacto;
                organization.TelefonoContacto = dto.TelefonoContacto ?? organization.TelefonoContacto;
                organization.FechaActualizacion = DateTime.UtcNow;

                // Check and update profile completion
                await UpdateOrganizationProfileCompletionAsync(organization);

                await _context.SaveChangesAsync();

                // Return updated profile
                return await GetOrganizationProfileAsync(organizationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization profile for organizationId: {OrganizationId}", organizationId);
                return new ApiResponseDto<OrganizationProfileDto>
                {
                    Success = false,
                    Message = "Error al actualizar el perfil de la organización",
                    Data = null
                };
            }
        }

        #endregion

        #region Profile Completion Methods

        public async Task<ApiResponseDto<ProfileCompletionDto>> CheckUserProfileCompletionAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new ApiResponseDto<ProfileCompletionDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado",
                        Data = null
                    };
                }

                var completion = CalculateUserProfileCompletion(user);

                return new ApiResponseDto<ProfileCompletionDto>
                {
                    Success = true,
                    Message = "Análisis de completitud del perfil realizado",
                    Data = completion
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user profile completion for userId: {UserId}", userId);
                return new ApiResponseDto<ProfileCompletionDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<ProfileCompletionDto>> CheckOrganizationProfileCompletionAsync(int organizationId)
        {
            try
            {
                var organization = await _context.Organizaciones
                    .FirstOrDefaultAsync(o => o.Id == organizationId);

                if (organization == null)
                {
                    return new ApiResponseDto<ProfileCompletionDto>
                    {
                        Success = false,
                        Message = "Organización no encontrada",
                        Data = null
                    };
                }

                var completion = CalculateOrganizationProfileCompletion(organization);

                return new ApiResponseDto<ProfileCompletionDto>
                {
                    Success = true,
                    Message = "Análisis de completitud del perfil realizado",
                    Data = completion
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking organization profile completion for organizationId: {OrganizationId}", organizationId);
                return new ApiResponseDto<ProfileCompletionDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Data = null
                };
            }
        }

        #endregion

        #region Utility Methods

        public async Task<ApiResponseDto<bool>> UpdateUserAvatarAsync(int userId, string avatarUrl)
        {
            try
            {
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no encontrado",
                        Data = false
                    };
                }

                user.Avatar = avatarUrl;
                user.FechaActualizacion = DateTime.UtcNow;

                // Update profile completion
                await UpdateUserProfileCompletionAsync(user);

                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Avatar actualizado exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user avatar for userId: {UserId}", userId);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error al actualizar el avatar",
                    Data = false
                };
            }
        }

        public async Task<ApiResponseDto<bool>> UpdateOrganizationLogoAsync(int organizationId, string logoUrl)
        {
            try
            {
                var organization = await _context.Organizaciones
                    .FirstOrDefaultAsync(o => o.Id == organizationId);

                if (organization == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Organización no encontrada",
                        Data = false
                    };
                }

                organization.Logo = logoUrl;
                organization.FechaActualizacion = DateTime.UtcNow;

                // Update profile completion
                await UpdateOrganizationProfileCompletionAsync(organization);

                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Logo actualizado exitosamente",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization logo for organizationId: {OrganizationId}", organizationId);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error al actualizar el logo",
                    Data = false
                };
            }
        }

        #endregion

        #region Private Helper Methods

        private ProfileCompletionDto CalculateUserProfileCompletion(Usuario user)
        {
            var missingFields = new List<string>();
            var suggestions = new List<string>();
            int totalFields = 11; // Total number of profile fields to check
            int completedFields = 0;

            // Check required basic fields
            if (!string.IsNullOrEmpty(user.Telefono)) completedFields++;
            else { missingFields.Add("Telefono"); suggestions.Add("Agrega tu número de teléfono para que las organizaciones puedan contactarte"); }

            if (!string.IsNullOrEmpty(user.Direccion)) completedFields++;
            else { missingFields.Add("Direccion"); suggestions.Add("Incluye tu dirección para oportunidades locales"); }

            if (!string.IsNullOrEmpty(user.Avatar)) completedFields++;
            else { missingFields.Add("Avatar"); suggestions.Add("Sube una foto de perfil para personalizar tu cuenta"); }

            if (!string.IsNullOrEmpty(user.Biografia)) completedFields++;
            else { missingFields.Add("Biografia"); suggestions.Add("Escribe una breve biografía sobre ti"); }

            if (!string.IsNullOrEmpty(user.Intereses)) completedFields++;
            else { missingFields.Add("Intereses"); suggestions.Add("Comparte tus intereses para encontrar oportunidades afines"); }

            if (!string.IsNullOrEmpty(user.Habilidades)) completedFields++;
            else { missingFields.Add("Habilidades"); suggestions.Add("Lista tus habilidades relevantes"); }

            if (!string.IsNullOrEmpty(user.Disponibilidad)) completedFields++;
            else { missingFields.Add("Disponibilidad"); suggestions.Add("Indica tu disponibilidad de tiempo"); }

            if (!string.IsNullOrEmpty(user.ExperienciaPrevia)) completedFields++;
            else { missingFields.Add("ExperienciaPrevia"); suggestions.Add("Describe tu experiencia previa en voluntariado"); }

            if (!string.IsNullOrEmpty(user.Ubicacion)) completedFields++;
            else { missingFields.Add("Ubicacion"); suggestions.Add("Especifica tu ubicación para encontrar oportunidades cercanas"); }

            // Additional checks
            if (user.FechaNacimiento != default(DateTime)) completedFields++;
            else { missingFields.Add("FechaNacimiento"); suggestions.Add("Confirma tu fecha de nacimiento"); }

            // Basic info check (always complete for registered users)
            if (!string.IsNullOrEmpty(user.Nombre) && !string.IsNullOrEmpty(user.Email)) completedFields++;

            int percentage = (int)Math.Round((double)completedFields / totalFields * 100);
            bool isComplete = percentage >= 80; // Consider 80% as complete

            return new ProfileCompletionDto
            {
                IsComplete = isComplete,
                CompletionPercentage = percentage,
                MissingFields = missingFields,
                Suggestions = suggestions
            };
        }

        private ProfileCompletionDto CalculateOrganizationProfileCompletion(Organizacion organization)
        {
            var missingFields = new List<string>();
            var suggestions = new List<string>();
            int totalFields = 12; // Total number of profile fields to check
            int completedFields = 0;

            // Check basic required fields
            if (!string.IsNullOrEmpty(organization.Descripcion)) completedFields++;
            else { missingFields.Add("Descripcion"); suggestions.Add("Agrega una descripción de tu organización"); }

            if (!string.IsNullOrEmpty(organization.Telefono)) completedFields++;
            else { missingFields.Add("Telefono"); suggestions.Add("Incluye un número de contacto principal"); }

            if (!string.IsNullOrEmpty(organization.Direccion)) completedFields++;
            else { missingFields.Add("Direccion"); suggestions.Add("Proporciona la dirección de tu organización"); }

            if (!string.IsNullOrEmpty(organization.SitioWeb)) completedFields++;
            else { missingFields.Add("SitioWeb"); suggestions.Add("Comparte el sitio web de tu organización"); }

            if (!string.IsNullOrEmpty(organization.Logo)) completedFields++;
            else { missingFields.Add("Logo"); suggestions.Add("Sube el logo de tu organización"); }

            if (!string.IsNullOrEmpty(organization.Mision)) completedFields++;
            else { missingFields.Add("Mision"); suggestions.Add("Define la misión de tu organización"); }

            if (!string.IsNullOrEmpty(organization.Vision)) completedFields++;
            else { missingFields.Add("Vision"); suggestions.Add("Comparte la visión de tu organización"); }

            if (!string.IsNullOrEmpty(organization.AreasEnfoque)) completedFields++;
            else { missingFields.Add("AreasEnfoque"); suggestions.Add("Especifica las áreas de enfoque de tu trabajo"); }

            if (!string.IsNullOrEmpty(organization.PersonaContacto)) completedFields++;
            else { missingFields.Add("PersonaContacto"); suggestions.Add("Indica una persona de contacto"); }

            if (!string.IsNullOrEmpty(organization.CargoContacto)) completedFields++;
            else { missingFields.Add("CargoContacto"); suggestions.Add("Especifica el cargo de la persona de contacto"); }

            if (!string.IsNullOrEmpty(organization.TelefonoContacto)) completedFields++;
            else { missingFields.Add("TelefonoContacto"); suggestions.Add("Proporciona un teléfono de contacto directo"); }

            // Basic info (always complete for registered organizations)
            if (!string.IsNullOrEmpty(organization.Nombre) && !string.IsNullOrEmpty(organization.Email)) completedFields++;

            int percentage = (int)Math.Round((double)completedFields / totalFields * 100);
            bool isComplete = percentage >= 75; // Consider 75% as complete for organizations

            return new ProfileCompletionDto
            {
                IsComplete = isComplete,
                CompletionPercentage = percentage,
                MissingFields = missingFields,
                Suggestions = suggestions
            };
        }

        private async Task UpdateUserProfileCompletionAsync(Usuario user)
        {
            var completion = CalculateUserProfileCompletion(user);
            user.PerfilCompleto = completion.IsComplete;
        }

        private async Task UpdateOrganizationProfileCompletionAsync(Organizacion organization)
        {
            var completion = CalculateOrganizationProfileCompletion(organization);
            organization.PerfilCompleto = completion.IsComplete;
        }

        #endregion
    }
}