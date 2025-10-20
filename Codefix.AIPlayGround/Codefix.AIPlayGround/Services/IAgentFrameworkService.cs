using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public interface IAgentFrameworkService
{
    Task<AgentFrameworkResult> CreateAgentAsync(AgentEntity agent);
    Task<AgentFrameworkResult> UpdateAgentAsync(AgentEntity agent);
    Task<AgentFrameworkResult> DeployAgentAsync(string agentId);
    Task<AgentFrameworkResult> TestAgentAsync(string agentId, object input);
    Task<AgentFrameworkResult> ExecuteFlowAsync(FlowEntity flow, object input);
    Task<AgentFrameworkResult> GetAgentStatusAsync(string agentId);
    Task<AgentFrameworkResult> StopExecutionAsync(string executionId);
    Task<AgentFrameworkResult> ValidateAgentConfigurationAsync(AgentEntity agent);
    Task<AgentFrameworkResult> ValidateFlowConfigurationAsync(FlowEntity flow);
}

public class AgentFrameworkResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public static AgentFrameworkResult Success(string message = "Operation completed successfully", object? data = null)
    {
        return new AgentFrameworkResult
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public static AgentFrameworkResult Failure(string message, List<string>? errors = null)
    {
        return new AgentFrameworkResult
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static AgentFrameworkResult Warning(string message, List<string>? warnings = null)
    {
        return new AgentFrameworkResult
        {
            IsSuccess = true,
            Message = message,
            Warnings = warnings ?? new List<string>()
        };
    }
}

public class LLMConfiguration
{
    public string ModelName { get; set; } = "gpt-4";
    public string Provider { get; set; } = "OpenAI"; // OpenAI, Azure, Anthropic, etc.
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
    public string Type { get; set; } = string.Empty; // Function, API, MCP, etc.
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
    public string MemoryType { get; set; } = "conversation"; // conversation, semantic, vector
    public double RelevanceThreshold { get; set; } = 0.7;
    public TimeSpan MemoryRetention { get; set; } = TimeSpan.FromDays(30);
}

public class CheckpointConfiguration
{
    public bool EnableCheckpoints { get; set; } = true;
    public string CheckpointType { get; set; } = "automatic"; // automatic, manual, conditional
    public int CheckpointInterval { get; set; } = 10; // number of steps
    public List<string> CheckpointConditions { get; set; } = new();
    public bool EnableRecovery { get; set; } = true;
}

