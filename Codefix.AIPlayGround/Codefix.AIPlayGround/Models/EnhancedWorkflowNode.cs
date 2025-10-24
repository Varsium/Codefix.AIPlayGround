namespace Codefix.AIPlayGround.Models;

public class EnhancedWorkflowNode : WorkflowNode
{
    public AgentDefinition? AgentDefinition { get; set; }
    
    // Microsoft Agent Framework integration
    public MicrosoftAgentFrameworkDefinition? MicrosoftAgentFrameworkDefinition { get; set; }
    public MicrosoftAgentFrameworkType? MicrosoftAgentFrameworkType { get; set; }
    public WorkflowNodeOrchestrationSettings OrchestrationSettings { get; set; } = new();
    
    // PeerLLM integration
    public PeerLLMAgentDefinition? PeerLLMAgentDefinition { get; set; }
    public PeerLLMAgentType? PeerLLMAgentType { get; set; }
    
    public new List<ConnectionPort> InputPorts { get; set; } = new();
    public new List<ConnectionPort> OutputPorts { get; set; } = new();
    public NodeValidationRules ValidationRules { get; set; } = new();
    public NodeExecutionSettings ExecutionSettings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ConnectionPort
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "default"; // input, output, bidirectional
    public string DataType { get; set; } = "object"; // string, number, boolean, object, array
    public bool IsRequired { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public object? DefaultValue { get; set; }
    public List<string> AllowedConnections { get; set; } = new();
    public double X { get; set; } // Relative position on node
    public double Y { get; set; } // Relative position on node
}

public class NodeValidationRules
{
    public int MinInputConnections { get; set; } = 0;
    public int MaxInputConnections { get; set; } = int.MaxValue;
    public int MinOutputConnections { get; set; } = 0;
    public int MaxOutputConnections { get; set; } = int.MaxValue;
    public List<string> RequiredInputTypes { get; set; } = new();
    public List<string> AllowedOutputTypes { get; set; } = new();
    public List<ValidationRule> CustomRules { get; set; } = new();
}

public class ValidationRule
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // regex, range, custom
    public string Expression { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class NodeExecutionSettings
{
    public int TimeoutSeconds { get; set; } = 300;
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
    public bool EnableParallelExecution { get; set; } = false;
    public int MaxConcurrentExecutions { get; set; } = 1;
    public string ExecutionMode { get; set; } = "sequential"; // sequential, parallel, conditional
    public Dictionary<string, object> EnvironmentVariables { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework workflow node orchestration settings
/// Defines how this node participates in orchestration patterns
/// </summary>
public class WorkflowNodeOrchestrationSettings
{
    /// <summary>
    /// Whether this node can participate in orchestration
    /// </summary>
    public bool CanParticipateInOrchestration { get; set; } = true;
    
    /// <summary>
    /// Orchestration roles this node can play
    /// </summary>
    public List<WorkflowNodeOrchestrationRole> Roles { get; set; } = new();
    
    /// <summary>
    /// Priority for orchestration execution (higher = more priority)
    /// </summary>
    public int OrchestrationPriority { get; set; } = 0;
    
    /// <summary>
    /// Whether this node can be executed in parallel with other nodes
    /// </summary>
    public bool CanExecuteInParallel { get; set; } = true;
    
    /// <summary>
    /// Dependencies that must be satisfied before this node can execute
    /// </summary>
    public List<string> OrchestrationDependencies { get; set; } = new();
    
    /// <summary>
    /// Conditions that must be met for this node to participate in orchestration
    /// </summary>
    public List<WorkflowNodeOrchestrationCondition> OrchestrationConditions { get; set; } = new();
    
    /// <summary>
    /// Microsoft Agent Framework-specific orchestration settings
    /// </summary>
    public Dictionary<string, object> FrameworkOrchestrationSettings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework workflow node orchestration roles
/// </summary>
public enum WorkflowNodeOrchestrationRole
{
    /// <summary>
    /// Primary executor - main agent in orchestration
    /// </summary>
    PrimaryExecutor,
    
    /// <summary>
    /// Assistant - supports primary executor
    /// </summary>
    Assistant,
    
    /// <summary>
    /// Validator - validates results from other agents
    /// </summary>
    Validator,
    
    /// <summary>
    /// Aggregator - combines results from multiple agents
    /// </summary>
    Aggregator,
    
    /// <summary>
    /// Coordinator - manages orchestration flow
    /// </summary>
    Coordinator,
    
    /// <summary>
    /// Observer - monitors orchestration without participating
    /// </summary>
    Observer,
    
    /// <summary>
    /// Custom role
    /// </summary>
    Custom
}

/// <summary>
/// Microsoft Agent Framework workflow node orchestration condition
/// </summary>
public class WorkflowNodeOrchestrationCondition
{
    public string Name { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public string ConditionType { get; set; } = "javascript"; // javascript, csharp, jsonpath
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
