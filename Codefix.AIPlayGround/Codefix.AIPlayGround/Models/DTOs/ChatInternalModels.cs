using Microsoft.Extensions.AI;

namespace Codefix.AIPlayGround.Models.DTOs;

/// <summary>
/// Represents an active chat session with an agent.
/// Session-scoped and tied to a specific agent configuration.
/// Server-side only - uses Microsoft.Extensions.AI which is not available in WASM
/// </summary>
public class ChatSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Chat history using Microsoft.Extensions.AI.ChatMessage
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = new();
    
    /// <summary>
    /// Session metadata (e.g., user preferences, context variables)
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Internal response from sending a message (server-side only)
/// Uses Microsoft.Extensions.AI.ChatMessage which is not available in WASM
/// </summary>
public class SendMessageInternalResponse
{
    public string SessionId { get; set; } = string.Empty;
    public ChatMessage UserMessage { get; set; } = null!;
    public ChatMessage AgentMessage { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Internal response containing chat history (server-side only)
/// Uses Microsoft.Extensions.AI.ChatMessage which is not available in WASM
/// </summary>
public class GetChatHistoryInternalResponse
{
    public string SessionId { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new();
    public int TotalMessages { get; set; }
    public bool HasMore { get; set; }
}

/// <summary>
/// Helper class for converting between internal and external chat DTOs
/// </summary>
public static class ChatMessageDtoExtensions
{
    public static ChatMessageDto FromChatMessage(ChatMessage message)
    {
        return new ChatMessageDto
        {
            Role = message.Role.Value,
            Text = message.Text
        };
    }
    
    public static ChatMessageDto ToDto(this ChatMessage message)
    {
        return FromChatMessage(message);
    }
}

