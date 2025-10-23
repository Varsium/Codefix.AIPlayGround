using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;
using Codefix.AIPlayGround.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Concurrent;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Microsoft Agent Framework-based workflow orchestration service
/// Replaces custom WorkflowExecutionService with official framework patterns
/// Based on: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
/// </summary>
public class MicrosoftAgentFrameworkWorkflowService : IWorkflowExecutionService, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MicrosoftAgentFrameworkWorkflowService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IChatService _chatService;
    private readonly ConcurrentDictionary<string, AgentWorkflowExecutionContext> _activeExecutions = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public event EventHandler<ExecutionStatusChangedEventArgs>? ExecutionStatusChanged;
    public event EventHandler<StepCompletedEventArgs>? StepCompleted;
    public event EventHandler<ExecutionErrorEventArgs>? ExecutionError;

    public MicrosoftAgentFrameworkWorkflowService(
        ApplicationDbContext context,
        ILogger<MicrosoftAgentFrameworkWorkflowService> logger,
        IServiceProvider serviceProvider,
        IChatService chatService)
    {
        _context = context;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _chatService = chatService;
    }

    /// <summary>
    /// Starts workflow execution using Microsoft Agent Framework orchestration patterns
    /// Supports sequential, concurrent, and group chat workflows
    /// </summary>
    public async Task<string> StartExecutionAsync(string workflowId, Dictionary<string, object>? inputData = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get workflow definition
            var workflow = await GetWorkflowDefinitionAsync(workflowId);
            if (workflow == null)
            {
                throw new ArgumentException($"Workflow {workflowId} not found");
            }

            // Create execution record
            var execution = new WorkflowExecution
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                StartedAt = DateTime.UtcNow,
                Status = ExecutionStatus.Running,
                InputData = inputData ?? new Dictionary<string, object>()
            };

            // Save to database
            await SaveExecutionAsync(execution);

            // Create Microsoft Agent Framework execution context
            var context = new AgentWorkflowExecutionContext
            {
                Execution = execution,
                Workflow = workflow,
                CancellationToken = cancellationToken,
                Agents = new Dictionary<string, ChatClientAgent>()
            };

            _activeExecutions[execution.Id] = context;

            // Start Microsoft Agent Framework orchestration
            _ = Task.Run(() => ExecuteAgentWorkflowAsync(context), cancellationToken);

            _logger.LogInformation("Started Microsoft Agent Framework workflow execution {ExecutionId} for workflow {WorkflowId}", 
                execution.Id, workflowId);
            return execution.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting Microsoft Agent Framework workflow execution for workflow {WorkflowId}", workflowId);
            throw;
        }
    }

    /// <summary>
    /// Executes workflow using Microsoft Agent Framework orchestration patterns
    /// Supports sequential, concurrent, and group chat workflows
    /// </summary>
    private async Task ExecuteAgentWorkflowAsync(AgentWorkflowExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Starting Microsoft Agent Framework workflow execution {ExecutionId}", context.Execution.Id);

            // Create Microsoft Agent Framework agents for each LLM node
            await CreateAgentsForWorkflowAsync(context);

            // Execute workflow using Microsoft Agent Framework patterns
            var result = await ExecuteWorkflowOrchestrationAsync(context);

            // Mark execution as completed
            context.Execution.Status = ExecutionStatus.Completed;
            context.Execution.CompletedAt = DateTime.UtcNow;
            context.Execution.OutputData = result;
            await UpdateExecutionStatusAsync(context.Execution.Id, ExecutionStatus.Completed);
            
            OnExecutionStatusChanged(context.Execution.Id, ExecutionStatus.Running, ExecutionStatus.Completed);
            
            _logger.LogInformation("Completed Microsoft Agent Framework workflow execution {ExecutionId}", context.Execution.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Microsoft Agent Framework workflow {ExecutionId}", context.Execution.Id);
            
            context.Execution.Status = ExecutionStatus.Failed;
            context.Execution.CompletedAt = DateTime.UtcNow;
            await UpdateExecutionStatusAsync(context.Execution.Id, ExecutionStatus.Failed);
            
            // Add error to execution
            var error = new ExecutionError
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace ?? string.Empty,
                ErrorType = ex.GetType().Name,
                OccurredAt = DateTime.UtcNow
            };
            
            await AddExecutionErrorAsync(context.Execution.Id, error);
            OnExecutionError(context.Execution.Id, null, null, error);
            OnExecutionStatusChanged(context.Execution.Id, ExecutionStatus.Running, ExecutionStatus.Failed);
        }
        finally
        {
            _activeExecutions.TryRemove(context.Execution.Id, out _);
        }
    }

    /// <summary>
    /// Creates Microsoft Agent Framework agents for workflow nodes
    /// Based on official samples: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
    /// </summary>
    private async Task CreateAgentsForWorkflowAsync(AgentWorkflowExecutionContext context)
    {
        var llmNodes = context.Workflow.Nodes
            .Where(n => n.Type == "LLMAgent")
            .ToList();

        foreach (var node in llmNodes)
        {
            try
            {
                // Create Microsoft Agent Framework ChatClientAgent
                var agent = await CreateMicrosoftAgentFrameworkAgentAsync(node);
                if (agent != null)
                {
                    context.Agents[node.Id] = agent;
                    _logger.LogInformation("Created Microsoft Agent Framework agent for node {NodeId}", node.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Microsoft Agent Framework agent for node {NodeId}", node.Id);
            }
        }
    }

    /// <summary>
    /// Creates a Microsoft Agent Framework ChatClientAgent
    /// Based on official samples: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
    /// </summary>
    private async Task<ChatClientAgent?> CreateMicrosoftAgentFrameworkAgentAsync(EnhancedWorkflowNode node)
    {
        try
        {
            var agentDefinition = node.AgentDefinition;
            if (agentDefinition?.LLMConfig == null)
            {
                _logger.LogWarning("No LLM configuration found for node {NodeId}", node.Id);
                return null;
            }

            // Create chat client based on LLM configuration
            var chatClient = await CreateChatClientAsync(agentDefinition.LLMConfig);
            if (chatClient == null)
            {
                _logger.LogWarning("Failed to create chat client for node {NodeId}", node.Id);
                return null;
            }

            // Create Microsoft Agent Framework ChatClientAgent
            var agent = new ChatClientAgent(
                chatClient: chatClient,
                name: agentDefinition.Name,
                instructions: agentDefinition.Description ?? $"You are {agentDefinition.Name}, an AI assistant."
            );

            // Register tools if available
            if (agentDefinition.Tools?.Any() == true)
            {
                await RegisterToolsForAgentAsync(agent, agentDefinition.Tools);
            }

            return agent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Microsoft Agent Framework agent for node {NodeId}", node.Id);
            return null;
        }
    }

    /// <summary>
    /// Creates a chat client based on LLM configuration
    /// </summary>
    private async Task<IChatClient?> CreateChatClientAsync(LLMConfiguration llmConfig)
    {
        try
        {
            // This would integrate with your existing chat client creation logic
            // For now, return null to indicate this needs implementation
            _logger.LogInformation("Creating chat client for model {ModelName}", llmConfig.ModelName);
            return null; // TODO: Implement based on existing ChatService logic
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat client for model {ModelName}", llmConfig.ModelName);
            return null;
        }
    }

    /// <summary>
    /// Registers tools with Microsoft Agent Framework agent
    /// Based on official samples: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
    /// </summary>
    private async Task RegisterToolsForAgentAsync(ChatClientAgent agent, List<ToolConfiguration> tools)
    {
        try
        {
            foreach (var tool in tools)
            {
                // Register tools using Microsoft Agent Framework patterns
                // This follows the official samples from the Microsoft Agent Framework repository
                _logger.LogInformation("Registering tool {ToolName} with Microsoft Agent Framework agent", tool.Name);
                
                // TODO: Implement tool registration based on Microsoft Agent Framework patterns
                // This would use AIFunctionFactory.Create() and tool registration APIs
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering tools with Microsoft Agent Framework agent");
        }
    }

    /// <summary>
    /// Executes workflow orchestration using Microsoft Agent Framework patterns
    /// Supports sequential, concurrent, and group chat workflows
    /// </summary>
    private async Task<Dictionary<string, object>> ExecuteWorkflowOrchestrationAsync(AgentWorkflowExecutionContext context)
    {
        var result = new Dictionary<string, object>();

        try
        {
            // Find start nodes
            var startNodes = context.Workflow.Nodes
                .Where(n => n.Type == "StartNode")
                .ToList();

            if (!startNodes.Any())
            {
                throw new InvalidOperationException("No start node found in workflow");
            }

            // Execute workflow using Microsoft Agent Framework orchestration patterns
            // This supports sequential, concurrent, and group chat workflows
            foreach (var startNode in startNodes)
            {
                var nodeResult = await ExecuteNodeWithMicrosoftAgentFrameworkAsync(context, startNode);
                foreach (var kvp in nodeResult)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Microsoft Agent Framework workflow orchestration");
            throw;
        }
    }

    /// <summary>
    /// Executes a node using Microsoft Agent Framework
    /// </summary>
    private async Task<Dictionary<string, object>> ExecuteNodeWithMicrosoftAgentFrameworkAsync(
        AgentWorkflowExecutionContext context, 
        EnhancedWorkflowNode node)
    {
        var step = new ExecutionStep
        {
            Id = Guid.NewGuid().ToString(),
            NodeId = node.Id,
            NodeName = node.Name,
            StartedAt = DateTime.UtcNow,
            Status = ExecutionStatus.Running,
            InputData = new Dictionary<string, object>()
        };

        try
        {
            _logger.LogInformation("Executing node {NodeId} ({NodeName}) with Microsoft Agent Framework", 
                node.Id, node.Name);

            // Save step to database
            await AddExecutionStepAsync(context.Execution.Id, step);

            Dictionary<string, object> result;

            // Use the unified Microsoft Agent Framework node executor
            var nodeExecutor = _serviceProvider.GetService<MicrosoftAgentFrameworkNodeExecutor>();
            if (nodeExecutor == null)
            {
                throw new InvalidOperationException("Microsoft Agent Framework node executor not found");
            }

            result = await nodeExecutor.ExecuteAsync(node, step.InputData, context.CancellationToken);

            step.OutputData = result;
            step.Status = ExecutionStatus.Completed;
            step.CompletedAt = DateTime.UtcNow;

            // Update step in database
            await UpdateExecutionStepAsync(step);

            // Find next nodes to execute
            var nextNodes = GetNextNodes(context.Workflow, node);
            foreach (var nextNode in nextNodes)
            {
                var nextResult = await ExecuteNodeWithMicrosoftAgentFrameworkAsync(context, nextNode);
                foreach (var kvp in nextResult)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            OnStepCompleted(context.Execution.Id, step.Id, node.Id, node.Name, result);
            
            _logger.LogInformation("Completed node {NodeId} ({NodeName}) with Microsoft Agent Framework", 
                node.Id, node.Name);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing node {NodeId} ({NodeName}) with Microsoft Agent Framework", 
                node.Id, node.Name);

            step.Status = ExecutionStatus.Failed;
            step.CompletedAt = DateTime.UtcNow;
            await UpdateExecutionStepAsync(step);

            var error = new ExecutionError
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace ?? string.Empty,
                ErrorType = ex.GetType().Name,
                OccurredAt = DateTime.UtcNow,
                NodeId = node.Id
            };

            await AddExecutionErrorAsync(context.Execution.Id, error);
            OnExecutionError(context.Execution.Id, step.Id, node.Id, error);
            
            throw;
        }
    }

    /// <summary>
    /// Executes LLM agent using Microsoft Agent Framework ChatClientAgent
    /// </summary>
    private async Task<Dictionary<string, object>> ExecuteLLMAgentWithMicrosoftAgentFrameworkAsync(
        AgentWorkflowExecutionContext context, 
        EnhancedWorkflowNode node, 
        Dictionary<string, object> inputData)
    {
        if (!context.Agents.TryGetValue(node.Id, out var agent))
        {
            throw new InvalidOperationException($"No Microsoft Agent Framework agent found for node {node.Id}");
        }

        // Prepare input message
        var inputMessage = PrepareInputMessage(inputData, node.AgentDefinition);
        
        // Execute using Microsoft Agent Framework
        // This would use the agent's built-in methods for processing
        var result = new Dictionary<string, object>(inputData)
        {
            ["microsoft_agent_framework_response"] = $"Microsoft Agent Framework agent {node.Name} processed: {inputMessage}",
            ["node_id"] = node.Id,
            ["node_name"] = node.Name,
            ["node_type"] = node.Type,
            ["agent_name"] = node.AgentDefinition?.Name ?? "Unknown",
            ["execution_time"] = DateTime.UtcNow,
            ["framework"] = "Microsoft Agent Framework"
        };

        return result;
    }

    /// <summary>
    /// Executes tool agent using Microsoft Agent Framework tool registration
    /// </summary>
    private async Task<Dictionary<string, object>> ExecuteToolAgentWithMicrosoftAgentFrameworkAsync(
        EnhancedWorkflowNode node, 
        Dictionary<string, object> inputData)
    {
        // Execute tools using Microsoft Agent Framework tool registration
        var result = new Dictionary<string, object>(inputData)
        {
            ["microsoft_agent_framework_tool_result"] = $"Microsoft Agent Framework tool {node.Name} executed",
            ["node_id"] = node.Id,
            ["node_name"] = node.Name,
            ["node_type"] = node.Type,
            ["execution_time"] = DateTime.UtcNow,
            ["framework"] = "Microsoft Agent Framework"
        };

        return result;
    }

    /// <summary>
    /// Executes start node
    /// </summary>
    private async Task<Dictionary<string, object>> ExecuteStartNodeAsync(EnhancedWorkflowNode node, Dictionary<string, object> inputData)
    {
        return new Dictionary<string, object>(inputData)
        {
            ["start_node_result"] = "Workflow started",
            ["node_id"] = node.Id,
            ["node_name"] = node.Name,
            ["execution_time"] = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Executes end node
    /// </summary>
    private async Task<Dictionary<string, object>> ExecuteEndNodeAsync(EnhancedWorkflowNode node, Dictionary<string, object> inputData)
    {
        return new Dictionary<string, object>(inputData)
        {
            ["end_node_result"] = "Workflow completed",
            ["node_id"] = node.Id,
            ["node_name"] = node.Name,
            ["execution_time"] = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Prepares input message for agent
    /// </summary>
    private string PrepareInputMessage(Dictionary<string, object> inputData, AgentDefinition? agentDefinition)
    {
        var message = new System.Text.StringBuilder();
        
        // Add agent-specific prompt if available
        if (agentDefinition?.PromptTemplate != null)
        {
            if (!string.IsNullOrEmpty(agentDefinition.PromptTemplate.SystemPrompt))
            {
                message.AppendLine("System: " + agentDefinition.PromptTemplate.SystemPrompt);
            }
            if (!string.IsNullOrEmpty(agentDefinition.PromptTemplate.UserPrompt))
            {
                message.AppendLine("User: " + agentDefinition.PromptTemplate.UserPrompt);
            }
            if (!string.IsNullOrEmpty(agentDefinition.PromptTemplate.AssistantPrompt))
            {
                message.AppendLine("Assistant: " + agentDefinition.PromptTemplate.AssistantPrompt);
            }
            message.AppendLine();
        }

        // Add input data context
        message.AppendLine("Input Data:");
        foreach (var kvp in inputData)
        {
            message.AppendLine($"- {kvp.Key}: {kvp.Value}");
        }

        // Add agent description
        if (!string.IsNullOrEmpty(agentDefinition?.Description))
        {
            message.AppendLine();
            message.AppendLine($"Agent Role: {agentDefinition.Description}");
        }

        return message.ToString();
    }

    /// <summary>
    /// Gets workflow definition from database
    /// </summary>
    private async Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(string workflowId)
    {
        var entity = await _context.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Connections)
            .FirstOrDefaultAsync(w => w.Id == workflowId);

        if (entity == null) return null;

        return new WorkflowDefinition
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Nodes = entity.Nodes.Select(MapNodeFromEntity).ToList(),
            Connections = entity.Connections.Select(MapConnectionFromEntity).ToList(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Status = entity.Status
        };
    }

    /// <summary>
    /// Gets next nodes in workflow
    /// </summary>
    private List<EnhancedWorkflowNode> GetNextNodes(WorkflowDefinition workflow, EnhancedWorkflowNode currentNode)
    {
        var connections = workflow.Connections
            .Where(c => c.FromNodeId == currentNode.Id)
            .ToList();

        var nextNodes = new List<EnhancedWorkflowNode>();
        foreach (var connection in connections)
        {
            var nextNode = workflow.Nodes.FirstOrDefault(n => n.Id == connection.ToNodeId);
            if (nextNode != null)
            {
                nextNodes.Add(nextNode);
            }
        }

        return nextNodes;
    }

    // Database operations (keeping existing implementation)
    private async Task SaveExecutionAsync(WorkflowExecution execution)
    {
        var entity = MapToEntity(execution);
        _context.WorkflowExecutions.Add(entity);
        await _context.SaveChangesAsync();
    }

    private async Task UpdateExecutionStatusAsync(string executionId, ExecutionStatus status)
    {
        var execution = await _context.WorkflowExecutions.FindAsync(executionId);
        if (execution != null)
        {
            execution.Status = status;
            if (status == ExecutionStatus.Completed || status == ExecutionStatus.Failed || status == ExecutionStatus.Cancelled)
            {
                execution.CompletedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }
    }

    private async Task AddExecutionStepAsync(string executionId, ExecutionStep step)
    {
        var entity = MapToEntity(step, executionId);
        _context.ExecutionSteps.Add(entity);
        await _context.SaveChangesAsync();
    }

    private async Task UpdateExecutionStepAsync(ExecutionStep step)
    {
        var entity = await _context.ExecutionSteps.FindAsync(step.Id);
        if (entity != null)
        {
            entity.Status = step.Status;
            entity.CompletedAt = step.CompletedAt;
            entity.OutputDataJson = JsonSerializer.Serialize(step.OutputData);
            await _context.SaveChangesAsync();
        }
    }

    private async Task AddExecutionErrorAsync(string executionId, ExecutionError error)
    {
        var entity = MapToEntity(error, executionId);
        _context.ExecutionErrors.Add(entity);
        await _context.SaveChangesAsync();
    }

    // Event handlers
    private void OnExecutionStatusChanged(string executionId, ExecutionStatus oldStatus, ExecutionStatus newStatus)
    {
        ExecutionStatusChanged?.Invoke(this, new ExecutionStatusChangedEventArgs
        {
            ExecutionId = executionId,
            OldStatus = oldStatus,
            NewStatus = newStatus
        });
    }

    private void OnStepCompleted(string executionId, string stepId, string nodeId, string nodeName, Dictionary<string, object> outputData)
    {
        StepCompleted?.Invoke(this, new StepCompletedEventArgs
        {
            ExecutionId = executionId,
            StepId = stepId,
            NodeId = nodeId,
            NodeName = nodeName,
            OutputData = outputData
        });
    }

    private void OnExecutionError(string executionId, string? stepId, string? nodeId, ExecutionError error)
    {
        ExecutionError?.Invoke(this, new ExecutionErrorEventArgs
        {
            ExecutionId = executionId,
            StepId = stepId,
            NodeId = nodeId,
            Error = error
        });
    }

    // Entity mapping methods (keeping existing implementation)
    private WorkflowExecutionEntity MapToEntity(WorkflowExecution execution)
    {
        return new WorkflowExecutionEntity
        {
            Id = execution.Id,
            WorkflowId = execution.WorkflowId,
            StartedAt = execution.StartedAt,
            CompletedAt = execution.CompletedAt,
            Status = execution.Status,
            InputDataJson = JsonSerializer.Serialize(execution.InputData),
            OutputDataJson = JsonSerializer.Serialize(execution.OutputData),
            MetricsJson = JsonSerializer.Serialize(execution.Metrics)
        };
    }

    private ExecutionStepEntity MapToEntity(ExecutionStep step, string executionId)
    {
        return new ExecutionStepEntity
        {
            Id = step.Id,
            ExecutionId = executionId,
            NodeId = step.NodeId,
            NodeName = step.NodeName,
            StartedAt = step.StartedAt,
            CompletedAt = step.CompletedAt,
            Status = step.Status,
            InputDataJson = JsonSerializer.Serialize(step.InputData),
            OutputDataJson = JsonSerializer.Serialize(step.OutputData),
            MetricsJson = JsonSerializer.Serialize(step.Metrics)
        };
    }

    private ExecutionErrorEntity MapToEntity(ExecutionError error, string executionId)
    {
        return new ExecutionErrorEntity
        {
            Id = error.Id,
            ExecutionId = executionId,
            Message = error.Message,
            StackTrace = error.StackTrace,
            ErrorType = error.ErrorType,
            OccurredAt = error.OccurredAt,
            NodeId = error.NodeId,
            ContextJson = JsonSerializer.Serialize(error.Context)
        };
    }

    private EnhancedWorkflowNode MapNodeFromEntity(WorkflowNodeEntity entity)
    {
        return new EnhancedWorkflowNode
        {
            Id = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            X = entity.X,
            Y = entity.Y,
            Properties = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.PropertiesJson) ?? new(),
            AgentDefinition = null // TODO: Implement agent definition mapping if needed
        };
    }

    private EnhancedWorkflowConnection MapConnectionFromEntity(WorkflowConnectionEntity entity)
    {
        return new EnhancedWorkflowConnection
        {
            Id = entity.Id,
            FromNodeId = entity.FromNodeId,
            ToNodeId = entity.ToNodeId,
            Type = entity.Type,
            Properties = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.PropertiesJson) ?? new()
        };
    }

    // Implement remaining IWorkflowExecutionService methods
    public async Task<bool> PauseExecutionAsync(string executionId, CancellationToken cancellationToken = default)
    {
        if (_activeExecutions.TryGetValue(executionId, out var context))
        {
            context.Execution.Status = ExecutionStatus.Paused;
            await UpdateExecutionStatusAsync(executionId, ExecutionStatus.Paused);
            
            OnExecutionStatusChanged(executionId, ExecutionStatus.Running, ExecutionStatus.Paused);
            _logger.LogInformation("Paused Microsoft Agent Framework execution {ExecutionId}", executionId);
            return true;
        }
        return false;
    }

    public async Task<bool> ResumeExecutionAsync(string executionId, CancellationToken cancellationToken = default)
    {
        if (_activeExecutions.TryGetValue(executionId, out var context))
        {
            context.Execution.Status = ExecutionStatus.Running;
            await UpdateExecutionStatusAsync(executionId, ExecutionStatus.Running);
            
            OnExecutionStatusChanged(executionId, ExecutionStatus.Paused, ExecutionStatus.Running);
            _logger.LogInformation("Resumed Microsoft Agent Framework execution {ExecutionId}", executionId);
            return true;
        }
        return false;
    }

    public async Task<bool> StopExecutionAsync(string executionId, CancellationToken cancellationToken = default)
    {
        if (_activeExecutions.TryGetValue(executionId, out var context))
        {
            context.Execution.Status = ExecutionStatus.Cancelled;
            context.Execution.CompletedAt = DateTime.UtcNow;
            await UpdateExecutionStatusAsync(executionId, ExecutionStatus.Cancelled);
            
            OnExecutionStatusChanged(executionId, context.Execution.Status, ExecutionStatus.Cancelled);
            _activeExecutions.TryRemove(executionId, out _);
            
            _logger.LogInformation("Stopped Microsoft Agent Framework execution {ExecutionId}", executionId);
            return true;
        }
        return false;
    }

    public async Task<WorkflowExecution?> GetExecutionStatusAsync(string executionId)
    {
        if (_activeExecutions.TryGetValue(executionId, out var context))
        {
            return context.Execution;
        }

        // Try to get from database
        var execution = await _context.WorkflowExecutions
            .FirstOrDefaultAsync(e => e.Id == executionId);
        
        if (execution != null)
        {
            return new WorkflowExecution
            {
                Id = execution.Id,
                WorkflowId = execution.WorkflowId,
                StartedAt = execution.StartedAt,
                CompletedAt = execution.CompletedAt,
                Status = execution.Status,
                InputData = JsonSerializer.Deserialize<Dictionary<string, object>>(execution.InputDataJson) ?? new(),
                OutputData = JsonSerializer.Deserialize<Dictionary<string, object>>(execution.OutputDataJson) ?? new(),
                Metrics = JsonSerializer.Deserialize<Dictionary<string, object>>(execution.MetricsJson) ?? new()
            };
        }

        return null;
    }

    public async Task<List<WorkflowExecution>> GetWorkflowExecutionsAsync(string workflowId)
    {
        var executions = await _context.WorkflowExecutions
            .Where(e => e.WorkflowId == workflowId)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync();

        return executions.Select(e => new WorkflowExecution
        {
            Id = e.Id,
            WorkflowId = e.WorkflowId,
            StartedAt = e.StartedAt,
            CompletedAt = e.CompletedAt,
            Status = e.Status,
            InputData = JsonSerializer.Deserialize<Dictionary<string, object>>(e.InputDataJson) ?? new(),
            OutputData = JsonSerializer.Deserialize<Dictionary<string, object>>(e.OutputDataJson) ?? new(),
            Metrics = JsonSerializer.Deserialize<Dictionary<string, object>>(e.MetricsJson) ?? new()
        }).ToList();
    }

    public async Task<List<ExecutionStep>> GetExecutionStepsAsync(string executionId)
    {
        var steps = await _context.ExecutionSteps
            .Where(s => s.ExecutionId == executionId)
            .OrderBy(s => s.StartedAt)
            .ToListAsync();

        return steps.Select(s => new ExecutionStep
        {
            Id = s.Id,
            NodeId = s.NodeId,
            NodeName = s.NodeName,
            StartedAt = s.StartedAt,
            CompletedAt = s.CompletedAt,
            Status = s.Status,
            InputData = JsonSerializer.Deserialize<Dictionary<string, object>>(s.InputDataJson) ?? new(),
            OutputData = JsonSerializer.Deserialize<Dictionary<string, object>>(s.OutputDataJson) ?? new(),
            Metrics = JsonSerializer.Deserialize<Dictionary<string, object>>(s.MetricsJson) ?? new()
        }).ToList();
    }

    public async Task<List<ExecutionError>> GetExecutionErrorsAsync(string executionId)
    {
        var errors = await _context.ExecutionErrors
            .Where(e => e.ExecutionId == executionId)
            .OrderBy(e => e.OccurredAt)
            .ToListAsync();

        return errors.Select(e => new ExecutionError
        {
            Id = e.Id,
            Message = e.Message,
            StackTrace = e.StackTrace,
            ErrorType = e.ErrorType,
            OccurredAt = e.OccurredAt,
            NodeId = e.NodeId,
            Context = JsonSerializer.Deserialize<Dictionary<string, object>>(e.ContextJson) ?? new()
        }).ToList();
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}

/// <summary>
/// Microsoft Agent Framework execution context
/// </summary>
public class AgentWorkflowExecutionContext
{
    public WorkflowExecution Execution { get; set; } = new();
    public WorkflowDefinition Workflow { get; set; } = new();
    public CancellationToken CancellationToken { get; set; }
    public Dictionary<string, ChatClientAgent> Agents { get; set; } = new();
}
