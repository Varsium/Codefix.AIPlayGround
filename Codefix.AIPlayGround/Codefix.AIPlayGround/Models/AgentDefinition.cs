using Codefix.AIPlayGround.Models.DTOs;

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
    PeerLLMAgent,
    StartNode,
    EndNode,
    FunctionNode
}
