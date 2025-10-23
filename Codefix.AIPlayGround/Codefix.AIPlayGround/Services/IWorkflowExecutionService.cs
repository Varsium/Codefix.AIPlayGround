using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public interface IWorkflowExecutionService
{
    /// <summary>
    /// Starts execution of a workflow
    /// </summary>
    /// <param name="workflowId">The ID of the workflow to execute</param>
    /// <param name="inputData">Initial input data for the workflow</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution ID for tracking</returns>
    Task<string> StartExecutionAsync(string workflowId, Dictionary<string, object>? inputData = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses execution of a running workflow
    /// </summary>
    /// <param name="executionId">The execution ID to pause</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> PauseExecutionAsync(string executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes execution of a paused workflow
    /// </summary>
    /// <param name="executionId">The execution ID to resume</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> ResumeExecutionAsync(string executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops execution of a running workflow
    /// </summary>
    /// <param name="executionId">The execution ID to stop</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> StopExecutionAsync(string executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a workflow execution
    /// </summary>
    /// <param name="executionId">The execution ID to check</param>
    /// <returns>Current execution status and progress</returns>
    Task<WorkflowExecution?> GetExecutionStatusAsync(string executionId);

    /// <summary>
    /// Gets all executions for a specific workflow
    /// </summary>
    /// <param name="workflowId">The workflow ID</param>
    /// <returns>List of executions for the workflow</returns>
    Task<List<WorkflowExecution>> GetWorkflowExecutionsAsync(string workflowId);

    /// <summary>
    /// Gets execution steps for a specific execution
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <returns>List of execution steps</returns>
    Task<List<ExecutionStep>> GetExecutionStepsAsync(string executionId);

    /// <summary>
    /// Gets execution errors for a specific execution
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <returns>List of execution errors</returns>
    Task<List<ExecutionError>> GetExecutionErrorsAsync(string executionId);

    /// <summary>
    /// Event that fires when execution status changes
    /// </summary>
    event EventHandler<ExecutionStatusChangedEventArgs>? ExecutionStatusChanged;

    /// <summary>
    /// Event that fires when a step completes
    /// </summary>
    event EventHandler<StepCompletedEventArgs>? StepCompleted;

    /// <summary>
    /// Event that fires when an error occurs
    /// </summary>
    event EventHandler<ExecutionErrorEventArgs>? ExecutionError;
}

public class ExecutionStatusChangedEventArgs : EventArgs
{
    public string ExecutionId { get; set; } = string.Empty;
    public ExecutionStatus OldStatus { get; set; }
    public ExecutionStatus NewStatus { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class StepCompletedEventArgs : EventArgs
{
    public string ExecutionId { get; set; } = string.Empty;
    public string StepId { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public Dictionary<string, object> OutputData { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public class ExecutionErrorEventArgs : EventArgs
{
    public string ExecutionId { get; set; } = string.Empty;
    public string? StepId { get; set; }
    public string? NodeId { get; set; }
    public ExecutionError Error { get; set; } = new();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
