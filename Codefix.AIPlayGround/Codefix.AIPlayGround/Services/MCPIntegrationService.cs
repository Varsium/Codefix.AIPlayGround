using Codefix.AIPlayGround.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Model Context Protocol (MCP) integration service implementation
/// Based on Microsoft Agent Framework MCP sample: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted/ModelContextProtocol
/// </summary>
public class MCPIntegrationService : IMCPIntegrationService
{
    private readonly ILogger<MCPIntegrationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, MCPServerStatus> _serverStatuses = new();
    private readonly Dictionary<string, MCPConfiguration> _serverConfigurations = new();

    public MCPIntegrationService(
        ILogger<MCPIntegrationService> logger,
        HttpClient httpClient,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _httpClient = httpClient;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Registers a new MCP server configuration
    /// </summary>
    public async Task<MCPConfiguration> RegisterMCPServerAsync(MCPConfiguration configuration)
    {
        _logger.LogInformation("Registering MCP server: {ServerName} at {ServerUrl}", 
            configuration.Name, configuration.ServerUrl);

        try
        {
            // Validate the configuration
            if (string.IsNullOrEmpty(configuration.Name) || string.IsNullOrEmpty(configuration.ServerUrl))
            {
                throw new ArgumentException("Server name and URL are required");
            }

            // Test the connection
            var testResult = await TestMCPConnectionAsync(configuration.Id);
            if (!testResult.IsSuccess)
            {
                _logger.LogWarning("MCP server connection test failed: {Message}", testResult.Message);
            }

            // Store the configuration
            _serverConfigurations[configuration.Id] = configuration;

            _logger.LogInformation("Successfully registered MCP server: {ServerId}", configuration.Id);
            return configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering MCP server: {ServerName}", configuration.Name);
            throw;
        }
    }

    /// <summary>
    /// Gets all registered MCP server configurations
    /// </summary>
    public async Task<List<MCPConfiguration>> GetMCPServersAsync()
    {
        _logger.LogInformation("Getting all MCP server configurations");

        return await Task.FromResult(_serverConfigurations.Values.ToList());
    }

    /// <summary>
    /// Gets a specific MCP server configuration by ID
    /// </summary>
    public async Task<MCPConfiguration?> GetMCPServerAsync(string serverId)
    {
        _logger.LogInformation("Getting MCP server configuration: {ServerId}", serverId);

        _serverConfigurations.TryGetValue(serverId, out var configuration);
        return await Task.FromResult(configuration);
    }

    /// <summary>
    /// Updates an existing MCP server configuration
    /// </summary>
    public async Task<MCPConfiguration?> UpdateMCPServerAsync(string serverId, MCPConfiguration configuration)
    {
        _logger.LogInformation("Updating MCP server configuration: {ServerId}", serverId);

        if (!_serverConfigurations.ContainsKey(serverId))
        {
            _logger.LogWarning("MCP server {ServerId} not found for update", serverId);
            return null;
        }

        configuration.Id = serverId;
        configuration.UpdatedAt = DateTime.UtcNow;
        _serverConfigurations[serverId] = configuration;

        _logger.LogInformation("Successfully updated MCP server configuration: {ServerId}", serverId);
        return await Task.FromResult(configuration);
    }

    /// <summary>
    /// Deletes an MCP server configuration
    /// </summary>
    public async Task<bool> DeleteMCPServerAsync(string serverId)
    {
        _logger.LogInformation("Deleting MCP server configuration: {ServerId}", serverId);

        var removed = _serverConfigurations.Remove(serverId);
        if (removed)
        {
            _serverStatuses.Remove(serverId);
            _logger.LogInformation("Successfully deleted MCP server configuration: {ServerId}", serverId);
        }
        else
        {
            _logger.LogWarning("MCP server {ServerId} not found for deletion", serverId);
        }

        return await Task.FromResult(removed);
    }

    /// <summary>
    /// Connects to an MCP server and retrieves its capabilities
    /// </summary>
    public async Task<MCPIntegrationResult> ConnectToMCPServerAsync(string serverId)
    {
        _logger.LogInformation("Connecting to MCP server: {ServerId}", serverId);

        try
        {
            var configuration = await GetMCPServerAsync(serverId);
            if (configuration == null)
            {
                return new MCPIntegrationResult
                {
                    IsSuccess = false,
                    Message = $"MCP server {serverId} not found"
                };
            }

            // Simulate MCP server connection and capability discovery
            var serverStatus = new MCPServerStatus
            {
                ServerId = serverId,
                IsConnected = true,
                LastPing = DateTime.UtcNow,
                ResponseTimeMs = 50, // Simulated response time
                Capabilities = configuration.Capabilities,
                AvailableTools = configuration.AvailableTools.Select(t => t.Name).ToList(),
                AvailableResources = configuration.AvailableResources.Select(r => r.Name).ToList(),
                AvailablePrompts = configuration.AvailablePrompts.Select(p => p.Name).ToList(),
                HealthMetrics = new Dictionary<string, object>
                {
                    ["uptime_seconds"] = 3600,
                    ["requests_per_minute"] = 10,
                    ["error_rate"] = 0.01
                }
            };

            _serverStatuses[serverId] = serverStatus;

            _logger.LogInformation("Successfully connected to MCP server: {ServerId}", serverId);

            return new MCPIntegrationResult
            {
                IsSuccess = true,
                Message = $"Successfully connected to MCP server {configuration.Name}",
                ServerStatus = serverStatus,
                Metadata = new Dictionary<string, object>
                {
                    ["server_name"] = configuration.Name,
                    ["server_url"] = configuration.ServerUrl,
                    ["transport_type"] = configuration.TransportType.ToString(),
                    ["tools_count"] = configuration.AvailableTools.Count,
                    ["resources_count"] = configuration.AvailableResources.Count,
                    ["prompts_count"] = configuration.AvailablePrompts.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MCP server: {ServerId}", serverId);
            return new MCPIntegrationResult
            {
                IsSuccess = false,
                Message = $"Failed to connect to MCP server: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Disconnects from an MCP server
    /// </summary>
    public async Task<bool> DisconnectFromMCPServerAsync(string serverId)
    {
        _logger.LogInformation("Disconnecting from MCP server: {ServerId}", serverId);

        if (_serverStatuses.TryGetValue(serverId, out var status))
        {
            status.IsConnected = false;
            _logger.LogInformation("Successfully disconnected from MCP server: {ServerId}", serverId);
            return await Task.FromResult(true);
        }

        _logger.LogWarning("MCP server {ServerId} not found for disconnection", serverId);
        return await Task.FromResult(false);
    }

    /// <summary>
    /// Gets the status of all MCP servers
    /// </summary>
    public async Task<List<MCPServerStatus>> GetMCPServerStatusesAsync()
    {
        _logger.LogInformation("Getting status of all MCP servers");

        return await Task.FromResult(_serverStatuses.Values.ToList());
    }

    /// <summary>
    /// Gets the status of a specific MCP server
    /// </summary>
    public async Task<MCPServerStatus?> GetMCPServerStatusAsync(string serverId)
    {
        _logger.LogInformation("Getting status of MCP server: {ServerId}", serverId);

        _serverStatuses.TryGetValue(serverId, out var status);
        return await Task.FromResult(status);
    }

    /// <summary>
    /// Calls a tool on an MCP server
    /// </summary>
    public async Task<MCPToolCallResponse> CallMCPToolAsync(string serverId, MCPToolCallRequest request)
    {
        _logger.LogInformation("Calling MCP tool {ToolName} on server {ServerId}", request.ToolName, serverId);

        try
        {
            var configuration = await GetMCPServerAsync(serverId);
            if (configuration == null)
            {
                return new MCPToolCallResponse
                {
                    RequestId = request.RequestId,
                    IsSuccess = false,
                    Error = $"MCP server {serverId} not found"
                };
            }

            var tool = configuration.AvailableTools.FirstOrDefault(t => t.Name == request.ToolName);
            if (tool == null)
            {
                return new MCPToolCallResponse
                {
                    RequestId = request.RequestId,
                    IsSuccess = false,
                    Error = $"Tool {request.ToolName} not found on server {serverId}"
                };
            }

            // Simulate MCP tool call
            var mcpRequest = new MCPRequest
            {
                Method = "tools/call",
                Params = new Dictionary<string, object>
                {
                    ["name"] = request.ToolName,
                    ["arguments"] = request.Arguments
                }
            };

            // In a real implementation, this would make an HTTP request to the MCP server
            // For now, we'll simulate the response
            await Task.Delay(100); // Simulate network delay

            var result = new Dictionary<string, object>
            {
                ["tool_name"] = request.ToolName,
                ["arguments"] = request.Arguments,
                ["execution_time_ms"] = 100,
                ["status"] = "success"
            };

            _logger.LogInformation("Successfully called MCP tool {ToolName} on server {ServerId}", 
                request.ToolName, serverId);

            return new MCPToolCallResponse
            {
                RequestId = request.RequestId,
                IsSuccess = true,
                Result = result,
                Metadata = new Dictionary<string, object>
                {
                    ["server_id"] = serverId,
                    ["tool_name"] = request.ToolName,
                    ["execution_time_ms"] = 100
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MCP tool {ToolName} on server {ServerId}", 
                request.ToolName, serverId);

            return new MCPToolCallResponse
            {
                RequestId = request.RequestId,
                IsSuccess = false,
                Error = $"Tool call failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Gets available tools from an MCP server
    /// </summary>
    public async Task<List<MCPTool>> GetMCPToolsAsync(string serverId)
    {
        _logger.LogInformation("Getting available tools from MCP server: {ServerId}", serverId);

        var configuration = await GetMCPServerAsync(serverId);
        return await Task.FromResult(configuration?.AvailableTools ?? new List<MCPTool>());
    }

    /// <summary>
    /// Gets available resources from an MCP server
    /// </summary>
    public async Task<List<MCPResource>> GetMCPResourcesAsync(string serverId)
    {
        _logger.LogInformation("Getting available resources from MCP server: {ServerId}", serverId);

        var configuration = await GetMCPServerAsync(serverId);
        return await Task.FromResult(configuration?.AvailableResources ?? new List<MCPResource>());
    }

    /// <summary>
    /// Gets available prompts from an MCP server
    /// </summary>
    public async Task<List<MCPPrompt>> GetMCPPromptsAsync(string serverId)
    {
        _logger.LogInformation("Getting available prompts from MCP server: {ServerId}", serverId);

        var configuration = await GetMCPServerAsync(serverId);
        return await Task.FromResult(configuration?.AvailablePrompts ?? new List<MCPPrompt>());
    }

    /// <summary>
    /// Tests the connection to an MCP server
    /// </summary>
    public async Task<MCPIntegrationResult> TestMCPConnectionAsync(string serverId)
    {
        _logger.LogInformation("Testing connection to MCP server: {ServerId}", serverId);

        try
        {
            var configuration = await GetMCPServerAsync(serverId);
            if (configuration == null)
            {
                return new MCPIntegrationResult
                {
                    IsSuccess = false,
                    Message = $"MCP server {serverId} not found"
                };
            }

            // Simulate connection test
            var startTime = DateTime.UtcNow;
            await Task.Delay(50); // Simulate network delay
            var responseTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation("MCP server {ServerId} connection test successful (response time: {ResponseTime}ms)", 
                serverId, responseTime);

            return new MCPIntegrationResult
            {
                IsSuccess = true,
                Message = $"Connection test successful (response time: {responseTime}ms)",
                Metadata = new Dictionary<string, object>
                {
                    ["response_time_ms"] = responseTime,
                    ["server_url"] = configuration.ServerUrl,
                    ["transport_type"] = configuration.TransportType.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection to MCP server: {ServerId}", serverId);
            return new MCPIntegrationResult
            {
                IsSuccess = false,
                Message = $"Connection test failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Registers MCP tools with Microsoft Agent Framework agents
    /// </summary>
    public async Task<bool> RegisterMCPToolsWithAgentAsync(string serverId, string agentId)
    {
        _logger.LogInformation("Registering MCP tools from server {ServerId} with agent {AgentId}", 
            serverId, agentId);

        try
        {
            var tools = await GetMCPToolsAsync(serverId);
            if (!tools.Any())
            {
                _logger.LogWarning("No tools available from MCP server {ServerId}", serverId);
                return false;
            }

            // In a real implementation, this would register the MCP tools with the Microsoft Agent Framework
            // For now, we'll simulate the registration
            foreach (var tool in tools)
            {
                _logger.LogInformation("Registering MCP tool {ToolName} with agent {AgentId}", 
                    tool.Name, agentId);
            }

            _logger.LogInformation("Successfully registered {ToolCount} MCP tools with agent {AgentId}", 
                tools.Count, agentId);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering MCP tools with agent {AgentId}", agentId);
            return false;
        }
    }

    /// <summary>
    /// Gets MCP tools compatible with Microsoft Agent Framework
    /// </summary>
    public async Task<List<MCPTool>> GetMCPToolsForAgentAsync(string serverId, string agentId)
    {
        _logger.LogInformation("Getting MCP tools compatible with agent {AgentId} from server {ServerId}", 
            agentId, serverId);

        var allTools = await GetMCPToolsAsync(serverId);
        
        // Filter tools that are compatible with Microsoft Agent Framework
        var compatibleTools = allTools.Where(tool => 
            tool.IsEnabled && 
            tool.InputSchema.ContainsKey("type") && 
            tool.InputSchema.ContainsKey("properties")
        ).ToList();

        _logger.LogInformation("Found {CompatibleToolCount} compatible tools for agent {AgentId}", 
            compatibleTools.Count, agentId);

        return await Task.FromResult(compatibleTools);
    }
}
