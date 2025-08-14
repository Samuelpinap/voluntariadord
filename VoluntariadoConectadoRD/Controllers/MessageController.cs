using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IMessageService messageService, ILogger<MessageController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// Send a message to another user
        /// </summary>
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponseDto<MessageDto>>> SendMessage([FromForm] SendMessageDto messageDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int senderId))
                {
                    return BadRequest(new ApiResponseDto<MessageDto>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var message = await _messageService.SendMessageAsync(senderId, messageDto);

                return Ok(new ApiResponseDto<MessageDto>
                {
                    Success = true,
                    Message = "Mensaje enviado",
                    Data = message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDto<MessageDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new ApiResponseDto<MessageDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get user's conversations
        /// </summary>
        [HttpGet("conversations")]
        public async Task<ActionResult<ApiResponseDto<ConversationListDto>>> GetConversations(int page = 1, int pageSize = 20)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<ConversationListDto>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var conversations = await _messageService.GetUserConversationsAsync(userId, page, pageSize);
                
                _logger.LogInformation("Returning {ConversationCount} conversations for user {UserId}", 
                    conversations?.Conversations?.Count ?? 0, userId);

                return Ok(new ApiResponseDto<ConversationListDto>
                {
                    Success = true,
                    Message = "Conversaciones obtenidas",
                    Data = conversations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations");
                return StatusCode(500, new ApiResponseDto<ConversationListDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get messages from a specific conversation
        /// </summary>
        [HttpGet("conversation/{conversationId}/messages")]
        public async Task<ActionResult<ApiResponseDto<ConversationMessagesDto>>> GetConversationMessages(
            string conversationId, int page = 1, int pageSize = 50)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<ConversationMessagesDto>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var messages = await _messageService.GetConversationMessagesAsync(userId, conversationId, page, pageSize);

                return Ok(new ApiResponseDto<ConversationMessagesDto>
                {
                    Success = true,
                    Message = "Mensajes obtenidos",
                    Data = messages
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDto<ConversationMessagesDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation messages");
                return StatusCode(500, new ApiResponseDto<ConversationMessagesDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Start a new conversation
        /// </summary>
        [HttpPost("conversation/start")]
        public async Task<ActionResult<ApiResponseDto<ConversationDto>>> StartConversation([FromBody] StartConversationDto startDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int senderId))
                {
                    return BadRequest(new ApiResponseDto<ConversationDto>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var conversation = await _messageService.StartConversationAsync(senderId, startDto);

                return Ok(new ApiResponseDto<ConversationDto>
                {
                    Success = true,
                    Message = "Conversación iniciada",
                    Data = conversation
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDto<ConversationDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting conversation");
                return StatusCode(500, new ApiResponseDto<ConversationDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Mark messages in a conversation as read
        /// </summary>
        [HttpPut("conversation/{conversationId}/read")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkMessagesAsRead(string conversationId)
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

                var result = await _messageService.MarkMessagesAsReadAsync(userId, conversationId);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Mensajes marcados como leídos",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Edit a message
        /// </summary>
        [HttpPut("{messageId}")]
        public async Task<ActionResult<ApiResponseDto<MessageDto>>> EditMessage(int messageId, [FromBody] EditMessageDto editDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<MessageDto>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var message = await _messageService.EditMessageAsync(messageId, userId, editDto);

                return Ok(new ApiResponseDto<MessageDto>
                {
                    Success = true,
                    Message = "Mensaje editado",
                    Data = message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponseDto<MessageDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message");
                return StatusCode(500, new ApiResponseDto<MessageDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Delete a message
        /// </summary>
        [HttpDelete("{messageId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteMessage(int messageId)
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

                var result = await _messageService.DeleteMessageAsync(messageId, userId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Mensaje no encontrado"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Mensaje eliminado",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Get conversation statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponseDto<ConversationStatsDto>>> GetConversationStats()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new ApiResponseDto<ConversationStatsDto>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                var stats = await _messageService.GetConversationStatsAsync(userId);

                return Ok(new ApiResponseDto<ConversationStatsDto>
                {
                    Success = true,
                    Message = "Estadísticas obtenidas",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation stats");
                return StatusCode(500, new ApiResponseDto<ConversationStatsDto>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Archive a conversation
        /// </summary>
        [HttpPut("conversation/{conversationId}/archive")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ArchiveConversation(string conversationId)
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

                var result = await _messageService.ArchiveConversationAsync(userId, conversationId);

                if (!result)
                {
                    return NotFound(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Conversación no encontrada"
                    });
                }

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Conversación archivada",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving conversation");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }

        /// <summary>
        /// Send typing indicator
        /// </summary>
        [HttpPost("typing")]
        public async Task<ActionResult<ApiResponseDto<bool>>> SendTypingIndicator([FromBody] TypingIndicatorDto typingDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int senderId))
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no válido"
                    });
                }

                // Extract recipient ID from conversation ID
                var conversationParts = typingDto.ConversationId.Split('_');
                if (conversationParts.Length != 2 || 
                    !int.TryParse(conversationParts[0], out int user1Id) || 
                    !int.TryParse(conversationParts[1], out int user2Id))
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "ID de conversación inválido"
                    });
                }

                var recipientId = user1Id == senderId ? user2Id : user1Id;

                await _messageService.NotifyTyping(senderId, recipientId, typingDto.ConversationId, typingDto.IsTyping);

                return Ok(new ApiResponseDto<bool>
                {
                    Success = true,
                    Message = "Indicador de escritura enviado",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing indicator");
                return StatusCode(500, new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                });
            }
        }
    }
}