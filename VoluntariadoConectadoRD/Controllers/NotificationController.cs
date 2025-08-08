using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoluntariadoConectadoRD.Attributes;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService, 
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get user's notifications with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<NotificationListDto>>> GetNotifications(int page = 1, int pageSize = 20)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<NotificationListDto>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);

                return Ok(new ApiResponseDto<NotificationListDto>
                {
                    Success = true,
                    Message = "Notificaciones obtenidas",
                    Data = notifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, new ApiResponseDto<NotificationListDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get unread notifications count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponseDto<int>>> GetUnreadCount()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<int>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var count = await _notificationService.GetUnreadNotificationsCountAsync(userId);

                return Ok(new ApiResponseDto<int>
                {
                    Success = true,
                    Message = "Conteo de notificaciones no leídas",
                    Data = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new ApiResponseDto<int>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPut("{notificationId}/read")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAsRead(int notificationId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Notificación no encontrada"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Notificación marcada como leída",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPut("read-all")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAllAsRead()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                await _notificationService.MarkAllNotificationsAsReadAsync(userId);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Todas las notificaciones marcadas como leídas",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{notificationId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteNotification(int notificationId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var result = await _notificationService.DeleteNotificationAsync(notificationId, userId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Notificación no encontrada"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Notificación eliminada",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Create a notification - Admin/Organization only
        /// </summary>
        [HttpPost]
        [OrganizacionOrAdmin]
        public async Task<ActionResult<ApiResponseDto<NotificationDto>>> CreateNotification([FromBody] CreateNotificationDto notificationDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int senderId))
                {
                    notificationDto.SenderId = senderId;
                }

                var notification = await _notificationService.CreateNotificationAsync(notificationDto);

                return Ok(new ApiResponseDto<NotificationDto>
                {
                    Success = true,
                    Message = "Notificación creada",
                    Data = notification
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(500, new ApiResponseDto<NotificationDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Create bulk notifications - Admin/Organization only
        /// </summary>
        [HttpPost("bulk")]
        [OrganizacionOrAdmin]
        public async Task<ActionResult<ApiResponseDto<List<NotificationDto>>>> CreateBulkNotification([FromBody] BulkNotificationDto notificationDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int senderId))
                {
                    notificationDto.SenderId = senderId;
                }

                var notifications = await _notificationService.CreateBulkNotificationAsync(notificationDto);

                return Ok(new ApiResponseDto<List<NotificationDto>>
                {
                    Success = true,
                    Message = $"{notifications.Count} notificaciones creadas",
                    Data = notifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk notifications");
                return StatusCode(500, new ApiResponseDto<List<NotificationDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get user online status
        /// </summary>
        [HttpGet("online-status/{userId}")]
        public async Task<ActionResult<ApiResponseDto<OnlineStatusDto>>> GetOnlineStatus(int userId)
        {
            try
            {
                var status = await _notificationService.GetUserOnlineStatus(userId);

                return Ok(new ApiResponseDto<OnlineStatusDto>
                {
                    Success = true,
                    Message = "Estado de conexión obtenido",
                    Data = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online status for user {UserId}", userId);
                return StatusCode(500, new ApiResponseDto<OnlineStatusDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get all online users - Admin only
        /// </summary>
        [HttpGet("online-users")]
        [AdminOnly]
        public async Task<ActionResult<ApiResponseDto<List<OnlineStatusDto>>>> GetOnlineUsers()
        {
            try
            {
                var onlineUsers = await _notificationService.GetOnlineUsers();

                return Ok(new ApiResponseDto<List<OnlineStatusDto>>
                {
                    Success = true,
                    Message = "Usuarios en línea obtenidos",
                    Data = onlineUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users");
                return StatusCode(500, new ApiResponseDto<List<OnlineStatusDto>>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }
    }
}