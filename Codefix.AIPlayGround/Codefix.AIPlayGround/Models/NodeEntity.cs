using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Codefix.AIPlayGround.Models;

[Table("Nodes")]
public class NodeEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string NodeType { get; set; } = string.Empty; // Agent, Function, Condition, Input, Output, etc.
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string ConfigurationJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string InputSchemaJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string OutputSchemaJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string PropertiesJson { get; set; } = "{}";
    
    public NodeStatus Status { get; set; } = NodeStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<FlowNodeEntity> FlowNodes { get; set; } = new List<FlowNodeEntity>();
    public virtual ICollection<NodeConnectionEntity> SourceConnections { get; set; } = new List<NodeConnectionEntity>();
    public virtual ICollection<NodeConnectionEntity> TargetConnections { get; set; } = new List<NodeConnectionEntity>();
}

public enum NodeStatus
{
    Draft,
    Active,
    Inactive,
    Error
}

[Table("FlowNodes")]
public class FlowNodeEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string FlowId { get; set; } = string.Empty;
    
    [Required]
    public string NodeId { get; set; } = string.Empty;
    
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 150;
    public double Height { get; set; } = 80;
    
    public int Order { get; set; } = 0;
    
    [Column(TypeName = "nvarchar(max)")]
    public string FlowSpecificConfigurationJson { get; set; } = "{}";
    
    // Navigation properties
    [ForeignKey("FlowId")]
    public virtual FlowEntity Flow { get; set; } = null!;
    
    [ForeignKey("NodeId")]
    public virtual NodeEntity Node { get; set; } = null!;
}

[Table("NodeConnections")]
public class NodeConnectionEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string SourceNodeId { get; set; } = string.Empty;
    
    [Required]
    public string TargetNodeId { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string SourcePort { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string TargetPort { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Label { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string ConnectionType { get; set; } = "Data"; // Data, Control, Conditional
    
    [Column(TypeName = "nvarchar(max)")]
    public string ConfigurationJson { get; set; } = "{}";
    
    // Navigation properties
    [ForeignKey("SourceNodeId")]
    public virtual NodeEntity SourceNode { get; set; } = null!;
    
    [ForeignKey("TargetNodeId")]
    public virtual NodeEntity TargetNode { get; set; } = null!;
}

[Table("NodeTemplates")]
public class NodeTemplateEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string NodeType { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string DefaultConfigurationJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string DefaultInputSchemaJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string DefaultOutputSchemaJson { get; set; } = "{}";
    
    [Column(TypeName = "nvarchar(max)")]
    public string PropertiesJson { get; set; } = "{}";
    
    public bool IsSystemTemplate { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}

