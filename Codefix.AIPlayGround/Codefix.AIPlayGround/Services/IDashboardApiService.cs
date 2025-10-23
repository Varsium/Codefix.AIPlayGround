using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Services;

public interface IDashboardApiService
{
    Task<DashboardStatsResponse?> GetDashboardStatsAsync();
    Task<IEnumerable<DashboardActivityResponse>> GetRecentActivityAsync(int count = 10);
    Task<IEnumerable<AgentDistributionResponse>> GetAgentDistributionAsync();
    Task<IEnumerable<ExecutionTrendResponse>> GetExecutionTrendsAsync(int days = 30);
}
