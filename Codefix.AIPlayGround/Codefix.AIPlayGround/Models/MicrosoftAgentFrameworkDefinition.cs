using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Codefix.AIPlayGround.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Codefix.AIPlayGround.Models;

/// <summary>
/// Microsoft Agent Framework-aligned agent definition
/// Based on: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
/// </summary>
public class MicrosoftAgentFrameworkDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public MicrosoftAgentFrameworkType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    
    // Microsoft Agent Framework-specific configurations
    public MicrosoftAgentFrameworkLLMConfiguration? LLMConfig { get; set; }
    public List<MicrosoftAgentFrameworkToolConfiguration> Tools { get; set; } = new();
    public MicrosoftAgentFrameworkPromptConfiguration? PromptConfig { get; set; }
    public MicrosoftAgentFrameworkMemoryConfiguration? MemoryConfig { get; set; }
    public MicrosoftAgentFrameworkCheckpointConfiguration? CheckpointConfig { get; set; }
    public MicrosoftAgentFrameworkOrchestrationConfiguration? OrchestrationConfig { get; set; }
    
    // Framework-specific properties
    public Dictionary<string, object> FrameworkProperties { get; set; } = new();
    public MicrosoftAgentFrameworkCapabilities Capabilities { get; set; } = new();
    public MicrosoftAgentFrameworkSecurityConfiguration? SecurityConfig { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public MicrosoftAgentFrameworkStatus Status { get; set; } = MicrosoftAgentFrameworkStatus.Draft;
    public string Version { get; set; } = "1.0.0";
}

/// <summary>
/// AI Provider types supported by Microsoft Agent Framework
/// </summary>
public enum AIProviderType
{
    /// <summary>
    /// OpenAI provider
    /// </summary>
    OpenAI,
    
    /// <summary>
    /// Azure OpenAI provider
    /// </summary>
    AzureOpenAI,
    
    /// <summary>
    /// Anthropic provider
    /// </summary>
    Anthropic,
    
    /// <summary>
    /// PeerLLM decentralized provider
    /// </summary>
    PeerLLM,
    
    /// <summary>
    /// Ollama local provider (runs models like Llama2, Mistral, CodeLlama locally)
    /// </summary>
    Ollama,
    
    /// <summary>
    /// Google AI provider
    /// </summary>
    GoogleAI,
    
    /// <summary>
    /// Docker Model Runner - Docker's native AI model hosting
    /// See: https://docs.docker.com/ai/model-runner/
    /// </summary>
    DockerModelRunner,
    
    /// <summary>
    /// Generic local model provider (custom Docker containers, local endpoints)
    /// </summary>
    LocalModel,
    
    /// <summary>
    /// LM Studio local provider
    /// </summary>
    LMStudio,
    
    /// <summary>
    /// Custom provider
    /// </summary>
    Custom
}

/// <summary>
/// Microsoft Agent Framework agent types
/// Based on official framework patterns
/// </summary>
public enum MicrosoftAgentFrameworkType
{
    /// <summary>
    /// Chat-based LLM agent using ChatClientAgent
    /// </summary>
    ChatAgent,
    
    /// <summary>
    /// Tool-calling agent with function capabilities
    /// </summary>
    FunctionAgent,
    
    /// <summary>
    /// Sequential orchestration agent
    /// </summary>
    SequentialAgent,
    
    /// <summary>
    /// Concurrent orchestration agent
    /// </summary>
    ConcurrentAgent,
    
    /// <summary>
    /// Group chat orchestration agent
    /// </summary>
    GroupChatAgent,
    
    /// <summary>
    /// Model Context Protocol (MCP) agent
    /// </summary>
    MCPAgent,
    
    /// <summary>
    /// Custom agent with specific capabilities
    /// </summary>
    CustomAgent
}

/// <summary>
/// Microsoft Agent Framework status
/// </summary>
public enum MicrosoftAgentFrameworkStatus
{
    Draft,
    Active,
    Inactive,
    Deprecated,
    Testing
}

/// <summary>
/// Microsoft Agent Framework LLM configuration
/// Aligned with official framework patterns
/// </summary>
public class MicrosoftAgentFrameworkLLMConfiguration
{
    /// <summary>
    /// Model name (e.g., "gpt-4", "gpt-3.5-turbo", "claude-3", "mistral-7b-instruct")
    /// </summary>
    public string ModelName { get; set; } = "gpt-4";
    
    /// <summary>
    /// Provider (e.g., "OpenAI", "Azure OpenAI", "Anthropic", "PeerLLM", "Ollama")
    /// </summary>
    public string Provider { get; set; } = "OpenAI";
    
    /// <summary>
    /// AI Provider type for framework integration
    /// </summary>
    public AIProviderType ProviderType { get; set; } = AIProviderType.OpenAI;
    
    /// <summary>
    /// API endpoint URL
    /// </summary>
    public string? Endpoint { get; set; }
    
    /// <summary>
    /// API key (should be stored securely)
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// Temperature for response generation (0.0 to 2.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;
    
    /// <summary>
    /// Maximum tokens in response
    /// </summary>
    public int MaxTokens { get; set; } = 2000;
    
    /// <summary>
    /// Top-p sampling parameter
    /// </summary>
    public double TopP { get; set; } = 1.0;
    
    /// <summary>
    /// Frequency penalty (-2.0 to 2.0)
    /// </summary>
    public double FrequencyPenalty { get; set; } = 0.0;
    
    /// <summary>
    /// Presence penalty (-2.0 to 2.0)
    /// </summary>
    public double PresencePenalty { get; set; } = 0.0;
    
    /// <summary>
    /// Stop sequences
    /// </summary>
    public List<string> StopSequences { get; set; } = new();
    
    /// <summary>
    /// Microsoft Agent Framework-specific settings
    /// </summary>
    public Dictionary<string, object> FrameworkSettings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework tool configuration
/// Aligned with official framework tool registration patterns
/// </summary>
public class MicrosoftAgentFrameworkToolConfiguration
{
    /// <summary>
    /// Tool name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Tool description (used for [Description] attribute)
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Tool type (function, api, mcp, etc.)
    /// </summary>
    public string Type { get; set; } = "function";
    
    /// <summary>
    /// Tool category
    /// </summary>
    public string Category { get; set; } = "general";
    
    /// <summary>
    /// Tool parameters schema
    /// </summary>
    public Dictionary<string, MicrosoftAgentFrameworkToolParameter> Parameters { get; set; } = new();
    
    /// <summary>
    /// Return type
    /// </summary>
    public string ReturnType { get; set; } = "string";
    
    /// <summary>
    /// Whether the tool is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Tool execution timeout
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Microsoft Agent Framework-specific tool settings
    /// </summary>
    public Dictionary<string, object> FrameworkSettings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework tool parameter
/// </summary>
public class MicrosoftAgentFrameworkToolParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public object? DefaultValue { get; set; }
    public List<string>? AllowedValues { get; set; }
    public Dictionary<string, object>? ValidationRules { get; set; }
}

/// <summary>
/// Microsoft Agent Framework prompt configuration
/// Aligned with official framework prompt patterns
/// </summary>
public class MicrosoftAgentFrameworkPromptConfiguration
{
    /// <summary>
    /// System prompt
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;
    
    /// <summary>
    /// User prompt template
    /// </summary>
    public string UserPromptTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// Assistant prompt template
    /// </summary>
    public string AssistantPromptTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// Prompt variables
    /// </summary>
    public List<MicrosoftAgentFrameworkPromptVariable> Variables { get; set; } = new();
    
    /// <summary>
    /// Prompt template engine
    /// </summary>
    public string TemplateEngine { get; set; } = "handlebars";
    
    /// <summary>
    /// Microsoft Agent Framework-specific prompt settings
    /// </summary>
    public Dictionary<string, object> FrameworkSettings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework prompt variable
/// </summary>
public class MicrosoftAgentFrameworkPromptVariable
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string Description { get; set; } = string.Empty;
    public object? DefaultValue { get; set; }
    public bool IsRequired { get; set; } = true;
    public Dictionary<string, object>? ValidationRules { get; set; }
}

/// <summary>
/// Microsoft Agent Framework memory configuration
/// Aligned with official framework memory patterns
/// </summary>
public class MicrosoftAgentFrameworkMemoryConfiguration
{
    /// <summary>
    /// Whether memory is enabled
    /// </summary>
    public bool EnableMemory { get; set; } = true;
    
    /// <summary>
    /// Maximum memory items
    /// </summary>
    public int MaxMemoryItems { get; set; } = 100;
    
    /// <summary>
    /// Memory type (conversation, semantic, hybrid)
    /// </summary>
    public string MemoryType { get; set; } = "conversation";
    
    /// <summary>
    /// Relevance threshold for memory retrieval
    /// </summary>
    public double RelevanceThreshold { get; set; } = 0.7;
    
    /// <summary>
    /// Memory retention period
    /// </summary>
    public TimeSpan MemoryRetention { get; set; } = TimeSpan.FromDays(30);
    
    /// <summary>
    /// Microsoft Agent Framework-specific memory settings
    /// </summary>
    public Dictionary<string, object> FrameworkSettings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework checkpoint configuration
/// Aligned with official framework checkpoint patterns
/// </summary>
public class MicrosoftAgentFrameworkCheckpointConfiguration
{
    /// <summary>
    /// Whether checkpoints are enabled
    /// </summary>
    public bool EnableCheckpoints { get; set; } = true;
    
    /// <summary>
    /// Checkpoint type (automatic, manual, conditional)
    /// </summary>
    public string CheckpointType { get; set; } = "automatic";
    
    /// <summary>
    /// Checkpoint interval (in steps)
    /// </summary>
    public int CheckpointInterval { get; set; } = 10;
    
    /// <summary>
    /// Checkpoint conditions
    /// </summary>
    public List<string> CheckpointConditions { get; set; } = new();
    
    /// <summary>
    /// Whether recovery is enabled
    /// </summary>
    public bool EnableRecovery { get; set; } = true;
    
    /// <summary>
    /// Microsoft Agent Framework-specific checkpoint settings
    /// </summary>
    public Dictionary<string, object> FrameworkSettings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework orchestration configuration
/// Aligned with official framework orchestration patterns
/// </summary>
public class MicrosoftAgentFrameworkOrchestrationConfiguration
{
    /// <summary>
    /// Orchestration type (sequential, concurrent, group_chat)
    /// </summary>
    public string OrchestrationType { get; set; } = "sequential";
    
    /// <summary>
    /// Maximum concurrent executions
    /// </summary>
    public int MaxConcurrentExecutions { get; set; } = 5;
    
    /// <summary>
    /// Execution timeout
    /// </summary>
    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(10);
    
    /// <summary>
    /// Retry configuration
    /// </summary>
    public MicrosoftAgentFrameworkRetryConfiguration? RetryConfig { get; set; }
    
    /// <summary>
    /// Microsoft Agent Framework-specific orchestration settings
    /// </summary>
    public Dictionary<string, object> FrameworkSettings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework retry configuration
/// </summary>
public class MicrosoftAgentFrameworkRetryConfiguration
{
    public int MaxRetries { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public double BackoffMultiplier { get; set; } = 2.0;
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);
    public List<string> RetryableErrors { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework capabilities
/// </summary>
public class MicrosoftAgentFrameworkCapabilities
{
    /// <summary>
    /// Whether the agent can call tools
    /// </summary>
    public bool CanCallTools { get; set; } = true;
    
    /// <summary>
    /// Whether the agent can use memory
    /// </summary>
    public bool CanUseMemory { get; set; } = true;
    
    /// <summary>
    /// Whether the agent can use checkpoints
    /// </summary>
    public bool CanUseCheckpoints { get; set; } = true;
    
    /// <summary>
    /// Whether the agent can orchestrate other agents
    /// </summary>
    public bool CanOrchestrate { get; set; } = false;
    
    /// <summary>
    /// Whether the agent can use MCP
    /// </summary>
    public bool CanUseMCP { get; set; } = false;
    
    /// <summary>
    /// Whether the agent can stream responses
    /// </summary>
    public bool CanStreamResponses { get; set; } = true;
    
    /// <summary>
    /// Whether the agent can handle parallel execution
    /// </summary>
    public bool CanHandleParallelExecution { get; set; } = false;
}

/// <summary>
/// Microsoft Agent Framework security configuration
/// </summary>
public class MicrosoftAgentFrameworkSecurityConfiguration
{
    /// <summary>
    /// Whether security is enabled
    /// </summary>
    public bool EnableSecurity { get; set; } = true;
    
    /// <summary>
    /// Allowed domains for external calls
    /// </summary>
    public List<string> AllowedDomains { get; set; } = new();
    
    /// <summary>
    /// Blocked domains
    /// </summary>
    public List<string> BlockedDomains { get; set; } = new();
    
    /// <summary>
    /// Maximum request size
    /// </summary>
    public int MaxRequestSize { get; set; } = 1024 * 1024; // 1MB
    
    /// <summary>
    /// Whether to validate input
    /// </summary>
    public bool ValidateInput { get; set; } = true;
    
    /// <summary>
    /// Whether to sanitize output
    /// </summary>
    public bool SanitizeOutput { get; set; } = true;
    
    /// <summary>
    /// Microsoft Agent Framework-specific security settings
    /// </summary>
    public Dictionary<string, object> FrameworkSettings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework type definition
/// </summary>
public class MicrosoftAgentFrameworkTypeDefinition
{
    public MicrosoftAgentFrameworkType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public MicrosoftAgentFrameworkCapabilities DefaultCapabilities { get; set; } = new();
    public Dictionary<string, object> DefaultSettings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework agent entity for database storage
/// </summary>
public class MicrosoftAgentFrameworkEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public MicrosoftAgentFrameworkType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string ConfigurationJson { get; set; } = string.Empty;
    public string CapabilitiesJson { get; set; } = string.Empty;
    public string SecurityConfigJson { get; set; } = string.Empty;
    public string FrameworkPropertiesJson { get; set; } = string.Empty;
    public MicrosoftAgentFrameworkStatus Status { get; set; } = MicrosoftAgentFrameworkStatus.Draft;
    public string Version { get; set; } = "1.0.0";
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Microsoft Agent Framework DTOs for API requests and responses
/// </summary>

/// <summary>
/// Microsoft Agent Framework create agent request DTO
/// </summary>
public class MicrosoftAgentFrameworkCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MicrosoftAgentFrameworkType AgentType { get; set; } = MicrosoftAgentFrameworkType.ChatAgent;
    public string Instructions { get; set; } = string.Empty;
    public MicrosoftAgentFrameworkLLMConfiguration? LLMConfiguration { get; set; }
    public List<MicrosoftAgentFrameworkToolConfiguration> Tools { get; set; } = new();
    public MicrosoftAgentFrameworkPromptConfiguration? PromptConfiguration { get; set; }
    public MicrosoftAgentFrameworkMemoryConfiguration? MemoryConfiguration { get; set; }
    public MicrosoftAgentFrameworkCheckpointConfiguration? CheckpointConfiguration { get; set; }
    public MicrosoftAgentFrameworkOrchestrationConfiguration? OrchestrationConfiguration { get; set; }
    public MicrosoftAgentFrameworkSecurityConfiguration? SecurityConfiguration { get; set; }
    public Dictionary<string, object> FrameworkProperties { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework get agents request DTO
/// </summary>
public class MicrosoftAgentFrameworkGetRequest
{
    public string? Name { get; set; }
    public MicrosoftAgentFrameworkType? AgentType { get; set; }
    public MicrosoftAgentFrameworkStatus? Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Microsoft Agent Framework update agent request DTO
/// </summary>
public class MicrosoftAgentFrameworkUpdateRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public MicrosoftAgentFrameworkStatus? Status { get; set; }
    public MicrosoftAgentFrameworkLLMConfiguration? LLMConfiguration { get; set; }
    public List<MicrosoftAgentFrameworkToolConfiguration>? Tools { get; set; }
    public MicrosoftAgentFrameworkPromptConfiguration? PromptConfiguration { get; set; }
    public MicrosoftAgentFrameworkMemoryConfiguration? MemoryConfiguration { get; set; }
    public MicrosoftAgentFrameworkCheckpointConfiguration? CheckpointConfiguration { get; set; }
    public MicrosoftAgentFrameworkOrchestrationConfiguration? OrchestrationConfiguration { get; set; }
    public MicrosoftAgentFrameworkSecurityConfiguration? SecurityConfiguration { get; set; }
    public Dictionary<string, object>? FrameworkProperties { get; set; }
}

/// <summary>
/// Microsoft Agent Framework test request DTO
/// </summary>
public class MicrosoftAgentFrameworkTestRequest
{
    public Dictionary<string, object> Input { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
    public MicrosoftAgentFrameworkTestConfiguration? TestConfiguration { get; set; }
}

/// <summary>
/// Microsoft Agent Framework test configuration DTO
/// </summary>
public class MicrosoftAgentFrameworkTestConfiguration
{
    public bool EnableMetrics { get; set; } = true;
    public bool EnableDebugging { get; set; } = false;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public Dictionary<string, object> Settings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework test result DTO
/// </summary>
public class MicrosoftAgentFrameworkTestResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Input { get; set; } = new();
    public Dictionary<string, object> Output { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public MicrosoftAgentFrameworkExecutionResponse? Execution { get; set; }
}

/// <summary>
/// Microsoft Agent Framework execution response DTO
/// </summary>
public class MicrosoftAgentFrameworkExecutionResponse
{
    public string Id { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public MicrosoftAgentFrameworkExecutionStatus Status { get; set; }
    public Dictionary<string, object> InputData { get; set; } = new();
    public Dictionary<string, object> OutputData { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public MicrosoftAgentFrameworkExecutionType ExecutionType { get; set; } = MicrosoftAgentFrameworkExecutionType.Single;
}

/// <summary>
/// Microsoft Agent Framework deployment result DTO
/// </summary>
public class MicrosoftAgentFrameworkDeploymentResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DeploymentId { get; set; }
    public string? Endpoint { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public MicrosoftAgentFrameworkDeploymentStatus Status { get; set; } = MicrosoftAgentFrameworkDeploymentStatus.Pending;
}

/// <summary>
/// Microsoft Agent Framework status response DTO
/// </summary>
public class MicrosoftAgentFrameworkStatusResponse
{
    public string AgentId { get; set; } = string.Empty;
    public MicrosoftAgentFrameworkStatus Status { get; set; }
    public MicrosoftAgentFrameworkExecutionResponse? LastExecution { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    public MicrosoftAgentFrameworkHealthStatus HealthStatus { get; set; } = MicrosoftAgentFrameworkHealthStatus.Healthy;
}

/// <summary>
/// Additional Microsoft Agent Framework enums
/// </summary>
public enum MicrosoftAgentFrameworkExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    Timeout
}

public enum MicrosoftAgentFrameworkExecutionType
{
    Single,
    Sequential,
    Concurrent,
    GroupChat
}

public enum MicrosoftAgentFrameworkHealthStatus
{
    Healthy,
    Warning,
    Critical,
    Unknown
}

public enum MicrosoftAgentFrameworkDeploymentStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled
}
