using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Codefix.AIPlayGround.Services;

public class EnhancedWorkflowService : IEnhancedWorkflowService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EnhancedWorkflowService> _logger;

    public EnhancedWorkflowService(ApplicationDbContext context, ILogger<EnhancedWorkflowService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Workflow Management
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

        var entity = MapToEntity(workflow);
        _context.Workflows.Add(entity);
        await _context.SaveChangesAsync();

        return workflow;
    }

    public async Task<Models.WorkflowDefinition> GetWorkflowAsync(string id)
    {
        var entity = await _context.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Connections)
            .Include(w => w.Metadata)
            .Include(w => w.Settings)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (entity == null)
            return new Models.WorkflowDefinition { Id = id, Name = "Not Found" };

        return MapFromEntity(entity);
    }

    public async Task<List<Models.WorkflowDefinition>> GetAllWorkflowsAsync()
    {
        var entities = await _context.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Connections)
            .Include(w => w.Metadata)
            .Include(w => w.Settings)
            .ToListAsync();

        return entities.Select(MapFromEntity).ToList();
    }

    public async Task ReloadWorkflowsAsync()
    {
        // Clear any cached data if needed
        _context.ChangeTracker.Clear();
    }

    public async Task<Models.WorkflowDefinition> UpdateWorkflowAsync(Models.WorkflowDefinition workflow)
    {
        workflow.UpdatedAt = DateTime.UtcNow;
        
        var existingEntity = await _context.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Connections)
            .Include(w => w.Metadata)
            .Include(w => w.Settings)
            .FirstOrDefaultAsync(w => w.Id == workflow.Id);

        if (existingEntity == null)
        {
            // Create new
            var newEntity = MapToEntity(workflow);
            _context.Workflows.Add(newEntity);
        }
        else
        {
            // Update existing
            UpdateEntity(existingEntity, workflow);
        }

        await _context.SaveChangesAsync();
        return workflow;
    }

    public async Task<bool> DeleteWorkflowAsync(string id)
    {
        var entity = await _context.Workflows.FindAsync(id);
        if (entity == null)
            return false;

        _context.Workflows.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // Node Management
    public async Task<EnhancedWorkflowNode> AddNodeAsync(string workflowId, AgentType nodeType, double x, double y)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        if (workflow == null)
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
        var workflow = await GetWorkflowAsync(workflowId);
        if (workflow == null)
            throw new ArgumentException($"Workflow {workflowId} not found");

        var existingNode = workflow.Nodes.FirstOrDefault(n => n.Id == node.Id);
        if (existingNode != null)
        {
            workflow.Nodes.Remove(existingNode);
        }
        workflow.Nodes.Add(node);
        workflow.UpdatedAt = DateTime.UtcNow;
        await UpdateWorkflowAsync(workflow);
        return node;
    }

    public async Task<bool> RemoveNodeAsync(string workflowId, string nodeId)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        if (workflow == null)
            return false;

        var node = workflow.Nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node == null)
            return false;

        workflow.Nodes.Remove(node);
        
        // Also remove connections to/from this node
        workflow.Connections.RemoveAll(c => c.FromNodeId == nodeId || c.ToNodeId == nodeId);
        
        workflow.UpdatedAt = DateTime.UtcNow;
        await UpdateWorkflowAsync(workflow);
        return true;
    }

    public async Task<List<EnhancedWorkflowNode>> GetWorkflowNodesAsync(string workflowId)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        return workflow?.Nodes ?? new List<EnhancedWorkflowNode>();
    }

    // Connection Management
    public async Task<EnhancedWorkflowConnection> AddConnectionAsync(string workflowId, string fromNodeId, string toNodeId, ConnectionType connectionType = ConnectionType.DataFlow)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        if (workflow == null)
            throw new ArgumentException($"Workflow {workflowId} not found");

        var connection = new EnhancedWorkflowConnection
        {
            Id = Guid.NewGuid().ToString(),
            FromNodeId = fromNodeId,
            ToNodeId = toNodeId,
            Type = connectionType.ToString(),
            Label = $"{fromNodeId} -> {toNodeId}"
        };

        workflow.Connections.Add(connection);
        workflow.UpdatedAt = DateTime.UtcNow;
        await UpdateWorkflowAsync(workflow);
        return connection;
    }

    public async Task<EnhancedWorkflowConnection> UpdateConnectionAsync(string workflowId, EnhancedWorkflowConnection connection)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        if (workflow == null)
            throw new ArgumentException($"Workflow {workflowId} not found");

        var existingConnection = workflow.Connections.FirstOrDefault(c => c.Id == connection.Id);
        if (existingConnection != null)
        {
            workflow.Connections.Remove(existingConnection);
        }
        workflow.Connections.Add(connection);
        workflow.UpdatedAt = DateTime.UtcNow;
        await UpdateWorkflowAsync(workflow);
        return connection;
    }

    public async Task<bool> RemoveConnectionAsync(string workflowId, string connectionId)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        if (workflow == null)
            return false;

        var connection = workflow.Connections.FirstOrDefault(c => c.Id == connectionId);
        if (connection == null)
            return false;

        workflow.Connections.Remove(connection);
        workflow.UpdatedAt = DateTime.UtcNow;
        await UpdateWorkflowAsync(workflow);
        return true;
    }

    public async Task<List<EnhancedWorkflowConnection>> GetWorkflowConnectionsAsync(string workflowId)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        return workflow?.Connections ?? new List<EnhancedWorkflowConnection>();
    }

    // Mermaid Integration
    public async Task<string> GenerateMermaidDiagramAsync(string workflowId)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        if (workflow == null)
            return string.Empty;

        var mermaid = new System.Text.StringBuilder();
        mermaid.AppendLine("graph TD");

        foreach (var node in workflow.Nodes)
        {
            var nodeId = SanitizeId(node.Id);
            var nodeLabel = $"{node.Name}";
            var nodeShape = GetMermaidNodeShape(node.Type);
            mermaid.AppendLine($"    {nodeId}{nodeShape[0]}{nodeLabel}{nodeShape[1]}");
        }

        foreach (var connection in workflow.Connections)
        {
            var fromId = SanitizeId(connection.FromNodeId);
            var toId = SanitizeId(connection.ToNodeId);
            mermaid.AppendLine($"    {fromId} --> {toId}");
        }

        return mermaid.ToString();
    }

    public async Task<Models.WorkflowDefinition> ParseMermaidDiagramAsync(string mermaidContent)
    {
        // Basic parsing - can be enhanced
        var workflow = new Models.WorkflowDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Imported Workflow",
            Description = "Imported from Mermaid diagram"
        };

        return workflow;
    }

    // File Persistence (for backward compatibility - not used in DB mode)
    public Task SaveWorkflowToFileAsync(string workflowId, string filePath)
    {
        _logger.LogInformation("SaveWorkflowToFileAsync called but not implemented in database mode");
        return Task.CompletedTask;
    }

    public Task<Models.WorkflowDefinition> LoadWorkflowFromFileAsync(string filePath)
    {
        _logger.LogWarning("LoadWorkflowFromFileAsync called but not implemented in database mode");
        return Task.FromResult(new Models.WorkflowDefinition());
    }

    public Task<List<string>> GetSavedWorkflowFilesAsync()
    {
        _logger.LogWarning("GetSavedWorkflowFilesAsync called but not implemented in database mode");
        return Task.FromResult(new List<string>());
    }

    // Validation
    public async Task<List<string>> ValidateWorkflowAsync(string workflowId)
    {
        var errors = new List<string>();
        var workflow = await GetWorkflowAsync(workflowId);

        if (workflow == null)
        {
            errors.Add("Workflow not found");
            return errors;
        }

        if (workflow.Nodes.Count == 0)
        {
            errors.Add("Workflow must contain at least one node");
        }

        // Check for disconnected nodes
        var connectedNodeIds = new HashSet<string>();
        foreach (var conn in workflow.Connections)
        {
            connectedNodeIds.Add(conn.FromNodeId);
            connectedNodeIds.Add(conn.ToNodeId);
        }

        var disconnectedNodes = workflow.Nodes
            .Where(n => !connectedNodeIds.Contains(n.Id) && workflow.Nodes.Count > 1)
            .ToList();

        foreach (var node in disconnectedNodes)
        {
            errors.Add($"Node '{node.Name}' is not connected");
        }

        return errors;
    }

    public async Task<bool> ValidateConnectionAsync(string workflowId, string fromNodeId, string toNodeId)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        if (workflow == null)
            return false;

        var fromNode = workflow.Nodes.FirstOrDefault(n => n.Id == fromNodeId);
        var toNode = workflow.Nodes.FirstOrDefault(n => n.Id == toNodeId);

        return fromNode != null && toNode != null;
    }

    // Helper methods
    private string GetDefaultNodeName(AgentType nodeType)
    {
        return nodeType switch
        {
            AgentType.StartNode => "Start",
            AgentType.EndNode => "End",
            AgentType.LLMAgent => "LLM Agent",
            AgentType.ToolAgent => "Tool Agent",
            AgentType.ConditionalAgent => "Conditional",
            AgentType.ParallelAgent => "Parallel",
            AgentType.CheckpointAgent => "Checkpoint",
            AgentType.MCPAgent => "MCP Agent",
            AgentType.FunctionNode => "Function",
            _ => "Node"
        };
    }

    private AgentDefinition CreateDefaultAgentDefinition(AgentType nodeType)
    {
        return new AgentDefinition
        {
            Type = nodeType,
            Name = GetDefaultNodeName(nodeType),
            Description = $"Default {nodeType} agent"
        };
    }

    private void AddDefaultPorts(EnhancedWorkflowNode node, AgentType nodeType)
    {
        switch (nodeType)
        {
            case AgentType.StartNode:
                node.OutputPorts.Add(new ConnectionPort { Name = "output", Type = "output" });
                break;
            case AgentType.EndNode:
                node.InputPorts.Add(new ConnectionPort { Name = "input", Type = "input" });
                break;
            case AgentType.LLMAgent:
            case AgentType.ToolAgent:
            case AgentType.FunctionNode:
                node.InputPorts.Add(new ConnectionPort { Name = "input", Type = "input" });
                node.OutputPorts.Add(new ConnectionPort { Name = "output", Type = "output" });
                break;
            case AgentType.ConditionalAgent:
                node.InputPorts.Add(new ConnectionPort { Name = "input", Type = "input" });
                node.OutputPorts.Add(new ConnectionPort { Name = "true", Type = "output" });
                node.OutputPorts.Add(new ConnectionPort { Name = "false", Type = "output" });
                break;
            case AgentType.ParallelAgent:
                node.InputPorts.Add(new ConnectionPort { Name = "input", Type = "input" });
                node.OutputPorts.Add(new ConnectionPort { Name = "output1", Type = "output" });
                node.OutputPorts.Add(new ConnectionPort { Name = "output2", Type = "output" });
                break;
            default:
                node.InputPorts.Add(new ConnectionPort { Name = "input", Type = "input" });
                node.OutputPorts.Add(new ConnectionPort { Name = "output", Type = "output" });
                break;
        }
    }

    private string SanitizeId(string id)
    {
        return Regex.Replace(id, "[^a-zA-Z0-9]", "");
    }

    private string[] GetMermaidNodeShape(string nodeType)
    {
        return nodeType.ToLower() switch
        {
            "startnode" or "start" => new[] { "[", "]" },
            "endnode" or "end" => new[] { "((", "))" },
            "conditionalagent" or "condition" => new[] { "{", "}" },
            _ => new[] { "[", "]" }
        };
    }

    // Entity mapping methods
    private WorkflowEntity MapToEntity(Models.WorkflowDefinition workflow)
    {
        var entity = new WorkflowEntity
        {
            Id = workflow.Id,
            Name = workflow.Name,
            Description = workflow.Description,
            Version = workflow.Version,
            CreatedAt = workflow.CreatedAt,
            UpdatedAt = workflow.UpdatedAt,
            CreatedBy = workflow.CreatedBy,
            Status = workflow.Status
        };

        // Add nodes
        foreach (var node in workflow.Nodes)
        {
            entity.Nodes.Add(new WorkflowNodeEntity
            {
                Id = node.Id,
                Name = node.Name,
                Type = node.Type,
                X = node.X,
                Y = node.Y,
                Width = node.Width,
                Height = node.Height,
                Status = node.Status,
                IsSelected = node.IsSelected,
                PropertiesJson = JsonSerializer.Serialize(node.Properties),
                InputPortsJson = JsonSerializer.Serialize(node.InputPorts),
                OutputPortsJson = JsonSerializer.Serialize(node.OutputPorts),
                WorkflowId = workflow.Id
            });
        }

        // Add connections
        foreach (var conn in workflow.Connections)
        {
            entity.Connections.Add(new WorkflowConnectionEntity
            {
                Id = conn.Id,
                FromNodeId = conn.FromNodeId,
                ToNodeId = conn.ToNodeId,
                FromPort = conn.FromPort,
                ToPort = conn.ToPort,
                Label = conn.Label,
                Type = conn.Type,
                IsSelected = conn.IsSelected,
                From = conn.From,
                To = conn.To,
                PropertiesJson = JsonSerializer.Serialize(conn.Properties),
                WorkflowId = workflow.Id
            });
        }

        // Add metadata
        if (workflow.Metadata != null)
        {
            entity.Metadata = new WorkflowMetadataEntity
            {
                WorkflowId = workflow.Id,
                Category = workflow.Metadata.Category,
                Author = workflow.Metadata.Author,
                License = workflow.Metadata.License,
                TagsJson = JsonSerializer.Serialize(workflow.Metadata.Tags),
                CustomPropertiesJson = JsonSerializer.Serialize(workflow.Metadata.CustomProperties)
            };
        }

        // Add settings
        if (workflow.Settings != null)
        {
            entity.Settings = new WorkflowSettingsEntity
            {
                WorkflowId = workflow.Id,
                EnableCheckpoints = workflow.Settings.EnableCheckpoints,
                EnableLogging = workflow.Settings.EnableLogging,
                EnableMetrics = workflow.Settings.EnableMetrics,
                MaxExecutionTimeMinutes = workflow.Settings.MaxExecutionTimeMinutes,
                MaxRetryAttempts = workflow.Settings.MaxRetryAttempts,
                ExecutionMode = workflow.Settings.ExecutionMode,
                EnvironmentVariablesJson = JsonSerializer.Serialize(workflow.Settings.EnvironmentVariables)
            };
        }

        return entity;
    }

    private Models.WorkflowDefinition MapFromEntity(WorkflowEntity entity)
    {
        var workflow = new Models.WorkflowDefinition
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Version = entity.Version,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            Status = entity.Status
        };

        // Map nodes
        foreach (var nodeEntity in entity.Nodes)
        {
            var node = new EnhancedWorkflowNode
            {
                Id = nodeEntity.Id,
                Name = nodeEntity.Name,
                Type = nodeEntity.Type,
                X = nodeEntity.X,
                Y = nodeEntity.Y,
                Width = nodeEntity.Width,
                Height = nodeEntity.Height,
                Status = nodeEntity.Status,
                IsSelected = nodeEntity.IsSelected,
                Properties = JsonSerializer.Deserialize<Dictionary<string, object>>(nodeEntity.PropertiesJson) ?? new(),
                InputPorts = JsonSerializer.Deserialize<List<ConnectionPort>>(nodeEntity.InputPortsJson) ?? new(),
                OutputPorts = JsonSerializer.Deserialize<List<ConnectionPort>>(nodeEntity.OutputPortsJson) ?? new()
            };
            workflow.Nodes.Add(node);
        }

        // Map connections
        foreach (var connEntity in entity.Connections)
        {
            var conn = new EnhancedWorkflowConnection
            {
                Id = connEntity.Id,
                FromNodeId = connEntity.FromNodeId,
                ToNodeId = connEntity.ToNodeId,
                FromPort = connEntity.FromPort,
                ToPort = connEntity.ToPort,
                Label = connEntity.Label,
                Type = connEntity.Type,
                IsSelected = connEntity.IsSelected,
                From = connEntity.From,
                To = connEntity.To,
                Properties = JsonSerializer.Deserialize<Dictionary<string, object>>(connEntity.PropertiesJson) ?? new()
            };
            workflow.Connections.Add(conn);
        }

        // Map metadata
        if (entity.Metadata != null)
        {
            workflow.Metadata = new WorkflowMetadata
            {
                Category = entity.Metadata.Category,
                Author = entity.Metadata.Author,
                License = entity.Metadata.License,
                Tags = JsonSerializer.Deserialize<List<string>>(entity.Metadata.TagsJson) ?? new(),
                CustomProperties = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata.CustomPropertiesJson) ?? new()
            };
        }

        // Map settings
        if (entity.Settings != null)
        {
            workflow.Settings = new WorkflowSettings
            {
                EnableCheckpoints = entity.Settings.EnableCheckpoints,
                EnableLogging = entity.Settings.EnableLogging,
                EnableMetrics = entity.Settings.EnableMetrics,
                MaxExecutionTimeMinutes = entity.Settings.MaxExecutionTimeMinutes,
                MaxRetryAttempts = entity.Settings.MaxRetryAttempts,
                ExecutionMode = entity.Settings.ExecutionMode,
                EnvironmentVariables = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings.EnvironmentVariablesJson) ?? new()
            };
        }

        return workflow;
    }

    private void UpdateEntity(WorkflowEntity entity, Models.WorkflowDefinition workflow)
    {
        entity.Name = workflow.Name;
        entity.Description = workflow.Description;
        entity.Version = workflow.Version;
        entity.UpdatedAt = workflow.UpdatedAt;
        entity.Status = workflow.Status;

        // Update nodes
        entity.Nodes.Clear();
        foreach (var node in workflow.Nodes)
        {
            entity.Nodes.Add(new WorkflowNodeEntity
            {
                Id = node.Id,
                Name = node.Name,
                Type = node.Type,
                X = node.X,
                Y = node.Y,
                Width = node.Width,
                Height = node.Height,
                Status = node.Status,
                IsSelected = node.IsSelected,
                PropertiesJson = JsonSerializer.Serialize(node.Properties),
                InputPortsJson = JsonSerializer.Serialize(node.InputPorts),
                OutputPortsJson = JsonSerializer.Serialize(node.OutputPorts),
                WorkflowId = workflow.Id
            });
        }

        // Update connections
        entity.Connections.Clear();
        foreach (var conn in workflow.Connections)
        {
            entity.Connections.Add(new WorkflowConnectionEntity
            {
                Id = conn.Id,
                FromNodeId = conn.FromNodeId,
                ToNodeId = conn.ToNodeId,
                FromPort = conn.FromPort,
                ToPort = conn.ToPort,
                Label = conn.Label,
                Type = conn.Type,
                IsSelected = conn.IsSelected,
                From = conn.From,
                To = conn.To,
                PropertiesJson = JsonSerializer.Serialize(conn.Properties),
                WorkflowId = workflow.Id
            });
        }
    }
}

public enum ConnectionType
{
    DataFlow,
    ControlFlow,
    Conditional
}
