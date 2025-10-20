using System.ComponentModel.DataAnnotations;

namespace Codefix.AIPlayGround.Models.DTOs;

public class NodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class NodeDetailDto : NodeDto
{
    public Dictionary<string, object> Configuration { get; set; } = new();
    public Dictionary<string, object> InputSchema { get; set; } = new();
    public Dictionary<string, object> OutputSchema { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<FlowDto> Flows { get; set; } = new();
    public List<NodeConnectionDto> SourceConnections { get; set; } = new();
    public List<NodeConnectionDto> TargetConnections { get; set; } = new();
}

public class CreateNodeDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string NodeType { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public Dictionary<string, object> Configuration { get; set; } = new();
    public Dictionary<string, object> InputSchema { get; set; } = new();
    public Dictionary<string, object> OutputSchema { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class UpdateNodeDto
{
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [MaxLength(100)]
    public string? NodeType { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public Dictionary<string, object>? Configuration { get; set; }
    public Dictionary<string, object>? InputSchema { get; set; }
    public Dictionary<string, object>? OutputSchema { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

public class NodeFilterDto
{
    public string? Name { get; set; }
    public string? NodeType { get; set; }
    public string? Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class NodeConnectionDto
{
    public string Id { get; set; } = string.Empty;
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string SourcePort { get; set; } = string.Empty;
    public string TargetPort { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class CreateConnectionDto
{
    [Required]
    public string TargetNodeId { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string SourcePort { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string TargetPort { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Label { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string ConnectionType { get; set; } = "Data";
    
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class NodeTemplateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> DefaultConfiguration { get; set; } = new();
    public Dictionary<string, object> DefaultInputSchema { get; set; } = new();
    public Dictionary<string, object> DefaultOutputSchema { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
    public bool IsSystemTemplate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateNodeTemplateDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string NodeType { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public Dictionary<string, object> DefaultConfiguration { get; set; } = new();
    public Dictionary<string, object> DefaultInputSchema { get; set; } = new();
    public Dictionary<string, object> DefaultOutputSchema { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
    public bool IsSystemTemplate { get; set; } = false;
}


