using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;
using Codefix.AIPlayGround.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Direct service implementation for Server-side rendering
/// Accesses database and services directly without HTTP calls
/// Uses DbContextFactory for safe parallel operations
/// </summary>
public class DirectAgentService : IAgentApiService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<DirectAgentService> _logger;

    public DirectAgentService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<DirectAgentService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<AgentResponse>> GetAgentsAsync(GetAgentsRequest filter)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Agents.AsQueryable();

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

            return agents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agents");
            return Enumerable.Empty<AgentResponse>();
        }
    }

    public async Task<AgentDetailResponse?> GetAgentAsync(string id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var agent = await context.Agents
                .Include(a => a.FlowAgents)
                    .ThenInclude(fa => fa.Flow)
                .Include(a => a.Executions.OrderByDescending(e => e.StartedAt).Take(10))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (agent == null)
                return null;

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
                Flows = agent.FlowAgents.Select(fa => new FlowResponse
                {
                    Id = fa.Flow.Id,
                    Name = fa.Flow.Name,
                    Description = fa.Flow.Description,
                    Version = fa.Flow.Version,
                    FlowType = fa.Flow.FlowType,
                    Status = fa.Flow.Status.ToString(),
                    CreatedAt = fa.Flow.CreatedAt,
                    UpdatedAt = fa.Flow.UpdatedAt,
                    CreatedBy = fa.Flow.CreatedBy
                }).ToList(),
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

            return agentDetail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent {AgentId}", id);
            return null;
        }
    }

    public async Task<AgentResponse> CreateAgentAsync(CreateAgentRequest dto)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
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
                CreatedBy = "System" // Will be updated with actual user
            };

            context.Agents.Add(agent);
            await context.SaveChangesAsync();

            return new AgentResponse
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent");
            throw;
        }
    }

    public async Task<AgentResponse> UpdateAgentAsync(string id, UpdateAgentRequest dto)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var agent = await context.Agents.FindAsync(id);
            if (agent == null)
                throw new InvalidOperationException($"Agent {id} not found");

            if (!string.IsNullOrEmpty(dto.Name))
                agent.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Description))
                agent.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.Instructions))
                agent.Instructions = dto.Instructions;

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

            context.Agents.Update(agent);
            await context.SaveChangesAsync();

            return new AgentResponse
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent {AgentId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAgentAsync(string id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var agent = await context.Agents.FindAsync(id);
            if (agent == null)
                return false;

            context.Agents.Remove(agent);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting agent {AgentId}", id);
            return false;
        }
    }

    public async Task<IEnumerable<AgentExecutionResponse>> GetAgentExecutionsAsync(string agentId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var executions = await context.AgentExecutions
                .Where(e => e.AgentId == agentId)
                .OrderByDescending(e => e.StartedAt)
                .Take(50)
                .Select(e => new AgentExecutionResponse
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
                })
                .ToListAsync();

            return executions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching agent executions for {AgentId}", agentId);
            return Enumerable.Empty<AgentExecutionResponse>();
        }
    }

    public async Task<bool> ToggleAgentStatusAsync(string id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var agent = await context.Agents.FindAsync(id);
            if (agent == null)
                return false;

            agent.Status = agent.Status == AgentStatus.Active 
                ? AgentStatus.Inactive 
                : AgentStatus.Active;
            
            agent.UpdatedAt = DateTime.UtcNow;

            context.Agents.Update(agent);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling agent status {AgentId}", id);
            return false;
        }
    }
}
