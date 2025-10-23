using System.ComponentModel.DataAnnotations;

namespace Codefix.AIPlayGround.Models;

public class WorkflowExecutionEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string WorkflowId { get; set; } = string.Empty;
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    [Required]
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    
    public string InputDataJson { get; set; } = "{}";
    public string OutputDataJson { get; set; } = "{}";
    public string MetricsJson { get; set; } = "{}";
}

public class ExecutionStepEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string ExecutionId { get; set; } = string.Empty;
    
    [Required]
    public string NodeId { get; set; } = string.Empty;
    
    [Required]
    public string NodeName { get; set; } = string.Empty;
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    [Required]
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    
    public string InputDataJson { get; set; } = "{}";
    public string OutputDataJson { get; set; } = "{}";
    public string MetricsJson { get; set; } = "{}";
}

public class ExecutionErrorEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string ExecutionId { get; set; } = string.Empty;
    
    public string? StepId { get; set; }
    public string? NodeId { get; set; }
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string StackTrace { get; set; } = string.Empty;
    
    [Required]
    public string ErrorType { get; set; } = string.Empty;
    
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    
    public string ContextJson { get; set; } = "{}";
}
