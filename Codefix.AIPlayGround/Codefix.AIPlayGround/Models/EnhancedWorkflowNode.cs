namespace Codefix.AIPlayGround.Models;

public class EnhancedWorkflowNode : WorkflowNode
{
    public AgentDefinition? AgentDefinition { get; set; }
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
