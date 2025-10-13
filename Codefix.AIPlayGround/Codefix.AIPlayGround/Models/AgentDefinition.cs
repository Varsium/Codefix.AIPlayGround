namespace Codefix.AIPlayGround.Models;

public class AgentDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public AgentType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public LLMConfiguration? LLMConfig { get; set; }
    public List<ToolConfiguration> Tools { get; set; } = new();
    public PromptTemplate? PromptTemplate { get; set; }
    public MemoryConfiguration? MemoryConfig { get; set; }
    public CheckpointConfiguration? CheckpointConfig { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum AgentType
{
    LLMAgent,
    ToolAgent,
    ConditionalAgent,
    ParallelAgent,
    CheckpointAgent,
    MCPAgent,
    StartNode,
    EndNode,
    FunctionNode
}

public class LLMConfiguration
{
    public string ModelName { get; set; } = "gpt-4";
    public string Provider { get; set; } = "OpenAI"; // OpenAI, Azure, Anthropic, etc.
    public string ApiKey { get; set; } = string.Empty;
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
