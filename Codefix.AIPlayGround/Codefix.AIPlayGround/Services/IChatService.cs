using Codefix.AIPlayGround.Models.DTOs;
using Microsoft.Extensions.AI;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Service for managing chat sessions with AI agents.
/// Scoped to the user session - maintains active agent instances.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Start a new chat session with an agent
    /// </summary>
    Task<StartChatSessionResponse> StartSessionAsync(StartChatSessionRequest request, string userId);
    
    /// <summary>
    /// Send a message to an active chat session
    /// </summary>
    Task<SendMessageInternalResponse> SendMessageAsync(SendMessageRequest request, string userId);
    
    /// <summary>
    /// Send a message with streaming response
    /// </summary>
    Task SendMessageStreamAsync(SendMessageRequest request, string userId, Stream outputStream);
    
    /// <summary>
    /// Get chat history for a session
    /// </summary>
    Task<GetChatHistoryInternalResponse> GetChatHistoryAsync(GetChatHistoryRequest request, string userId);
    
    /// <summary>
    /// Get all active conversations for a user
    /// </summary>
    Task<List<ChatConversationResponse>> GetUserConversationsAsync(string userId);
    
    /// <summary>
    /// Get a specific session
    /// </summary>
    Task<ChatSession?> GetSessionAsync(string sessionId, string userId);
    
    /// <summary>
    /// End a chat session and cleanup resources
    /// </summary>
    Task EndSessionAsync(string sessionId, string userId);
    
    /// <summary>
    /// Clear all sessions for a user
    /// </summary>
    Task ClearAllSessionsAsync(string userId);
}

