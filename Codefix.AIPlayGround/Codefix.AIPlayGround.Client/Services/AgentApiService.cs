using Codefix.AIPlayGround.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace Codefix.AIPlayGround.Services;

public class AgentApiService : IAgentApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AgentApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<IEnumerable<AgentResponse>> GetAgentsAsync(GetAgentsRequest filter)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(filter.Name))
                queryParams.Add($"name={Uri.EscapeDataString(filter.Name)}");
            if (!string.IsNullOrEmpty(filter.AgentType))
                queryParams.Add($"agentType={Uri.EscapeDataString(filter.AgentType)}");
            if (!string.IsNullOrEmpty(filter.Status))
                queryParams.Add($"status={Uri.EscapeDataString(filter.Status)}");
            if (filter.Page > 0)
                queryParams.Add($"page={filter.Page}");
            if (filter.PageSize > 0)
                queryParams.Add($"pageSize={filter.PageSize}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/api/agents{queryString}");
            response.EnsureSuccessStatusCode();

            var agents = await response.Content.ReadFromJsonAsync<IEnumerable<AgentResponse>>(_jsonOptions);
            return agents ?? Enumerable.Empty<AgentResponse>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<AgentResponse>();
        }
    }

    public async Task<AgentDetailResponse?> GetAgentAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/agents/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AgentDetailResponse>(_jsonOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<AgentResponse> CreateAgentAsync(CreateAgentRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/agents", request, _jsonOptions);
            response.EnsureSuccessStatusCode();

            var createdAgent = await response.Content.ReadFromJsonAsync<AgentResponse>(_jsonOptions);
            return createdAgent ?? throw new InvalidOperationException("Failed to create agent");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<AgentResponse> UpdateAgentAsync(string id, UpdateAgentRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/agents/{id}", request, _jsonOptions);
            response.EnsureSuccessStatusCode();

            var updatedAgent = await response.Content.ReadFromJsonAsync<AgentResponse>(_jsonOptions);
            return updatedAgent ?? throw new InvalidOperationException("Failed to update agent");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> DeleteAgentAsync(string id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/agents/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<IEnumerable<AgentExecutionResponse>> GetAgentExecutionsAsync(string agentId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/agents/{agentId}/executions");
            response.EnsureSuccessStatusCode();

            var executions = await response.Content.ReadFromJsonAsync<IEnumerable<AgentExecutionResponse>>(_jsonOptions);
            return executions ?? Enumerable.Empty<AgentExecutionResponse>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<AgentExecutionResponse>();
        }
    }

    public async Task<bool> ToggleAgentStatusAsync(string id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/agents/{id}/toggle-status", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
