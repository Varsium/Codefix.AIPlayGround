using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Services;

public interface IAgentFrameworkService
{
    Task<AgentFrameworkResult> CreateAgentAsync(AgentEntity agent);
    Task<AgentFrameworkResult> UpdateAgentAsync(AgentEntity agent);
    Task<AgentFrameworkResult> DeployAgentAsync(string agentId);
    Task<AgentFrameworkResult> TestAgentAsync(string agentId, object input);
    Task<AgentFrameworkResult> ExecuteFlowAsync(FlowEntity flow, object input);
    Task<AgentFrameworkResult> GetAgentStatusAsync(string agentId);
    Task<AgentFrameworkResult> StopExecutionAsync(string executionId);
    Task<AgentFrameworkResult> ValidateAgentConfigurationAsync(AgentEntity agent);
    Task<AgentFrameworkResult> ValidateFlowConfigurationAsync(FlowEntity flow);
}

public class AgentFrameworkResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public static AgentFrameworkResult Success(string message = "Operation completed successfully", object? data = null)
    {
        return new AgentFrameworkResult
        {
            IsSuccess = true,
            Message = message,
            Data = data
        };
    }

    public static AgentFrameworkResult Failure(string message, List<string>? errors = null)
    {
        return new AgentFrameworkResult
        {
            IsSuccess = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static AgentFrameworkResult Warning(string message, List<string>? warnings = null)
    {
        return new AgentFrameworkResult
        {
            IsSuccess = true,
            Message = message,
            Warnings = warnings ?? new List<string>()
        };
    }
}

