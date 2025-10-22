using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Service for executing generated agent code with security and monitoring
/// </summary>
public interface ICodeExecutionService
{
    /// <summary>
    /// Executes a generated agent with input data
    /// </summary>
    Task<AgentExecutionResult> ExecuteAgentAsync(string agentId, object input, Dictionary<string, object>? context = null);
    
    /// <summary>
    /// Executes a specific method on a generated agent
    /// </summary>
    Task<AgentExecutionResult> ExecuteAgentMethodAsync(string agentId, string methodName, object[]? parameters = null);
    
    /// <summary>
    /// Gets the status of a running agent execution
    /// </summary>
    Task<AgentExecutionStatus> GetExecutionStatusAsync(string executionId);
    
    /// <summary>
    /// Cancels a running agent execution
    /// </summary>
    Task<bool> CancelExecutionAsync(string executionId);
    
    /// <summary>
    /// Gets execution history for an agent
    /// </summary>
    Task<List<AgentExecutionResult>> GetExecutionHistoryAsync(string agentId, int limit = 50);
    
    /// <summary>
    /// Validates agent execution permissions
    /// </summary>
    Task<bool> ValidateExecutionPermissionsAsync(string agentId, string methodName);
    
    /// <summary>
    /// Gets agent performance metrics
    /// </summary>
    Task<AgentPerformanceMetrics> GetPerformanceMetricsAsync(string agentId);
    
    /// <summary>
    /// Cleans up old executions and temporary files
    /// </summary>
    Task CleanupAsync();
}

/// <summary>
/// Result of agent execution
/// </summary>
public class AgentExecutionResult
{
    public string ExecutionId { get; set; } = Guid.NewGuid().ToString();
    public string AgentId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public object? Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public TimeSpan Duration => CompletedAt?.Subtract(StartedAt) ?? TimeSpan.Zero;
    public List<string> Logs { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public AgentExecutionStatus Status { get; set; } = AgentExecutionStatus.Pending;
}

/// <summary>
/// Agent execution status
/// </summary>
public enum AgentExecutionStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
    Timeout
}

/// <summary>
/// Agent performance metrics
/// </summary>
public class AgentPerformanceMetrics
{
    public string AgentId { get; set; } = string.Empty;
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public double AverageExecutionTimeMs { get; set; }
    public double MinExecutionTimeMs { get; set; }
    public double MaxExecutionTimeMs { get; set; }
    public DateTime LastExecutionAt { get; set; }
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
}
