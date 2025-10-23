namespace Codefix.AIPlayGround.Models;

public class EnhancedWorkflowConnection : WorkflowConnection
{
    public ConnectionPort? SourcePort { get; set; }
    public ConnectionPort? TargetPort { get; set; }
    public ConnectionType ConnectionType { get; set; } = ConnectionType.DataFlow;
    public ConnectionCondition? Condition { get; set; }
    public ConnectionValidation Validation { get; set; } = new();
    
    // Microsoft Agent Framework orchestration support
    public WorkflowConnectionOrchestrationSettings OrchestrationSettings { get; set; } = new();
    public MicrosoftAgentFrameworkConnectionType? MicrosoftAgentFrameworkConnectionType { get; set; }
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum ConnectionType
{
    DataFlow,
    ControlFlow,
    Conditional,
    Parallel,
    Error,
    Signal
}

public class ConnectionCondition
{
    public string Expression { get; set; } = string.Empty;
    public string ConditionType { get; set; } = "javascript"; // javascript, csharp, jsonpath
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
}

public class ConnectionValidation
{
    public bool ValidateDataType { get; set; } = true;
    public bool ValidateSchema { get; set; } = false;
    public string? ExpectedDataType { get; set; }
    public string? SchemaDefinition { get; set; }
    public List<ValidationRule> CustomValidations { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework connection types for orchestration
/// </summary>
public enum MicrosoftAgentFrameworkConnectionType
{
    /// <summary>
    /// Data flow between agents
    /// </summary>
    DataFlow,
    
    /// <summary>
    /// Control flow for orchestration
    /// </summary>
    ControlFlow,
    
    /// <summary>
    /// Handoff between agents
    /// </summary>
    Handoff,
    
    /// <summary>
    /// Parallel execution coordination
    /// </summary>
    ParallelCoordination,
    
    /// <summary>
    /// Group chat participation
    /// </summary>
    GroupChatParticipation,
    
    /// <summary>
    /// Result aggregation
    /// </summary>
    ResultAggregation,
    
    /// <summary>
    /// Error handling flow
    /// </summary>
    ErrorHandling,
    
    /// <summary>
    /// Custom connection type
    /// </summary>
    Custom
}

/// <summary>
/// Microsoft Agent Framework workflow connection orchestration settings
/// </summary>
public class WorkflowConnectionOrchestrationSettings
{
    /// <summary>
    /// Whether this connection participates in orchestration
    /// </summary>
    public bool ParticipatesInOrchestration { get; set; } = true;
    
    /// <summary>
    /// Orchestration pattern this connection supports
    /// </summary>
    public List<WorkflowOrchestrationType> SupportedOrchestrationTypes { get; set; } = new();
    
    /// <summary>
    /// Priority for orchestration execution
    /// </summary>
    public int OrchestrationPriority { get; set; } = 0;
    
    /// <summary>
    /// Whether this connection can be executed in parallel
    /// </summary>
    public bool CanExecuteInParallel { get; set; } = true;
    
    /// <summary>
    /// Conditions for orchestration participation
    /// </summary>
    public List<WorkflowConnectionOrchestrationCondition> OrchestrationConditions { get; set; } = new();
    
    /// <summary>
    /// Microsoft Agent Framework-specific connection settings
    /// </summary>
    public Dictionary<string, object> FrameworkConnectionSettings { get; set; } = new();
}

/// <summary>
/// Microsoft Agent Framework workflow connection orchestration condition
/// </summary>
public class WorkflowConnectionOrchestrationCondition
{
    public string Name { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public string ConditionType { get; set; } = "javascript"; // javascript, csharp, jsonpath
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
