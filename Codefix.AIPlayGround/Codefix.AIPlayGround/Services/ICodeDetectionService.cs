using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Service for detecting and analyzing code patterns, tools, and capabilities
/// </summary>
public interface ICodeDetectionService
{
    /// <summary>
    /// Analyzes a codebase to detect available tools and capabilities
    /// </summary>
    Task<CodeAnalysisResult> AnalyzeCodebaseAsync(string codebasePath, CodeAnalysisOptions? options = null);
    
    /// <summary>
    /// Detects tools and functions in a specific file
    /// </summary>
    Task<FileAnalysisResult> AnalyzeFileAsync(string filePath, FileAnalysisOptions? options = null);
    
    /// <summary>
    /// Discovers available APIs and endpoints in the codebase
    /// </summary>
    Task<List<DiscoveredApi>> DiscoverApisAsync(string codebasePath);
    
    /// <summary>
    /// Detects database models and entities
    /// </summary>
    Task<List<DiscoveredModel>> DiscoverModelsAsync(string codebasePath);
    
    /// <summary>
    /// Finds service classes and their methods
    /// </summary>
    Task<List<DiscoveredService>> DiscoverServicesAsync(string codebasePath);
    
    /// <summary>
    /// Detects configuration patterns and settings
    /// </summary>
    Task<List<DiscoveredConfiguration>> DiscoverConfigurationsAsync(string codebasePath);
    
    /// <summary>
    /// Analyzes dependencies and package references
    /// </summary>
    Task<List<DiscoveredDependency>> DiscoverDependenciesAsync(string codebasePath);
    
    /// <summary>
    /// Detects test patterns and testing frameworks
    /// </summary>
    Task<List<DiscoveredTest>> DiscoverTestsAsync(string codebasePath);
    
    /// <summary>
    /// Generates a comprehensive codebase map
    /// </summary>
    Task<CodebaseMap> GenerateCodebaseMapAsync(string codebasePath);
    
    /// <summary>
    /// Validates code patterns against security best practices
    /// </summary>
    Task<SecurityAnalysisResult> AnalyzeSecurityAsync(string codebasePath);
    
    /// <summary>
    /// Detects performance patterns and potential issues
    /// </summary>
    Task<PerformanceAnalysisResult> AnalyzePerformanceAsync(string codebasePath);
}

/// <summary>
/// Options for code analysis
/// </summary>
public class CodeAnalysisOptions
{
    public bool IncludeTests { get; set; } = true;
    public bool IncludeDocumentation { get; set; } = true;
    public bool IncludeConfiguration { get; set; } = true;
    public bool IncludeDependencies { get; set; } = true;
    public List<string> ExcludePatterns { get; set; } = new();
    public List<string> IncludePatterns { get; set; } = new();
    public int MaxDepth { get; set; } = 10;
    public bool AnalyzeSecurity { get; set; } = true;
    public bool AnalyzePerformance { get; set; } = true;
}

/// <summary>
/// Options for file analysis
/// </summary>
public class FileAnalysisOptions
{
    public bool IncludeComments { get; set; } = true;
    public bool IncludeImports { get; set; } = true;
    public bool IncludeMetadata { get; set; } = true;
    public List<string> TargetPatterns { get; set; } = new();
}

/// <summary>
/// Result of code analysis
/// </summary>
public class CodeAnalysisResult
{
    public string CodebasePath { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public List<DiscoveredApi> Apis { get; set; } = new();
    public List<DiscoveredModel> Models { get; set; } = new();
    public List<DiscoveredService> Services { get; set; } = new();
    public List<DiscoveredConfiguration> Configurations { get; set; } = new();
    public List<DiscoveredDependency> Dependencies { get; set; } = new();
    public List<DiscoveredTest> Tests { get; set; } = new();
    public SecurityAnalysisResult Security { get; set; } = new();
    public PerformanceAnalysisResult Performance { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of file analysis
/// </summary>
public class FileAnalysisResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public List<DiscoveredFunction> Functions { get; set; } = new();
    public List<DiscoveredClass> Classes { get; set; } = new();
    public List<DiscoveredInterface> Interfaces { get; set; } = new();
    public List<DiscoveredEnum> Enums { get; set; } = new();
    public List<DiscoveredAttribute> Attributes { get; set; } = new();
    public List<DiscoveredMethod> Methods { get; set; } = new();
    public List<DiscoveredProperty> Properties { get; set; } = new();
    public List<string> Imports { get; set; } = new();
    public List<string> Comments { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Discovered API endpoint
/// </summary>
public class DiscoveredApi
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public List<DiscoveredParameter> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = string.Empty;
    public List<string> Attributes { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered data model
/// </summary>
public class DiscoveredModel
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string BaseClass { get; set; } = string.Empty;
    public List<string> Interfaces { get; set; } = new();
    public List<DiscoveredProperty> Properties { get; set; } = new();
    public List<DiscoveredMethod> Methods { get; set; } = new();
    public List<string> Attributes { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered service class
/// </summary>
public class DiscoveredService
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Interface { get; set; } = string.Empty;
    public List<DiscoveredMethod> Methods { get; set; } = new();
    public List<DiscoveredProperty> Properties { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public List<string> Attributes { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered configuration
/// </summary>
public class DiscoveredConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public object? DefaultValue { get; set; }
}

/// <summary>
/// Discovered dependency
/// </summary>
public class DiscoveredDependency
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // NuGet, npm, etc.
    public string FilePath { get; set; } = string.Empty;
    public bool IsDevelopment { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
}

/// <summary>
/// Discovered test
/// </summary>
public class DiscoveredTest
{
    public string Name { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public List<string> Categories { get; set; } = new();
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered function
/// </summary>
public class DiscoveredFunction
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<DiscoveredParameter> Parameters { get; set; } = new();
    public string AccessModifier { get; set; } = string.Empty;
    public bool IsAsync { get; set; }
    public bool IsStatic { get; set; }
    public List<string> Attributes { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered class
/// </summary>
public class DiscoveredClass
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string BaseClass { get; set; } = string.Empty;
    public List<string> Interfaces { get; set; } = new();
    public string AccessModifier { get; set; } = string.Empty;
    public bool IsAbstract { get; set; }
    public bool IsStatic { get; set; }
    public List<string> Attributes { get; set; } = new();
    public List<DiscoveredMethod> Methods { get; set; } = new();
    public List<DiscoveredProperty> Properties { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered interface
/// </summary>
public class DiscoveredInterface
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public List<string> BaseInterfaces { get; set; } = new();
    public List<DiscoveredMethod> Methods { get; set; } = new();
    public List<DiscoveredProperty> Properties { get; set; } = new();
    public string AccessModifier { get; set; } = string.Empty;
    public List<string> Attributes { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered enum
/// </summary>
public class DiscoveredEnum
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string UnderlyingType { get; set; } = string.Empty;
    public List<DiscoveredEnumValue> Values { get; set; } = new();
    public string AccessModifier { get; set; } = string.Empty;
    public List<string> Attributes { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered enum value
/// </summary>
public class DiscoveredEnumValue
{
    public string Name { get; set; } = string.Empty;
    public object Value { get; set; } = string.Empty;
    public List<string> Attributes { get; set; } = new();
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered attribute
/// </summary>
public class DiscoveredAttribute
{
    public string Name { get; set; } = string.Empty;
    public List<object> Arguments { get; set; } = new();
    public string Target { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

/// <summary>
/// Discovered parameter
/// </summary>
public class DiscoveredParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
    public object? DefaultValue { get; set; }
    public List<string> Attributes { get; set; } = new();
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered property
/// </summary>
public class DiscoveredProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string AccessModifier { get; set; } = string.Empty;
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public bool IsOptional { get; set; }
    public object? DefaultValue { get; set; }
    public List<string> Attributes { get; set; } = new();
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered method
/// </summary>
public class DiscoveredMethod
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<DiscoveredParameter> Parameters { get; set; } = new();
    public string AccessModifier { get; set; } = string.Empty;
    public bool IsAsync { get; set; }
    public bool IsStatic { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsAbstract { get; set; }
    public List<string> Attributes { get; set; } = new();
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Documentation { get; set; } = string.Empty;
}

/// <summary>
/// Discovered tool for Microsoft Agent Framework integration
/// </summary>
public class DiscoveredTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<DiscoveredProperty> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public System.Reflection.MethodInfo? MethodInfo { get; set; }
}

/// <summary>
/// Codebase map
/// </summary>
public class CodebaseMap
{
    public string CodebasePath { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<DiscoveredApi> Apis { get; set; } = new();
    public List<DiscoveredModel> Models { get; set; } = new();
    public List<DiscoveredService> Services { get; set; } = new();
    public List<DiscoveredConfiguration> Configurations { get; set; } = new();
    public List<DiscoveredDependency> Dependencies { get; set; } = new();
    public List<DiscoveredTest> Tests { get; set; } = new();
    public Dictionary<string, List<string>> FileStructure { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Security analysis result
/// </summary>
public class SecurityAnalysisResult
{
    public List<SecurityIssue> Issues { get; set; } = new();
    public List<SecurityRecommendation> Recommendations { get; set; } = new();
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Security issue
/// </summary>
public class SecurityIssue
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Security recommendation
/// </summary>
public class SecurityRecommendation
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Implementation { get; set; } = string.Empty;
}

/// <summary>
/// Performance analysis result
/// </summary>
public class PerformanceAnalysisResult
{
    public List<PerformanceIssue> Issues { get; set; } = new();
    public List<PerformanceRecommendation> Recommendations { get; set; } = new();
    public int PerformanceScore { get; set; }
    public string PerformanceLevel { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Performance issue
/// </summary>
public class PerformanceIssue
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

/// <summary>
/// Performance recommendation
/// </summary>
public class PerformanceRecommendation
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Implementation { get; set; } = string.Empty;
}
