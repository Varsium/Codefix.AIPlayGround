using Codefix.AIPlayGround.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace Codefix.AIPlayGround.Services;

public class DashboardApiService : IDashboardApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public DashboardApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<DashboardStatsResponse?> GetDashboardStatsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/dashboard/stats");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<DashboardStatsResponse>(_jsonOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<IEnumerable<DashboardActivityResponse>> GetRecentActivityAsync(int limit = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/dashboard/recent-activity?limit={limit}");
            response.EnsureSuccessStatusCode();

            var activities = await response.Content.ReadFromJsonAsync<IEnumerable<DashboardActivityResponse>>(_jsonOptions);
            return activities ?? Enumerable.Empty<DashboardActivityResponse>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<DashboardActivityResponse>();
        }
    }

    public async Task<IEnumerable<AgentDistributionResponse>> GetAgentDistributionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/dashboard/agent-distribution");
            response.EnsureSuccessStatusCode();

            var distribution = await response.Content.ReadFromJsonAsync<IEnumerable<AgentDistributionResponse>>(_jsonOptions);
            return distribution ?? Enumerable.Empty<AgentDistributionResponse>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<AgentDistributionResponse>();
        }
    }

    public async Task<IEnumerable<ExecutionTrendResponse>> GetExecutionTrendsAsync(int days = 7)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/dashboard/execution-trends?days={days}");
            response.EnsureSuccessStatusCode();

            var trends = await response.Content.ReadFromJsonAsync<IEnumerable<ExecutionTrendResponse>>(_jsonOptions);
            return trends ?? Enumerable.Empty<ExecutionTrendResponse>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<ExecutionTrendResponse>();
        }
    }
}
