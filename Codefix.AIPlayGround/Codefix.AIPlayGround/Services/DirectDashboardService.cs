using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;
using Codefix.AIPlayGround.Services;
using Microsoft.EntityFrameworkCore;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Direct service implementation for Server-side rendering
/// Accesses database directly without HTTP calls
/// Uses DbContextFactory for parallel operations to avoid concurrency issues
/// </summary>
public class DirectDashboardService : IDashboardApiService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<DirectDashboardService> _logger;

    public DirectDashboardService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<DirectDashboardService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<DashboardStatsResponse?> GetDashboardStatsAsync()
    {
        try
        {
            // Create a new context instance for this operation
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var now = DateTime.UtcNow;
            var lastMonth = now.AddMonths(-1);
            var lastWeek = now.AddDays(-7);
            var today = now.Date;

            // Get total agents
            var totalAgents = await context.Agents.CountAsync();
            var activeAgents = await context.Agents.CountAsync(a => a.Status == AgentStatus.Active);
            
            // Get agents from last month for comparison
            var totalAgentsLastMonth = await context.Agents.CountAsync(a => a.CreatedAt <= lastMonth);
            var activeAgentsLastWeek = await context.Agents.CountAsync(a => 
                a.Status == AgentStatus.Active && a.UpdatedAt >= lastWeek);

            // Calculate percentage changes
            var agentGrowth = totalAgentsLastMonth > 0 
                ? ((double)(totalAgents - totalAgentsLastMonth) / totalAgentsLastMonth * 100) 
                : 0;
            var activeGrowth = activeAgentsLastWeek > 0 
                ? ((double)(activeAgents - activeAgentsLastWeek) / activeAgentsLastWeek * 100) 
                : 0;

            // Get total executions
            var totalExecutions = await context.AgentExecutions.CountAsync();
            var executionsToday = await context.AgentExecutions.CountAsync(e => e.StartedAt.Date == today);
            var executionsYesterday = await context.AgentExecutions
                .CountAsync(e => e.StartedAt.Date == today.AddDays(-1));
            
            var executionGrowth = executionsYesterday > 0 
                ? ((double)(executionsToday - executionsYesterday) / executionsYesterday * 100) 
                : 0;

            // Calculate success rate
            var completedExecutions = await context.AgentExecutions
                .CountAsync(e => e.Status == ExecutionStatus.Completed);
            var successRate = totalExecutions > 0 
                ? ((double)completedExecutions / totalExecutions * 100) 
                : 0;

            return new DashboardStatsResponse
            {
                TotalAgents = totalAgents,
                ActiveAgents = activeAgents,
                TotalExecutions = totalExecutions,
                SuccessRate = Math.Round(successRate, 1),
                AgentGrowth = Math.Round(agentGrowth, 1),
                ActiveGrowth = Math.Round(activeGrowth, 1),
                ExecutionGrowth = Math.Round(executionGrowth, 1)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return null;
        }
    }

    public async Task<IEnumerable<DashboardActivityResponse>> GetRecentActivityAsync(int limit = 10)
    {
        try
        {
            // Create a new context instance for this operation
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var activities = new List<DashboardActivityResponse>();

            // Get recent executions
            var recentExecutions = await context.AgentExecutions
                .Include(e => e.Agent)
                .OrderByDescending(e => e.StartedAt)
                .Take(limit / 2)
                .ToListAsync();

            foreach (var execution in recentExecutions)
            {
                activities.Add(new DashboardActivityResponse
                {
                    Type = "execution",
                    Title = execution.Status == ExecutionStatus.Completed 
                        ? "Agent Execution Completed" 
                        : "Agent Execution Failed",
                    Description = $"{execution.Agent.Name} {(execution.Status == ExecutionStatus.Completed ? "completed successfully" : "failed")}",
                    Timestamp = execution.CompletedAt ?? execution.StartedAt,
                    Icon = execution.Status == ExecutionStatus.Completed ? "play-fill" : "x-circle",
                    IconColor = execution.Status == ExecutionStatus.Completed ? "success" : "danger"
                });
            }

            // Get recently created agents
            var recentAgents = await context.Agents
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit / 2)
                .ToListAsync();

            foreach (var agent in recentAgents)
            {
                activities.Add(new DashboardActivityResponse
                {
                    Type = "agent_created",
                    Title = "New Agent Created",
                    Description = $"{agent.Name} was created",
                    Timestamp = agent.CreatedAt,
                    Icon = "plus-circle",
                    IconColor = "primary"
                });
            }

            // Sort by timestamp and limit
            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent activity");
            return Enumerable.Empty<DashboardActivityResponse>();
        }
    }

    public async Task<IEnumerable<AgentDistributionResponse>> GetAgentDistributionAsync()
    {
        try
        {
            // Create a new context instance for this operation
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var distribution = await context.Agents
                .GroupBy(a => a.AgentType)
                .Select(g => new AgentDistributionResponse
                {
                    AgentType = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return distribution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent distribution");
            return Enumerable.Empty<AgentDistributionResponse>();
        }
    }

    public async Task<IEnumerable<ExecutionTrendResponse>> GetExecutionTrendsAsync(int days = 7)
    {
        try
        {
            // Create a new context instance for this operation
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            
            var trends = await context.AgentExecutions
                .Where(e => e.StartedAt >= startDate)
                .GroupBy(e => e.StartedAt.Date)
                .Select(g => new ExecutionTrendResponse
                {
                    Date = g.Key,
                    TotalExecutions = g.Count(),
                    SuccessfulExecutions = g.Count(e => e.Status == ExecutionStatus.Completed),
                    FailedExecutions = g.Count(e => e.Status == ExecutionStatus.Failed)
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution trends");
            return Enumerable.Empty<ExecutionTrendResponse>();
        }
    }
}
