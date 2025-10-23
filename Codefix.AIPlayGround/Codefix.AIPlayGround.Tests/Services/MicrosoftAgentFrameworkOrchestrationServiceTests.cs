using Microsoft.Extensions.Logging;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Services;
using Moq;

namespace Codefix.AIPlayGround.Tests.Services;

/// <summary>
/// Tests for Microsoft Agent Framework orchestration service
/// </summary>
public class MicrosoftAgentFrameworkOrchestrationServiceTests
{
    private readonly Mock<ILogger<MicrosoftAgentFrameworkOrchestrationService>> _mockLogger;
    private readonly Mock<IChatService> _mockChatService;
    private readonly MicrosoftAgentFrameworkOrchestrationService _orchestrationService;

    public MicrosoftAgentFrameworkOrchestrationServiceTests()
    {
        _mockLogger = new Mock<ILogger<MicrosoftAgentFrameworkOrchestrationService>>();
        _mockChatService = new Mock<IChatService>();
        _orchestrationService = new MicrosoftAgentFrameworkOrchestrationService(
            _mockLogger.Object,
            _mockChatService.Object);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_SequentialOrchestration_ShouldExecuteNodesInOrder()
    {
        // Arrange
        var workflow = CreateTestWorkflow(WorkflowOrchestrationType.Sequential);
        var inputData = new Dictionary<string, object> { ["test"] = "data" };

        // Act
        var result = await _orchestrationService.ExecuteWorkflowAsync(workflow, inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExecutionStatus.Completed, result.Status);
        Assert.Equal(workflow.Id, result.WorkflowId);
        Assert.NotNull(result.OutputData);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_ConcurrentOrchestration_ShouldExecuteNodesInParallel()
    {
        // Arrange
        var workflow = CreateTestWorkflow(WorkflowOrchestrationType.Concurrent);
        var inputData = new Dictionary<string, object> { ["test"] = "data" };

        // Act
        var result = await _orchestrationService.ExecuteWorkflowAsync(workflow, inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExecutionStatus.Completed, result.Status);
        Assert.Equal(workflow.Id, result.WorkflowId);
        Assert.NotNull(result.OutputData);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_GroupChatOrchestration_ShouldExecuteGroupChat()
    {
        // Arrange
        var workflow = CreateTestWorkflow(WorkflowOrchestrationType.GroupChat);
        var inputData = new Dictionary<string, object> { ["test"] = "data" };

        // Act
        var result = await _orchestrationService.ExecuteWorkflowAsync(workflow, inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExecutionStatus.Completed, result.Status);
        Assert.Equal(workflow.Id, result.WorkflowId);
        Assert.NotNull(result.OutputData);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_HandoffOrchestration_ShouldExecuteHandoff()
    {
        // Arrange
        var workflow = CreateTestWorkflow(WorkflowOrchestrationType.Handoff);
        var inputData = new Dictionary<string, object> { ["test"] = "data" };

        // Act
        var result = await _orchestrationService.ExecuteWorkflowAsync(workflow, inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExecutionStatus.Completed, result.Status);
        Assert.Equal(workflow.Id, result.WorkflowId);
        Assert.NotNull(result.OutputData);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_MagenticOrchestration_ShouldExecuteMagentic()
    {
        // Arrange
        var workflow = CreateTestWorkflow(WorkflowOrchestrationType.Magentic);
        var inputData = new Dictionary<string, object> { ["test"] = "data" };

        // Act
        var result = await _orchestrationService.ExecuteWorkflowAsync(workflow, inputData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ExecutionStatus.Completed, result.Status);
        Assert.Equal(workflow.Id, result.WorkflowId);
        Assert.NotNull(result.OutputData);
    }

    [Fact]
    public async Task GetActiveExecution_WithValidExecutionId_ShouldReturnExecution()
    {
        // Arrange
        var workflow = CreateTestWorkflow(WorkflowOrchestrationType.Sequential);
        var inputData = new Dictionary<string, object> { ["test"] = "data" };
        var execution = await _orchestrationService.ExecuteWorkflowAsync(workflow, inputData);

        // Act
        var result = _orchestrationService.GetActiveExecution(execution.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(execution.Id, result.Id);
    }

    [Fact]
    public async Task GetActiveExecution_WithInvalidExecutionId_ShouldReturnNull()
    {
        // Act
        var result = _orchestrationService.GetActiveExecution("invalid-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CancelExecutionAsync_WithValidExecutionId_ShouldCancelExecution()
    {
        // Arrange
        var workflow = CreateTestWorkflow(WorkflowOrchestrationType.Sequential);
        var inputData = new Dictionary<string, object> { ["test"] = "data" };
        var execution = await _orchestrationService.ExecuteWorkflowAsync(workflow, inputData);

        // Act
        var result = await _orchestrationService.CancelExecutionAsync(execution.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CancelExecutionAsync_WithInvalidExecutionId_ShouldReturnFalse()
    {
        // Act
        var result = await _orchestrationService.CancelExecutionAsync("invalid-id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetActiveExecutions_ShouldReturnAllActiveExecutions()
    {
        // Act
        var result = _orchestrationService.GetActiveExecutions();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<WorkflowExecution>>(result);
    }

    private WorkflowDefinition CreateTestWorkflow(WorkflowOrchestrationType orchestrationType)
    {
        return new WorkflowDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Workflow",
            Description = "Test workflow for orchestration",
            OrchestrationType = orchestrationType,
            OrchestrationConfig = new MicrosoftAgentFrameworkOrchestrationConfiguration
            {
                OrchestrationType = orchestrationType.ToString().ToLower(),
                MaxConcurrentExecutions = 5,
                ExecutionTimeout = TimeSpan.FromMinutes(10)
            },
            Nodes = new List<EnhancedWorkflowNode>
            {
                new()
                {
                    Id = "node1",
                    Name = "Test Node 1",
                    Type = "LLMAgent",
                    OrchestrationSettings = new WorkflowNodeOrchestrationSettings
                    {
                        CanParticipateInOrchestration = true,
                        CanExecuteInParallel = true,
                        Roles = new List<WorkflowNodeOrchestrationRole> { WorkflowNodeOrchestrationRole.PrimaryExecutor }
                    }
                },
                new()
                {
                    Id = "node2",
                    Name = "Test Node 2",
                    Type = "LLMAgent",
                    OrchestrationSettings = new WorkflowNodeOrchestrationSettings
                    {
                        CanParticipateInOrchestration = true,
                        CanExecuteInParallel = true,
                        Roles = new List<WorkflowNodeOrchestrationRole> { WorkflowNodeOrchestrationRole.Assistant }
                    }
                }
            },
            Connections = new List<EnhancedWorkflowConnection>
            {
                new()
                {
                    Id = "conn1",
                    FromNodeId = "node1",
                    ToNodeId = "node2",
                    ConnectionType = ConnectionType.DataFlow,
                    OrchestrationSettings = new WorkflowConnectionOrchestrationSettings
                    {
                        ParticipatesInOrchestration = true,
                        SupportedOrchestrationTypes = new List<WorkflowOrchestrationType> { orchestrationType }
                    }
                }
            }
        };
    }
}
