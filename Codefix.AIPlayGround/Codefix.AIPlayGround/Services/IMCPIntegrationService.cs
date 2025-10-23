using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Interface for Model Context Protocol (MCP) integration service
/// </summary>
public interface IMCPIntegrationService
{
    /// <summary>
    /// Registers a new MCP server configuration
    /// </summary>
    Task<MCPConfiguration> RegisterMCPServerAsync(MCPConfiguration configuration);

    /// <summary>
    /// Gets all registered MCP server configurations
    /// </summary>
    Task<List<MCPConfiguration>> GetMCPServersAsync();

    /// <summary>
    /// Gets a specific MCP server configuration by ID
    /// </summary>
    Task<MCPConfiguration?> GetMCPServerAsync(string serverId);

    /// <summary>
    /// Updates an existing MCP server configuration
    /// </summary>
    Task<MCPConfiguration?> UpdateMCPServerAsync(string serverId, MCPConfiguration configuration);

    /// <summary>
    /// Deletes an MCP server configuration
    /// </summary>
    Task<bool> DeleteMCPServerAsync(string serverId);

    /// <summary>
    /// Connects to an MCP server and retrieves its capabilities
    /// </summary>
    Task<MCPIntegrationResult> ConnectToMCPServerAsync(string serverId);

    /// <summary>
    /// Disconnects from an MCP server
    /// </summary>
    Task<bool> DisconnectFromMCPServerAsync(string serverId);

    /// <summary>
    /// Gets the status of all MCP servers
    /// </summary>
    Task<List<MCPServerStatus>> GetMCPServerStatusesAsync();

    /// <summary>
    /// Gets the status of a specific MCP server
    /// </summary>
    Task<MCPServerStatus?> GetMCPServerStatusAsync(string serverId);

    /// <summary>
    /// Calls a tool on an MCP server
    /// </summary>
    Task<MCPToolCallResponse> CallMCPToolAsync(string serverId, MCPToolCallRequest request);

    /// <summary>
    /// Gets available tools from an MCP server
    /// </summary>
    Task<List<MCPTool>> GetMCPToolsAsync(string serverId);

    /// <summary>
    /// Gets available resources from an MCP server
    /// </summary>
    Task<List<MCPResource>> GetMCPResourcesAsync(string serverId);

    /// <summary>
    /// Gets available prompts from an MCP server
    /// </summary>
    Task<List<MCPPrompt>> GetMCPPromptsAsync(string serverId);

    /// <summary>
    /// Tests the connection to an MCP server
    /// </summary>
    Task<MCPIntegrationResult> TestMCPConnectionAsync(string serverId);

    /// <summary>
    /// Registers MCP tools with Microsoft Agent Framework agents
    /// </summary>
    Task<bool> RegisterMCPToolsWithAgentAsync(string serverId, string agentId);

    /// <summary>
    /// Gets MCP tools compatible with Microsoft Agent Framework
    /// </summary>
    Task<List<MCPTool>> GetMCPToolsForAgentAsync(string serverId, string agentId);
}
