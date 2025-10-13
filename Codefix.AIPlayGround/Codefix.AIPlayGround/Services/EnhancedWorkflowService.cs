using Codefix.AIPlayGround.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Codefix.AIPlayGround.Services;

public class EnhancedWorkflowService : IEnhancedWorkflowService
{
    private readonly Dictionary<string, Models.WorkflowDefinition> _workflows = new();
    private readonly string _workflowsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Workflows");

    public EnhancedWorkflowService()
    {
        InitializeWorkflowsDirectory();
        LoadWorkflowsFromFiles();
    }

    private void InitializeWorkflowsDirectory()
    {
        if (!Directory.Exists(_workflowsDirectory))
        {
            Directory.CreateDirectory(_workflowsDirectory);
        }
    }

    private void LoadWorkflowsFromFiles()
    {
        try
        {
            var files = Directory.GetFiles(_workflowsDirectory, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var workflow = JsonSerializer.Deserialize<Models.WorkflowDefinition>(json);
                    if (workflow != null)
                    {
                        _workflows[workflow.Id] = workflow;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading workflow from {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading workflows: {ex.Message}");
        }
    }

    public async Task<Models.WorkflowDefinition> CreateWorkflowAsync(string name, string description = "")
    {
        var workflow = new Models.WorkflowDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = Models.WorkflowStatus.Draft
        };

        _workflows[workflow.Id] = workflow;
        await SaveWorkflowToFileAsync(workflow.Id, GetWorkflowFilePath(workflow.Id));
        return workflow;
    }

    public async Task<Models.WorkflowDefinition> GetWorkflowAsync(string id)
    {
        return _workflows.TryGetValue(id, out var workflow) ? workflow : new Models.WorkflowDefinition();
    }

    public async Task<List<Models.WorkflowDefinition>> GetAllWorkflowsAsync()
    {
        return _workflows.Values.ToList();
    }

    public async Task<Models.WorkflowDefinition> UpdateWorkflowAsync(Models.WorkflowDefinition workflow)
    {
        workflow.UpdatedAt = DateTime.UtcNow;
        _workflows[workflow.Id] = workflow;
        await SaveWorkflowToFileAsync(workflow.Id, GetWorkflowFilePath(workflow.Id));
        return workflow;
    }

    public async Task<bool> DeleteWorkflowAsync(string id)
    {
        if (_workflows.Remove(id))
        {
            var filePath = GetWorkflowFilePath(id);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return true;
        }
        return false;
    }

    public async Task<EnhancedWorkflowNode> AddNodeAsync(string workflowId, AgentType nodeType, double x, double y)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
            throw new ArgumentException($"Workflow {workflowId} not found");

        var node = new EnhancedWorkflowNode
        {
            Id = Guid.NewGuid().ToString(),
            Name = GetDefaultNodeName(nodeType),
            Type = nodeType.ToString(),
            X = x,
            Y = y,
            AgentDefinition = CreateDefaultAgentDefinition(nodeType)
        };

        // Add default ports based on node type
        AddDefaultPorts(node, nodeType);

        workflow.Nodes.Add(node);
        workflow.UpdatedAt = DateTime.UtcNow;
        await UpdateWorkflowAsync(workflow);
        return node;
    }

    public async Task<EnhancedWorkflowNode> UpdateNodeAsync(string workflowId, EnhancedWorkflowNode node)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
            throw new ArgumentException($"Workflow {workflowId} not found");

        var existingNode = workflow.Nodes.FirstOrDefault(n => n.Id == node.Id);
        if (existingNode != null)
        {
            var index = workflow.Nodes.IndexOf(existingNode);
            workflow.Nodes[index] = node;
            workflow.UpdatedAt = DateTime.UtcNow;
            await UpdateWorkflowAsync(workflow);
        }
        return node;
    }

    public async Task<bool> RemoveNodeAsync(string workflowId, string nodeId)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
            return false;

        var node = workflow.Nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node != null)
        {
            workflow.Nodes.Remove(node);
            // Remove all connections involving this node
            workflow.Connections.RemoveAll(c => c.FromNodeId == nodeId || c.ToNodeId == nodeId);
            workflow.UpdatedAt = DateTime.UtcNow;
            await UpdateWorkflowAsync(workflow);
            return true;
        }
        return false;
    }

    public async Task<List<EnhancedWorkflowNode>> GetWorkflowNodesAsync(string workflowId)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
            return new List<EnhancedWorkflowNode>();
        return workflow.Nodes;
    }

    public async Task<EnhancedWorkflowConnection> AddConnectionAsync(string workflowId, string fromNodeId, string toNodeId, ConnectionType connectionType = ConnectionType.DataFlow)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
            throw new ArgumentException($"Workflow {workflowId} not found");

        var connection = new EnhancedWorkflowConnection
        {
            Id = Guid.NewGuid().ToString(),
            FromNodeId = fromNodeId,
            ToNodeId = toNodeId,
            ConnectionType = connectionType,
            Label = GetDefaultConnectionLabel(connectionType)
        };

        workflow.Connections.Add(connection);
        workflow.UpdatedAt = DateTime.UtcNow;
        await UpdateWorkflowAsync(workflow);
        return connection;
    }

    public async Task<EnhancedWorkflowConnection> UpdateConnectionAsync(string workflowId, EnhancedWorkflowConnection connection)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
            throw new ArgumentException($"Workflow {workflowId} not found");

        var existingConnection = workflow.Connections.FirstOrDefault(c => c.Id == connection.Id);
        if (existingConnection != null)
        {
            var index = workflow.Connections.IndexOf(existingConnection);
            workflow.Connections[index] = connection;
            workflow.UpdatedAt = DateTime.UtcNow;
            await UpdateWorkflowAsync(workflow);
        }
        return connection;
    }

    public async Task<bool> RemoveConnectionAsync(string workflowId, string connectionId)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
            return false;

        var connection = workflow.Connections.FirstOrDefault(c => c.Id == connectionId);
        if (connection != null)
        {
            workflow.Connections.Remove(connection);
            workflow.UpdatedAt = DateTime.UtcNow;
            await UpdateWorkflowAsync(workflow);
            return true;
        }
        return false;
    }

    public async Task<List<EnhancedWorkflowConnection>> GetWorkflowConnectionsAsync(string workflowId)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
            return new List<EnhancedWorkflowConnection>();
        return workflow.Connections;
    }

    public async Task<string> GenerateMermaidDiagramAsync(string workflowId)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
            return string.Empty;

        var diagram = new System.Text.StringBuilder();
        diagram.AppendLine("graph TD");

        // Add nodes
        foreach (var node in workflow.Nodes)
        {
            var nodeId = SanitizeNodeId(node.Id);
            var nodeLabel = $"{node.Name}\\n({node.Type})";
            var nodeStyle = GetMermaidNodeStyle(node.Type);
            diagram.AppendLine($"    {nodeId}[\"{nodeLabel}\"]{nodeStyle}");
        }

        // Add connections
        foreach (var connection in workflow.Connections)
        {
            var fromId = SanitizeNodeId(connection.FromNodeId);
            var toId = SanitizeNodeId(connection.ToNodeId);
            var label = string.IsNullOrEmpty(connection.Label) ? "" : $"|{connection.Label}|";
            var connectionStyle = GetMermaidConnectionStyle(connection.ConnectionType);
            diagram.AppendLine($"    {fromId} -->{label} {toId}{connectionStyle}");
        }

        return diagram.ToString();
    }

    public async Task<Models.WorkflowDefinition> ParseMermaidDiagramAsync(string mermaidContent)
    {
        var workflow = new Models.WorkflowDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Imported from Mermaid",
            Description = "Workflow imported from Mermaid diagram",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var lines = mermaidContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var nodePattern = new Regex(@"\s*(\w+)\[\\""([^\\""]+)\\]");
        var connectionPattern = new Regex(@"\s*(\w+)\s*-->\s*(\w+)");

        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("graph") || line.Trim().StartsWith("digraph"))
                continue;

            var nodeMatch = nodePattern.Match(line);
            if (nodeMatch.Success)
            {
                var nodeId = nodeMatch.Groups[1].Value;
                var nodeLabel = nodeMatch.Groups[2].Value;
                var parts = nodeLabel.Split("\\n");
                var nodeName = parts[0];
                var nodeType = parts.Length > 1 ? parts[1].Trim('(', ')') : "agent";

                var node = new EnhancedWorkflowNode
                {
                    Id = nodeId,
                    Name = nodeName,
                    Type = nodeType,
                    X = Random.Shared.Next(100, 500),
                    Y = Random.Shared.Next(100, 400)
                };

                workflow.Nodes.Add(node);
            }

            var connectionMatch = connectionPattern.Match(line);
            if (connectionMatch.Success)
            {
                var fromId = connectionMatch.Groups[1].Value;
                var toId = connectionMatch.Groups[2].Value;

                var connection = new EnhancedWorkflowConnection
                {
                    Id = Guid.NewGuid().ToString(),
                    FromNodeId = fromId,
                    ToNodeId = toId,
                    ConnectionType = ConnectionType.DataFlow
                };

                workflow.Connections.Add(connection);
            }
        }

        _workflows[workflow.Id] = workflow;
        await SaveWorkflowToFileAsync(workflow.Id, GetWorkflowFilePath(workflow.Id));
        return workflow;
    }

    public async Task SaveWorkflowToFileAsync(string workflowId, string filePath)
    {
        if (_workflows.TryGetValue(workflowId, out var workflow))
        {
            var json = JsonSerializer.Serialize(workflow, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
    }

    public async Task<Models.WorkflowDefinition> LoadWorkflowFromFileAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath);
            var workflow = JsonSerializer.Deserialize<Models.WorkflowDefinition>(json);
            if (workflow != null)
            {
                _workflows[workflow.Id] = workflow;
                return workflow;
            }
        }
        return new Models.WorkflowDefinition();
    }

    public async Task<List<string>> GetSavedWorkflowFilesAsync()
    {
        var files = Directory.GetFiles(_workflowsDirectory, "*.json");
        return files.Select(Path.GetFileNameWithoutExtension).ToList();
    }

    public async Task<List<string>> ValidateWorkflowAsync(string workflowId)
    {
        var errors = new List<string>();
        if (!_workflows.TryGetValue(workflowId, out var workflow))
        {
            errors.Add("Workflow not found");
            return errors;
        }

        // Validate nodes
        foreach (var node in workflow.Nodes)
        {
            if (string.IsNullOrEmpty(node.Name))
                errors.Add($"Node {node.Id} has no name");
        }

        // Validate connections
        foreach (var connection in workflow.Connections)
        {
            if (!workflow.Nodes.Any(n => n.Id == connection.FromNodeId))
                errors.Add($"Connection {connection.Id} references non-existent source node {connection.FromNodeId}");
            
            if (!workflow.Nodes.Any(n => n.Id == connection.ToNodeId))
                errors.Add($"Connection {connection.Id} references non-existent target node {connection.ToNodeId}");
        }

        return errors;
    }

    public async Task<bool> ValidateConnectionAsync(string workflowId, string fromNodeId, string toNodeId)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
            return false;

        var fromNode = workflow.Nodes.FirstOrDefault(n => n.Id == fromNodeId);
        var toNode = workflow.Nodes.FirstOrDefault(n => n.Id == toNodeId);

        return fromNode != null && toNode != null;
    }

    // Helper methods
    private string GetWorkflowFilePath(string workflowId)
    {
        return Path.Combine(_workflowsDirectory, $"{workflowId}.json");
    }

    private string GetDefaultNodeName(AgentType nodeType)
    {
        return nodeType switch
        {
            AgentType.StartNode => "Start",
            AgentType.EndNode => "End",
            AgentType.LLMAgent => "LLM Agent",
            AgentType.ToolAgent => "Tool Agent",
            AgentType.ConditionalAgent => "Conditional Agent",
            AgentType.ParallelAgent => "Parallel Agent",
            AgentType.CheckpointAgent => "Checkpoint Agent",
            AgentType.MCPAgent => "MCP Agent",
            AgentType.FunctionNode => "Function",
            _ => "Node"
        };
    }

    private AgentDefinition CreateDefaultAgentDefinition(AgentType nodeType)
    {
        return new AgentDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = GetDefaultNodeName(nodeType),
            Type = nodeType,
            LLMConfig = nodeType == AgentType.LLMAgent ? new LLMConfiguration() : null,
            PromptTemplate = new PromptTemplate(),
            MemoryConfig = new MemoryConfiguration(),
            CheckpointConfig = new CheckpointConfiguration()
        };
    }

    private void AddDefaultPorts(EnhancedWorkflowNode node, AgentType nodeType)
    {
        switch (nodeType)
        {
            case AgentType.StartNode:
                node.OutputPorts.Add(new ConnectionPort { Name = "output", Type = "output", DataType = "object" });
                break;
            case AgentType.EndNode:
                node.InputPorts.Add(new ConnectionPort { Name = "input", Type = "input", DataType = "object" });
                break;
            default:
                node.InputPorts.Add(new ConnectionPort { Name = "input", Type = "input", DataType = "object" });
                node.OutputPorts.Add(new ConnectionPort { Name = "output", Type = "output", DataType = "object" });
                break;
        }
    }

    private string GetDefaultConnectionLabel(ConnectionType connectionType)
    {
        return connectionType switch
        {
            ConnectionType.DataFlow => "Data",
            ConnectionType.ControlFlow => "Control",
            ConnectionType.Conditional => "Condition",
            ConnectionType.Parallel => "Parallel",
            ConnectionType.Error => "Error",
            ConnectionType.Signal => "Signal",
            _ => "Connection"
        };
    }

    private string SanitizeNodeId(string nodeId)
    {
        return nodeId.Replace("-", "").Replace(" ", "");
    }

    private string GetMermaidNodeStyle(string nodeType)
    {
        return nodeType.ToLower() switch
        {
            "startnode" or "start" => ":::start",
            "endnode" or "end" => ":::end",
            "llmagent" or "agent" => ":::agent",
            "toolagent" or "tool" => ":::tool",
            "conditionalagent" or "condition" => ":::condition",
            "parallelagent" or "parallel" => ":::parallel",
            _ => ""
        };
    }

    private string GetMermaidConnectionStyle(ConnectionType connectionType)
    {
        return connectionType switch
        {
            ConnectionType.Conditional => ":::conditional",
            ConnectionType.Parallel => ":::parallel",
            ConnectionType.Error => ":::error",
            _ => ""
        };
    }
}
