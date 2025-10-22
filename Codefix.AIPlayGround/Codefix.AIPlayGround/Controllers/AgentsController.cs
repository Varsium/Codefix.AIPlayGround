using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;
using Codefix.AIPlayGround.Services;

namespace Codefix.AIPlayGround.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAgentFrameworkService _agentFrameworkService;
    private readonly ILogger<AgentsController> _logger;

    public AgentsController(
        ApplicationDbContext context,
        IAgentFrameworkService agentFrameworkService,
        ILogger<AgentsController> logger)
    {
        _context = context;
        _agentFrameworkService = agentFrameworkService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgentResponse>>> GetAgents([FromQuery] GetAgentsRequest filter)
    {
        try
        {
            var query = _context.Agents.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(a => a.Name.Contains(filter.Name));

            if (!string.IsNullOrEmpty(filter.AgentType))
                query = query.Where(a => a.AgentType == filter.AgentType);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(a => a.Status.ToString() == filter.Status);

            if (!string.IsNullOrEmpty(filter.CreatedBy))
                query = query.Where(a => a.CreatedBy == filter.CreatedBy);

            if (filter.CreatedAfter.HasValue)
                query = query.Where(a => a.CreatedAt >= filter.CreatedAfter.Value);

            if (filter.CreatedBefore.HasValue)
                query = query.Where(a => a.CreatedAt <= filter.CreatedBefore.Value);

            var agents = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new AgentResponse
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    AgentType = a.AgentType,
                    Status = a.Status.ToString(),
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    CreatedBy = a.CreatedBy
                })
                .ToListAsync();

            return Ok(agents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agents");
            return StatusCode(500, "An error occurred while retrieving agents");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AgentDetailResponse>> GetAgent(string id)
    {
        try
        {
            var agent = await _context.Agents
                .Include(a => a.Executions.OrderByDescending(e => e.StartedAt).Take(10))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (agent == null)
                return NotFound();

            var agentDetail = new AgentDetailResponse
            {
                Id = agent.Id,
                Name = agent.Name,
                Description = agent.Description,
                AgentType = agent.AgentType,
                Status = agent.Status.ToString(),
                Instructions = agent.Instructions,
                CreatedAt = agent.CreatedAt,
                UpdatedAt = agent.UpdatedAt,
                CreatedBy = agent.CreatedBy,
                LLMConfiguration = JsonSerializer.Deserialize<LLMConfiguration>(agent.LLMConfigurationJson),
                Tools = JsonSerializer.Deserialize<List<ToolConfiguration>>(agent.ToolsConfigurationJson) ?? new(),
                PromptTemplate = JsonSerializer.Deserialize<PromptTemplate>(agent.PromptTemplateJson),
                MemoryConfiguration = JsonSerializer.Deserialize<MemoryConfiguration>(agent.MemoryConfigurationJson),
                CheckpointConfiguration = JsonSerializer.Deserialize<CheckpointConfiguration>(agent.CheckpointConfigurationJson),
                Properties = JsonSerializer.Deserialize<Dictionary<string, object>>(agent.PropertiesJson) ?? new(),
                RecentExecutions = agent.Executions.Select(e => new AgentExecutionResponse
                {
                    Id = e.Id,
                    AgentId = e.AgentId,
                    StartedAt = e.StartedAt,
                    CompletedAt = e.CompletedAt,
                    Status = e.Status.ToString(),
                    InputData = JsonSerializer.Deserialize<Dictionary<string, object>>(e.InputDataJson) ?? new(),
                    OutputData = JsonSerializer.Deserialize<Dictionary<string, object>>(e.OutputDataJson) ?? new(),
                    Metrics = JsonSerializer.Deserialize<Dictionary<string, object>>(e.MetricsJson) ?? new(),
                    Errors = JsonSerializer.Deserialize<List<string>>(e.ErrorsJson) ?? new()
                }).ToList()
            };

            return Ok(agentDetail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent {AgentId}", id);
            return StatusCode(500, "An error occurred while retrieving the agent");
        }
    }

    [HttpPost]
    public async Task<ActionResult<AgentResponse>> CreateAgent([FromBody] CreateAgentRequest dto)
    {
        try
        {
            if (dto == null)
                return BadRequest("Agent data is required");

            var agent = new AgentEntity
            {
                Name = dto.Name,
                Description = dto.Description,
                AgentType = dto.AgentType,
                Instructions = dto.Instructions,
                LLMConfigurationJson = JsonSerializer.Serialize(dto.LLMConfiguration ?? new LLMConfiguration()),
                ToolsConfigurationJson = JsonSerializer.Serialize(dto.Tools ?? new List<ToolConfiguration>()),
                PromptTemplateJson = JsonSerializer.Serialize(dto.PromptTemplate ?? new PromptTemplate()),
                MemoryConfigurationJson = JsonSerializer.Serialize(dto.MemoryConfiguration ?? new MemoryConfiguration()),
                CheckpointConfigurationJson = JsonSerializer.Serialize(dto.CheckpointConfiguration ?? new CheckpointConfiguration()),
                PropertiesJson = JsonSerializer.Serialize(dto.Properties ?? new Dictionary<string, object>()),
                CreatedBy = User.Identity?.Name ?? "System"
            };

            _context.Agents.Add(agent);
            await _context.SaveChangesAsync();

            var agentDto = new AgentResponse
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

            return CreatedAtAction(nameof(GetAgent), new { id = agent.Id }, agentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent");
            return StatusCode(500, "An error occurred while creating the agent");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AgentResponse>> UpdateAgent(string id, [FromBody] UpdateAgentRequest dto)
    {
        try
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null)
                return NotFound();

            if (!string.IsNullOrEmpty(dto.Name))
                agent.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Description))
                agent.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.Instructions))
                agent.Instructions = dto.Instructions;

            // Update status if provided
            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (Enum.TryParse<AgentStatus>(dto.Status, out var status))
                {
                    agent.Status = status;
                }
            }

            if (dto.LLMConfiguration != null)
                agent.LLMConfigurationJson = JsonSerializer.Serialize(dto.LLMConfiguration);

            if (dto.Tools != null)
                agent.ToolsConfigurationJson = JsonSerializer.Serialize(dto.Tools);

            if (dto.PromptTemplate != null)
                agent.PromptTemplateJson = JsonSerializer.Serialize(dto.PromptTemplate);

            if (dto.MemoryConfiguration != null)
                agent.MemoryConfigurationJson = JsonSerializer.Serialize(dto.MemoryConfiguration);

            if (dto.CheckpointConfiguration != null)
                agent.CheckpointConfigurationJson = JsonSerializer.Serialize(dto.CheckpointConfiguration);

            if (dto.Properties != null)
                agent.PropertiesJson = JsonSerializer.Serialize(dto.Properties);

            agent.UpdatedAt = DateTime.UtcNow;

            _context.Agents.Update(agent);
            await _context.SaveChangesAsync();

            var agentDto = new AgentResponse
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

            return Ok(agentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent {AgentId}", id);
            return StatusCode(500, "An error occurred while updating the agent");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAgent(string id)
    {
        try
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null)
                return NotFound();

            _context.Agents.Remove(agent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting agent {AgentId}", id);
            return StatusCode(500, "An error occurred while deleting the agent");
        }
    }

    [HttpPost("{id}/deploy")]
    public async Task<ActionResult<DeploymentResult>> DeployAgent(string id)
    {
        try
        {
            var result = await _agentFrameworkService.DeployAgentAsync(id);
            
            if (result.IsSuccess)
            {
                return Ok(new DeploymentResult
                {
                    IsSuccess = true,
                    Message = result.Message,
                    Metadata = result.Metadata
                });
            }
            else
            {
                return BadRequest(new DeploymentResult
                {
                    IsSuccess = false,
                    Message = result.Message
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying agent {AgentId}", id);
            return StatusCode(500, "An error occurred while deploying the agent");
        }
    }

    [HttpPost("{id}/test")]
    public async Task<ActionResult<TestResult>> TestAgent(string id, [FromBody] TestAgentRequest input)
    {
        try
        {
            if (input == null)
                return BadRequest("Test input is required");

            var result = await _agentFrameworkService.TestAgentAsync(id, input.Input);
            
            if (result.IsSuccess)
            {
                var testResult = new TestResult
                {
                    IsSuccess = true,
                    Message = result.Message,
                    Input = input.Input,
                    Output = result.Data != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(result.Data.ToString() ?? "{}") ?? new() : new(),
                    Metrics = result.Metadata
                };
                return Ok(testResult);
            }
            else
            {
                var testResult = new TestResult
                {
                    IsSuccess = false,
                    Message = result.Message,
                    Input = input.Input,
                    Errors = result.Errors
                };
                return BadRequest(testResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing agent {AgentId}", id);
            return StatusCode(500, "An error occurred while testing the agent");
        }
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<AgentStatusResponse>> GetAgentStatus(string id)
    {
        try
        {
            var result = await _agentFrameworkService.GetAgentStatusAsync(id);
            
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(new { Message = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent status {AgentId}", id);
            return StatusCode(500, "An error occurred while getting the agent status");
        }
    }

    [HttpPost("{id}/llm-config")]
    public async Task<ActionResult> UpdateLLMConfiguration(string id, [FromBody] LLMConfiguration config)
    {
        try
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null)
                return NotFound();

            agent.LLMConfigurationJson = JsonSerializer.Serialize(config);
            agent.UpdatedAt = DateTime.UtcNow;

            _context.Agents.Update(agent);
            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating LLM configuration for agent {AgentId}", id);
            return StatusCode(500, "An error occurred while updating the LLM configuration");
        }
    }

    [HttpPost("{id}/tools")]
    public async Task<ActionResult> UpdateTools(string id, [FromBody] List<ToolConfiguration> tools)
    {
        try
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null)
                return NotFound();

            agent.ToolsConfigurationJson = JsonSerializer.Serialize(tools);
            agent.UpdatedAt = DateTime.UtcNow;

            _context.Agents.Update(agent);
            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tools for agent {AgentId}", id);
            return StatusCode(500, "An error occurred while updating the tools");
        }
    }

    [HttpPost("{id}/prompt-template")]
    public async Task<ActionResult> UpdatePromptTemplate(string id, [FromBody] PromptTemplate template)
    {
        try
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null)
                return NotFound();

            agent.PromptTemplateJson = JsonSerializer.Serialize(template);
            agent.UpdatedAt = DateTime.UtcNow;

            _context.Agents.Update(agent);
            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prompt template for agent {AgentId}", id);
            return StatusCode(500, "An error occurred while updating the prompt template");
        }
    }
}
