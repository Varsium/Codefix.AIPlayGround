using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codefix.AIPlayGround.Models;

[Table("Flows")]
public class FlowEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Version { get; set; } = "1.0.0";
    
    [MaxLength(100)]
    public string FlowType { get; set; } = "Sequential"; // Sequential, Parallel, Conditional, Nested
    
    [Column(TypeName = "nvarchar(max)")]
    public string ConfigurationJson { get; set; } = "{}";
    
    public string? ParentFlowId { get; set; }
    
    public FlowStatus Status { get; set; } = FlowStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    [ForeignKey("ParentFlowId")]
    public virtual FlowEntity? ParentFlow { get; set; }
    public virtual ICollection<FlowEntity> SubFlows { get; set; } = new List<FlowEntity>();
    public virtual ICollection<FlowAgentEntity> FlowAgents { get; set; } = new List<FlowAgentEntity>();
    public virtual ICollection<FlowNodeEntity> FlowNodes { get; set; } = new List<FlowNodeEntity>();
    public virtual ICollection<FlowExecutionEntity> Executions { get; set; } = new List<FlowExecutionEntity>();
}

public enum FlowStatus
{
    Draft,
    Published,
    Running,
    Completed,
    Failed,
    Archived
}

[Table("FlowAgents")]
public class FlowAgentEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string FlowId { get; set; } = string.Empty;
    
    [Required]
    public string AgentId { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Role { get; set; } = "Participant"; // Primary, Secondary, Observer, etc.
    
    [Column(TypeName = "nvarchar(max)")]
    public string ConfigurationJson { get; set; } = "{}";
    
    public int Order { get; set; } = 0;
    
    // Navigation properties
    [ForeignKey("FlowId")]
    public virtual FlowEntity Flow { get; set; } = null!;
    
    [ForeignKey("AgentId")]
    public virtual AgentEntity Agent { get; set; } = null!;
}

[Table("FlowExecutions")]
public class FlowExecutionEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string FlowId { get; set; } = string.Empty;
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Running;
    
    [Column(TypeName = "nvarchar(max)")]
    public string InputDataJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string OutputDataJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string MetricsJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string ErrorsJson { get; set; } = "[]";
    
    [Column(TypeName = "nvarchar(max)")]
    public string StepsJson { get; set; } = "[]";
    
    // Navigation property
    [ForeignKey("FlowId")]
    public virtual FlowEntity Flow { get; set; } = null!;
}

