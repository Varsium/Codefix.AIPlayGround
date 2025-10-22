using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Factory interface for creating agents based on Microsoft Agent Framework patterns
/// </summary>
public interface IAgentFactory
{
    /// <summary>
    /// Creates an LLM-based agent with specified configuration
    /// </summary>
    Task<AgentEntity> CreateLLMAgentAsync(string name, string instructions, LLMConfiguration? config = null);

    /// <summary>
    /// Creates a tool-enabled agent with specific tools
    /// </summary>
    Task<AgentEntity> CreateToolAgentAsync(string name, List<ToolConfiguration> tools, string? instructions = null);

    /// <summary>
    /// Creates a conditional routing agent
    /// </summary>
    Task<AgentEntity> CreateConditionalAgentAsync(string name, string condition, string? instructions = null);

    /// <summary>
    /// Creates a parallel execution agent
    /// </summary>
    Task<AgentEntity> CreateParallelAgentAsync(string name, int maxConcurrency = 5);

    /// <summary>
    /// Creates an agent with checkpoint capabilities
    /// </summary>
    Task<AgentEntity> CreateCheckpointAgentAsync(string name, CheckpointConfiguration? config = null);

    /// <summary>
    /// Creates an MCP (Model Context Protocol) enabled agent
    /// </summary>
    Task<AgentEntity> CreateMCPAgentAsync(string name, List<string> mcpServers, string? instructions = null);

    /// <summary>
    /// Creates an agent from a predefined template
    /// </summary>
    Task<AgentEntity> CreateFromTemplateAsync(string templateName, Dictionary<string, object>? parameters = null);

    /// <summary>
    /// Creates a custom agent with full configuration
    /// </summary>
    Task<AgentEntity> CreateCustomAgentAsync(CreateAgentRequest agentDto);

    /// <summary>
    /// Gets available agent templates
    /// </summary>
    Task<List<AgentTemplate>> GetAvailableTemplatesAsync();

    /// <summary>
    /// Validates agent configuration before creation
    /// </summary>
    Task<ValidationResult> ValidateAgentConfigurationAsync(CreateAgentRequest agentDto);

    /// <summary>
    /// Clones an existing agent with modifications
    /// </summary>
    Task<AgentEntity> CloneAgentAsync(string sourceAgentId, string newName, Dictionary<string, object>? modifications = null);
    
    // Code generation methods
    Task<AgentEntity> CreateCodeGeneratedAgentAsync(AgentCodeSpecification specification);
    Task<AgentEntity> CreateAgentFromTemplateAsync(string templateName, Dictionary<string, object> parameters);
    Task<AgentExecutionResult> ExecuteCodeGeneratedAgentAsync(string agentId, object input, Dictionary<string, object>? context = null);
    Task<AgentExecutionResult> ExecuteAgentMethodAsync(string agentId, string methodName, object[]? parameters = null);
    Task<List<CodeTemplate>> GetCodeTemplatesAsync();
}

/// <summary>
/// Agent template definition
/// </summary>
public class AgentTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AgentType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, TemplateParameter> Parameters { get; set; } = new();
    public CreateAgentRequest BaseConfiguration { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Template parameter definition
/// </summary>
public class TemplateParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string"; // string, number, boolean, object
    public string Description { get; set; } = string.Empty;
    public object? DefaultValue { get; set; }
    public bool IsRequired { get; set; } = false;
    public List<object>? AllowedValues { get; set; }
}

/// <summary>
/// Validation result for agent configuration
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Suggestions { get; set; } = new();
}

