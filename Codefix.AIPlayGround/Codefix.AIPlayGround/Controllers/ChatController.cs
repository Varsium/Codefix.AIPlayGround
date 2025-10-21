using Microsoft.AspNetCore.Mvc;
using Codefix.AIPlayGround.Models.DTOs;
using Codefix.AIPlayGround.Services;
using System.Security.Claims;

namespace Codefix.AIPlayGround.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatService chatService,
        ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Start a new chat session with an agent
    /// </summary>
    [HttpPost("sessions")]
    public async Task<ActionResult<StartChatSessionResponse>> StartSession([FromBody] StartChatSessionRequest request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _chatService.StartSessionAsync(request, userId);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when starting chat session");
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting chat session");
            return StatusCode(500, new { Message = "An error occurred while starting the chat session" });
        }
    }

    /// <summary>
    /// Send a message to an active chat session (non-streaming)
    /// </summary>
    [HttpPost("sessions/{sessionId}/messages")]
    public async Task<ActionResult<SendMessageResponse>> SendMessage(
        string sessionId,
        [FromBody] SendMessageRequest request)
    {
        try
        {
            // Ensure sessionId matches
            if (request.SessionId != sessionId)
            {
                request.SessionId = sessionId;
            }

            var userId = GetUserId();
            var response = await _chatService.SendMessageAsync(request, userId);
            
            // Convert to DTO for WASM client
            var responseDto = new SendMessageResponse
            {
                SessionId = response.SessionId,
                UserMessage = ChatMessageDtoExtensions.FromChatMessage(response.UserMessage),
                AgentMessage = ChatMessageDtoExtensions.FromChatMessage(response.AgentMessage),
                Timestamp = response.Timestamp
            };
            
            return Ok(responseDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when sending message to session {SessionId}", sessionId);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to session {SessionId}", sessionId);
            return StatusCode(500, new { Message = "An error occurred while sending the message" });
        }
    }

    /// <summary>
    /// Send a message with streaming response (for real-time chat experience)
    /// </summary>
    [HttpPost("sessions/{sessionId}/messages/stream")]
    public async Task SendMessageStream(
        string sessionId,
        [FromBody] SendMessageRequest request)
    {
        try
        {
            // Ensure sessionId matches
            if (request.SessionId != sessionId)
            {
                request.SessionId = sessionId;
            }

            var userId = GetUserId();
            
            // Set response headers for streaming
            Response.ContentType = "text/plain";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("X-Accel-Buffering", "no");

            // Stream the response
            await _chatService.SendMessageStreamAsync(request, userId, Response.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming message to session {SessionId}", sessionId);
            await Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes($"\nError: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get chat history for a session
    /// </summary>
    [HttpGet("sessions/{sessionId}/messages")]
    public async Task<ActionResult<GetChatHistoryResponse>> GetChatHistory(
        string sessionId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        try
        {
            var userId = GetUserId();
            var request = new GetChatHistoryRequest
            {
                SessionId = sessionId,
                Skip = skip,
                Take = take
            };

            var response = await _chatService.GetChatHistoryAsync(request, userId);
            
            // Convert to DTO for WASM client
            var responseDto = new GetChatHistoryResponse
            {
                SessionId = response.SessionId,
                Messages = response.Messages.Select(ChatMessageDtoExtensions.FromChatMessage).ToList(),
                TotalMessages = response.TotalMessages,
                HasMore = response.HasMore
            };
            
            return Ok(responseDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when getting chat history for session {SessionId}", sessionId);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history for session {SessionId}", sessionId);
            return StatusCode(500, new { Message = "An error occurred while retrieving chat history" });
        }
    }

    /// <summary>
    /// Get all conversations for the current user
    /// </summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<List<ChatConversationResponse>>> GetConversations()
    {
        try
        {
            var userId = GetUserId();
            var conversations = await _chatService.GetUserConversationsAsync(userId);
            return Ok(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations for user");
            return StatusCode(500, new { Message = "An error occurred while retrieving conversations" });
        }
    }

    /// <summary>
    /// Get a specific session
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<ChatSession>> GetSession(string sessionId)
    {
        try
        {
            var userId = GetUserId();
            var session = await _chatService.GetSessionAsync(sessionId, userId);
            
            if (session == null)
            {
                return NotFound(new { Message = $"Session {sessionId} not found" });
            }

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
            return StatusCode(500, new { Message = "An error occurred while retrieving the session" });
        }
    }

    /// <summary>
    /// End a chat session
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    public async Task<IActionResult> EndSession(string sessionId)
    {
        try
        {
            var userId = GetUserId();
            await _chatService.EndSessionAsync(sessionId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session {SessionId}", sessionId);
            return StatusCode(500, new { Message = "An error occurred while ending the session" });
        }
    }

    /// <summary>
    /// Clear all sessions for the current user
    /// </summary>
    [HttpDelete("sessions")]
    public async Task<IActionResult> ClearAllSessions()
    {
        try
        {
            var userId = GetUserId();
            await _chatService.ClearAllSessionsAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all sessions for user");
            return StatusCode(500, new { Message = "An error occurred while clearing sessions" });
        }
    }

    #region Helper Methods

    private string GetUserId()
    {
        // Try to get user ID from authenticated user
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Fallback to session ID or anonymous user for development
        if (string.IsNullOrEmpty(userId))
        {
            userId = HttpContext.Session.Id;
            if (string.IsNullOrEmpty(userId))
            {
                userId = "anonymous";
            }
        }

        return userId;
    }

    #endregion
}

