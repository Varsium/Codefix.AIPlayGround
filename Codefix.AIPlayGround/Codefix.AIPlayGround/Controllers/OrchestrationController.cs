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
    private readonly IUnifiedAIProviderService _aiProviderService;
    private readonly ILogger<OrchestrationController> _logger;

    public OrchestrationController(
        IMicrosoftAgentFrameworkOrchestrationService orchestrationService,
        IEnhancedWorkflowService workflowService,
        IUnifiedAIProviderService aiProviderService,
        ILogger<OrchestrationController> logger)
    {
        _orchestrationService = orchestrationService;
        _workflowService = workflowService;
        _aiProviderService = aiProviderService;
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

        return Ok(new OrchestrationTypesResponse { Types = types.Cast<object>().ToList() });
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

        return Ok(new OrchestrationStepTypesResponse { StepTypes = stepTypes.Cast<object>().ToList() });
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

    // ===== AI Provider Endpoints =====

    /// <summary>
    /// Create AI agent with specified provider (PeerLLM, Ollama, OpenAI, etc.)
    /// </summary>
    [HttpPost("ai-agent")]
    public async Task<ActionResult<MicrosoftAgentFrameworkEntity>> CreateAIAgent(
        [FromBody] CreateAIAgentRequest request)
    {
        try
        {
            _logger.LogInformation("Creating AI agent {AgentName} with provider {ProviderType}", 
                request.Name, request.ProviderType);

            var agent = await _aiProviderService.CreateAgentAsync(
                request.Name,
                request.Description,
                request.ProviderType,
                request.ModelName,
                request.ProviderConfig);

            return Ok(agent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AI agent {AgentName}", request.Name);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Chat with AI agent using specified provider
    /// </summary>
    [HttpPost("ai-agent/{agentId}/chat")]
    public async Task<ActionResult<AIProviderResponse>> ChatWithAIAgent(
        string agentId,
        [FromBody] ChatWithAIAgentRequest request)
    {
        try
        {
            _logger.LogInformation("Chatting with AI agent {AgentId}", agentId);

            var response = await _aiProviderService.ExecuteChatAsync(
                agentId,
                request.Message,
                request.Context);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "AI agent {AgentId} not found", agentId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error chatting with AI agent {AgentId}", agentId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Stream chat response from AI agent
    /// </summary>
    [HttpPost("ai-agent/{agentId}/chat/stream")]
    public async Task<IActionResult> StreamChatWithAIAgent(
        string agentId,
        [FromBody] ChatWithAIAgentRequest request)
    {
        try
        {
            _logger.LogInformation("Streaming chat with AI agent {AgentId}", agentId);

            var stream = _aiProviderService.StreamChatAsync(agentId, request.Message, request.Context);
            
            return new StreamingResult(stream);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "AI agent {AgentId} not found", agentId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming chat with AI agent {AgentId}", agentId);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get available models for AI provider
    /// </summary>
    [HttpGet("ai-provider/{providerType}/models")]
    public async Task<ActionResult<List<string>>> GetAvailableModels(AIProviderType providerType)
    {
        try
        {
            var models = await _aiProviderService.GetAvailableModelsAsync(providerType);
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting models for provider {ProviderType}", providerType);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Test AI provider connection
    /// </summary>
    [HttpPost("ai-provider/{providerType}/test")]
    public async Task<ActionResult<AIProviderTestResult>> TestAIProvider(
        AIProviderType providerType,
        [FromBody] TestAIProviderRequest request)
    {
        try
        {
            _logger.LogInformation("Testing AI provider {ProviderType} with model {ModelName}", 
                providerType, request.ModelName);

            var result = await _aiProviderService.TestProviderAsync(providerType, request.ModelName);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing AI provider {ProviderType}", providerType);
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all supported AI providers
    /// </summary>
    [HttpGet("ai-providers")]
    public ActionResult<AIProvidersResponse> GetAIProviders()
    {
        var providers = Enum.GetValues<AIProviderType>()
            .Select(p => new
            {
                Type = p.ToString(),
                Name = GetProviderDisplayName(p),
                Description = GetProviderDescription(p),
                SupportsStreaming = GetProviderSupportsStreaming(p),
                SupportsTools = GetProviderSupportsTools(p)
            })
            .ToList();

        return Ok(new AIProvidersResponse { Providers = providers.Cast<object>().ToList() });
    }

    private string GetProviderDisplayName(AIProviderType providerType)
    {
        return providerType switch
        {
            AIProviderType.OpenAI => "OpenAI",
            AIProviderType.AzureOpenAI => "Azure OpenAI",
            AIProviderType.Anthropic => "Anthropic Claude",
            AIProviderType.PeerLLM => "PeerLLM (Decentralized)",
            AIProviderType.Ollama => "Ollama (Local)",
            AIProviderType.GoogleAI => "Google AI",
            AIProviderType.Custom => "Custom Provider",
            _ => providerType.ToString()
        };
    }

    private string GetProviderDescription(AIProviderType providerType)
    {
        return providerType switch
        {
            AIProviderType.OpenAI => "OpenAI's GPT models via API",
            AIProviderType.AzureOpenAI => "OpenAI models via Azure",
            AIProviderType.Anthropic => "Anthropic's Claude models",
            AIProviderType.PeerLLM => "Decentralized AI network with community models",
            AIProviderType.Ollama => "Local AI models for privacy and offline use",
            AIProviderType.GoogleAI => "Google's Gemini models",
            AIProviderType.Custom => "Custom AI provider implementation",
            _ => "Unknown provider"
        };
    }

    private bool GetProviderSupportsStreaming(AIProviderType providerType)
    {
        return providerType switch
        {
            AIProviderType.OpenAI => true,
            AIProviderType.AzureOpenAI => true,
            AIProviderType.Anthropic => true,
            AIProviderType.PeerLLM => true,
            AIProviderType.Ollama => true,
            AIProviderType.GoogleAI => true,
            AIProviderType.Custom => true,
            _ => false
        };
    }

    private bool GetProviderSupportsTools(AIProviderType providerType)
    {
        return providerType switch
        {
            AIProviderType.OpenAI => true,
            AIProviderType.AzureOpenAI => true,
            AIProviderType.Anthropic => true,
            AIProviderType.PeerLLM => true,
            AIProviderType.Ollama => false, // Ollama typically doesn't support function calling
            AIProviderType.GoogleAI => true,
            AIProviderType.Custom => true,
            _ => false
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

/// <summary>
/// Create AI agent request DTO
/// </summary>
public class CreateAIAgentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AIProviderType ProviderType { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public Dictionary<string, object>? ProviderConfig { get; set; }
}

/// <summary>
/// Chat with AI agent request DTO
/// </summary>
public class ChatWithAIAgentRequest
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Context { get; set; }
}

/// <summary>
/// Test AI provider request DTO
/// </summary>
public class TestAIProviderRequest
{
    public string ModelName { get; set; } = string.Empty;
}

/// <summary>
/// AI providers response DTO
/// </summary>
public class AIProvidersResponse
{
    public List<object> Providers { get; set; } = new();
}

/// <summary>
/// Streaming result for async enumerable responses
/// </summary>
public class StreamingResult : IActionResult
{
    private readonly IAsyncEnumerable<string> _stream;

    public StreamingResult(IAsyncEnumerable<string> stream)
    {
        _stream = stream;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = "text/plain";
        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("Connection", "keep-alive");

        await foreach (var chunk in _stream)
        {
            await response.WriteAsync(chunk);
            await response.Body.FlushAsync();
        }
    }
}
