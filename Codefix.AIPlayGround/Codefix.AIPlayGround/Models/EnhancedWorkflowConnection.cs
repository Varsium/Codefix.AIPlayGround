namespace Codefix.AIPlayGround.Models;

public class EnhancedWorkflowConnection : WorkflowConnection
{
    public ConnectionPort? SourcePort { get; set; }
    public ConnectionPort? TargetPort { get; set; }
    public ConnectionType ConnectionType { get; set; } = ConnectionType.DataFlow;
    public ConnectionCondition? Condition { get; set; }
    public ConnectionValidation Validation { get; set; } = new();
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
