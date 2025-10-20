using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codefix.AIPlayGround.Models;

[Table("Workflows")]
public class WorkflowEntity
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
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(255)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    
    // Navigation properties
    public virtual ICollection<WorkflowNodeEntity> Nodes { get; set; } = new List<WorkflowNodeEntity>();
    public virtual ICollection<WorkflowConnectionEntity> Connections { get; set; } = new List<WorkflowConnectionEntity>();
    public virtual WorkflowMetadataEntity? Metadata { get; set; }
    public virtual WorkflowSettingsEntity? Settings { get; set; }
}

[Table("WorkflowNodes")]
public class WorkflowNodeEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;
    
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 150;
    public double Height { get; set; } = 80;
    
    [MaxLength(50)]
    public string Status { get; set; } = "idle";
    
    public bool IsSelected { get; set; }
    
    // JSON serialized properties
    [Column(TypeName = "nvarchar(max)")]
    public string PropertiesJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string InputPortsJson { get; set; } = "[]";
    
    [Column(TypeName = "nvarchar(max)")]
    public string OutputPortsJson { get; set; } = "[]";
    
    // Foreign key
    [Required]
    public string WorkflowId { get; set; } = string.Empty;
    
    // Navigation property
    [ForeignKey("WorkflowId")]
    public virtual WorkflowEntity Workflow { get; set; } = null!;
}

[Table("WorkflowConnections")]
public class WorkflowConnectionEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(255)]
    public string FromNodeId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string ToNodeId { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string FromPort { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string ToPort { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Label { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Type { get; set; } = "default";
    
    public bool IsSelected { get; set; }
    
    // Compatibility properties
    [MaxLength(255)]
    public string From { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string To { get; set; } = string.Empty;
    
    // JSON serialized properties
    [Column(TypeName = "nvarchar(max)")]
    public string PropertiesJson { get; set; } = "{}";
    
    // Foreign key
    [Required]
    public string WorkflowId { get; set; } = string.Empty;
    
    // Navigation property
    [ForeignKey("WorkflowId")]
    public virtual WorkflowEntity Workflow { get; set; } = null!;
}

[Table("WorkflowMetadata")]
public class WorkflowMetadataEntity
{
    [Key]
    public string WorkflowId { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Category { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Author { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string License { get; set; } = string.Empty;
    
    // JSON serialized properties
    [Column(TypeName = "nvarchar(max)")]
    public string TagsJson { get; set; } = "[]";
    
    [Column(TypeName = "nvarchar(max)")]
    public string CustomPropertiesJson { get; set; } = "{}";
    
    // Navigation property
    [ForeignKey("WorkflowId")]
    public virtual WorkflowEntity Workflow { get; set; } = null!;
}

[Table("WorkflowSettings")]
public class WorkflowSettingsEntity
{
    [Key]
    public string WorkflowId { get; set; } = string.Empty;
    
    public bool EnableCheckpoints { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public int MaxExecutionTimeMinutes { get; set; } = 60;
    public int MaxRetryAttempts { get; set; } = 3;
    
    [MaxLength(50)]
    public string ExecutionMode { get; set; } = "sequential";
    
    // JSON serialized properties
    [Column(TypeName = "nvarchar(max)")]
    public string EnvironmentVariablesJson { get; set; } = "{}";
    
    // Navigation property
    [ForeignKey("WorkflowId")]
    public virtual WorkflowEntity Workflow { get; set; } = null!;
}
