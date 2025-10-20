using Microsoft.AspNetCore.Mvc;
using Codefix.AIPlayGround.Models.DTOs;
using Codefix.AIPlayGround.Services;

namespace Codefix.AIPlayGround.Controllers;

/// <summary>
/// API Controller for Agent Factory operations
/// Provides endpoints for creating agents using factory patterns based on Microsoft Agent Framework
/// Reference: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted/Agents
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AgentFactoryController : ControllerBase
{
    private readonly IAgentFactory _agentFactory;
    private readonly ILogger<AgentFactoryController> _logger;

    public AgentFactoryController(
        IAgentFactory agentFactory,
        ILogger<AgentFactoryController> logger)
    {
        _agentFactory = agentFactory;
        _logger = logger;
    }

    /// <summary>
    /// Creates an LLM-based agent
    /// </summary>
    /// <param name="request">LLM agent creation request</param>
    /// <returns>Created agent details</returns>
    [HttpPost("llm")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentDto>> CreateLLMAgent([FromBody] CreateLLMAgentRequest request)
    {
        try
        {
            _logger.LogInformation("Creating LLM agent via factory: {Name}", request.Name);

            var agent = await _agentFactory.CreateLLMAgentAsync(
                request.Name,
                request.Instructions,
                request.LLMConfiguration
            );

            var dto = new AgentDto
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

            return CreatedAtAction(nameof(CreateLLMAgent), new { id = agent.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating LLM agent");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a tool-enabled agent
    /// </summary>
    [HttpPost("tool")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentDto>> CreateToolAgent([FromBody] CreateToolAgentRequest request)
    {
        try
        {
            _logger.LogInformation("Creating tool agent via factory: {Name}", request.Name);

            var agent = await _agentFactory.CreateToolAgentAsync(
                request.Name,
                request.Tools,
                request.Instructions
            );

            var dto = new AgentDto
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

            return CreatedAtAction(nameof(CreateToolAgent), new { id = agent.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tool agent");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a conditional routing agent
    /// </summary>
    [HttpPost("conditional")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AgentDto>> CreateConditionalAgent([FromBody] CreateConditionalAgentRequest request)
    {
        try
        {
            var agent = await _agentFactory.CreateConditionalAgentAsync(
                request.Name,
                request.Condition,
                request.Instructions
            );

            var dto = new AgentDto
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

            return CreatedAtAction(nameof(CreateConditionalAgent), new { id = agent.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conditional agent");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a parallel execution agent
    /// </summary>
    [HttpPost("parallel")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AgentDto>> CreateParallelAgent([FromBody] CreateParallelAgentRequest request)
    {
        try
        {
            var agent = await _agentFactory.CreateParallelAgentAsync(
                request.Name,
                request.MaxConcurrency
            );

            var dto = new AgentDto
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

            return CreatedAtAction(nameof(CreateParallelAgent), new { id = agent.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parallel agent");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a checkpoint-enabled agent
    /// </summary>
    [HttpPost("checkpoint")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AgentDto>> CreateCheckpointAgent([FromBody] CreateCheckpointAgentRequest request)
    {
        try
        {
            var agent = await _agentFactory.CreateCheckpointAgentAsync(
                request.Name,
                request.CheckpointConfiguration
            );

            var dto = new AgentDto
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

            return CreatedAtAction(nameof(CreateCheckpointAgent), new { id = agent.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkpoint agent");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates an MCP (Model Context Protocol) enabled agent
    /// </summary>
    [HttpPost("mcp")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AgentDto>> CreateMCPAgent([FromBody] CreateMCPAgentRequest request)
    {
        try
        {
            var agent = await _agentFactory.CreateMCPAgentAsync(
                request.Name,
                request.MCPServers,
                request.Instructions
            );

            var dto = new AgentDto
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

            return CreatedAtAction(nameof(CreateMCPAgent), new { id = agent.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MCP agent");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all available agent templates
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(List<AgentTemplate>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AgentTemplate>>> GetTemplates()
    {
        try
        {
            var templates = await _agentFactory.GetAvailableTemplatesAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Creates an agent from a predefined template
    /// </summary>
    [HttpPost("from-template")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentDto>> CreateFromTemplate([FromBody] CreateFromTemplateRequest request)
    {
        try
        {
            var agent = await _agentFactory.CreateFromTemplateAsync(
                request.TemplateName,
                request.Parameters
            );

            var dto = new AgentDto
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

            return CreatedAtAction(nameof(CreateFromTemplate), new { id = agent.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Template not found: {TemplateName}", request.TemplateName);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent from template");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validates agent configuration before creation
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidationResult>> ValidateConfiguration([FromBody] CreateAgentDto agentDto)
    {
        try
        {
            var result = await _agentFactory.ValidateAgentConfigurationAsync(agentDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating configuration");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Clones an existing agent with optional modifications
    /// </summary>
    [HttpPost("clone/{sourceAgentId}")]
    [ProducesResponseType(typeof(AgentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentDto>> CloneAgent(
        string sourceAgentId,
        [FromBody] CloneAgentRequest request)
    {
        try
        {
            var agent = await _agentFactory.CloneAgentAsync(
                sourceAgentId,
                request.NewName,
                request.Modifications
            );

            var dto = new AgentDto
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

            return CreatedAtAction(nameof(CloneAgent), new { id = agent.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Source agent not found: {SourceAgentId}", sourceAgentId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning agent");
            return BadRequest(new { error = ex.Message });
        }
    }
}

// Request DTOs
public class CreateLLMAgentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public LLMConfigurationDto? LLMConfiguration { get; set; }
}

public class CreateToolAgentRequest
{
    public string Name { get; set; } = string.Empty;
    public List<ToolConfigurationDto> Tools { get; set; } = new();
    public string? Instructions { get; set; }
}

public class CreateConditionalAgentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string? Instructions { get; set; }
}

public class CreateParallelAgentRequest
{
    public string Name { get; set; } = string.Empty;
    public int MaxConcurrency { get; set; } = 5;
}

public class CreateCheckpointAgentRequest
{
    public string Name { get; set; } = string.Empty;
    public CheckpointConfigurationDto? CheckpointConfiguration { get; set; }
}

public class CreateMCPAgentRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> MCPServers { get; set; } = new();
    public string? Instructions { get; set; }
}

public class CreateFromTemplateRequest
{
    public string TemplateName { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CloneAgentRequest
{
    public string NewName { get; set; } = string.Empty;
    public Dictionary<string, object>? Modifications { get; set; }
}

