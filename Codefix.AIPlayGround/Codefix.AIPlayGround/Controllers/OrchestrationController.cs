using Microsoft.AspNetCore.Mvc;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Services;

namespace Codefix.AIPlayGround.Controllers;

/// <summary>
/// Microsoft Agent Framework orchestration controller
/// Provides API endpoints for workflow orchestration using Microsoft Agent Framework patterns
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrchestrationController : ControllerBase
{
    private readonly IMicrosoftAgentFrameworkOrchestrationService _orchestrationService;
    private readonly IEnhancedWorkflowService _workflowService;
    private readonly ILogger<OrchestrationController> _logger;

    public OrchestrationController(
        IMicrosoftAgentFrameworkOrchestrationService orchestrationService,
        IEnhancedWorkflowService workflowService,
        ILogger<OrchestrationController> logger)
    {
        _orchestrationService = orchestrationService;
        _workflowService = workflowService;
        _logger = logger;
    }

    /// <summary>
    /// Execute workflow using Microsoft Agent Framework orchestration
    /// </summary>
    [HttpPost("execute/{workflowId}")]
    public async Task<ActionResult<WorkflowExecution>> ExecuteWorkflow(
        string workflowId,
        [FromBody] WorkflowExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing workflow {WorkflowId} with orchestration", workflowId);

            var execution = await _workflowService.ExecuteWorkflowWithOrchestrationAsync(
                workflowId, 
                request.InputData, 
                cancellationToken);

            return Ok(execution);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Workflow {WorkflowId} not found", workflowId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing workflow {WorkflowId}", workflowId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get workflow execution by ID
    /// </summary>
    [HttpGet("execution/{executionId}")]
    public async Task<ActionResult<WorkflowExecution>> GetExecution(string executionId)
    {
        try
        {
            var execution = await _workflowService.GetWorkflowExecutionAsync(executionId);
            if (execution == null)
            {
                return NotFound(new { message = $"Execution {executionId} not found" });
            }

            return Ok(execution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution {ExecutionId}", executionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancel workflow execution
    /// </summary>
    [HttpPost("execution/{executionId}/cancel")]
    public async Task<ActionResult> CancelExecution(string executionId)
    {
        try
        {
            var success = await _workflowService.CancelWorkflowExecutionAsync(executionId);
            if (!success)
            {
                return NotFound(new { message = $"Execution {executionId} not found or already completed" });
            }

            return Ok(new { message = "Execution cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all active workflow executions
    /// </summary>
    [HttpGet("executions")]
    public async Task<ActionResult<List<WorkflowExecution>>> GetActiveExecutions()
    {
        try
        {
            var executions = await _workflowService.GetActiveWorkflowExecutionsAsync();
            return Ok(executions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active executions");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Configure workflow orchestration settings
    /// </summary>
    [HttpPost("configure/{workflowId}")]
    public async Task<ActionResult> ConfigureOrchestration(
        string workflowId,
        [FromBody] OrchestrationConfigurationRequest request)
    {
        try
        {
            var success = await _workflowService.ConfigureWorkflowOrchestrationAsync(
                workflowId, 
                request.OrchestrationType, 
                request.OrchestrationConfig);

            if (!success)
            {
                return NotFound(new { message = $"Workflow {workflowId} not found" });
            }

            return Ok(new { message = "Orchestration configured successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring orchestration for workflow {WorkflowId}", workflowId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Add orchestration step to workflow
    /// </summary>
    [HttpPost("step/{workflowId}")]
    public async Task<ActionResult> AddOrchestrationStep(
        string workflowId,
        [FromBody] WorkflowOrchestrationStep orchestrationStep)
    {
        try
        {
            var success = await _workflowService.AddOrchestrationStepAsync(workflowId, orchestrationStep);
            if (!success)
            {
                return NotFound(new { message = $"Workflow {workflowId} not found" });
            }

            return Ok(new { message = "Orchestration step added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding orchestration step to workflow {WorkflowId}", workflowId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Update orchestration step in workflow
    /// </summary>
    [HttpPut("step/{workflowId}/{stepId}")]
    public async Task<ActionResult> UpdateOrchestrationStep(
        string workflowId,
        string stepId,
        [FromBody] WorkflowOrchestrationStep updatedStep)
    {
        try
        {
            var success = await _workflowService.UpdateOrchestrationStepAsync(workflowId, stepId, updatedStep);
            if (!success)
            {
                return NotFound(new { message = $"Workflow {workflowId} or step {stepId} not found" });
            }

            return Ok(new { message = "Orchestration step updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating orchestration step {StepId} in workflow {WorkflowId}", stepId, workflowId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Remove orchestration step from workflow
    /// </summary>
    [HttpDelete("step/{workflowId}/{stepId}")]
    public async Task<ActionResult> RemoveOrchestrationStep(string workflowId, string stepId)
    {
        try
        {
            var success = await _workflowService.RemoveOrchestrationStepAsync(workflowId, stepId);
            if (!success)
            {
                return NotFound(new { message = $"Workflow {workflowId} or step {stepId} not found" });
            }

            return Ok(new { message = "Orchestration step removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing orchestration step {StepId} from workflow {WorkflowId}", stepId, workflowId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get available orchestration types
    /// </summary>
    [HttpGet("types")]
    public ActionResult<OrchestrationTypesResponse> GetOrchestrationTypes()
    {
        var types = Enum.GetValues<WorkflowOrchestrationType>()
            .Select(t => new
            {
                Type = t.ToString(),
                Description = GetOrchestrationTypeDescription(t)
            })
            .ToList();

        return Ok(new OrchestrationTypesResponse { Types = types });
    }

    /// <summary>
    /// Get orchestration step types
    /// </summary>
    [HttpGet("step-types")]
    public ActionResult<OrchestrationStepTypesResponse> GetOrchestrationStepTypes()
    {
        var stepTypes = Enum.GetValues<WorkflowOrchestrationStepType>()
            .Select(st => new
            {
                Type = st.ToString(),
                Description = GetOrchestrationStepTypeDescription(st)
            })
            .ToList();

        return Ok(new OrchestrationStepTypesResponse { StepTypes = stepTypes });
    }

    private string GetOrchestrationTypeDescription(WorkflowOrchestrationType type)
    {
        return type switch
        {
            WorkflowOrchestrationType.Sequential => "Agents execute in pipeline order",
            WorkflowOrchestrationType.Concurrent => "Agents execute in parallel",
            WorkflowOrchestrationType.GroupChat => "Agents collaborate in group conversation",
            WorkflowOrchestrationType.Handoff => "Control passes dynamically between agents",
            WorkflowOrchestrationType.Magentic => "Dynamic agent selection based on context",
            WorkflowOrchestrationType.Custom => "User-defined orchestration pattern",
            _ => "Unknown orchestration type"
        };
    }

    private string GetOrchestrationStepTypeDescription(WorkflowOrchestrationStepType stepType)
    {
        return stepType switch
        {
            WorkflowOrchestrationStepType.AgentExecution => "Execute agents in this step",
            WorkflowOrchestrationStepType.WaitCondition => "Wait for condition before proceeding",
            WorkflowOrchestrationStepType.MergeResults => "Merge results from parallel executions",
            WorkflowOrchestrationStepType.Branch => "Branch execution based on condition",
            WorkflowOrchestrationStepType.Loop => "Loop execution",
            WorkflowOrchestrationStepType.Custom => "Custom step type",
            _ => "Unknown step type"
        };
    }
}

/// <summary>
/// Workflow execution request DTO
/// </summary>
public class WorkflowExecutionRequest
{
    public Dictionary<string, object> InputData { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
    public MicrosoftAgentFrameworkTestConfiguration? TestConfiguration { get; set; }
}

/// <summary>
/// Orchestration configuration request DTO
/// </summary>
public class OrchestrationConfigurationRequest
{
    public WorkflowOrchestrationType OrchestrationType { get; set; }
    public MicrosoftAgentFrameworkOrchestrationConfiguration? OrchestrationConfig { get; set; }
}

/// <summary>
/// Orchestration types response DTO
/// </summary>
public class OrchestrationTypesResponse
{
    public List<object> Types { get; set; } = new();
}

/// <summary>
/// Orchestration step types response DTO
/// </summary>
public class OrchestrationStepTypesResponse
{
    public List<object> StepTypes { get; set; } = new();
}
