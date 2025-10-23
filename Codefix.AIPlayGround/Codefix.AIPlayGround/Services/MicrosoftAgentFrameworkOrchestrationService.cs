using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Codefix.AIPlayGround.Models;
using System.Text.Json;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Microsoft Agent Framework orchestration service
/// Handles execution of workflows using Microsoft Agent Framework orchestration patterns
/// Based on: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
/// </summary>
public class MicrosoftAgentFrameworkOrchestrationService : IMicrosoftAgentFrameworkOrchestrationService
{
    private readonly ILogger<MicrosoftAgentFrameworkOrchestrationService> _logger;
    private readonly IChatService _chatService;
    private readonly Dictionary<string, ChatClientAgent> _orchestrationAgents = new();
    private readonly Dictionary<string, WorkflowExecution> _activeExecutions = new();

    public MicrosoftAgentFrameworkOrchestrationService(
        ILogger<MicrosoftAgentFrameworkOrchestrationService> logger,
        IChatService chatService)
    {
        _logger = logger;
        _chatService = chatService;
        InitializeMicrosoftAgentFrameworkOrchestration();
    }

    /// <summary>
    /// Execute workflow using Microsoft Agent Framework orchestration patterns
    /// </summary>
    public async Task<WorkflowExecution> ExecuteWorkflowAsync(
        WorkflowDefinition workflow, 
        Dictionary<string, object> inputData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting Microsoft Agent Framework orchestration for workflow {WorkflowId}", workflow.Id);

            var execution = new WorkflowExecution
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflow.Id,
                StartedAt = DateTime.UtcNow,
                Status = ExecutionStatus.Running,
                InputData = inputData
            };

            _activeExecutions[execution.Id] = execution;

            // Execute based on orchestration type
            switch (workflow.OrchestrationType)
            {
                case WorkflowOrchestrationType.Sequential:
                    await ExecuteSequentialOrchestrationAsync(workflow, execution, cancellationToken);
                    break;
                case WorkflowOrchestrationType.Concurrent:
                    await ExecuteConcurrentOrchestrationAsync(workflow, execution, cancellationToken);
                    break;
                case WorkflowOrchestrationType.GroupChat:
                    await ExecuteGroupChatOrchestrationAsync(workflow, execution, cancellationToken);
                    break;
                case WorkflowOrchestrationType.Handoff:
                    await ExecuteHandoffOrchestrationAsync(workflow, execution, cancellationToken);
                    break;
                case WorkflowOrchestrationType.Magentic:
                    await ExecuteMagenticOrchestrationAsync(workflow, execution, cancellationToken);
                    break;
                default:
                    await ExecuteCustomOrchestrationAsync(workflow, execution, cancellationToken);
                    break;
            }

            execution.CompletedAt = DateTime.UtcNow;
            execution.Status = ExecutionStatus.Completed;

            _logger.LogInformation("Completed Microsoft Agent Framework orchestration for workflow {WorkflowId}", workflow.Id);
            return execution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Microsoft Agent Framework orchestration for workflow {WorkflowId}", workflow.Id);
            
            if (_activeExecutions.TryGetValue(workflow.Id, out var execution))
            {
                execution.Status = ExecutionStatus.Failed;
                execution.Errors.Add(new ExecutionError
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? string.Empty,
                    ErrorType = ex.GetType().Name,
                    OccurredAt = DateTime.UtcNow
                });
                return execution;
            }

            throw;
        }
    }

    /// <summary>
    /// Execute sequential orchestration - agents execute in pipeline order
    /// </summary>
    private async Task ExecuteSequentialOrchestrationAsync(
        WorkflowDefinition workflow, 
        WorkflowExecution execution, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing sequential orchestration for workflow {WorkflowId}", workflow.Id);

        var orderedNodes = GetOrderedNodesForSequentialExecution(workflow);
        var currentData = execution.InputData;

        foreach (var node in orderedNodes)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                execution.Status = ExecutionStatus.Cancelled;
                return;
            }

            var step = new ExecutionStep
            {
                NodeId = node.Id,
                NodeName = node.Name,
                StartedAt = DateTime.UtcNow,
                Status = ExecutionStatus.Running,
                InputData = currentData
            };

            try
            {
                // Execute node using Microsoft Agent Framework
                var nodeResult = await ExecuteNodeWithMicrosoftAgentFrameworkAsync(node, currentData, cancellationToken);
                
                step.OutputData = nodeResult;
                step.CompletedAt = DateTime.UtcNow;
                step.Status = ExecutionStatus.Completed;
                
                currentData = nodeResult; // Pass output to next node
            }
            catch (Exception ex)
            {
                step.Status = ExecutionStatus.Failed;
                step.Errors.Add(new ExecutionError
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? string.Empty,
                    ErrorType = ex.GetType().Name,
                    NodeId = node.Id,
                    OccurredAt = DateTime.UtcNow
                });
                
                execution.Errors.AddRange(step.Errors);
                throw; // Sequential execution stops on error
            }

            execution.Steps.Add(step);
        }

        execution.OutputData = currentData;
    }

    /// <summary>
    /// Execute concurrent orchestration - agents execute in parallel
    /// </summary>
    private async Task ExecuteConcurrentOrchestrationAsync(
        WorkflowDefinition workflow, 
        WorkflowExecution execution, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing concurrent orchestration for workflow {WorkflowId}", workflow.Id);

        var executableNodes = workflow.Nodes.Where(n => 
            n.OrchestrationSettings.CanParticipateInOrchestration && 
            n.OrchestrationSettings.CanExecuteInParallel).ToList();

        var tasks = executableNodes.Select(async node =>
        {
            var step = new ExecutionStep
            {
                NodeId = node.Id,
                NodeName = node.Name,
                StartedAt = DateTime.UtcNow,
                Status = ExecutionStatus.Running,
                InputData = execution.InputData
            };

            try
            {
                var nodeResult = await ExecuteNodeWithMicrosoftAgentFrameworkAsync(node, execution.InputData, cancellationToken);
                
                step.OutputData = nodeResult;
                step.CompletedAt = DateTime.UtcNow;
                step.Status = ExecutionStatus.Completed;
                
                return step;
            }
            catch (Exception ex)
            {
                step.Status = ExecutionStatus.Failed;
                step.Errors.Add(new ExecutionError
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? string.Empty,
                    ErrorType = ex.GetType().Name,
                    NodeId = node.Id,
                    OccurredAt = DateTime.UtcNow
                });
                
                return step;
            }
        });

        var results = await Task.WhenAll(tasks);
        
        // Aggregate results
        var aggregatedData = new Dictionary<string, object>();
        foreach (var step in results)
        {
            execution.Steps.Add(step);
            if (step.Status == ExecutionStatus.Completed)
            {
                aggregatedData[step.NodeId] = step.OutputData;
            }
            else
            {
                execution.Errors.AddRange(step.Errors);
            }
        }

        execution.OutputData = aggregatedData;
    }

    /// <summary>
    /// Execute group chat orchestration - agents collaborate in group conversation
    /// </summary>
    private async Task ExecuteGroupChatOrchestrationAsync(
        WorkflowDefinition workflow, 
        WorkflowExecution execution, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing group chat orchestration for workflow {WorkflowId}", workflow.Id);

        var groupChatNodes = workflow.Nodes.Where(n => 
            n.OrchestrationSettings.CanParticipateInOrchestration &&
            n.OrchestrationSettings.Roles.Contains(WorkflowNodeOrchestrationRole.PrimaryExecutor) ||
            n.OrchestrationSettings.Roles.Contains(WorkflowNodeOrchestrationRole.Assistant)).ToList();

        // Create group chat agent
        var groupChatAgent = await GetOrCreateGroupChatAgentAsync("group-chat-orchestrator");
        if (groupChatAgent == null)
        {
            throw new InvalidOperationException("Failed to create group chat agent for orchestration");
        }

        // Initialize group chat with workflow context
        var groupChatContext = new Dictionary<string, object>
        {
            ["workflowId"] = workflow.Id,
            ["executionId"] = execution.Id,
            ["inputData"] = execution.InputData,
            ["participants"] = groupChatNodes.Select(n => new { n.Id, n.Name, n.Type }).ToList()
        };

        // Execute group chat orchestration
        var groupChatResult = await ExecuteGroupChatWithMicrosoftAgentFrameworkAsync(
            groupChatAgent, 
            groupChatNodes, 
            groupChatContext, 
            cancellationToken);

        execution.OutputData = groupChatResult;
    }

    /// <summary>
    /// Execute handoff orchestration - control passes dynamically between agents
    /// </summary>
    private async Task ExecuteHandoffOrchestrationAsync(
        WorkflowDefinition workflow, 
        WorkflowExecution execution, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing handoff orchestration for workflow {WorkflowId}", workflow.Id);

        var currentData = execution.InputData;
        var currentNode = GetStartingNodeForHandoff(workflow);
        var visitedNodes = new HashSet<string>();

        while (currentNode != null && !visitedNodes.Contains(currentNode.Id))
        {
            visitedNodes.Add(currentNode.Id);

            var step = new ExecutionStep
            {
                NodeId = currentNode.Id,
                NodeName = currentNode.Name,
                StartedAt = DateTime.UtcNow,
                Status = ExecutionStatus.Running,
                InputData = currentData
            };

            try
            {
                var nodeResult = await ExecuteNodeWithMicrosoftAgentFrameworkAsync(currentNode, currentData, cancellationToken);
                
                step.OutputData = nodeResult;
                step.CompletedAt = DateTime.UtcNow;
                step.Status = ExecutionStatus.Completed;
                
                currentData = nodeResult;
                
                // Determine next node based on handoff logic
                currentNode = GetNextNodeForHandoff(workflow, currentNode, currentData);
            }
            catch (Exception ex)
            {
                step.Status = ExecutionStatus.Failed;
                step.Errors.Add(new ExecutionError
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? string.Empty,
                    ErrorType = ex.GetType().Name,
                    NodeId = currentNode.Id,
                    OccurredAt = DateTime.UtcNow
                });
                
                execution.Errors.AddRange(step.Errors);
                break;
            }

            execution.Steps.Add(step);
        }

        execution.OutputData = currentData;
    }

    /// <summary>
    /// Execute Magentic orchestration - dynamic agent selection based on context
    /// </summary>
    private async Task ExecuteMagenticOrchestrationAsync(
        WorkflowDefinition workflow, 
        WorkflowExecution execution, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing Magentic orchestration for workflow {WorkflowId}", workflow.Id);

        var magenticAgent = await GetOrCreateMagenticAgentAsync("magentic-orchestrator");
        if (magenticAgent == null)
        {
            throw new InvalidOperationException("Failed to create Magentic agent for orchestration");
        }

        var availableNodes = workflow.Nodes.Where(n => 
            n.OrchestrationSettings.CanParticipateInOrchestration).ToList();

        var currentData = execution.InputData;
        var maxIterations = 10; // Prevent infinite loops
        var iteration = 0;

        while (iteration < maxIterations)
        {
            iteration++;

            // Use Magentic agent to select next node
            var selectedNode = await SelectNodeWithMagenticAgentAsync(
                magenticAgent, 
                availableNodes, 
                currentData, 
                execution.Steps);

            if (selectedNode == null)
            {
                break; // No more nodes to execute
            }

            var step = new ExecutionStep
            {
                NodeId = selectedNode.Id,
                NodeName = selectedNode.Name,
                StartedAt = DateTime.UtcNow,
                Status = ExecutionStatus.Running,
                InputData = currentData
            };

            try
            {
                var nodeResult = await ExecuteNodeWithMicrosoftAgentFrameworkAsync(selectedNode, currentData, cancellationToken);
                
                step.OutputData = nodeResult;
                step.CompletedAt = DateTime.UtcNow;
                step.Status = ExecutionStatus.Completed;
                
                currentData = nodeResult;
            }
            catch (Exception ex)
            {
                step.Status = ExecutionStatus.Failed;
                step.Errors.Add(new ExecutionError
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? string.Empty,
                    ErrorType = ex.GetType().Name,
                    NodeId = selectedNode.Id,
                    OccurredAt = DateTime.UtcNow
                });
                
                execution.Errors.AddRange(step.Errors);
                break;
            }

            execution.Steps.Add(step);
        }

        execution.OutputData = currentData;
    }

    /// <summary>
    /// Execute custom orchestration - user-defined orchestration pattern
    /// </summary>
    private async Task ExecuteCustomOrchestrationAsync(
        WorkflowDefinition workflow, 
        WorkflowExecution execution, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing custom orchestration for workflow {WorkflowId}", workflow.Id);

        // Execute custom orchestration steps
        foreach (var orchestrationStep in workflow.OrchestrationSteps.OrderBy(s => s.Order))
        {
            if (!orchestrationStep.IsEnabled)
                continue;

            switch (orchestrationStep.StepType)
            {
                case WorkflowOrchestrationStepType.AgentExecution:
                    await ExecuteOrchestrationStepAsync(orchestrationStep, execution, cancellationToken);
                    break;
                case WorkflowOrchestrationStepType.WaitCondition:
                    await WaitForConditionAsync(orchestrationStep, execution, cancellationToken);
                    break;
                case WorkflowOrchestrationStepType.MergeResults:
                    await MergeResultsAsync(orchestrationStep, execution, cancellationToken);
                    break;
                case WorkflowOrchestrationStepType.Branch:
                    await ExecuteBranchAsync(orchestrationStep, execution, cancellationToken);
                    break;
                case WorkflowOrchestrationStepType.Loop:
                    await ExecuteLoopAsync(orchestrationStep, execution, cancellationToken);
                    break;
            }
        }
    }

    // Helper methods for orchestration execution

    private List<EnhancedWorkflowNode> GetOrderedNodesForSequentialExecution(WorkflowDefinition workflow)
    {
        // Implement topological sort based on connections
        var nodes = workflow.Nodes.ToList();
        var connections = workflow.Connections.ToList();
        
        // Simple implementation - can be enhanced with proper topological sorting
        return nodes.OrderBy(n => n.X).ToList(); // Sort by position for now
    }

    private EnhancedWorkflowNode? GetStartingNodeForHandoff(WorkflowDefinition workflow)
    {
        return workflow.Nodes.FirstOrDefault(n => 
            n.Type == "StartNode" || 
            n.OrchestrationSettings.Roles.Contains(WorkflowNodeOrchestrationRole.Coordinator));
    }

    private EnhancedWorkflowNode? GetNextNodeForHandoff(
        WorkflowDefinition workflow, 
        EnhancedWorkflowNode currentNode, 
        Dictionary<string, object> currentData)
    {
        // Find connections from current node
        var outgoingConnections = workflow.Connections.Where(c => c.FromNodeId == currentNode.Id).ToList();
        
        if (!outgoingConnections.Any())
            return null;

        // Simple implementation - select first connection
        // Can be enhanced with condition evaluation
        var nextConnection = outgoingConnections.First();
        return workflow.Nodes.FirstOrDefault(n => n.Id == nextConnection.ToNodeId);
    }

    private async Task<Dictionary<string, object>> ExecuteNodeWithMicrosoftAgentFrameworkAsync(
        EnhancedWorkflowNode node, 
        Dictionary<string, object> inputData, 
        CancellationToken cancellationToken)
    {
        // Implementation would use Microsoft Agent Framework to execute the node
        // This is a placeholder - actual implementation would depend on the specific agent type
        
        _logger.LogInformation("Executing node {NodeId} with Microsoft Agent Framework", node.Id);
        
        // Simulate execution for now
        await Task.Delay(100, cancellationToken);
        
        return new Dictionary<string, object>
        {
            ["nodeId"] = node.Id,
            ["nodeName"] = node.Name,
            ["executionTime"] = DateTime.UtcNow,
            ["processedData"] = inputData
        };
    }

    private async Task<ChatClientAgent?> GetOrCreateGroupChatAgentAsync(string agentId)
    {
        if (_orchestrationAgents.TryGetValue(agentId, out var existingAgent))
            return existingAgent;

        // Create group chat agent using Microsoft Agent Framework
        // This is a placeholder - actual implementation would use the framework
        return null;
    }

    private async Task<ChatClientAgent?> GetOrCreateMagenticAgentAsync(string agentId)
    {
        if (_orchestrationAgents.TryGetValue(agentId, out var existingAgent))
            return existingAgent;

        // Create Magentic agent using Microsoft Agent Framework
        // This is a placeholder - actual implementation would use the framework
        return null;
    }

    private async Task<Dictionary<string, object>> ExecuteGroupChatWithMicrosoftAgentFrameworkAsync(
        ChatClientAgent groupChatAgent,
        List<EnhancedWorkflowNode> participants,
        Dictionary<string, object> context,
        CancellationToken cancellationToken)
    {
        // Implementation would use Microsoft Agent Framework group chat capabilities
        // This is a placeholder
        return new Dictionary<string, object>
        {
            ["groupChatResult"] = "Simulated group chat execution",
            ["participants"] = participants.Count,
            ["context"] = context
        };
    }

    private async Task<EnhancedWorkflowNode?> SelectNodeWithMagenticAgentAsync(
        ChatClientAgent magenticAgent,
        List<EnhancedWorkflowNode> availableNodes,
        Dictionary<string, object> currentData,
        List<ExecutionStep> previousSteps)
    {
        // Implementation would use Microsoft Agent Framework Magentic capabilities
        // This is a placeholder
        return availableNodes.FirstOrDefault();
    }

    private async Task ExecuteOrchestrationStepAsync(
        WorkflowOrchestrationStep step,
        WorkflowExecution execution,
        CancellationToken cancellationToken)
    {
        // Execute nodes in this orchestration step
        foreach (var nodeId in step.NodeIds)
        {
            var node = execution.Steps.FirstOrDefault(s => s.NodeId == nodeId)?.NodeName;
            if (node != null)
            {
                // Execute node
                _logger.LogInformation("Executing orchestration step {StepId} for node {NodeId}", step.Id, nodeId);
            }
        }
    }

    private async Task WaitForConditionAsync(
        WorkflowOrchestrationStep step,
        WorkflowExecution execution,
        CancellationToken cancellationToken)
    {
        // Wait for condition to be met
        _logger.LogInformation("Waiting for condition in orchestration step {StepId}", step.Id);
        await Task.Delay(1000, cancellationToken); // Placeholder
    }

    private async Task MergeResultsAsync(
        WorkflowOrchestrationStep step,
        WorkflowExecution execution,
        CancellationToken cancellationToken)
    {
        // Merge results from multiple nodes
        _logger.LogInformation("Merging results in orchestration step {StepId}", step.Id);
    }

    private async Task ExecuteBranchAsync(
        WorkflowOrchestrationStep step,
        WorkflowExecution execution,
        CancellationToken cancellationToken)
    {
        // Execute branch based on condition
        _logger.LogInformation("Executing branch in orchestration step {StepId}", step.Id);
    }

    private async Task ExecuteLoopAsync(
        WorkflowOrchestrationStep step,
        WorkflowExecution execution,
        CancellationToken cancellationToken)
    {
        // Execute loop
        _logger.LogInformation("Executing loop in orchestration step {StepId}", step.Id);
    }

    private void InitializeMicrosoftAgentFrameworkOrchestration()
    {
        _logger.LogInformation("Initializing Microsoft Agent Framework orchestration service");
        // Initialize orchestration capabilities
    }

    /// <summary>
    /// Get active workflow execution
    /// </summary>
    public WorkflowExecution? GetActiveExecution(string executionId)
    {
        return _activeExecutions.TryGetValue(executionId, out var execution) ? execution : null;
    }

    /// <summary>
    /// Cancel active workflow execution
    /// </summary>
    public async Task<bool> CancelExecutionAsync(string executionId)
    {
        if (_activeExecutions.TryGetValue(executionId, out var execution))
        {
            execution.Status = ExecutionStatus.Cancelled;
            execution.CompletedAt = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get all active executions
    /// </summary>
    public List<WorkflowExecution> GetActiveExecutions()
    {
        return _activeExecutions.Values.ToList();
    }
}

/// <summary>
/// Microsoft Agent Framework orchestration service interface
/// </summary>
public interface IMicrosoftAgentFrameworkOrchestrationService
{
    Task<WorkflowExecution> ExecuteWorkflowAsync(
        WorkflowDefinition workflow, 
        Dictionary<string, object> inputData,
        CancellationToken cancellationToken = default);
    
    WorkflowExecution? GetActiveExecution(string executionId);
    Task<bool> CancelExecutionAsync(string executionId);
    List<WorkflowExecution> GetActiveExecutions();
}
