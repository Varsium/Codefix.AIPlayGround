namespace Codefix.AIPlayGround.Models;

public class WorkflowDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public List<EnhancedWorkflowNode> Nodes { get; set; } = new();
    public List<EnhancedWorkflowConnection> Connections { get; set; } = new();
    public WorkflowMetadata Metadata { get; set; } = new();
    public WorkflowSettings Settings { get; set; } = new();
    
    // Microsoft Agent Framework orchestration configuration
    public MicrosoftAgentFrameworkOrchestrationConfiguration? OrchestrationConfig { get; set; }
    public WorkflowOrchestrationType OrchestrationType { get; set; } = WorkflowOrchestrationType.Sequential;
    public List<WorkflowOrchestrationStep> OrchestrationSteps { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
}

public class WorkflowMetadata
{
    public List<string> Tags { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}

public class WorkflowSettings
{
    public bool EnableCheckpoints { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public int MaxExecutionTimeMinutes { get; set; } = 60;
    public int MaxRetryAttempts { get; set; } = 3;
    public string ExecutionMode { get; set; } = "sequential"; // sequential, parallel, hybrid
    public Dictionary<string, object> EnvironmentVariables { get; set; } = new();
}

public enum WorkflowStatus
{
    Draft,
    Published,
    Archived,
    Running,
    Completed,
    Failed,
    Paused
}

public class WorkflowExecution
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WorkflowId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Running;
    public Dictionary<string, object> InputData { get; set; } = new();
    public Dictionary<string, object> OutputData { get; set; } = new();
    public List<ExecutionStep> Steps { get; set; } = new();
    public List<ExecutionError> Errors { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

public enum ExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    Paused
}

public class ExecutionStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    public Dictionary<string, object> InputData { get; set; } = new();
    public Dictionary<string, object> OutputData { get; set; } = new();
    public List<ExecutionError> Errors { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

public class ExecutionError
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Message { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? NodeId { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework workflow orchestration types
/// Based on official framework patterns: Sequential, Concurrent, GroupChat, Handoff, Magentic
/// </summary>
public enum WorkflowOrchestrationType
{
    /// <summary>
    /// Sequential orchestration - agents execute in pipeline order
    /// </summary>
    Sequential,
    
    /// <summary>
    /// Concurrent orchestration - agents execute in parallel
    /// </summary>
    Concurrent,
    
    /// <summary>
    /// Group chat orchestration - agents collaborate in group conversation
    /// </summary>
    GroupChat,
    
    /// <summary>
    /// Handoff orchestration - control passes dynamically between agents
    /// </summary>
    Handoff,
    
    /// <summary>
    /// Magentic orchestration - dynamic agent selection based on context
    /// </summary>
    Magentic,
    
    /// <summary>
    /// Custom orchestration - user-defined orchestration pattern
    /// </summary>
    Custom
}

/// <summary>
/// Microsoft Agent Framework workflow orchestration step
/// Defines how agents are orchestrated within the workflow
/// </summary>
public class WorkflowOrchestrationStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<string> NodeIds { get; set; } = new();
    public WorkflowOrchestrationStepType StepType { get; set; } = WorkflowOrchestrationStepType.AgentExecution;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public WorkflowOrchestrationCondition? Condition { get; set; }
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Microsoft Agent Framework workflow orchestration step types
/// </summary>
public enum WorkflowOrchestrationStepType
{
    /// <summary>
    /// Execute agents in this step
    /// </summary>
    AgentExecution,
    
    /// <summary>
    /// Wait for condition before proceeding
    /// </summary>
    WaitCondition,
    
    /// <summary>
    /// Merge results from parallel executions
    /// </summary>
    MergeResults,
    
    /// <summary>
    /// Branch execution based on condition
    /// </summary>
    Branch,
    
    /// <summary>
    /// Loop execution
    /// </summary>
    Loop,
    
    /// <summary>
    /// Custom step type
    /// </summary>
    Custom
}

/// <summary>
/// Microsoft Agent Framework workflow orchestration condition
/// </summary>
public class WorkflowOrchestrationCondition
{
    public string Expression { get; set; } = string.Empty;
    public string ConditionType { get; set; } = "javascript"; // javascript, csharp, jsonpath
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
