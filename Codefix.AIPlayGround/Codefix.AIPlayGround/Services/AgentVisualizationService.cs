using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public class AgentVisualizationService : IAgentVisualizationService
{
    private readonly List<WorkflowNode> _workflowNodes = new();
    private readonly List<WorkflowConnection> _workflowConnections = new();
    private readonly List<WorkflowDefinition> _workflows = new();

    public AgentVisualizationService()
    {
        InitializeSampleData();
    }

    public Task<string> GenerateWorkflowDiagramAsync(List<WorkflowNode> nodes, List<WorkflowConnection> connections)
    {
        // Generate a simple Mermaid diagram
        var diagram = new System.Text.StringBuilder();
        diagram.AppendLine("graph TD");
        
        foreach (var node in nodes)
        {
            var nodeId = node.Id.Replace("-", "");
            var nodeLabel = $"{node.Name}\\n({node.Type})";
            diagram.AppendLine($"    {nodeId}[\"{nodeLabel}\"]");
        }
        
        foreach (var connection in connections)
        {
            var sourceId = connection.FromNodeId.Replace("-", "");
            var targetId = connection.ToNodeId.Replace("-", "");
            diagram.AppendLine($"    {sourceId} --> {targetId}");
        }
        
        return Task.FromResult(diagram.ToString());
    }

    public Task<List<WorkflowNode>> GetWorkflowNodesAsync()
    {
        return Task.FromResult(_workflowNodes.ToList());
    }

    public Task<List<WorkflowConnection>> GetWorkflowConnectionsAsync()
    {
        return Task.FromResult(_workflowConnections.ToList());
    }

    public Task SaveWorkflowAsync(string name, List<WorkflowNode> nodes, List<WorkflowConnection> connections)
    {
        var workflow = new WorkflowDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Nodes = nodes,
            Connections = connections
        };
        
        _workflows.Add(workflow);
        return Task.CompletedTask;
    }

    public Task<WorkflowDefinition> LoadWorkflowAsync(string id)
    {
        var workflow = _workflows.FirstOrDefault(w => w.Id == id);
        return Task.FromResult(workflow ?? new WorkflowDefinition());
    }

    private void InitializeSampleData()
    {
        // Add sample nodes
        _workflowNodes.AddRange(new[]
        {
            new WorkflowNode
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Start",
                Type = "start",
                X = 100,
                Y = 100
            },
            new WorkflowNode
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Process Data",
                Type = "process",
                X = 300,
                Y = 100
            },
            new WorkflowNode
            {
                Id = Guid.NewGuid().ToString(),
                Name = "End",
                Type = "end",
                X = 500,
                Y = 100
            }
        });

        // Add sample connections
        if (_workflowNodes.Count >= 3)
        {
            _workflowConnections.Add(new WorkflowConnection
            {
                Id = Guid.NewGuid().ToString(),
                FromNodeId = _workflowNodes[0].Id,
                ToNodeId = _workflowNodes[1].Id,
                Label = "Start Processing"
            });
            
            _workflowConnections.Add(new WorkflowConnection
            {
                Id = Guid.NewGuid().ToString(),
                FromNodeId = _workflowNodes[1].Id,
                ToNodeId = _workflowNodes[2].Id,
                Label = "Complete"
            });
        }
    }
}

public class WorkflowDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<WorkflowNode> Nodes { get; set; } = new();
    public List<WorkflowConnection> Connections { get; set; } = new();
    public string CreatedBy { get; internal set; }
    public string Version { get; internal set; }
    public string Description { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
    public WorkflowStatus Status { get; internal set; }
    public WorkflowMetadataEntity Metadata { get; internal set; }
    public WorkflowSettingsEntity Settings { get; internal set; }
}
