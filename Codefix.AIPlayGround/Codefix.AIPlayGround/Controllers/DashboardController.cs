using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ApplicationDbContext context,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsResponse>> GetDashboardStats()
    {
        try
        {
            var now = DateTime.UtcNow;
            var lastMonth = now.AddMonths(-1);
            var lastWeek = now.AddDays(-7);
            var today = now.Date;

            // Get total agents
            var totalAgents = await _context.Agents.CountAsync();
            var activeAgents = await _context.Agents.CountAsync(a => a.Status == AgentStatus.Active);
            
            // Get agents from last month for comparison
            var totalAgentsLastMonth = await _context.Agents.CountAsync(a => a.CreatedAt <= lastMonth);
            var activeAgentsLastWeek = await _context.Agents.CountAsync(a => 
                a.Status == AgentStatus.Active && a.UpdatedAt >= lastWeek);

            // Calculate percentage changes
            var agentGrowth = totalAgentsLastMonth > 0 
                ? ((double)(totalAgents - totalAgentsLastMonth) / totalAgentsLastMonth * 100) 
                : 0;
            var activeGrowth = activeAgentsLastWeek > 0 
                ? ((double)(activeAgents - activeAgentsLastWeek) / activeAgentsLastWeek * 100) 
                : 0;

            // Get total executions
            var totalExecutions = await _context.AgentExecutions.CountAsync();
            var executionsToday = await _context.AgentExecutions.CountAsync(e => e.StartedAt.Date == today);
            var executionsYesterday = await _context.AgentExecutions
                .CountAsync(e => e.StartedAt.Date == today.AddDays(-1));
            
            var executionGrowth = executionsYesterday > 0 
                ? ((double)(executionsToday - executionsYesterday) / executionsYesterday * 100) 
                : 0;

            // Calculate success rate
            var completedExecutions = await _context.AgentExecutions
                .CountAsync(e => e.Status == ExecutionStatus.Completed);
            var successRate = totalExecutions > 0 
                ? ((double)completedExecutions / totalExecutions * 100) 
                : 0;

            var stats = new DashboardStatsResponse
            {
                TotalAgents = totalAgents,
                ActiveAgents = activeAgents,
                TotalExecutions = totalExecutions,
                SuccessRate = Math.Round(successRate, 1),
                AgentGrowth = Math.Round(agentGrowth, 1),
                ActiveGrowth = Math.Round(activeGrowth, 1),
                ExecutionGrowth = Math.Round(executionGrowth, 1)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500, "An error occurred while retrieving dashboard statistics");
        }
    }

    /// <summary>
    /// Get recent activity for dashboard
    /// </summary>
    [HttpGet("recent-activity")]
    public async Task<ActionResult<IEnumerable<DashboardActivityResponse>>> GetRecentActivity([FromQuery] int limit = 10)
    {
        try
        {
            var activities = new List<DashboardActivityResponse>();

            // Get recent executions
            var recentExecutions = await _context.AgentExecutions
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
            var recentAgents = await _context.Agents
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
            var sortedActivities = activities
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToList();

            return Ok(sortedActivities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent activity");
            return StatusCode(500, "An error occurred while retrieving recent activity");
        }
    }

    /// <summary>
    /// Get agent type distribution for charts
    /// </summary>
    [HttpGet("agent-distribution")]
    public async Task<ActionResult<IEnumerable<AgentDistributionResponse>>> GetAgentDistribution()
    {
        try
        {
            var distribution = await _context.Agents
                .GroupBy(a => a.AgentType)
                .Select(g => new AgentDistributionResponse
                {
                    AgentType = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent distribution");
            return StatusCode(500, "An error occurred while retrieving agent distribution");
        }
    }

    /// <summary>
    /// Get execution trends over time
    /// </summary>
    [HttpGet("execution-trends")]
    public async Task<ActionResult<IEnumerable<ExecutionTrendResponse>>> GetExecutionTrends([FromQuery] int days = 7)
    {
        try
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            
            var trends = await _context.AgentExecutions
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

            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution trends");
            return StatusCode(500, "An error occurred while retrieving execution trends");
        }
    }
}

