using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codefix.AIPlayGround.Models;

[Table("Agents")]
public class AgentEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string AgentType { get; set; } = "LLMAgent"; // LLMAgent, ToolAgent, ConditionalAgent, etc.
    
    [Column(TypeName = "nvarchar(max)")]
    public string Instructions { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string LLMConfigurationJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string ToolsConfigurationJson { get; set; } = "[]";
    
    [Column(TypeName = "nvarchar(max)")]
    public string PromptTemplateJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string MemoryConfigurationJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string CheckpointConfigurationJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string PropertiesJson { get; set; } = "{}";
    
    public AgentStatus Status { get; set; } = AgentStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<FlowAgentEntity> FlowAgents { get; set; } = new List<FlowAgentEntity>();
    public virtual ICollection<AgentExecutionEntity> Executions { get; set; } = new List<AgentExecutionEntity>();
}

public enum AgentStatus
{
    Draft,
    Active,
    Inactive,
    Deployed,
    Error
}

[Table("AgentExecutions")]
public class AgentExecutionEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string AgentId { get; set; } = string.Empty;
    
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
    
    // Navigation property
    [ForeignKey("AgentId")]
    public virtual AgentEntity Agent { get; set; } = null!;
}

