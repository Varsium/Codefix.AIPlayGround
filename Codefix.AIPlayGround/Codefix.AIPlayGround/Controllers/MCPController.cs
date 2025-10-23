using Microsoft.AspNetCore.Mvc;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Services;

namespace Codefix.AIPlayGround.Controllers;

/// <summary>
/// Controller for Model Context Protocol (MCP) integration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MCPController : ControllerBase
{
    private readonly IMCPIntegrationService _mcpService;
    private readonly ILogger<MCPController> _logger;

    public MCPController(IMCPIntegrationService mcpService, ILogger<MCPController> logger)
    {
        _mcpService = mcpService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new MCP server configuration
    /// </summary>
    [HttpPost("servers")]
    public async Task<ActionResult<MCPConfiguration>> RegisterMCPServer([FromBody] MCPConfiguration configuration)
    {
        try
        {
            var result = await _mcpService.RegisterMCPServerAsync(configuration);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering MCP server: {ServerName}", configuration.Name);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all registered MCP server configurations
    /// </summary>
    [HttpGet("servers")]
    public async Task<ActionResult<List<MCPConfiguration>>> GetMCPServers()
    {
        try
        {
            var servers = await _mcpService.GetMCPServersAsync();
            return Ok(servers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MCP servers");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific MCP server configuration by ID
    /// </summary>
    [HttpGet("servers/{serverId}")]
    public async Task<ActionResult<MCPConfiguration>> GetMCPServer(string serverId)
    {
        try
        {
            var server = await _mcpService.GetMCPServerAsync(serverId);
            if (server == null)
            {
                return NotFound(new { error = $"MCP server {serverId} not found" });
            }
            return Ok(server);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MCP server: {ServerId}", serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing MCP server configuration
    /// </summary>
    [HttpPut("servers/{serverId}")]
    public async Task<ActionResult<MCPConfiguration>> UpdateMCPServer(string serverId, [FromBody] MCPConfiguration configuration)
    {
        try
        {
            var result = await _mcpService.UpdateMCPServerAsync(serverId, configuration);
            if (result == null)
            {
                return NotFound(new { error = $"MCP server {serverId} not found" });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating MCP server: {ServerId}", serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an MCP server configuration
    /// </summary>
    [HttpDelete("servers/{serverId}")]
    public async Task<ActionResult> DeleteMCPServer(string serverId)
    {
        try
        {
            var deleted = await _mcpService.DeleteMCPServerAsync(serverId);
            if (!deleted)
            {
                return NotFound(new { error = $"MCP server {serverId} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting MCP server: {ServerId}", serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Connects to an MCP server and retrieves its capabilities
    /// </summary>
    [HttpPost("servers/{serverId}/connect")]
    public async Task<ActionResult<MCPIntegrationResult>> ConnectToMCPServer(string serverId)
    {
        try
        {
            var result = await _mcpService.ConnectToMCPServerAsync(serverId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MCP server: {ServerId}", serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Disconnects from an MCP server
    /// </summary>
    [HttpPost("servers/{serverId}/disconnect")]
    public async Task<ActionResult> DisconnectFromMCPServer(string serverId)
    {
        try
        {
            var disconnected = await _mcpService.DisconnectFromMCPServerAsync(serverId);
            if (!disconnected)
            {
                return NotFound(new { error = $"MCP server {serverId} not found" });
            }
            return Ok(new { message = $"Disconnected from MCP server {serverId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from MCP server: {ServerId}", serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the status of all MCP servers
    /// </summary>
    [HttpGet("servers/status")]
    public async Task<ActionResult<List<MCPServerStatus>>> GetMCPServerStatuses()
    {
        try
        {
            var statuses = await _mcpService.GetMCPServerStatusesAsync();
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MCP server statuses");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the status of a specific MCP server
    /// </summary>
    [HttpGet("servers/{serverId}/status")]
    public async Task<ActionResult<MCPServerStatus>> GetMCPServerStatus(string serverId)
    {
        try
        {
            var status = await _mcpService.GetMCPServerStatusAsync(serverId);
            if (status == null)
            {
                return NotFound(new { error = $"MCP server {serverId} not found" });
            }
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MCP server status: {ServerId}", serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Calls a tool on an MCP server
    /// </summary>
    [HttpPost("servers/{serverId}/tools/call")]
    public async Task<ActionResult<MCPToolCallResponse>> CallMCPTool(string serverId, [FromBody] MCPToolCallRequest request)
    {
        try
        {
            var result = await _mcpService.CallMCPToolAsync(serverId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling MCP tool: {ToolName} on server: {ServerId}", request.ToolName, serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets available tools from an MCP server
    /// </summary>
    [HttpGet("servers/{serverId}/tools")]
    public async Task<ActionResult<List<MCPTool>>> GetMCPTools(string serverId)
    {
        try
        {
            var tools = await _mcpService.GetMCPToolsAsync(serverId);
            return Ok(tools);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MCP tools from server: {ServerId}", serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets available resources from an MCP server
    /// </summary>
    [HttpGet("servers/{serverId}/resources")]
    public async Task<ActionResult<List<MCPResource>>> GetMCPResources(string serverId)
    {
        try
        {
            var resources = await _mcpService.GetMCPResourcesAsync(serverId);
            return Ok(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MCP resources from server: {ServerId}", serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets available prompts from an MCP server
    /// </summary>
    [HttpGet("servers/{serverId}/prompts")]
    public async Task<ActionResult<List<MCPPrompt>>> GetMCPPrompts(string serverId)
    {
        try
        {
            var prompts = await _mcpService.GetMCPPromptsAsync(serverId);
            return Ok(prompts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MCP prompts from server: {ServerId}", serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Tests the connection to an MCP server
    /// </summary>
    [HttpPost("servers/{serverId}/test")]
    public async Task<ActionResult<MCPIntegrationResult>> TestMCPConnection(string serverId)
    {
        try
        {
            var result = await _mcpService.TestMCPConnectionAsync(serverId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing MCP connection: {ServerId}", serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Registers MCP tools with Microsoft Agent Framework agents
    /// </summary>
    [HttpPost("servers/{serverId}/agents/{agentId}/register-tools")]
    public async Task<ActionResult> RegisterMCPToolsWithAgent(string serverId, string agentId)
    {
        try
        {
            var registered = await _mcpService.RegisterMCPToolsWithAgentAsync(serverId, agentId);
            if (!registered)
            {
                return BadRequest(new { error = "Failed to register MCP tools with agent" });
            }
            return Ok(new { message = $"Successfully registered MCP tools from server {serverId} with agent {agentId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering MCP tools with agent: {AgentId} from server: {ServerId}", agentId, serverId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets MCP tools compatible with Microsoft Agent Framework
    /// </summary>
    [HttpGet("servers/{serverId}/agents/{agentId}/tools")]
    public async Task<ActionResult<List<MCPTool>>> GetMCPToolsForAgent(string serverId, string agentId)
    {
        try
        {
            var tools = await _mcpService.GetMCPToolsForAgentAsync(serverId, agentId);
            return Ok(tools);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MCP tools for agent: {AgentId} from server: {ServerId}", agentId, serverId);
            return BadRequest(new { error = ex.Message });
        }
    }
}
