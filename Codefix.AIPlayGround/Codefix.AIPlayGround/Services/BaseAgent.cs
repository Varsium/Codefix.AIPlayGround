using System.Text.Json;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Base class for all generated agents
/// </summary>
public abstract class BaseAgent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Properties { get; set; } = new();
    public List<string> Logs { get; set; } = new();
    
    /// <summary>
    /// Logs a message to the agent's log
    /// </summary>
    protected void Log(string message)
    {
        var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}";
        Logs.Add(logEntry);
        Console.WriteLine(logEntry);
    }
    
    /// <summary>
    /// Logs an error message to the agent's log
    /// </summary>
    protected void LogError(string message, Exception? exception = null)
    {
        var errorMessage = exception != null ? $"{message}: {exception.Message}" : message;
        var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ERROR: {errorMessage}";
        Logs.Add(logEntry);
        Console.WriteLine(logEntry);
    }
    
    /// <summary>
    /// Serializes an object to JSON
    /// </summary>
    protected string ToJson(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
    }
    
    /// <summary>
    /// Deserializes JSON to an object
    /// </summary>
    protected T? FromJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }
    
    /// <summary>
    /// Gets a property value with default
    /// </summary>
    protected T GetProperty<T>(string key, T defaultValue = default!)
    {
        if (Properties.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// Sets a property value
    /// </summary>
    protected void SetProperty<T>(string key, T value)
    {
        Properties[key] = value!;
    }
    
    /// <summary>
    /// Validates input data
    /// </summary>
    protected virtual bool ValidateInput(object input)
    {
        return input != null;
    }
    
    /// <summary>
    /// Handles errors during execution
    /// </summary>
    protected virtual object HandleError(Exception exception, object? input = null)
    {
        LogError("Execution error", exception);
        return new { Error = exception.Message, Input = input };
    }
    
    /// <summary>
    /// Clears the agent's logs
    /// </summary>
    public void ClearLogs()
    {
        Logs.Clear();
    }
    
    /// <summary>
    /// Gets the agent's status
    /// </summary>
    public virtual object GetStatus()
    {
        return new
        {
            Id,
            Name,
            Description,
            CreatedAt,
            LogCount = Logs.Count,
            Properties = Properties.Count
        };
    }
}

/// <summary>
/// Attribute to mark generated agents
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class GeneratedAgentAttribute : Attribute
{
    public string Version { get; set; } = "1.0.0";
    public string GeneratedBy { get; set; } = "CodeGenerationService";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
