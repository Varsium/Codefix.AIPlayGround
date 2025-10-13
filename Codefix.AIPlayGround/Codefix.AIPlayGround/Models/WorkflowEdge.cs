namespace Codefix.AIPlayGround.Models;

public class WorkflowEdge
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "default";
    public bool IsSelected { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
    
    // Compatibility properties for Home.razor
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}
