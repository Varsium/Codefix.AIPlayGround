using System.ComponentModel.DataAnnotations;

namespace Codefix.AIPlayGround.Models.DTOs;

/// <summary>
/// Note: ChatSession and Internal types are in the Server project as they use Microsoft.Extensions.AI
/// which is not available in WASM. These are the API-facing DTOs.
/// </summary>

/// <summary>
/// Response for listing conversations in the UI
/// </summary>
public class ChatConversationResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? LastMessage { get; set; }
    public DateTime LastActivityAt { get; set; }
    public int MessageCount { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request to send a message to an agent
/// </summary>
public class SendMessageRequest
{
    [Required]
    public string SessionId { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional context/metadata for the message
    /// </summary>
    public Dictionary<string, object>? Context { get; set; }
}

/// <summary>
/// Response from sending a message (API response)
/// </summary>
public class SendMessageResponse
{
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// The user's message (echo)
    /// </summary>
    public ChatMessageDto UserMessage { get; set; } = null!;
    
    /// <summary>
    /// The agent's response
    /// </summary>
    public ChatMessageDto AgentMessage { get; set; } = null!;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Serializable chat message for API/WASM client
/// </summary>
public class ChatMessageDto
{
    public string? Role { get; set; }
    public string? Text { get; set; }
}

/// <summary>
/// Request to start a new chat session with an agent
/// </summary>
public class StartChatSessionRequest
{
    [Required]
    public string AgentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional initial message to send
    /// </summary>
    public string? InitialMessage { get; set; }
    
    /// <summary>
    /// Optional session metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response when starting a new chat session
/// </summary>
public class StartChatSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// If an initial message was sent, this contains the agent's response
    /// </summary>
    public ChatMessageDto? InitialResponse { get; set; }
}

/// <summary>
/// Request to get chat history
/// </summary>
public class GetChatHistoryRequest
{
    [Required]
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of messages to skip (for pagination)
    /// </summary>
    public int Skip { get; set; } = 0;
    
    /// <summary>
    /// Number of messages to take
    /// </summary>
    public int Take { get; set; } = 50;
}

/// <summary>
/// Response containing chat history (API response)
/// </summary>
public class GetChatHistoryResponse
{
    public string SessionId { get; set; } = string.Empty;
    public List<ChatMessageDto> Messages { get; set; } = new();
    public int TotalMessages { get; set; }
    public bool HasMore { get; set; }
}

