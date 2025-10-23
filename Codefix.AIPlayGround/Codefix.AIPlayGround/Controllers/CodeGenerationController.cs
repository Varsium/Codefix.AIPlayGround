using Microsoft.AspNetCore.Mvc;
using Codefix.AIPlayGround.Services;
using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Controllers;

/// <summary>
/// API Controller for code generation and execution operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CodeGenerationController : ControllerBase
{
    private readonly ICodeGenerationService _codeGenerationService;
    private readonly ICodeExecutionService _codeExecutionService;
    private readonly IAgentFactory _agentFactory;
    private readonly ILogger<CodeGenerationController> _logger;

    public CodeGenerationController(
        ICodeGenerationService codeGenerationService,
        ICodeExecutionService codeExecutionService,
        IAgentFactory agentFactory,
        ILogger<CodeGenerationController> logger)
    {
        _codeGenerationService = codeGenerationService;
        _codeExecutionService = codeExecutionService;
        _agentFactory = agentFactory;
        _logger = logger;
    }

    /// <summary>
    /// Generates C# code for an agent based on specifications
    /// </summary>
    [HttpPost("generate-code")]
    [ProducesResponseType(typeof(CodeGenerationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CodeGenerationResult>> GenerateAgentCode([FromBody] AgentCodeSpecification specification)
    {
        try
        {
            _logger.LogInformation("Generating agent code: {AgentName}", specification.AgentName);

            var result = await _codeGenerationService.GenerateAgentCodeAsync(specification);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating agent code");
            return BadRequest(new CodeGenerationResult
            {
                IsSuccess = false,
                Message = $"Code generation failed: {ex.Message}",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Compiles generated C# code into an assembly
    /// </summary>
    [HttpPost("compile-code")]
    [ProducesResponseType(typeof(CompilationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompilationResult>> CompileAgentCode([FromBody] CompileCodeRequest request)
    {
        try
        {
            _logger.LogInformation("Compiling agent code: {AssemblyName}", request.AssemblyName);

            var result = await _codeGenerationService.CompileAgentCodeAsync(request.Code, request.AssemblyName);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling agent code");
            return BadRequest(new CompilationResult
            {
                IsSuccess = false,
                Message = $"Code compilation failed: {ex.Message}",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Creates a complete code-generated agent
    /// </summary>
    [HttpPost("create-agent")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentResponse>> CreateCodeGeneratedAgent([FromBody] AgentCodeSpecification specification)
    {
        try
        {
            _logger.LogInformation("Creating code-generated agent: {AgentName}", specification.AgentName);

            var agent = await _agentFactory.CreateCodeGeneratedAgentAsync(specification);

            var response = new AgentResponse
            {
                Id = agent.Id,
                Name = agent.Name,
                Description = agent.Description,
                AgentType = agent.AgentType,
                Status = agent.Status.ToString(),
                CreatedAt = agent.CreatedAt,
                UpdatedAt = agent.UpdatedAt,
                CreatedBy = agent.CreatedBy
            };

            return CreatedAtAction(nameof(CreateCodeGeneratedAgent), new { id = agent.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating code-generated agent");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates an agent from a code template
    /// </summary>
    [HttpPost("create-from-template")]
    [ProducesResponseType(typeof(AgentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentResponse>> CreateAgentFromTemplate([FromBody] CreateFromCodeTemplateRequest request)
    {
        try
        {
            _logger.LogInformation("Creating agent from template: {TemplateName}", request.TemplateName);

            var agent = await _agentFactory.CreateAgentFromTemplateAsync(request.TemplateName, request.Parameters);

            var response = new AgentResponse
            {
                Id = agent.Id,
                Name = agent.Name,
                Description = agent.Description,
                AgentType = agent.AgentType,
                Status = agent.Status.ToString(),
                CreatedAt = agent.CreatedAt,
                UpdatedAt = agent.UpdatedAt,
                CreatedBy = agent.CreatedBy
            };

            return CreatedAtAction(nameof(CreateAgentFromTemplate), new { id = agent.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent from template");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Executes a code-generated agent
    /// </summary>
    [HttpPost("execute/{agentId}")]
    [ProducesResponseType(typeof(AgentExecutionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentExecutionResult>> ExecuteAgent(string agentId, [FromBody] ExecuteAgentRequest request)
    {
        try
        {
            _logger.LogInformation("Executing code-generated agent: {AgentId}", agentId);

            var result = await _agentFactory.ExecuteCodeGeneratedAgentAsync(agentId, request.Input, request.Context);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing agent: {AgentId}", agentId);
            return BadRequest(new AgentExecutionResult
            {
                IsSuccess = false,
                Message = $"Execution failed: {ex.Message}",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Executes a specific method on a code-generated agent
    /// </summary>
    [HttpPost("execute-method/{agentId}/{methodName}")]
    [ProducesResponseType(typeof(AgentExecutionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentExecutionResult>> ExecuteAgentMethod(
        string agentId, 
        string methodName, 
        [FromBody] ExecuteMethodRequest request)
    {
        try
        {
            _logger.LogInformation("Executing agent method: {AgentId}.{MethodName}", agentId, methodName);

            var result = await _agentFactory.ExecuteAgentMethodAsync(agentId, methodName, request.Parameters);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing agent method: {AgentId}.{MethodName}", agentId, methodName);
            return BadRequest(new AgentExecutionResult
            {
                IsSuccess = false,
                Message = $"Method execution failed: {ex.Message}",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Gets available code templates
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(List<CodeTemplate>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CodeTemplate>>> GetTemplates()
    {
        try
        {
            var templates = await _codeGenerationService.GetAvailableTemplatesAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validates generated code for security and syntax
    /// </summary>
    [HttpPost("validate-code")]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidationResult>> ValidateCode([FromBody] ValidateCodeRequest request)
    {
        try
        {
            var result = await _codeGenerationService.ValidateGeneratedCodeAsync(request.Code);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating code");
            return BadRequest(new Codefix.AIPlayGround.Services.ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Gets execution history for an agent
    /// </summary>
    [HttpGet("execution-history/{agentId}")]
    [ProducesResponseType(typeof(List<AgentExecutionResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AgentExecutionResult>>> GetExecutionHistory(string agentId, [FromQuery] int limit = 50)
    {
        try
        {
            var history = await _codeExecutionService.GetExecutionHistoryAsync(agentId, limit);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution history for agent: {AgentId}", agentId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets performance metrics for an agent
    /// </summary>
    [HttpGet("metrics/{agentId}")]
    [ProducesResponseType(typeof(AgentPerformanceMetrics), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgentPerformanceMetrics>> GetPerformanceMetrics(string agentId)
    {
        try
        {
            var metrics = await _codeExecutionService.GetPerformanceMetricsAsync(agentId);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance metrics for agent: {AgentId}", agentId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancels a running agent execution
    /// </summary>
    [HttpPost("cancel-execution/{executionId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CancelExecution(string executionId)
    {
        try
        {
            var cancelled = await _codeExecutionService.CancelExecutionAsync(executionId);
            return Ok(cancelled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling execution: {ExecutionId}", executionId);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// Request DTOs
public class CompileCodeRequest
{
    public string Code { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
}

public class CreateFromCodeTemplateRequest
{
    public string TemplateName { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ExecuteAgentRequest
{
    public object Input { get; set; } = new();
    public Dictionary<string, object>? Context { get; set; }
}

public class ExecuteMethodRequest
{
    public object[]? Parameters { get; set; }
}

public class ValidateCodeRequest
{
    public string Code { get; set; } = string.Empty;
}
