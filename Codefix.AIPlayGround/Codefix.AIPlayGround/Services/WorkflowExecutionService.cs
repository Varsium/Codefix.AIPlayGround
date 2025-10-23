using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Codefix.AIPlayGround.Services;

public class WorkflowExecutionService : IWorkflowExecutionService, IDisposable
{
    private readonly IEnhancedWorkflowService _workflowService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WorkflowExecutionService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, WorkflowExecutionContext> _activeExecutions = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public event EventHandler<ExecutionStatusChangedEventArgs>? ExecutionStatusChanged;
    public event EventHandler<StepCompletedEventArgs>? StepCompleted;
    public event EventHandler<ExecutionErrorEventArgs>? ExecutionError;

    public WorkflowExecutionService(
        IEnhancedWorkflowService workflowService,
        ApplicationDbContext context,
        ILogger<WorkflowExecutionService> logger,
        IServiceProvider serviceProvider)
    {
        _workflowService = workflowService;
        _context = context;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> StartExecutionAsync(string workflowId, Dictionary<string, object>? inputData = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the workflow definition
            var workflow = await _workflowService.GetWorkflowAsync(workflowId);
            if (workflow == null)
            {
                throw new ArgumentException($"Workflow {workflowId} not found");
            }

            // Validate the workflow
            var validationErrors = await _workflowService.ValidateWorkflowAsync(workflowId);
            if (validationErrors.Any())
            {
                throw new InvalidOperationException($"Workflow validation failed: {string.Join(", ", validationErrors)}");
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

            // Create execution context
            var context = new WorkflowExecutionContext
            {
                Execution = execution,
                Workflow = workflow,
                CancellationToken = cancellationToken
            };

            _activeExecutions[execution.Id] = context;

            // Start execution in background
            _ = Task.Run(() => ExecuteWorkflowAsync(context), cancellationToken);

            _logger.LogInformation("Started execution {ExecutionId} for workflow {WorkflowId}", execution.Id, workflowId);
            return execution.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting execution for workflow {WorkflowId}", workflowId);
            throw;
        }
    }

    public async Task<bool> PauseExecutionAsync(string executionId, CancellationToken cancellationToken = default)
    {
        if (_activeExecutions.TryGetValue(executionId, out var context))
        {
            context.Execution.Status = ExecutionStatus.Paused;
            await UpdateExecutionStatusAsync(executionId, ExecutionStatus.Paused);
            
            OnExecutionStatusChanged(executionId, ExecutionStatus.Running, ExecutionStatus.Paused);
            _logger.LogInformation("Paused execution {ExecutionId}", executionId);
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
            _logger.LogInformation("Resumed execution {ExecutionId}", executionId);
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
            
            _logger.LogInformation("Stopped execution {ExecutionId}", executionId);
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
            return MapFromEntity(execution);
        }

        return null;
    }

    public async Task<List<WorkflowExecution>> GetWorkflowExecutionsAsync(string workflowId)
    {
        var executions = await _context.WorkflowExecutions
            .Where(e => e.WorkflowId == workflowId)
            .OrderByDescending(e => e.StartedAt)
            .ToListAsync();

        return executions.Select(MapFromEntity).ToList();
    }

    public async Task<List<ExecutionStep>> GetExecutionStepsAsync(string executionId)
    {
        var steps = await _context.ExecutionSteps
            .Where(s => s.ExecutionId == executionId)
            .OrderBy(s => s.StartedAt)
            .ToListAsync();

        return steps.Select(MapFromEntity).ToList();
    }

    public async Task<List<ExecutionError>> GetExecutionErrorsAsync(string executionId)
    {
        var errors = await _context.ExecutionErrors
            .Where(e => e.ExecutionId == executionId)
            .OrderBy(e => e.OccurredAt)
            .ToListAsync();

        return errors.Select(MapFromEntity).ToList();
    }

    private async Task ExecuteWorkflowAsync(WorkflowExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Starting workflow execution {ExecutionId}", context.Execution.Id);

            // Find start nodes
            var startNodes = context.Workflow.Nodes
                .Where(n => n.Type == "StartNode")
                .ToList();

            if (!startNodes.Any())
            {
                throw new InvalidOperationException("No start node found in workflow");
            }

            // Execute start nodes
            foreach (var startNode in startNodes)
            {
                await ExecuteNodeAsync(context, startNode);
            }

            // Mark execution as completed
            context.Execution.Status = ExecutionStatus.Completed;
            context.Execution.CompletedAt = DateTime.UtcNow;
            await UpdateExecutionStatusAsync(context.Execution.Id, ExecutionStatus.Completed);
            
            OnExecutionStatusChanged(context.Execution.Id, ExecutionStatus.Running, ExecutionStatus.Completed);
            
            _logger.LogInformation("Completed workflow execution {ExecutionId}", context.Execution.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow {ExecutionId}", context.Execution.Id);
            
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

    private async Task ExecuteNodeAsync(WorkflowExecutionContext context, EnhancedWorkflowNode node)
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
            _logger.LogInformation("Executing node {NodeId} ({NodeName}) in execution {ExecutionId}", 
                node.Id, node.Name, context.Execution.Id);

            // Save step to database
            await AddExecutionStepAsync(context.Execution.Id, step);

            // Get node executor
            var executor = GetNodeExecutor(node.Type);
            if (executor == null)
            {
                throw new InvalidOperationException($"No executor found for node type: {node.Type}");
            }

            // Execute the node
            var result = await executor.ExecuteAsync(node, step.InputData, context.CancellationToken);
            
            step.OutputData = result;
            step.Status = ExecutionStatus.Completed;
            step.CompletedAt = DateTime.UtcNow;

            // Update step in database
            await UpdateExecutionStepAsync(step);

            // Find next nodes to execute
            var nextNodes = GetNextNodes(context.Workflow, node);
            foreach (var nextNode in nextNodes)
            {
                await ExecuteNodeAsync(context, nextNode);
            }

            OnStepCompleted(context.Execution.Id, step.Id, node.Id, node.Name, result);
            
            _logger.LogInformation("Completed node {NodeId} ({NodeName}) in execution {ExecutionId}", 
                node.Id, node.Name, context.Execution.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing node {NodeId} ({NodeName}) in execution {ExecutionId}", 
                node.Id, node.Name, context.Execution.Id);

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

    private INodeExecutor? GetNodeExecutor(string nodeType)
    {
        return nodeType switch
        {
            "StartNode" => _serviceProvider.GetService<StartNodeExecutor>(),
            "EndNode" => _serviceProvider.GetService<EndNodeExecutor>(),
            "LLMAgent" => _serviceProvider.GetService<LLMAgentExecutor>(),
            "ToolAgent" => _serviceProvider.GetService<ToolAgentExecutor>(),
            "ConditionalAgent" => _serviceProvider.GetService<ConditionalAgentExecutor>(),
            "ParallelAgent" => _serviceProvider.GetService<ParallelAgentExecutor>(),
            "CheckpointAgent" => _serviceProvider.GetService<CheckpointAgentExecutor>(),
            "MCPAgent" => _serviceProvider.GetService<MCPAgentExecutor>(),
            "FunctionNode" => _serviceProvider.GetService<FunctionNodeExecutor>(),
            _ => null
        };
    }

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

    // Entity mapping methods
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

    private WorkflowExecution MapFromEntity(WorkflowExecutionEntity entity)
    {
        return new WorkflowExecution
        {
            Id = entity.Id,
            WorkflowId = entity.WorkflowId,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            Status = entity.Status,
            InputData = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.InputDataJson) ?? new(),
            OutputData = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.OutputDataJson) ?? new(),
            Metrics = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.MetricsJson) ?? new()
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

    private ExecutionStep MapFromEntity(ExecutionStepEntity entity)
    {
        return new ExecutionStep
        {
            Id = entity.Id,
            NodeId = entity.NodeId,
            NodeName = entity.NodeName,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            Status = entity.Status,
            InputData = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.InputDataJson) ?? new(),
            OutputData = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.OutputDataJson) ?? new(),
            Metrics = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.MetricsJson) ?? new()
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

    private ExecutionError MapFromEntity(ExecutionErrorEntity entity)
    {
        return new ExecutionError
        {
            Id = entity.Id,
            Message = entity.Message,
            StackTrace = entity.StackTrace,
            ErrorType = entity.ErrorType,
            OccurredAt = entity.OccurredAt,
            NodeId = entity.NodeId,
            Context = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.ContextJson) ?? new()
        };
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}

public class WorkflowExecutionContext
{
    public WorkflowExecution Execution { get; set; } = new();
    public WorkflowDefinition Workflow { get; set; } = new();
    public CancellationToken CancellationToken { get; set; }
}
