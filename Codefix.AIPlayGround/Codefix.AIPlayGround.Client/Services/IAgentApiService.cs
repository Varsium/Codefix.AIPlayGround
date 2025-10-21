using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Services;

public interface IAgentApiService
{
    Task<IEnumerable<AgentResponse>> GetAgentsAsync(GetAgentsRequest request);
    Task<AgentDetailResponse?> GetAgentAsync(string id);
    Task<AgentResponse> CreateAgentAsync(CreateAgentRequest request);
    Task<AgentResponse> UpdateAgentAsync(string id, UpdateAgentRequest request);
    Task<bool> DeleteAgentAsync(string id);
    Task<IEnumerable<AgentExecutionResponse>> GetAgentExecutionsAsync(string agentId);
    Task<bool> ToggleAgentStatusAsync(string id);
}
