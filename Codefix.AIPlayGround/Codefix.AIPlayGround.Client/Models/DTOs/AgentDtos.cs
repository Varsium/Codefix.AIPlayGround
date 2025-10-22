using System.ComponentModel.DataAnnotations;

namespace Codefix.AIPlayGround.Models.DTOs;

public class AgentResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AgentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class WorkflowResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class AgentDetailResponse : AgentResponse
{
    public string Instructions { get; set; } = string.Empty;
    public LLMConfiguration? LLMConfiguration { get; set; }
    public List<ToolConfiguration> Tools { get; set; } = new();
    public PromptTemplate? PromptTemplate { get; set; }
    public MemoryConfiguration? MemoryConfiguration { get; set; }
    public CheckpointConfiguration? CheckpointConfiguration { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<WorkflowResponse> Workflows { get; set; } = new();
    public List<AgentExecutionResponse> RecentExecutions { get; set; } = new();
}

public class CreateAgentRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string AgentType { get; set; } = "LLMAgent";
    
    [Required]
    public string Instructions { get; set; } = string.Empty;
    
    public LLMConfiguration? LLMConfiguration { get; set; }
    public List<ToolConfiguration> Tools { get; set; } = new();
    public PromptTemplate? PromptTemplate { get; set; }
    public MemoryConfiguration? MemoryConfiguration { get; set; }
    public CheckpointConfiguration? CheckpointConfiguration { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class UpdateAgentRequest
{
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public string? Instructions { get; set; }
    
    /// <summary>
    /// Status: Active, Inactive, Draft
    /// </summary>
    public string? Status { get; set; }
    
    public LLMConfiguration? LLMConfiguration { get; set; }
    public List<ToolConfiguration>? Tools { get; set; }
    public PromptTemplate? PromptTemplate { get; set; }
    public MemoryConfiguration? MemoryConfiguration { get; set; }
    public CheckpointConfiguration? CheckpointConfiguration { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

public class GetAgentsRequest
{
    public string? Name { get; set; }
    public string? AgentType { get; set; }
    public string? Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AgentStatusResponse
{
    public string AgentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public AgentExecutionResponse? LastExecution { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

public class AgentExecutionResponse
{
    public string Id { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> InputData { get; set; } = new();
    public Dictionary<string, object> OutputData { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class DeploymentResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DeploymentId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class TestResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Input { get; set; } = new();
    public Dictionary<string, object> Output { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class TestAgentRequest
{
    public Dictionary<string, object> Input { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
}

// Configuration Models
public class LLMConfiguration
{
    public string ModelName { get; set; } = "gpt-4";
    public string Provider { get; set; } = "OpenAI";
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2000;
    public double TopP { get; set; } = 1.0;
    public double FrequencyPenalty { get; set; } = 0.0;
    public double PresencePenalty { get; set; } = 0.0;
    public List<string> StopSequences { get; set; } = new();
}

public class ToolConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
}

public class PromptTemplate
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public string AssistantPrompt { get; set; } = string.Empty;
    public List<PromptVariable> Variables { get; set; } = new();
    public string Template { get; set; } = string.Empty;
}

public class PromptVariable
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string Description { get; set; } = string.Empty;
    public object? DefaultValue { get; set; }
    public bool IsRequired { get; set; } = true;
}

public class MemoryConfiguration
{
    public bool EnableMemory { get; set; } = true;
    public int MaxMemoryItems { get; set; } = 100;
    public string MemoryType { get; set; } = "conversation";
    public double RelevanceThreshold { get; set; } = 0.7;
    public TimeSpan MemoryRetention { get; set; } = TimeSpan.FromDays(30);
}

public class CheckpointConfiguration
{
    public bool EnableCheckpoints { get; set; } = true;
    public string CheckpointType { get; set; } = "automatic";
    public int CheckpointInterval { get; set; } = 10;
    public List<string> CheckpointConditions { get; set; } = new();
    public bool EnableRecovery { get; set; } = true;
}

