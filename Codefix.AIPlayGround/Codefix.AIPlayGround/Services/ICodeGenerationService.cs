using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Service for generating and compiling agent code at runtime
/// </summary>
public interface ICodeGenerationService
{
    /// <summary>
    /// Generates C# code for an agent based on specifications
    /// </summary>
    Task<CodeGenerationResult> GenerateAgentCodeAsync(AgentCodeSpecification specification);
    
    /// <summary>
    /// Compiles generated C# code into an assembly
    /// </summary>
    Task<CompilationResult> CompileAgentCodeAsync(string code, string assemblyName);
    
    /// <summary>
    /// Creates a new agent instance from compiled code
    /// </summary>
    Task<AgentInstanceResult> CreateAgentInstanceAsync(string assemblyPath, string className, Dictionary<string, object>? parameters = null);
    
    /// <summary>
    /// Executes a method on a generated agent instance
    /// </summary>
    Task<ExecutionResult> ExecuteAgentMethodAsync(object agentInstance, string methodName, object[]? parameters = null);
    
    /// <summary>
    /// Validates generated code for security and syntax
    /// </summary>
    Task<ValidationResult> ValidateGeneratedCodeAsync(string code);
    
    /// <summary>
    /// Gets available code templates for different agent types
    /// </summary>
    Task<List<CodeTemplate>> GetAvailableTemplatesAsync();
    
    /// <summary>
    /// Generates code from a template with parameters
    /// </summary>
    Task<CodeGenerationResult> GenerateFromTemplateAsync(string templateName, Dictionary<string, object> parameters);
}

/// <summary>
/// Specification for generating agent code
/// </summary>
public class AgentCodeSpecification
{
    public string AgentName { get; set; } = string.Empty;
    public string AgentType { get; set; } = "CustomAgent";
    public string Description { get; set; } = string.Empty;
    public List<AgentMethod> Methods { get; set; } = new();
    public List<AgentProperty> Properties { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public string BaseClass { get; set; } = "BaseAgent";
    public List<string> Interfaces { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
}

/// <summary>
/// Agent method definition
/// </summary>
public class AgentMethod
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = "Task<object>";
    public List<MethodParameter> Parameters { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public string AccessModifier { get; set; } = "public";
    public bool IsAsync { get; set; } = true;
    public List<string> Attributes { get; set; } = new();
}

/// <summary>
/// Method parameter definition
/// </summary>
public class MethodParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "object";
    public object? DefaultValue { get; set; }
    public bool IsOptional { get; set; } = false;
}

/// <summary>
/// Agent property definition
/// </summary>
public class AgentProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "object";
    public string AccessModifier { get; set; } = "public";
    public bool HasGetter { get; set; } = true;
    public bool HasSetter { get; set; } = true;
    public object? DefaultValue { get; set; }
    public List<string> Attributes { get; set; } = new();
}

/// <summary>
/// Security settings for generated code
/// </summary>
public class SecuritySettings
{
    public bool AllowFileSystemAccess { get; set; } = false;
    public bool AllowNetworkAccess { get; set; } = false;
    public bool AllowReflection { get; set; } = false;
    public bool AllowDynamicCode { get; set; } = false;
    public List<string> AllowedNamespaces { get; set; } = new();
    public List<string> BlockedNamespaces { get; set; } = new();
    public int MaxExecutionTimeSeconds { get; set; } = 300;
    public int MaxMemoryMB { get; set; } = 100;
}

/// <summary>
/// Code template definition
/// </summary>
public class CodeTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public List<CodeTemplateParameter> Parameters { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
}

/// <summary>
/// Code template parameter definition
/// </summary>
public class CodeTemplateParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public object? DefaultValue { get; set; }
    public List<string> AllowedValues { get; set; } = new();
}

/// <summary>
/// Result of code generation
/// </summary>
public class CodeGenerationResult
{
    public bool IsSuccess { get; set; }
    public string GeneratedCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of code compilation
/// </summary>
public class CompilationResult
{
    public bool IsSuccess { get; set; }
    public string AssemblyPath { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of agent instance creation
/// </summary>
public class AgentInstanceResult
{
    public bool IsSuccess { get; set; }
    public object? AgentInstance { get; set; }
    public string Message { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of method execution
/// </summary>
public class ExecutionResult
{
    public bool IsSuccess { get; set; }
    public object? Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
