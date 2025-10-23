using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;
using System.Text.Json;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Microsoft Agent Framework-based agent visualization service
/// Replaces custom AgentVisualizationService with official framework patterns
/// Based on: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
/// </summary>
public class MicrosoftAgentFrameworkVisualizationService : IAgentVisualizationService
{
    private readonly ILogger<MicrosoftAgentFrameworkVisualizationService> _logger;
    private readonly IChatService _chatService;
    private readonly Dictionary<string, ChatClientAgent> _visualizationAgents = new();
    private readonly List<WorkflowDefinition> _workflows = new();

    public MicrosoftAgentFrameworkVisualizationService(
        ILogger<MicrosoftAgentFrameworkVisualizationService> logger,
        IChatService chatService)
    {
        _logger = logger;
        _chatService = chatService;
        InitializeMicrosoftAgentFrameworkVisualization();
    }

    /// <summary>
    /// Generates workflow diagrams using Microsoft Agent Framework visualization capabilities
    /// Based on official samples: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
    /// </summary>
    public async Task<string> GenerateWorkflowDiagramAsync(List<WorkflowNode> nodes, List<WorkflowConnection> connections)
    {
        try
        {
            _logger.LogInformation("Generating Microsoft Agent Framework workflow diagram for {NodeCount} nodes and {ConnectionCount} connections", 
                nodes.Count, connections.Count);

            // Use Microsoft Agent Framework for intelligent diagram generation
            var diagramAgent = await GetOrCreateVisualizationAgentAsync("diagram-generator");
            if (diagramAgent == null)
            {
                // Fallback to enhanced Mermaid generation
                return await GenerateEnhancedMermaidDiagramAsync(nodes, connections);
            }

            // Generate diagram using Microsoft Agent Framework
            var diagramPrompt = CreateDiagramGenerationPrompt(nodes, connections);
            var diagramResult = await GenerateDiagramWithMicrosoftAgentFrameworkAsync(diagramAgent, diagramPrompt);
            
            _logger.LogInformation("Microsoft Agent Framework generated workflow diagram successfully");
            return diagramResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Microsoft Agent Framework workflow diagram");
            // Fallback to enhanced Mermaid generation
            return await GenerateEnhancedMermaidDiagramAsync(nodes, connections);
        }
    }

    /// <summary>
    /// Gets workflow nodes using Microsoft Agent Framework data processing
    /// </summary>
    public async Task<List<WorkflowNode>> GetWorkflowNodesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving workflow nodes using Microsoft Agent Framework");

            // Use Microsoft Agent Framework for intelligent data processing
            var dataAgent = await GetOrCreateVisualizationAgentAsync("data-processor");
            if (dataAgent != null)
            {
                var processedNodes = await ProcessNodesWithMicrosoftAgentFrameworkAsync(dataAgent, _workflows.SelectMany(w => w.Nodes).ToList());
                return processedNodes;
            }

            // Fallback to sample data
            return await GetSampleWorkflowNodesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow nodes with Microsoft Agent Framework");
            return await GetSampleWorkflowNodesAsync();
        }
    }

    /// <summary>
    /// Gets workflow connections using Microsoft Agent Framework data processing
    /// </summary>
    public async Task<List<WorkflowConnection>> GetWorkflowConnectionsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving workflow connections using Microsoft Agent Framework");

            // Use Microsoft Agent Framework for intelligent data processing
            var dataAgent = await GetOrCreateVisualizationAgentAsync("data-processor");
            if (dataAgent != null)
            {
                var processedConnections = await ProcessConnectionsWithMicrosoftAgentFrameworkAsync(dataAgent, _workflows.SelectMany(w => w.Connections).ToList());
                return processedConnections;
            }

            // Fallback to sample data
            return await GetSampleWorkflowConnectionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow connections with Microsoft Agent Framework");
            return await GetSampleWorkflowConnectionsAsync();
        }
    }

    /// <summary>
    /// Saves workflow using Microsoft Agent Framework data management
    /// </summary>
    public async Task SaveWorkflowAsync(string name, List<EnhancedWorkflowNode> nodes, List<EnhancedWorkflowConnection> connections)
    {
        try
        {
            _logger.LogInformation("Saving workflow '{WorkflowName}' using Microsoft Agent Framework", name);

            // Use Microsoft Agent Framework for intelligent workflow management
            var workflowAgent = await GetOrCreateVisualizationAgentAsync("workflow-manager");
            if (workflowAgent != null)
            {
                await SaveWorkflowWithMicrosoftAgentFrameworkAsync(workflowAgent, name, nodes, connections);
            }

            // Also save to local collection
            var workflow = new WorkflowDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = $"Microsoft Agent Framework workflow: {name}",
                Nodes = nodes,
                Connections = connections,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = WorkflowStatus.Draft
            };
            
            _workflows.Add(workflow);
            _logger.LogInformation("Microsoft Agent Framework saved workflow '{WorkflowName}' successfully", name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving workflow '{WorkflowName}' with Microsoft Agent Framework", name);
            throw;
        }
    }

    /// <summary>
    /// Loads workflow using Microsoft Agent Framework data retrieval
    /// </summary>
    public async Task<WorkflowDefinition> LoadWorkflowAsync(string id)
    {
        try
        {
            _logger.LogInformation("Loading workflow '{WorkflowId}' using Microsoft Agent Framework", id);

            // Use Microsoft Agent Framework for intelligent data retrieval
            var workflowAgent = await GetOrCreateVisualizationAgentAsync("workflow-manager");
            if (workflowAgent != null)
            {
                var workflow = await LoadWorkflowWithMicrosoftAgentFrameworkAsync(workflowAgent, id);
                if (workflow != null)
                {
                    return workflow;
                }
            }

            // Fallback to local collection
            var localWorkflow = _workflows.FirstOrDefault(w => w.Id == id);
            return localWorkflow ?? new WorkflowDefinition();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workflow '{WorkflowId}' with Microsoft Agent Framework", id);
            return new WorkflowDefinition();
        }
    }

    /// <summary>
    /// Initializes Microsoft Agent Framework visualization capabilities
    /// </summary>
    private void InitializeMicrosoftAgentFrameworkVisualization()
    {
        try
        {
            _logger.LogInformation("Initializing Microsoft Agent Framework visualization service");
            
            // Initialize sample workflows with Microsoft Agent Framework patterns
            InitializeSampleMicrosoftAgentFrameworkWorkflows();
            
            _logger.LogInformation("Microsoft Agent Framework visualization service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Microsoft Agent Framework visualization service");
        }
    }

    /// <summary>
    /// Gets or creates a Microsoft Agent Framework visualization agent
    /// </summary>
    private async Task<ChatClientAgent?> GetOrCreateVisualizationAgentAsync(string agentType)
    {
        try
        {
            if (_visualizationAgents.TryGetValue(agentType, out var cachedAgent))
            {
                return cachedAgent;
            }

            // Create Microsoft Agent Framework ChatClientAgent for visualization
            var chatClient = await CreateVisualizationChatClientAsync();
            if (chatClient == null)
            {
                _logger.LogWarning("Failed to create chat client for visualization agent {AgentType}", agentType);
                return null;
            }

            var agent = new ChatClientAgent(
                chatClient: chatClient,
                name: $"Microsoft Agent Framework {agentType}",
                instructions: GetVisualizationAgentInstructions(agentType)
            );

            // Register visualization-specific tools
            await RegisterVisualizationToolsAsync(agent, agentType);

            _visualizationAgents[agentType] = agent;
            _logger.LogInformation("Created Microsoft Agent Framework visualization agent: {AgentType}", agentType);
            return agent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Microsoft Agent Framework visualization agent {AgentType}", agentType);
            return null;
        }
    }

    /// <summary>
    /// Creates a chat client for visualization
    /// </summary>
    private async Task<IChatClient?> CreateVisualizationChatClientAsync()
    {
        try
        {
            // This would integrate with your existing chat client creation logic
            // For now, return null to indicate this needs implementation
            _logger.LogInformation("Creating Microsoft Agent Framework visualization chat client");
            return null; // TODO: Implement based on existing ChatService logic
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Microsoft Agent Framework visualization chat client");
            return null;
        }
    }

    /// <summary>
    /// Gets instructions for visualization agents
    /// </summary>
    private string GetVisualizationAgentInstructions(string agentType)
    {
        return agentType switch
        {
            "diagram-generator" => "You are a Microsoft Agent Framework diagram generation specialist. Generate clear, well-structured workflow diagrams using Mermaid syntax. Focus on readability and proper flow representation.",
            "data-processor" => "You are a Microsoft Agent Framework data processing specialist. Process workflow nodes and connections intelligently, ensuring data integrity and proper relationships.",
            "workflow-manager" => "You are a Microsoft Agent Framework workflow management specialist. Manage workflow lifecycle, validation, and optimization using framework best practices.",
            _ => $"You are a Microsoft Agent Framework {agentType} specialist. Use framework patterns and best practices for optimal results."
        };
    }

    /// <summary>
    /// Registers visualization-specific tools with Microsoft Agent Framework agent
    /// </summary>
    private async Task RegisterVisualizationToolsAsync(ChatClientAgent agent, string agentType)
    {
        try
        {
            // Register visualization tools using Microsoft Agent Framework patterns
            _logger.LogInformation("Registering visualization tools for Microsoft Agent Framework agent: {AgentType}", agentType);
            
            // TODO: Implement tool registration based on Microsoft Agent Framework patterns
            // This would use AIFunctionFactory.Create() and tool registration APIs
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering visualization tools for Microsoft Agent Framework agent {AgentType}", agentType);
        }
    }

    /// <summary>
    /// Generates diagram using Microsoft Agent Framework
    /// </summary>
    private async Task<string> GenerateDiagramWithMicrosoftAgentFrameworkAsync(ChatClientAgent agent, string prompt)
    {
        try
        {
            // Use Microsoft Agent Framework for intelligent diagram generation
            _logger.LogInformation("Generating diagram using Microsoft Agent Framework");
            
            // TODO: Implement actual diagram generation using Microsoft Agent Framework
            // This would use the agent's built-in methods for processing
            return await GenerateEnhancedMermaidDiagramAsync(new List<WorkflowNode>(), new List<WorkflowConnection>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating diagram with Microsoft Agent Framework");
            return await GenerateEnhancedMermaidDiagramAsync(new List<WorkflowNode>(), new List<WorkflowConnection>());
        }
    }

    /// <summary>
    /// Creates diagram generation prompt
    /// </summary>
    private string CreateDiagramGenerationPrompt(List<WorkflowNode> nodes, List<WorkflowConnection> connections)
    {
        var prompt = new System.Text.StringBuilder();
        prompt.AppendLine("Generate a Microsoft Agent Framework workflow diagram with the following components:");
        prompt.AppendLine();
        
        prompt.AppendLine("Nodes:");
        foreach (var node in nodes)
        {
            prompt.AppendLine($"- {node.Name} ({node.Type}) at position ({node.X}, {node.Y})");
        }
        
        prompt.AppendLine();
        prompt.AppendLine("Connections:");
        foreach (var connection in connections)
        {
            prompt.AppendLine($"- From {connection.FromNodeId} to {connection.ToNodeId}");
        }
        
        prompt.AppendLine();
        prompt.AppendLine("Use Microsoft Agent Framework patterns and best practices for the diagram structure.");
        
        return prompt.ToString();
    }

    /// <summary>
    /// Processes nodes using Microsoft Agent Framework
    /// </summary>
    private async Task<List<WorkflowNode>> ProcessNodesWithMicrosoftAgentFrameworkAsync(ChatClientAgent agent, List<EnhancedWorkflowNode> nodes)
    {
        try
        {
            // Use Microsoft Agent Framework for intelligent node processing
            _logger.LogInformation("Processing {NodeCount} nodes using Microsoft Agent Framework", nodes.Count);
            
            // TODO: Implement actual node processing using Microsoft Agent Framework
            return nodes.Select(n => new WorkflowNode
            {
                Id = n.Id,
                Name = n.Name,
                Type = n.Type,
                X = n.X,
                Y = n.Y
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing nodes with Microsoft Agent Framework");
            return await GetSampleWorkflowNodesAsync();
        }
    }

    /// <summary>
    /// Processes connections using Microsoft Agent Framework
    /// </summary>
    private async Task<List<WorkflowConnection>> ProcessConnectionsWithMicrosoftAgentFrameworkAsync(ChatClientAgent agent, List<EnhancedWorkflowConnection> connections)
    {
        try
        {
            // Use Microsoft Agent Framework for intelligent connection processing
            _logger.LogInformation("Processing {ConnectionCount} connections using Microsoft Agent Framework", connections.Count);
            
            // TODO: Implement actual connection processing using Microsoft Agent Framework
            return connections.Select(c => new WorkflowConnection
            {
                Id = c.Id,
                FromNodeId = c.FromNodeId,
                ToNodeId = c.ToNodeId,
                Label = c.Type.ToString()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing connections with Microsoft Agent Framework");
            return await GetSampleWorkflowConnectionsAsync();
        }
    }

    /// <summary>
    /// Saves workflow using Microsoft Agent Framework
    /// </summary>
    private async Task SaveWorkflowWithMicrosoftAgentFrameworkAsync(ChatClientAgent agent, string name, List<EnhancedWorkflowNode> nodes, List<EnhancedWorkflowConnection> connections)
    {
        try
        {
            // Use Microsoft Agent Framework for intelligent workflow saving
            _logger.LogInformation("Saving workflow '{WorkflowName}' using Microsoft Agent Framework", name);
            
            // TODO: Implement actual workflow saving using Microsoft Agent Framework
            // This would use the agent's built-in methods for workflow management
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving workflow '{WorkflowName}' with Microsoft Agent Framework", name);
        }
    }

    /// <summary>
    /// Loads workflow using Microsoft Agent Framework
    /// </summary>
    private async Task<WorkflowDefinition?> LoadWorkflowWithMicrosoftAgentFrameworkAsync(ChatClientAgent agent, string id)
    {
        try
        {
            // Use Microsoft Agent Framework for intelligent workflow loading
            _logger.LogInformation("Loading workflow '{WorkflowId}' using Microsoft Agent Framework", id);
            
            // TODO: Implement actual workflow loading using Microsoft Agent Framework
            // This would use the agent's built-in methods for workflow retrieval
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workflow '{WorkflowId}' with Microsoft Agent Framework", id);
            return null;
        }
    }

    /// <summary>
    /// Generates enhanced Mermaid diagram as fallback
    /// </summary>
    private async Task<string> GenerateEnhancedMermaidDiagramAsync(List<WorkflowNode> nodes, List<WorkflowConnection> connections)
    {
        try
        {
            var diagram = new System.Text.StringBuilder();
            diagram.AppendLine("graph TD");
            diagram.AppendLine("    %% Microsoft Agent Framework Workflow Diagram");
            diagram.AppendLine("    %% Generated using Microsoft Agent Framework patterns");
            diagram.AppendLine();
            
            // Add Microsoft Agent Framework styling
            diagram.AppendLine("    %% Microsoft Agent Framework node styling");
            diagram.AppendLine("    classDef microsoftAgent fill:#0078d4,stroke:#106ebe,stroke-width:2px,color:#fff");
            diagram.AppendLine("    classDef microsoftTool fill:#107c10,stroke:#0d5d0d,stroke-width:2px,color:#fff");
            diagram.AppendLine("    classDef microsoftConditional fill:#d83b01,stroke:#a52a00,stroke-width:2px,color:#fff");
            diagram.AppendLine("    classDef microsoftParallel fill:#8764b8,stroke:#6b46a3,stroke-width:2px,color:#fff");
            diagram.AppendLine();
            
            foreach (var node in nodes)
            {
                var nodeId = node.Id.Replace("-", "");
                var nodeLabel = $"{node.Name}\\n({node.Type})";
                var nodeClass = GetMicrosoftAgentFrameworkNodeClass(node.Type);
                
                diagram.AppendLine($"    {nodeId}[\"{nodeLabel}\"]");
                diagram.AppendLine($"    class {nodeId} {nodeClass}");
            }
            
            diagram.AppendLine();
            
            foreach (var connection in connections)
            {
                var sourceId = connection.FromNodeId.Replace("-", "");
                var targetId = connection.ToNodeId.Replace("-", "");
                var connectionLabel = !string.IsNullOrEmpty(connection.Label) ? $"|{connection.Label}|" : "";
                
                diagram.AppendLine($"    {sourceId} -->{connectionLabel} {targetId}");
            }
            
            diagram.AppendLine();
            diagram.AppendLine("    %% Microsoft Agent Framework workflow completed");
            
            return diagram.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating enhanced Mermaid diagram");
            return "graph TD\n    Error[\"Error generating diagram\"]";
        }
    }

    /// <summary>
    /// Gets Microsoft Agent Framework node class for styling
    /// </summary>
    private string GetMicrosoftAgentFrameworkNodeClass(string nodeType)
    {
        return nodeType switch
        {
            "LLMAgent" => "microsoftAgent",
            "ToolAgent" => "microsoftTool",
            "ConditionalAgent" => "microsoftConditional",
            "ParallelAgent" => "microsoftParallel",
            _ => "microsoftAgent"
        };
    }

    /// <summary>
    /// Initializes sample Microsoft Agent Framework workflows
    /// </summary>
    private void InitializeSampleMicrosoftAgentFrameworkWorkflows()
    {
        try
        {
            var sampleWorkflow = new WorkflowDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Microsoft Agent Framework Sample Workflow",
                Description = "Sample workflow demonstrating Microsoft Agent Framework patterns",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = WorkflowStatus.Published,
                Nodes = new List<EnhancedWorkflowNode>
                {
                    new EnhancedWorkflowNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Start",
                        Type = "StartNode",
                        X = 100,
                        Y = 100
                    },
                    new EnhancedWorkflowNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Microsoft Agent Framework LLM Agent",
                        Type = "LLMAgent",
                        X = 300,
                        Y = 100
                    },
                    new EnhancedWorkflowNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Microsoft Agent Framework Tool Agent",
                        Type = "ToolAgent",
                        X = 500,
                        Y = 100
                    },
                    new EnhancedWorkflowNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "End",
                        Type = "EndNode",
                        X = 700,
                        Y = 100
                    }
                },
                Connections = new List<EnhancedWorkflowConnection>
                {
                    new EnhancedWorkflowConnection
                    {
                        Id = Guid.NewGuid().ToString(),
                        FromNodeId = "", // Will be set after nodes are created
                        ToNodeId = "",
                        ConnectionType = Models.ConnectionType.DataFlow,
                        Label = "Microsoft Agent Framework Flow"
                    }
                }
            };

            // Set connection references
            if (sampleWorkflow.Nodes.Count >= 4)
            {
                sampleWorkflow.Connections[0].FromNodeId = sampleWorkflow.Nodes[0].Id;
                sampleWorkflow.Connections[0].ToNodeId = sampleWorkflow.Nodes[1].Id;
            }

            _workflows.Add(sampleWorkflow);
            _logger.LogInformation("Initialized Microsoft Agent Framework sample workflow");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Microsoft Agent Framework sample workflows");
        }
    }

    /// <summary>
    /// Gets sample workflow nodes
    /// </summary>
    private async Task<List<WorkflowNode>> GetSampleWorkflowNodesAsync()
    {
        return await Task.FromResult(new List<WorkflowNode>
        {
            new WorkflowNode
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Microsoft Agent Framework Start",
                Type = "StartNode",
                X = 100,
                Y = 100
            },
            new WorkflowNode
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Microsoft Agent Framework LLM Agent",
                Type = "LLMAgent",
                X = 300,
                Y = 100
            },
            new WorkflowNode
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Microsoft Agent Framework Tool Agent",
                Type = "ToolAgent",
                X = 500,
                Y = 100
            },
            new WorkflowNode
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Microsoft Agent Framework End",
                Type = "EndNode",
                X = 700,
                Y = 100
            }
        });
    }

    /// <summary>
    /// Gets sample workflow connections
    /// </summary>
    private async Task<List<WorkflowConnection>> GetSampleWorkflowConnectionsAsync()
    {
        var nodes = await GetSampleWorkflowNodesAsync();
        if (nodes.Count >= 4)
        {
            return await Task.FromResult(new List<WorkflowConnection>
            {
                new WorkflowConnection
                {
                    Id = Guid.NewGuid().ToString(),
                    FromNodeId = nodes[0].Id,
                    ToNodeId = nodes[1].Id,
                    Label = "Microsoft Agent Framework Flow"
                },
                new WorkflowConnection
                {
                    Id = Guid.NewGuid().ToString(),
                    FromNodeId = nodes[1].Id,
                    ToNodeId = nodes[2].Id,
                    Label = "Tool Execution"
                },
                new WorkflowConnection
                {
                    Id = Guid.NewGuid().ToString(),
                    FromNodeId = nodes[2].Id,
                    ToNodeId = nodes[3].Id,
                    Label = "Complete"
                }
            });
        }
        
        return await Task.FromResult(new List<WorkflowConnection>());
    }
}
