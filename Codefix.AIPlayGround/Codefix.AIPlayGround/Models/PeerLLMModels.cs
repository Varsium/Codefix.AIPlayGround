using Standard.AI.PeerLLM.Models.Foundations.Chats;
using System.ComponentModel.DataAnnotations;

namespace Codefix.AIPlayGround.Models;

/// <summary>
/// PeerLLM agent definition for database storage
/// Based on Standard.AI.PeerLLM integration
/// </summary>
public class PeerLLMAgentEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string HostEndpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public PeerLLMAgentStatus Status { get; set; } = PeerLLMAgentStatus.Available;
    public PeerLLMAgentType AgentType { get; set; } = PeerLLMAgentType.ChatAgent;
    
    // Configuration
    public string ConfigurationJson { get; set; } = string.Empty;
    public string CapabilitiesJson { get; set; } = string.Empty;
    
    // Metadata
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0.0";
    
    // Performance metrics
    public int TotalRequests { get; set; } = 0;
    public int SuccessfulRequests { get; set; } = 0;
    public int FailedRequests { get; set; } = 0;
    public double AverageResponseTimeMs { get; set; } = 0.0;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// PeerLLM agent types
/// </summary>
public enum PeerLLMAgentType
{
    /// <summary>
    /// Chat completion agent
    /// </summary>
    ChatAgent,
    
    /// <summary>
    /// Text generation agent
    /// </summary>
    TextGenerationAgent,
    
    /// <summary>
    /// Code generation agent
    /// </summary>
    CodeGenerationAgent,
    
    /// <summary>
    /// Question answering agent
    /// </summary>
    QuestionAnsweringAgent,
    
    /// <summary>
    /// Translation agent
    /// </summary>
    TranslationAgent,
    
    /// <summary>
    /// Summarization agent
    /// </summary>
    SummarizationAgent,
    
    /// <summary>
    /// Custom agent
    /// </summary>
    CustomAgent
}

/// <summary>
/// PeerLLM agent status
/// </summary>
public enum PeerLLMAgentStatus
{
    Available,
    Busy,
    Offline,
    Maintenance,
    Error
}

/// <summary>
/// PeerLLM agent configuration
/// </summary>
public class PeerLLMAgentConfiguration
{
    public string ModelName { get; set; } = "mistral-7b-instruct-v0.1.Q8_0";
    public string HostEndpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 2000;
    public double Temperature { get; set; } = 0.7;
    public double TopP { get; set; } = 1.0;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// PeerLLM agent capabilities
/// </summary>
public class PeerLLMAgentCapabilities
{
    public bool CanStream { get; set; } = true;
    public bool CanHandleImages { get; set; } = false;
    public bool CanHandleCode { get; set; } = true;
    public bool CanHandleLongContext { get; set; } = true;
    public int MaxContextLength { get; set; } = 4096;
    public List<string> SupportedLanguages { get; set; } = new();
    public List<string> SupportedTasks { get; set; } = new();
}

/// <summary>
/// PeerLLM conversation entity for database storage
/// </summary>
public class PeerLLMConversationEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string AgentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public PeerLLMConversationStatus Status { get; set; } = PeerLLMConversationStatus.Active;
    
    // Conversation data
    public string MessagesJson { get; set; } = string.Empty;
    public int MessageCount { get; set; } = 0;
    public int TotalTokens { get; set; } = 0;
    
    // Timestamps
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastMessageAt { get; set; }
    public DateTime? EndedAt { get; set; }
    
    // Metadata
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// PeerLLM conversation status
/// </summary>
public enum PeerLLMConversationStatus
{
    Active,
    Paused,
    Ended,
    Error
}

/// <summary>
/// PeerLLM message entity for database storage
/// </summary>
public class PeerLLMMessageEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string ConversationId { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public PeerLLMMessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    
    // Message metadata
    public int TokenCount { get; set; } = 0;
    public double ProcessingTimeMs { get; set; } = 0.0;
    public bool IsStreaming { get; set; } = false;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Error handling
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    
    // Metadata
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// PeerLLM message roles
/// </summary>
public enum PeerLLMMessageRole
{
    User,
    Assistant,
    System
}

/// <summary>
/// PeerLLM usage statistics entity
/// </summary>
public class PeerLLMUsageStatsEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string AgentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    
    // Usage metrics
    public int TotalRequests { get; set; } = 0;
    public int SuccessfulRequests { get; set; } = 0;
    public int FailedRequests { get; set; } = 0;
    public int TotalTokens { get; set; } = 0;
    public double TotalCost { get; set; } = 0.0;
    public double AverageResponseTimeMs { get; set; } = 0.0;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// PeerLLM agent definition for workflow integration
/// </summary>
public class PeerLLMAgentDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PeerLLMAgentType AgentType { get; set; } = PeerLLMAgentType.ChatAgent;
    public PeerLLMAgentConfiguration Configuration { get; set; } = new();
    public PeerLLMAgentCapabilities Capabilities { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Integration with Microsoft Agent Framework
    public bool IsMicrosoftAgentFrameworkCompatible { get; set; } = true;
    public string MicrosoftAgentFrameworkType { get; set; } = "ChatAgent";
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// PeerLLM chat session configuration
/// </summary>
public class PeerLLMChatSessionConfig
{
    public string ModelName { get; set; } = "mistral-7b-instruct-v0.1.Q8_0";
    public string SystemPrompt { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 2000;
    public double Temperature { get; set; } = 0.7;
    public double TopP { get; set; } = 1.0;
    public bool Stream { get; set; } = true;
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// PeerLLM chat request DTO
/// </summary>
public class PeerLLMChatRequest
{
    public string AgentId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public PeerLLMChatSessionConfig? SessionConfig { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// PeerLLM chat response DTO
/// </summary>
public class PeerLLMChatResponse
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AgentId { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TokenCount { get; set; } = 0;
    public double ProcessingTimeMs { get; set; } = 0.0;
    public bool IsStreaming { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// PeerLLM agent test result DTO
/// </summary>
public class PeerLLMAgentTestResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public double ResponseTimeMs { get; set; } = 0.0;
    public int TokenCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}
