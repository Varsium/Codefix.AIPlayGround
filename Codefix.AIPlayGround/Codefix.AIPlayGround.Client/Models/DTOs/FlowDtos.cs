using System.ComponentModel.DataAnnotations;

namespace Codefix.AIPlayGround.Models.DTOs;

public class FlowResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string FlowType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ParentFlowId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class FlowDetailResponse : FlowResponse
{
    public Dictionary<string, object> Configuration { get; set; } = new();
    public List<AgentResponse> Agents { get; set; } = new();
    public List<NodeResponse> Nodes { get; set; } = new();
    public List<FlowResponse> SubFlows { get; set; } = new();
    public List<FlowExecutionResponse> RecentExecutions { get; set; } = new();
}

public class CreateFlowRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Version { get; set; } = "1.0.0";
    
    [MaxLength(100)]
    public string FlowType { get; set; } = "Sequential";
    
    public Dictionary<string, object> Configuration { get; set; } = new();
    public string? ParentFlowId { get; set; }
}

public class UpdateFlowRequest
{
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? Version { get; set; }
    
    [MaxLength(100)]
    public string? FlowType { get; set; }
    
    public Dictionary<string, object>? Configuration { get; set; }
    public string? ParentFlowId { get; set; }
}

public class GetFlowsRequest
{
    public string? Name { get; set; }
    public string? FlowType { get; set; }
    public string? Status { get; set; }
    public string? CreatedBy { get; set; }
    public string? ParentFlowId { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class FlowExecutionResponse
{
    public string Id { get; set; } = string.Empty;
    public string FlowId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> InputData { get; set; } = new();
    public Dictionary<string, object> OutputData { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<object> Steps { get; set; } = new();
}

public class FlowExecutionResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ExecutionId { get; set; }
    public Dictionary<string, object> Output { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class ExecuteFlowRequest
{
    public Dictionary<string, object> Input { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
    public Dictionary<string, object> Options { get; set; } = new();
}

public class AddAgentToFlowRequest
{
    [Required]
    public string AgentId { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Role { get; set; } = "Participant";
    
    public Dictionary<string, object> Configuration { get; set; } = new();
    public int Order { get; set; } = 0;
}

public class AddNodeToFlowRequest
{
    [Required]
    public string NodeId { get; set; } = string.Empty;
    
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 150;
    public double Height { get; set; } = 80;
    
    public int Order { get; set; } = 0;
    
    public Dictionary<string, object> FlowSpecificConfiguration { get; set; } = new();
}

