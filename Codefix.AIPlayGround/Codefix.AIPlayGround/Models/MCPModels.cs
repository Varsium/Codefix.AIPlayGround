using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Codefix.AIPlayGround.Models;

/// <summary>
/// Model Context Protocol (MCP) models for Microsoft Agent Framework integration
/// Based on official sample: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted/ModelContextProtocol
/// </summary>

/// <summary>
/// Model Context Protocol (MCP) configuration for external tool integration
/// </summary>
public class MCPConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    [Required]
    public string ServerUrl { get; set; } = string.Empty;
    public MCPTransportType TransportType { get; set; } = MCPTransportType.HTTP;
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, object> Authentication { get; set; } = new();
    public List<MCPTool> AvailableTools { get; set; } = new();
    public List<MCPResource> AvailableResources { get; set; } = new();
    public List<MCPPrompt> AvailablePrompts { get; set; } = new();
    public MCPCapabilities Capabilities { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// MCP Tool definition
/// </summary>
public class MCPTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> InputSchema { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// MCP Resource definition
/// </summary>
public class MCPResource
{
    public string Uri { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// MCP Prompt definition
/// </summary>
public class MCPPrompt
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Arguments { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// MCP Server capabilities
/// </summary>
public class MCPCapabilities
{
    public bool SupportsTools { get; set; } = false;
    public bool SupportsResources { get; set; } = false;
    public bool SupportsPrompts { get; set; } = false;
    public bool SupportsLogging { get; set; } = false;
    public bool SupportsCompletions { get; set; } = false;
    public Dictionary<string, object> CustomCapabilities { get; set; } = new();
}

/// <summary>
/// MCP Transport types
/// </summary>
public enum MCPTransportType
{
    HTTP,
    WebSocket,
    Stdio,
    NamedPipe,
    TCP
}

/// <summary>
/// MCP Request/Response models
/// </summary>
public class MCPRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Method { get; set; } = string.Empty;
    public Dictionary<string, object>? Params { get; set; }
    public string JsonRpc { get; set; } = "2.0";
}

public class MCPResponse
{
    public string Id { get; set; } = string.Empty;
    public object? Result { get; set; }
    public MCPError? Error { get; set; }
    public string JsonRpc { get; set; } = "2.0";
}

public class MCPError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

/// <summary>
/// MCP Tool call request
/// </summary>
public class MCPToolCallRequest
{
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, object> Arguments { get; set; } = new();
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// MCP Tool call response
/// </summary>
public class MCPToolCallResponse
{
    public string RequestId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public object? Result { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// MCP Server status
/// </summary>
public class MCPServerStatus
{
    public string ServerId { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public DateTime LastPing { get; set; }
    public int ResponseTimeMs { get; set; }
    public MCPCapabilities Capabilities { get; set; } = new();
    public List<string> AvailableTools { get; set; } = new();
    public List<string> AvailableResources { get; set; } = new();
    public List<string> AvailablePrompts { get; set; } = new();
    public Dictionary<string, object> HealthMetrics { get; set; } = new();
}

/// <summary>
/// MCP Integration result
/// </summary>
public class MCPIntegrationResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public MCPServerStatus? ServerStatus { get; set; }
    public List<MCPToolCallResponse> ToolCalls { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// MCP Entity for database storage
/// </summary>
public class MCPEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public MCPTransportType TransportType { get; set; }
    public string ConfigurationJson { get; set; } = string.Empty;
    public string CapabilitiesJson { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
