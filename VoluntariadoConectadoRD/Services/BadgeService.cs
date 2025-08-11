using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public interface IBadgeService
    {
        Task<List<BadgeDto>> GetAllBadgesAsync();
        Task<List<BadgeDto>> GetUserBadgesAsync(int userId);
        Task<BadgeDto> CreateBadgeAsync(CreateBadgeDto createBadgeDto);
        Task<BadgeDto> UpdateBadgeAsync(int badgeId, UpdateBadgeDto updateBadgeDto);
        Task<bool> DeleteBadgeAsync(int badgeId);
        Task<bool> AwardBadgeToUserAsync(int userId, int badgeId, string? reason = null);
        Task<bool> RevokeBadgeFromUserAsync(int userId, int badgeId);
        Task<List<BadgeDto>> GetAvailableBadgesForUserAsync(int userId);
        Task CheckAndAwardAutomaticBadgesAsync(int userId);
        Task<BadgeStatsDto> GetBadgeStatsAsync();
        Task<List<UserBadgeDto>> GetBadgeHoldersAsync(int badgeId, int page = 1, int pageSize = 20);
    }

    public class BadgeService : IBadgeService
    {
        private readonly DbContextApplication _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<BadgeService> _logger;

        public BadgeService(
            DbContextApplication context,
            INotificationService notificationService,
            ILogger<BadgeService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<List<BadgeDto>> GetAllBadgesAsync()
        {
            try
            {
                var badges = await _context.Badges
                    .Include(b => b.UsuarioBadges)
                    .OrderBy(b => b.Categoria)
                    .ThenBy(b => b.Nombre)
                    .ToListAsync();

                return badges.Select(MapToBadgeDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all badges");
                throw;
            }
        }

        public async Task<List<BadgeDto>> GetUserBadgesAsync(int userId)
        {
            try
            {
                var userBadges = await _context.UsuarioBadges
                    .Include(ub => ub.Badge)
                    .Where(ub => ub.UsuarioId == userId)
                    .OrderByDescending(ub => ub.FechaObtenido)
                    .ToListAsync();

                return userBadges.Select(ub => new BadgeDto
                {
                    Id = ub.Badge.Id,
                    Nombre = ub.Badge.Nombre,
                    Descripcion = ub.Badge.Descripcion,
                    IconoUrl = ub.Badge.IconoUrl,
                    Color = ub.Badge.Color,
                    Categoria = ub.Badge.Categoria,
                    Requisitos = ub.Badge.Requisitos,
                    EsAutomatico = ub.Badge.EsAutomatico,
                    EsActivo = ub.Badge.EsActivo,
                    FechaCreacion = ub.Badge.FechaCreacion,
                    TotalOtorgados = 0, // Will be calculated separately if needed
                    FechaObtenido = ub.FechaObtenido,
                    RazonOtorgamiento = ub.RazonOtorgamiento
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user badges for user {UserId}", userId);
                throw;
            }
        }

        public async Task<BadgeDto> CreateBadgeAsync(CreateBadgeDto createBadgeDto)
        {
            try
            {
                var badge = new Badge
                {
                    Nombre = createBadgeDto.Nombre,
                    Descripcion = createBadgeDto.Descripcion,
                    IconoUrl = createBadgeDto.IconoUrl,
                    Color = createBadgeDto.Color,
                    Categoria = createBadgeDto.Categoria,
                    Requisitos = createBadgeDto.Requisitos,
                    EsAutomatico = createBadgeDto.EsAutomatico,
                    EsActivo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Badges.Add(badge);
                await _context.SaveChangesAsync();

                return MapToBadgeDto(badge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating badge");
                throw;
            }
        }

        public async Task<BadgeDto> UpdateBadgeAsync(int badgeId, UpdateBadgeDto updateBadgeDto)
        {
            try
            {
                var badge = await _context.Badges.FindAsync(badgeId);
                if (badge == null)
                {
                    throw new ArgumentException("Badge not found");
                }

                badge.Nombre = updateBadgeDto.Nombre;
                badge.Descripcion = updateBadgeDto.Descripcion;
                badge.IconoUrl = updateBadgeDto.IconoUrl;
                badge.Color = updateBadgeDto.Color;
                badge.Categoria = updateBadgeDto.Categoria;
                badge.Requisitos = updateBadgeDto.Requisitos;
                badge.EsAutomatico = updateBadgeDto.EsAutomatico;
                badge.EsActivo = updateBadgeDto.EsActivo;

                await _context.SaveChangesAsync();

                return MapToBadgeDto(badge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating badge {BadgeId}", badgeId);
                throw;
            }
        }

        public async Task<bool> DeleteBadgeAsync(int badgeId)
        {
            try
            {
                var badge = await _context.Badges
                    .Include(b => b.UsuarioBadges)
                    .FirstOrDefaultAsync(b => b.Id == badgeId);

                if (badge == null)
                {
                    return false;
                }

                // Remove all user badges first
                _context.UsuarioBadges.RemoveRange(badge.UsuarioBadges);
                
                // Remove the badge
                _context.Badges.Remove(badge);
                
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting badge {BadgeId}", badgeId);
                throw;
            }
        }

        public async Task<bool> AwardBadgeToUserAsync(int userId, int badgeId, string? reason = null)
        {
            try
            {
                // Check if user already has this badge
                var existingBadge = await _context.UsuarioBadges
                    .FirstOrDefaultAsync(ub => ub.UsuarioId == userId && ub.BadgeId == badgeId);

                if (existingBadge != null)
                {
                    return false; // User already has this badge
                }

                // Get badge and user info
                var badge = await _context.Badges.FindAsync(badgeId);
                var user = await _context.Usuarios.FindAsync(userId);

                if (badge == null || user == null)
                {
                    return false;
                }

                // Award the badge
                var userBadge = new UsuarioBadge
                {
                    UsuarioId = userId,
                    BadgeId = badgeId,
                    FechaObtenido = DateTime.UtcNow,
                    RazonOtorgamiento = reason
                };

                _context.UsuarioBadges.Add(userBadge);
                await _context.SaveChangesAsync();

                // Send notification
                var notification = new CreateNotificationDto
                {
                    RecipientId = userId,
                    Title = "¡Nueva insignia obtenida!",
                    Message = $"Has obtenido la insignia '{badge.Nombre}'. ¡Felicidades!",
                    Type = NotificationTypes.BADGE_EARNED,
                    ActionUrl = "/Profile/Badges",
                    Priority = NotificationPriority.Normal
                };

                await _notificationService.CreateNotificationAsync(notification);

                _logger.LogInformation("Badge {BadgeId} awarded to user {UserId}", badgeId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error awarding badge {BadgeId} to user {UserId}", badgeId, userId);
                throw;
            }
        }

        public async Task<bool> RevokeBadgeFromUserAsync(int userId, int badgeId)
        {
            try
            {
                var userBadge = await _context.UsuarioBadges
                    .FirstOrDefaultAsync(ub => ub.UsuarioId == userId && ub.BadgeId == badgeId);

                if (userBadge == null)
                {
                    return false;
                }

                _context.UsuarioBadges.Remove(userBadge);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Badge {BadgeId} revoked from user {UserId}", badgeId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking badge {BadgeId} from user {UserId}", badgeId, userId);
                throw;
            }
        }

        public async Task<List<BadgeDto>> GetAvailableBadgesForUserAsync(int userId)
        {
            try
            {
                // Get badges the user doesn't have yet
                var userBadgeIds = await _context.UsuarioBadges
                    .Where(ub => ub.UsuarioId == userId)
                    .Select(ub => ub.BadgeId)
                    .ToListAsync();

                var availableBadges = await _context.Badges
                    .Where(b => b.EsActivo && !userBadgeIds.Contains(b.Id))
                    .OrderBy(b => b.Categoria)
                    .ThenBy(b => b.Nombre)
                    .ToListAsync();

                return availableBadges.Select(MapToBadgeDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available badges for user {UserId}", userId);
                throw;
            }
        }

        public async Task CheckAndAwardAutomaticBadgesAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios
                    .Include(u => u.UsuarioBadges)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return;

                // Get automatic badges the user doesn't have
                var userBadgeIds = user.UsuarioBadges.Select(ub => ub.BadgeId).ToList();
                var automaticBadges = await _context.Badges
                    .Where(b => b.EsAutomatico && b.EsActivo && !userBadgeIds.Contains(b.Id))
                    .ToListAsync();

                foreach (var badge in automaticBadges)
                {
                    var shouldAward = await CheckBadgeRequirements(userId, badge);
                    if (shouldAward)
                    {
                        await AwardBadgeToUserAsync(userId, badge.Id, "Obtenido automáticamente");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking automatic badges for user {UserId}", userId);
            }
        }

        public async Task<BadgeStatsDto> GetBadgeStatsAsync()
        {
            try
            {
                var totalBadges = await _context.Badges.CountAsync(b => b.EsActivo);
                var totalAwarded = await _context.UsuarioBadges.CountAsync();
                var uniqueHolders = await _context.UsuarioBadges
                    .Select(ub => ub.UsuarioId)
                    .Distinct()
                    .CountAsync();

                var categoryStats = await _context.Badges
                    .Where(b => b.EsActivo)
                    .GroupBy(b => b.Categoria)
                    .Select(g => new BadgeCategoryStatsDto
                    {
                        Categoria = g.Key,
                        TotalBadges = g.Count(),
                        TotalAwarded = g.Sum(b => b.UsuarioBadges.Count)
                    })
                    .ToListAsync();

                var topBadges = await _context.Badges
                    .Where(b => b.EsActivo)
                    .Select(b => new BadgePopularityDto
                    {
                        BadgeId = b.Id,
                        Nombre = b.Nombre,
                        IconoUrl = b.IconoUrl,
                        TimesAwarded = b.UsuarioBadges.Count
                    })
                    .OrderByDescending(b => b.TimesAwarded)
                    .Take(10)
                    .ToListAsync();

                return new BadgeStatsDto
                {
                    TotalBadges = totalBadges,
                    TotalAwarded = totalAwarded,
                    UniqueHolders = uniqueHolders,
                    CategoryStats = categoryStats,
                    TopBadges = topBadges
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting badge stats");
                throw;
            }
        }

        public async Task<List<UserBadgeDto>> GetBadgeHoldersAsync(int badgeId, int page = 1, int pageSize = 20)
        {
            try
            {
                var badgeHolders = await _context.UsuarioBadges
                    .Include(ub => ub.Usuario)
                    .Include(ub => ub.Badge)
                    .Where(ub => ub.BadgeId == badgeId)
                    .OrderByDescending(ub => ub.FechaObtenido)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return badgeHolders.Select(ub => new UserBadgeDto
                {
                    UsuarioId = ub.UsuarioId,
                    BadgeId = ub.BadgeId,
                    Usuario = new UserBasicDto
                    {
                        Id = ub.Usuario.Id,
                        Nombre = ub.Usuario.Nombre,
                        Apellido = ub.Usuario.Apellido,
                        ImagenUrl = ub.Usuario.ImagenUrl
                    },
                    Badge = MapToBadgeDto(ub.Badge),
                    FechaObtenido = ub.FechaObtenido,
                    RazonOtorgamiento = ub.RazonOtorgamiento
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting badge holders for badge {BadgeId}", badgeId);
                throw;
            }
        }

        private async Task<bool> CheckBadgeRequirements(int userId, Badge badge)
        {
            // This is where we implement specific badge requirements
            // For now, we'll implement some common badge types

            return badge.Nombre switch
            {
                "Primer Voluntario" => await CheckFirstVolunteerBadge(userId),
                "Veterano" => await CheckVeteranBadge(userId),
                "Dedicado" => await CheckDedicatedBadge(userId),
                "Comunicador" => await CheckCommunicatorBadge(userId),
                "Líder Comunitario" => await CheckCommunityLeaderBadge(userId),
                _ => false
            };
        }

        private async Task<bool> CheckFirstVolunteerBadge(int userId)
        {
            // Award to users who completed their first volunteer activity
            var completedActivities = await _context.VolunteerApplications
                .CountAsync(va => va.VolunteerId == userId && va.Estado == (int)ApplicationStatus.Completado);

            return completedActivities >= 1;
        }

        private async Task<bool> CheckVeteranBadge(int userId)
        {
            // Award to users who have been volunteers for more than 1 year
            var user = await _context.Usuarios.FindAsync(userId);
            if (user == null) return false;

            var oneYearAgo = DateTime.UtcNow.AddYears(-1);
            return user.FechaCreacion <= oneYearAgo;
        }

        private async Task<bool> CheckDedicatedBadge(int userId)
        {
            // Award to users who completed 10+ volunteer activities
            var completedActivities = await _context.VolunteerApplications
                .CountAsync(va => va.VolunteerId == userId && va.Estado == (int)ApplicationStatus.Completado);

            return completedActivities >= 10;
        }

        private async Task<bool> CheckCommunicatorBadge(int userId)
        {
            // Award to users who sent 50+ messages
            var messageCount = await _context.Messages
                .CountAsync(m => m.SenderId == userId);

            return messageCount >= 50;
        }

        private async Task<bool> CheckCommunityLeaderBadge(int userId)
        {
            // Award to organizations that created 5+ opportunities
            var opportunityCount = await _context.VolunteerOpportunities
                .Include(vo => vo.Organizacion)
                .CountAsync(vo => vo.Organizacion.UsuarioId == userId);

            return opportunityCount >= 5;
        }

        private BadgeDto MapToBadgeDto(Badge badge)
        {
            return new BadgeDto
            {
                Id = badge.Id,
                Nombre = badge.Nombre,
                Descripcion = badge.Descripcion,
                IconoUrl = badge.IconoUrl,
                Color = badge.Color,
                Categoria = badge.Categoria,
                Requisitos = badge.Requisitos,
                EsAutomatico = badge.EsAutomatico,
                EsActivo = badge.EsActivo,
                FechaCreacion = badge.FechaCreacion,
                TotalOtorgados = badge.UsuarioBadges?.Count ?? 0
            };
        }
    }
}