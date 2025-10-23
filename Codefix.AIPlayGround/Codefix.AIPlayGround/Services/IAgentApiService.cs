using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Services;

public interface IAgentApiService
{
    Task<IEnumerable<AgentResponse>> GetAgentsAsync(GetAgentsRequest request);
    Task<AgentDetailResponse?> GetAgentAsync(string agentId);
    Task<AgentResponse> CreateAgentAsync(CreateAgentRequest request);
    Task<AgentResponse> UpdateAgentAsync(string agentId, UpdateAgentRequest request);
    Task<bool> DeleteAgentAsync(string agentId);
    Task<IEnumerable<AgentExecutionResponse>> GetAgentExecutionsAsync(string agentId);
    Task<AgentResponse> TestAgentAsync(string agentId, TestAgentRequest request);
    Task<DeploymentResult> DeployAgentAsync(string agentId);
}
