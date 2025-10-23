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
    private readonly IMicrosoftAgentFrameworkOrchestrationService _orchestrationService;

    public EnhancedWorkflowService(
        ApplicationDbContext context, 
        ILogger<EnhancedWorkflowService> logger,
        IMicrosoftAgentFrameworkOrchestrationService orchestrationService)
    {
        _context = context;
        _logger = logger;
        _orchestrationService = orchestrationService;
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

    public async Task<Models.WorkflowDefinition> CreateWorkflowFromTemplateAsync(string templateName, string name, string description = "")
    {
        var template = GetWorkflowTemplate(templateName);
        if (template == null)
        {
            throw new ArgumentException($"Template '{templateName}' not found");
        }

        var workflow = new Models.WorkflowDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = Models.WorkflowStatus.Draft,
            Nodes = template.Nodes.Select(n => new EnhancedWorkflowNode
            {
                Id = Guid.NewGuid().ToString(),
                Name = n.Name,
                Type = n.Type,
                X = n.X,
                Y = n.Y,
                Properties = n.Properties
            }).ToList(),
            Connections = template.Connections.Select(c => new EnhancedWorkflowConnection
            {
                Id = Guid.NewGuid().ToString(),
                FromNodeId = c.FromNodeId,
                ToNodeId = c.ToNodeId,
                FromPort = c.FromPort,
                ToPort = c.ToPort
            }).ToList()
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

    // Template Management
    public List<string> GetAvailableTemplates()
    {
        return new List<string>
        {
            "Greenfield Service Development",
            "Greenfield Fullstack Development", 
            "Greenfield UI Development",
            "Brownfield Service Enhancement",
            "Brownfield Fullstack Enhancement",
            "Brownfield UI Enhancement",
            "Simple Sequential Workflow",
            "Parallel Processing Workflow",
            "Conditional Decision Workflow"
        };
    }

    public Models.WorkflowDefinition? GetWorkflowTemplate(string templateName)
    {
        return templateName switch
        {
            "Greenfield Service Development" => CreateGreenfieldServiceTemplate(),
            "Greenfield Fullstack Development" => CreateGreenfieldFullstackTemplate(),
            "Greenfield UI Development" => CreateGreenfieldUITemplate(),
            "Brownfield Service Enhancement" => CreateBrownfieldServiceTemplate(),
            "Brownfield Fullstack Enhancement" => CreateBrownfieldFullstackTemplate(),
            "Brownfield UI Enhancement" => CreateBrownfieldUITemplate(),
            "Simple Sequential Workflow" => CreateSimpleSequentialTemplate(),
            "Parallel Processing Workflow" => CreateParallelProcessingTemplate(),
            "Conditional Decision Workflow" => CreateConditionalDecisionTemplate(),
            _ => null
        };
    }

    private Models.WorkflowDefinition CreateGreenfieldServiceTemplate()
    {
        var workflow = new Models.WorkflowDefinition
        {
            Name = "Greenfield Service Development",
            Description = "Complete workflow for building new backend services from concept to development",
            Status = Models.WorkflowStatus.Draft
        };

        // Create nodes with clear sequential positioning - spread out more
        var startNode = new EnhancedWorkflowNode { Id = "start", Name = "Start", Type = "StartNode", X = 250, Y = 450, InputPorts = [new() { Id = Guid.CreateVersion7().ToString() }],OutputPorts= [new() { Id = Guid.CreateVersion7().ToString() }] };
        var analystNode = new EnhancedWorkflowNode { Id = "analyst", Name = "Business Analyst", Type = "LLMAgent", X = 250, Y = 650 };
        var pmNode = new EnhancedWorkflowNode { Id = "pm", Name = "Product Manager", Type = "LLMAgent", X = 250, Y = 850 };
        var architectNode = new EnhancedWorkflowNode { Id = "architect", Name = "System Architect", Type = "LLMAgent", X = 250, Y = 1050 };
        var poNode = new EnhancedWorkflowNode { Id = "po", Name = "Product Owner", Type = "LLMAgent", X = 250, Y = 1250 };
        var endNode = new EnhancedWorkflowNode { Id = "end", Name = "End", Type = "EndNode", X = 250, Y = 1450 };

        workflow.Nodes = new List<EnhancedWorkflowNode> { startNode, analystNode, pmNode, architectNode, poNode, endNode };

        // Create connections
        workflow.Connections = new List<EnhancedWorkflowConnection>
        {
            new() { FromNodeId = startNode.Id, ToNodeId =analystNode.Id, FromPort = startNode.OutputPorts.FirstOrDefault()?.Id, ToPort = startNode.InputPorts.FirstOrDefault()?.Id, Label = "Start Analysis" },
            new() { FromNodeId = "analyst", ToNodeId = "pm", FromPort = "output1", ToPort = "input1", Label = "Requirements" },
            new() { FromNodeId = "pm", ToNodeId = "architect", FromPort = "output1", ToPort = "input1", Label = "Planning" },
            new() { FromNodeId = "architect", ToNodeId = "po", FromPort = "output1", ToPort = "input1", Label = "Architecture" },
            new() { FromNodeId = "po", ToNodeId = "end", FromPort = "output1", ToPort = "input1", Label = "Final Review" }
        };

        return workflow;
    }

    private Models.WorkflowDefinition CreateGreenfieldFullstackTemplate()
    {
        var workflow = new Models.WorkflowDefinition
        {
            Name = "Greenfield Fullstack Development",
            Description = "Complete workflow for building full-stack applications from concept to development",
            Status = Models.WorkflowStatus.Draft
        };

        // Create nodes with clear parallel flow positioning - spread out more
        var startNode = new EnhancedWorkflowNode { Id = "start", Name = "Start", Type = "StartNode", X = 250, Y = 450 };
        var analystNode = new EnhancedWorkflowNode { Id = "analyst", Name = "Business Analyst", Type = "LLMAgent", X = 400, Y = 450 };
        var pmNode = new EnhancedWorkflowNode { Id = "pm", Name = "Product Manager", Type = "LLMAgent", X = 550, Y = 450 };
        var uxNode = new EnhancedWorkflowNode { Id = "ux", Name = "UX Expert", Type = "LLMAgent", X = 700, Y = 350 };
        var architectNode = new EnhancedWorkflowNode { Id = "architect", Name = "System Architect", Type = "LLMAgent", X = 700, Y = 550 };
        var poNode = new EnhancedWorkflowNode { Id = "po", Name = "Product Owner", Type = "LLMAgent", X = 850, Y = 450 };
        var endNode = new EnhancedWorkflowNode { Id = "end", Name = "End", Type = "EndNode", X = 1000, Y = 450 };

        workflow.Nodes = new List<EnhancedWorkflowNode> { startNode, analystNode, pmNode, uxNode, architectNode, poNode, endNode };

        // Create connections
        workflow.Connections = new List<EnhancedWorkflowConnection>
        {
            new() { FromNodeId = "start", ToNodeId = "analyst", FromPort = "output1", ToPort = "input1", Label = "Start Analysis" },
            new() { FromNodeId = "analyst", ToNodeId = "pm", FromPort = "output1", ToPort = "input1", Label = "Requirements" },
            new() { FromNodeId = "pm", ToNodeId = "ux", FromPort = "output1", ToPort = "input1", Label = "UX Planning" },
            new() { FromNodeId = "pm", ToNodeId = "architect", FromPort = "output2", ToPort = "input1", Label = "Architecture Planning" },
            new() { FromNodeId = "ux", ToNodeId = "po", FromPort = "output1", ToPort = "input1", Label = "UX Design" },
            new() { FromNodeId = "architect", ToNodeId = "po", FromPort = "output1", ToPort = "input2", Label = "System Design" },
            new() { FromNodeId = "po", ToNodeId = "end", FromPort = "output1", ToPort = "input1", Label = "Final Review" }
        };

        return workflow;
    }

    private Models.WorkflowDefinition CreateGreenfieldUITemplate()
    {
        var workflow = new Models.WorkflowDefinition
        {
            Name = "Greenfield UI Development",
            Description = "Complete workflow for building frontend applications from concept to development",
            Status = Models.WorkflowStatus.Draft
        };

        // Create nodes with clear sequential positioning - spread out more
        var startNode = new EnhancedWorkflowNode { Id = "start", Name = "Start", Type = "StartNode", X = 250, Y = 450 };
        var analystNode = new EnhancedWorkflowNode { Id = "analyst", Name = "Business Analyst", Type = "LLMAgent", X = 400, Y = 450 };
        var pmNode = new EnhancedWorkflowNode { Id = "pm", Name = "Product Manager", Type = "LLMAgent", X = 550, Y = 450 };
        var uxNode = new EnhancedWorkflowNode { Id = "ux", Name = "UX Expert", Type = "LLMAgent", X = 700, Y = 450 };
        var architectNode = new EnhancedWorkflowNode { Id = "architect", Name = "Frontend Architect", Type = "LLMAgent", X = 850, Y = 450 };
        var endNode = new EnhancedWorkflowNode { Id = "end", Name = "End", Type = "EndNode", X = 1000, Y = 450 };

        workflow.Nodes = new List<EnhancedWorkflowNode> { startNode, analystNode, pmNode, uxNode, architectNode, endNode };

        // Create connections
        workflow.Connections = new List<EnhancedWorkflowConnection>
        {
            new() { FromNodeId = "start", ToNodeId = "analyst", FromPort = "output1", ToPort = "input1", Label = "Start Analysis" },
            new() { FromNodeId = "analyst", ToNodeId = "pm", FromPort = "output1", ToPort = "input1", Label = "Requirements" },
            new() { FromNodeId = "pm", ToNodeId = "ux", FromPort = "output1", ToPort = "input1", Label = "UX Planning" },
            new() { FromNodeId = "ux", ToNodeId = "architect", FromPort = "output1", ToPort = "input1", Label = "UX Design" },
            new() { FromNodeId = "architect", ToNodeId = "end", FromPort = "output1", ToPort = "input1", Label = "Frontend Architecture" }
        };

        return workflow;
    }

    private Models.WorkflowDefinition CreateBrownfieldServiceTemplate()
    {
        var workflow = new Models.WorkflowDefinition
        {
            Name = "Brownfield Service Enhancement",
            Description = "Workflow for enhancing existing backend services with new features",
            Status = Models.WorkflowStatus.Draft
        };

        // Create nodes with clear sequential positioning - spread out more
        var startNode = new EnhancedWorkflowNode { Id = "start", Name = "Start", Type = "StartNode", X = 250, Y = 450 };
        var analysisNode = new EnhancedWorkflowNode { Id = "analysis", Name = "Service Analysis", Type = "LLMAgent", X = 400, Y = 450 };
        var pmNode = new EnhancedWorkflowNode { Id = "pm", Name = "Product Manager", Type = "LLMAgent", X = 550, Y = 450 };
        var architectNode = new EnhancedWorkflowNode { Id = "architect", Name = "System Architect", Type = "LLMAgent", X = 700, Y = 450 };
        var poNode = new EnhancedWorkflowNode { Id = "po", Name = "Product Owner", Type = "LLMAgent", X = 850, Y = 450 };
        var endNode = new EnhancedWorkflowNode { Id = "end", Name = "End", Type = "EndNode", X = 1000, Y = 450 };

        workflow.Nodes = new List<EnhancedWorkflowNode> { startNode, analysisNode, pmNode, architectNode, poNode, endNode };

        // Create connections
        workflow.Connections = new List<EnhancedWorkflowConnection>
        {
            new() { FromNodeId = "start", ToNodeId = "analysis", FromPort = "output1", ToPort = "input1", Label = "Start Analysis" },
            new() { FromNodeId = "analysis", ToNodeId = "pm", FromPort = "output1", ToPort = "input1", Label = "Service Analysis" },
            new() { FromNodeId = "pm", ToNodeId = "architect", FromPort = "output1", ToPort = "input1", Label = "Planning" },
            new() { FromNodeId = "architect", ToNodeId = "po", FromPort = "output1", ToPort = "input1", Label = "Architecture" },
            new() { FromNodeId = "po", ToNodeId = "end", FromPort = "output1", ToPort = "input1", Label = "Final Review" }
        };

        return workflow;
    }

    private Models.WorkflowDefinition CreateBrownfieldFullstackTemplate()
    {
        var workflow = new Models.WorkflowDefinition
        {
            Name = "Brownfield Fullstack Enhancement",
            Description = "Workflow for enhancing existing full-stack applications",
            Status = Models.WorkflowStatus.Draft
        };

        // Create nodes with clear parallel flow positioning - spread out more
        var startNode = new EnhancedWorkflowNode { Id = "start", Name = "Start", Type = "StartNode", X = 250, Y = 450 };
        var analysisNode = new EnhancedWorkflowNode { Id = "analysis", Name = "System Analysis", Type = "LLMAgent", X = 400, Y = 450 };
        var pmNode = new EnhancedWorkflowNode { Id = "pm", Name = "Product Manager", Type = "LLMAgent", X = 550, Y = 450 };
        var uxNode = new EnhancedWorkflowNode { Id = "ux", Name = "UX Expert", Type = "LLMAgent", X = 700, Y = 350 };
        var architectNode = new EnhancedWorkflowNode { Id = "architect", Name = "System Architect", Type = "LLMAgent", X = 700, Y = 550 };
        var poNode = new EnhancedWorkflowNode { Id = "po", Name = "Product Owner", Type = "LLMAgent", X = 850, Y = 450 };
        var endNode = new EnhancedWorkflowNode { Id = "end", Name = "End", Type = "EndNode", X = 1000, Y = 450 };

        workflow.Nodes = new List<EnhancedWorkflowNode> { startNode, analysisNode, pmNode, uxNode, architectNode, poNode, endNode };

        // Create connections
        workflow.Connections = new List<EnhancedWorkflowConnection>
        {
            new() { FromNodeId = "start", ToNodeId = "analysis", FromPort = "output1", ToPort = "input1", Label = "Start Analysis" },
            new() { FromNodeId = "analysis", ToNodeId = "pm", FromPort = "output1", ToPort = "input1", Label = "System Analysis" },
            new() { FromNodeId = "pm", ToNodeId = "ux", FromPort = "output1", ToPort = "input1", Label = "UX Planning" },
            new() { FromNodeId = "pm", ToNodeId = "architect", FromPort = "output2", ToPort = "input1", Label = "Architecture Planning" },
            new() { FromNodeId = "ux", ToNodeId = "po", FromPort = "output1", ToPort = "input1", Label = "UX Enhancement" },
            new() { FromNodeId = "architect", ToNodeId = "po", FromPort = "output1", ToPort = "input2", Label = "System Enhancement" },
            new() { FromNodeId = "po", ToNodeId = "end", FromPort = "output1", ToPort = "input1", Label = "Final Review" }
        };

        return workflow;
    }

    private Models.WorkflowDefinition CreateBrownfieldUITemplate()
    {
        var workflow = new Models.WorkflowDefinition
        {
            Name = "Brownfield UI Enhancement",
            Description = "Workflow for enhancing existing frontend applications",
            Status = Models.WorkflowStatus.Draft
        };

        // Create nodes with clear sequential positioning - spread out more
        var startNode = new EnhancedWorkflowNode { Id = "start", Name = "Start", Type = "StartNode", X = 250, Y = 450 };
        var analysisNode = new EnhancedWorkflowNode { Id = "analysis", Name = "UI Analysis", Type = "LLMAgent", X = 400, Y = 450 };
        var pmNode = new EnhancedWorkflowNode { Id = "pm", Name = "Product Manager", Type = "LLMAgent", X = 550, Y = 450 };
        var uxNode = new EnhancedWorkflowNode { Id = "ux", Name = "UX Expert", Type = "LLMAgent", X = 700, Y = 450 };
        var architectNode = new EnhancedWorkflowNode { Id = "architect", Name = "Frontend Architect", Type = "LLMAgent", X = 850, Y = 450 };
        var endNode = new EnhancedWorkflowNode { Id = "end", Name = "End", Type = "EndNode", X = 1000, Y = 450 };

        workflow.Nodes = new List<EnhancedWorkflowNode> { startNode, analysisNode, pmNode, uxNode, architectNode, endNode };

        // Create connections
        workflow.Connections = new List<EnhancedWorkflowConnection>
        {
            new() { FromNodeId = "start", ToNodeId = "analysis", FromPort = "output1", ToPort = "input1", Label = "Start Analysis" },
            new() { FromNodeId = "analysis", ToNodeId = "pm", FromPort = "output1", ToPort = "input1", Label = "UI Analysis" },
            new() { FromNodeId = "pm", ToNodeId = "ux", FromPort = "output1", ToPort = "input1", Label = "UX Planning" },
            new() { FromNodeId = "ux", ToNodeId = "architect", FromPort = "output1", ToPort = "input1", Label = "UX Enhancement" },
            new() { FromNodeId = "architect", ToNodeId = "end", FromPort = "output1", ToPort = "input1", Label = "Frontend Enhancement" }
        };

        return workflow;
    }

    private Models.WorkflowDefinition CreateSimpleSequentialTemplate()
    {
        var workflow = new Models.WorkflowDefinition
        {
            Name = "Simple Sequential Workflow",
            Description = "Basic sequential workflow with start, process, and end nodes",
            Status = Models.WorkflowStatus.Draft
        };

        // Create nodes with clear linear flow - spread out more
        var startNode = new EnhancedWorkflowNode { Id = "start", Name = "Start", Type = "StartNode", X = 250, Y = 450 };
        var processNode = new EnhancedWorkflowNode { Id = "process", Name = "Process", Type = "LLMAgent", X = 550, Y = 450 };
        var endNode = new EnhancedWorkflowNode { Id = "end", Name = "End", Type = "EndNode", X = 850, Y = 450 };

        workflow.Nodes = new List<EnhancedWorkflowNode> { startNode, processNode, endNode };

        // Create connections
        workflow.Connections = new List<EnhancedWorkflowConnection>
        {
            new() { FromNodeId = "start", ToNodeId = "process", FromPort = "output1", ToPort = "input1", Label = "Start Process" },
            new() { FromNodeId = "process", ToNodeId = "end", FromPort = "output1", ToPort = "input1", Label = "Complete" }
        };

        return workflow;
    }

    private Models.WorkflowDefinition CreateParallelProcessingTemplate()
    {
        var workflow = new Models.WorkflowDefinition
        {
            Name = "Parallel Processing Workflow",
            Description = "Workflow with parallel processing capabilities",
            Status = Models.WorkflowStatus.Draft
        };

        // Create nodes with clear parallel processing flow - spread out more
        var startNode = new EnhancedWorkflowNode { Id = "start", Name = "Start", Type = "StartNode", X = 250, Y = 450 };
        var parallelNode = new EnhancedWorkflowNode { Id = "parallel", Name = "Parallel Processor", Type = "ParallelAgent", X = 450, Y = 450 };
        var process1Node = new EnhancedWorkflowNode { Id = "process1", Name = "Process 1", Type = "LLMAgent", X = 650, Y = 350 };
        var process2Node = new EnhancedWorkflowNode { Id = "process2", Name = "Process 2", Type = "LLMAgent", X = 650, Y = 550 };
        var mergeNode = new EnhancedWorkflowNode { Id = "merge", Name = "Merge Results", Type = "LLMAgent", X = 850, Y = 450 };
        var endNode = new EnhancedWorkflowNode { Id = "end", Name = "End", Type = "EndNode", X = 1050, Y = 450 };

        workflow.Nodes = new List<EnhancedWorkflowNode> { startNode, parallelNode, process1Node, process2Node, mergeNode, endNode };

        // Create connections
        workflow.Connections = new List<EnhancedWorkflowConnection>
        {
            new() { FromNodeId = "start", ToNodeId = "parallel", FromPort = "output1", ToPort = "input1", Label = "Start Parallel" },
            new() { FromNodeId = "parallel", ToNodeId = "process1", FromPort = "output1", ToPort = "input1", Label = "Branch 1" },
            new() { FromNodeId = "parallel", ToNodeId = "process2", FromPort = "output2", ToPort = "input1", Label = "Branch 2" },
            new() { FromNodeId = "process1", ToNodeId = "merge", FromPort = "output1", ToPort = "input1", Label = "Result 1" },
            new() { FromNodeId = "process2", ToNodeId = "merge", FromPort = "output1", ToPort = "input2", Label = "Result 2" },
            new() { FromNodeId = "merge", ToNodeId = "end", FromPort = "output1", ToPort = "input1", Label = "Merge Complete" }
        };

        return workflow;
    }

    private Models.WorkflowDefinition CreateConditionalDecisionTemplate()
    {
        var workflow = new Models.WorkflowDefinition
        {
            Name = "Conditional Decision Workflow",
            Description = "Workflow with conditional branching and decision points",
            Status = Models.WorkflowStatus.Draft
        };

        // Create nodes with clear conditional branching flow - spread out more
        var startNode = new EnhancedWorkflowNode { Id = "start", Name = "Start", Type = "StartNode", X = 250, Y = 450 };
        var decisionNode = new EnhancedWorkflowNode { Id = "decision", Name = "Decision Point", Type = "ConditionalAgent", X = 450, Y = 450 };
        var path1Node = new EnhancedWorkflowNode { Id = "path1", Name = "Path A", Type = "LLMAgent", X = 650, Y = 350 };
        var path2Node = new EnhancedWorkflowNode { Id = "path2", Name = "Path B", Type = "LLMAgent", X = 650, Y = 550 };
        var mergeNode = new EnhancedWorkflowNode { Id = "merge", Name = "Merge", Type = "LLMAgent", X = 850, Y = 450 };
        var endNode = new EnhancedWorkflowNode { Id = "end", Name = "End", Type = "EndNode", X = 1050, Y = 450 };

        workflow.Nodes = new List<EnhancedWorkflowNode> { startNode, decisionNode, path1Node, path2Node, mergeNode, endNode };

        // Create connections
        workflow.Connections = new List<EnhancedWorkflowConnection>
        {
            new() { FromNodeId = "start", ToNodeId = "decision", FromPort = "output1", ToPort = "input1", Label = "Start Decision" },
            new() { FromNodeId = "decision", ToNodeId = "path1", FromPort = "output1", ToPort = "input1", Label = "Path A" },
            new() { FromNodeId = "decision", ToNodeId = "path2", FromPort = "output2", ToPort = "input1", Label = "Path B" },
            new() { FromNodeId = "path1", ToNodeId = "merge", FromPort = "output1", ToPort = "input1", Label = "Result A" },
            new() { FromNodeId = "path2", ToNodeId = "merge", FromPort = "output1", ToPort = "input2", Label = "Result B" },
            new() { FromNodeId = "merge", ToNodeId = "end", FromPort = "output1", ToPort = "input1", Label = "Merge Complete" }
        };

        return workflow;
    }

    // Microsoft Agent Framework Orchestration Methods

    /// <summary>
    /// Execute workflow using Microsoft Agent Framework orchestration
    /// </summary>
    public async Task<WorkflowExecution> ExecuteWorkflowWithOrchestrationAsync(
        string workflowId, 
        Dictionary<string, object> inputData, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing workflow {WorkflowId} with Microsoft Agent Framework orchestration", workflowId);

            var workflow = await GetWorkflowAsync(workflowId);
            if (workflow == null)
            {
                throw new ArgumentException($"Workflow {workflowId} not found");
            }

            // Ensure workflow has orchestration configuration
            if (workflow.OrchestrationConfig == null)
            {
                workflow.OrchestrationConfig = new MicrosoftAgentFrameworkOrchestrationConfiguration
                {
                    OrchestrationType = workflow.OrchestrationType.ToString().ToLower(),
                    MaxConcurrentExecutions = 5,
                    ExecutionTimeout = TimeSpan.FromMinutes(10)
                };
            }

            // Execute using orchestration service
            var execution = await _orchestrationService.ExecuteWorkflowAsync(workflow, inputData, cancellationToken);

            _logger.LogInformation("Completed workflow {WorkflowId} execution with status {Status}", 
                workflowId, execution.Status);

            return execution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow {WorkflowId} with orchestration", workflowId);
            throw;
        }
    }

    /// <summary>
    /// Get workflow execution by ID
    /// </summary>
    public async Task<WorkflowExecution?> GetWorkflowExecutionAsync(string executionId)
    {
        try
        {
            return _orchestrationService.GetActiveExecution(executionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow execution {ExecutionId}", executionId);
            return null;
        }
    }

    /// <summary>
    /// Cancel workflow execution
    /// </summary>
    public async Task<bool> CancelWorkflowExecutionAsync(string executionId)
    {
        try
        {
            _logger.LogInformation("Cancelling workflow execution {ExecutionId}", executionId);
            return await _orchestrationService.CancelExecutionAsync(executionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling workflow execution {ExecutionId}", executionId);
            return false;
        }
    }

    /// <summary>
    /// Get all active workflow executions
    /// </summary>
    public async Task<List<WorkflowExecution>> GetActiveWorkflowExecutionsAsync()
    {
        try
        {
            return _orchestrationService.GetActiveExecutions();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active workflow executions");
            return new List<WorkflowExecution>();
        }
    }

    /// <summary>
    /// Configure workflow orchestration settings
    /// </summary>
    public async Task<bool> ConfigureWorkflowOrchestrationAsync(
        string workflowId, 
        WorkflowOrchestrationType orchestrationType,
        MicrosoftAgentFrameworkOrchestrationConfiguration? orchestrationConfig = null)
    {
        try
        {
            _logger.LogInformation("Configuring orchestration for workflow {WorkflowId} with type {OrchestrationType}", 
                workflowId, orchestrationType);

            var workflow = await GetWorkflowAsync(workflowId);
            if (workflow == null)
            {
                return false;
            }

            workflow.OrchestrationType = orchestrationType;
            workflow.OrchestrationConfig = orchestrationConfig ?? new MicrosoftAgentFrameworkOrchestrationConfiguration
            {
                OrchestrationType = orchestrationType.ToString().ToLower(),
                MaxConcurrentExecutions = 5,
                ExecutionTimeout = TimeSpan.FromMinutes(10)
            };

            await UpdateWorkflowAsync(workflow);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring orchestration for workflow {WorkflowId}", workflowId);
            return false;
        }
    }

    /// <summary>
    /// Add orchestration step to workflow
    /// </summary>
    public async Task<bool> AddOrchestrationStepAsync(
        string workflowId, 
        WorkflowOrchestrationStep orchestrationStep)
    {
        try
        {
            _logger.LogInformation("Adding orchestration step to workflow {WorkflowId}", workflowId);

            var workflow = await GetWorkflowAsync(workflowId);
            if (workflow == null)
            {
                return false;
            }

            workflow.OrchestrationSteps.Add(orchestrationStep);
            await UpdateWorkflowAsync(workflow);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding orchestration step to workflow {WorkflowId}", workflowId);
            return false;
        }
    }

    /// <summary>
    /// Update orchestration step in workflow
    /// </summary>
    public async Task<bool> UpdateOrchestrationStepAsync(
        string workflowId, 
        string stepId, 
        WorkflowOrchestrationStep updatedStep)
    {
        try
        {
            _logger.LogInformation("Updating orchestration step {StepId} in workflow {WorkflowId}", stepId, workflowId);

            var workflow = await GetWorkflowAsync(workflowId);
            if (workflow == null)
            {
                return false;
            }

            var stepIndex = workflow.OrchestrationSteps.FindIndex(s => s.Id == stepId);
            if (stepIndex == -1)
            {
                return false;
            }

            workflow.OrchestrationSteps[stepIndex] = updatedStep;
            await UpdateWorkflowAsync(workflow);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating orchestration step {StepId} in workflow {WorkflowId}", stepId, workflowId);
            return false;
        }
    }

    /// <summary>
    /// Remove orchestration step from workflow
    /// </summary>
    public async Task<bool> RemoveOrchestrationStepAsync(string workflowId, string stepId)
    {
        try
        {
            _logger.LogInformation("Removing orchestration step {StepId} from workflow {WorkflowId}", stepId, workflowId);

            var workflow = await GetWorkflowAsync(workflowId);
            if (workflow == null)
            {
                return false;
            }

            var removed = workflow.OrchestrationSteps.RemoveAll(s => s.Id == stepId) > 0;
            if (removed)
            {
                await UpdateWorkflowAsync(workflow);
            }

            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing orchestration step {StepId} from workflow {WorkflowId}", stepId, workflowId);
            return false;
        }
    }
}

public enum ConnectionType
{
    DataFlow,
    ControlFlow,
    Conditional
}
