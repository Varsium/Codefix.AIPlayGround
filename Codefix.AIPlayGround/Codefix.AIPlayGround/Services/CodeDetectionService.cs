using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Text.Json;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Codefix.AIPlayGround.Data;
using System.ComponentModel;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Service for detecting and analyzing code patterns, tools, and capabilities
/// </summary>
public class CodeDetectionService : ICodeDetectionService
{
    private readonly ILogger<CodeDetectionService> _logger;
    private readonly IConfiguration _configuration;

    public CodeDetectionService(ILogger<CodeDetectionService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<CodeAnalysisResult> AnalyzeCodebaseAsync(string codebasePath, CodeAnalysisOptions? options = null)
    {
        options ??= new CodeAnalysisOptions();
        
        _logger.LogInformation("Starting codebase analysis for: {CodebasePath}", codebasePath);

        var result = new CodeAnalysisResult
        {
            CodebasePath = codebasePath,
            AnalyzedAt = DateTime.UtcNow
        };

        try
        {
            // Discover different components
            if (options.IncludeDependencies)
            {
                result.Dependencies = await DiscoverDependenciesAsync(codebasePath);
            }

            result.Apis = await DiscoverApisAsync(codebasePath);
            result.Models = await DiscoverModelsAsync(codebasePath);
            result.Services = await DiscoverServicesAsync(codebasePath);
            result.Configurations = await DiscoverConfigurationsAsync(codebasePath);

            if (options.IncludeTests)
            {
                result.Tests = await DiscoverTestsAsync(codebasePath);
            }

            if (options.AnalyzeSecurity)
            {
                result.Security = await AnalyzeSecurityAsync(codebasePath);
            }

            if (options.AnalyzePerformance)
            {
                result.Performance = await AnalyzePerformanceAsync(codebasePath);
            }

            _logger.LogInformation("Codebase analysis completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during codebase analysis");
            result.Errors.Add($"Analysis failed: {ex.Message}");
        }

        return result;
    }

    public async Task<FileAnalysisResult> AnalyzeFileAsync(string filePath, FileAnalysisOptions? options = null)
    {
        options ??= new FileAnalysisOptions();
        
        _logger.LogInformation("Analyzing file: {FilePath}", filePath);

        var result = new FileAnalysisResult
        {
            FilePath = filePath,
            FileType = Path.GetExtension(filePath).ToLowerInvariant(),
            FileSize = new FileInfo(filePath).Length,
            LastModified = File.GetLastWriteTime(filePath)
        };

        try
        {
            if (result.FileType == ".cs")
            {
                await AnalyzeCSharpFileAsync(filePath, result, options);
            }
            else if (result.FileType == ".json")
            {
                await AnalyzeJsonFileAsync(filePath, result, options);
            }
            else if (result.FileType == ".xml")
            {
                await AnalyzeXmlFileAsync(filePath, result, options);
            }
            else
            {
                await AnalyzeTextFileAsync(filePath, result, options);
            }

            _logger.LogInformation("File analysis completed: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing file: {FilePath}", filePath);
            result.Metadata["Error"] = ex.Message;
        }

        return result;
    }

    public async Task<List<DiscoveredApi>> DiscoverApisAsync(string codebasePath)
    {
        var apis = new List<DiscoveredApi>();
        
        try
        {
            var controllerFiles = Directory.GetFiles(codebasePath, "*Controller.cs", SearchOption.AllDirectories);
            
            foreach (var file in controllerFiles)
            {
                var fileAnalysis = await AnalyzeFileAsync(file);
                
                foreach (var method in fileAnalysis.Methods)
                {
                    if (method.Attributes.Any(a => a.Contains("HttpGet") || a.Contains("HttpPost") || 
                                                 a.Contains("HttpPut") || a.Contains("HttpDelete")))
                    {
                        var api = new DiscoveredApi
                        {
                            Name = method.Name,
                            Controller = ExtractControllerName(file),
                            Action = method.Name,
                            Method = ExtractHttpMethod(method.Attributes),
                            Path = ExtractRoute(method.Attributes),
                            ReturnType = method.ReturnType,
                            Parameters = method.Parameters.Select(p => new DiscoveredParameter
                            {
                                Name = p.Name,
                                Type = p.Type,
                                IsOptional = p.IsOptional,
                                DefaultValue = p.DefaultValue,
                                Attributes = p.Attributes,
                                Documentation = p.Documentation
                            }).ToList(),
                            Attributes = method.Attributes,
                            FilePath = file,
                            LineNumber = method.LineNumber,
                            Documentation = method.Documentation
                        };
                        
                        apis.Add(api);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering APIs in: {CodebasePath}", codebasePath);
        }

        return apis;
    }

    public async Task<List<DiscoveredModel>> DiscoverModelsAsync(string codebasePath)
    {
        var models = new List<DiscoveredModel>();
        
        try
        {
            var modelFiles = Directory.GetFiles(codebasePath, "*.cs", SearchOption.AllDirectories)
                .Where(f => f.Contains("Model") || f.Contains("Entity") || f.Contains("Dto"))
                .ToList();

            foreach (var file in modelFiles)
            {
                var fileAnalysis = await AnalyzeFileAsync(file);
                
                foreach (var classInfo in fileAnalysis.Classes)
                {
                    var model = new DiscoveredModel
                    {
                        Name = classInfo.Name,
                        Namespace = classInfo.Namespace,
                        BaseClass = classInfo.BaseClass,
                        Interfaces = classInfo.Interfaces,
                        Properties = classInfo.Properties.Select(p => new DiscoveredProperty
                        {
                            Name = p.Name,
                            Type = p.Type,
                            AccessModifier = p.AccessModifier,
                            HasGetter = p.HasGetter,
                            HasSetter = p.HasSetter,
                            Attributes = p.Attributes,
                            Documentation = p.Documentation
                        }).ToList(),
                        Methods = classInfo.Methods.Select(m => new DiscoveredMethod
                        {
                            Name = m.Name,
                            ReturnType = m.ReturnType,
                            Parameters = m.Parameters.Select(p => new DiscoveredParameter
                            {
                                Name = p.Name,
                                Type = p.Type,
                                IsOptional = p.IsOptional,
                                DefaultValue = p.DefaultValue,
                                Attributes = p.Attributes,
                                Documentation = p.Documentation
                            }).ToList(),
                            AccessModifier = m.AccessModifier,
                            IsAsync = m.IsAsync,
                            IsStatic = m.IsStatic,
                            IsVirtual = m.IsVirtual,
                            IsAbstract = m.IsAbstract,
                            Attributes = m.Attributes,
                            FilePath = m.FilePath,
                            LineNumber = m.LineNumber,
                            Documentation = m.Documentation
                        }).ToList(),
                        Attributes = classInfo.Attributes,
                        FilePath = file,
                        LineNumber = classInfo.LineNumber,
                        Documentation = classInfo.Documentation
                    };
                    
                    models.Add(model);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering models in: {CodebasePath}", codebasePath);
        }

        return models;
    }

    public async Task<List<DiscoveredService>> DiscoverServicesAsync(string codebasePath)
    {
        var services = new List<DiscoveredService>();
        
        try
        {
            var serviceFiles = Directory.GetFiles(codebasePath, "*Service.cs", SearchOption.AllDirectories);
            
            foreach (var file in serviceFiles)
            {
                var fileAnalysis = await AnalyzeFileAsync(file);
                
                foreach (var classInfo in fileAnalysis.Classes)
                {
                    var service = new DiscoveredService
                    {
                        Name = classInfo.Name,
                        Namespace = classInfo.Namespace,
                        Interface = ExtractInterfaceName(classInfo.Interfaces),
                        Methods = classInfo.Methods.Select(m => new DiscoveredMethod
                        {
                            Name = m.Name,
                            ReturnType = m.ReturnType,
                            Parameters = m.Parameters.Select(p => new DiscoveredParameter
                            {
                                Name = p.Name,
                                Type = p.Type,
                                IsOptional = p.IsOptional,
                                DefaultValue = p.DefaultValue,
                                Attributes = p.Attributes,
                                Documentation = p.Documentation
                            }).ToList(),
                            AccessModifier = m.AccessModifier,
                            IsAsync = m.IsAsync,
                            IsStatic = m.IsStatic,
                            IsVirtual = m.IsVirtual,
                            IsAbstract = m.IsAbstract,
                            Attributes = m.Attributes,
                            FilePath = m.FilePath,
                            LineNumber = m.LineNumber,
                            Documentation = m.Documentation
                        }).ToList(),
                        Properties = classInfo.Properties.Select(p => new DiscoveredProperty
                        {
                            Name = p.Name,
                            Type = p.Type,
                            AccessModifier = p.AccessModifier,
                            HasGetter = p.HasGetter,
                            HasSetter = p.HasSetter,
                            Attributes = p.Attributes,
                            Documentation = p.Documentation
                        }).ToList(),
                        Dependencies = ExtractDependencies(classInfo.Methods),
                        Attributes = classInfo.Attributes,
                        FilePath = file,
                        LineNumber = classInfo.LineNumber,
                        Documentation = classInfo.Documentation
                    };
                    
                    services.Add(service);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering services in: {CodebasePath}", codebasePath);
        }

        return services;
    }

    public async Task<List<DiscoveredConfiguration>> DiscoverConfigurationsAsync(string codebasePath)
    {
        var configurations = new List<DiscoveredConfiguration>();
        
        try
        {
            var configFiles = Directory.GetFiles(codebasePath, "appsettings*.json", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(codebasePath, "*.config", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(codebasePath, "web.config", SearchOption.AllDirectories))
                .ToList();

            foreach (var file in configFiles)
            {
                if (file.EndsWith(".json"))
                {
                    await DiscoverJsonConfigurationsAsync(file, configurations);
                }
                else if (file.EndsWith(".config"))
                {
                    await DiscoverXmlConfigurationsAsync(file, configurations);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering configurations in: {CodebasePath}", codebasePath);
        }

        return configurations;
    }

    public async Task<List<DiscoveredDependency>> DiscoverDependenciesAsync(string codebasePath)
    {
        var dependencies = new List<DiscoveredDependency>();
        
        try
        {
            // Look for .csproj files
            var projectFiles = Directory.GetFiles(codebasePath, "*.csproj", SearchOption.AllDirectories);
            
            foreach (var projectFile in projectFiles)
            {
                await DiscoverProjectDependenciesAsync(projectFile, dependencies);
            }

            // Look for packages.config files
            var packagesConfigFiles = Directory.GetFiles(codebasePath, "packages.config", SearchOption.AllDirectories);
            
            foreach (var packagesFile in packagesConfigFiles)
            {
                await DiscoverPackagesConfigDependenciesAsync(packagesFile, dependencies);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering dependencies in: {CodebasePath}", codebasePath);
        }

        return dependencies;
    }

    public async Task<List<DiscoveredTest>> DiscoverTestsAsync(string codebasePath)
    {
        var tests = new List<DiscoveredTest>();
        
        try
        {
            var testFiles = Directory.GetFiles(codebasePath, "*Test*.cs", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(codebasePath, "*Tests*.cs", SearchOption.AllDirectories))
                .ToList();

            foreach (var file in testFiles)
            {
                var fileAnalysis = await AnalyzeFileAsync(file);
                
                foreach (var method in fileAnalysis.Methods)
                {
                    if (method.Attributes.Any(a => a.Contains("Test") || a.Contains("Fact") || a.Contains("Theory")))
                    {
                        var test = new DiscoveredTest
                        {
                            Name = method.Name,
                            Class = ExtractClassName(file),
                            Method = method.Name,
                            Framework = DetectTestFramework(method.Attributes),
                            FilePath = file,
                            LineNumber = method.LineNumber,
                            Categories = ExtractTestCategories(method.Attributes),
                            Documentation = method.Documentation
                        };
                        
                        tests.Add(test);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering tests in: {CodebasePath}", codebasePath);
        }

        return tests;
    }

    public async Task<CodebaseMap> GenerateCodebaseMapAsync(string codebasePath)
    {
        _logger.LogInformation("Generating codebase map for: {CodebasePath}", codebasePath);

        var map = new CodebaseMap
        {
            CodebasePath = codebasePath,
            GeneratedAt = DateTime.UtcNow
        };

        try
        {
            map.Apis = await DiscoverApisAsync(codebasePath);
            map.Models = await DiscoverModelsAsync(codebasePath);
            map.Services = await DiscoverServicesAsync(codebasePath);
            map.Configurations = await DiscoverConfigurationsAsync(codebasePath);
            map.Dependencies = await DiscoverDependenciesAsync(codebasePath);
            map.Tests = await DiscoverTestsAsync(codebasePath);
            map.FileStructure = await GenerateFileStructureAsync(codebasePath);

            _logger.LogInformation("Codebase map generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating codebase map for: {CodebasePath}", codebasePath);
        }

        return map;
    }

    public async Task<SecurityAnalysisResult> AnalyzeSecurityAsync(string codebasePath)
    {
        var result = new SecurityAnalysisResult();
        
        try
        {
            // Basic security analysis - can be enhanced with more sophisticated tools
            var issues = new List<SecurityIssue>();
            var recommendations = new List<SecurityRecommendation>();

            // Look for common security issues
            await AnalyzeSecurityPatternsAsync(codebasePath, issues, recommendations);

            result.Issues = issues;
            result.Recommendations = recommendations;
            result.RiskScore = CalculateRiskScore(issues);
            result.RiskLevel = DetermineRiskLevel(result.RiskScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during security analysis for: {CodebasePath}", codebasePath);
        }

        return result;
    }

    public async Task<PerformanceAnalysisResult> AnalyzePerformanceAsync(string codebasePath)
    {
        var result = new PerformanceAnalysisResult();
        
        try
        {
            // Basic performance analysis - can be enhanced with more sophisticated tools
            var issues = new List<PerformanceIssue>();
            var recommendations = new List<PerformanceRecommendation>();

            // Look for common performance issues
            await AnalyzePerformancePatternsAsync(codebasePath, issues, recommendations);

            result.Issues = issues;
            result.Recommendations = recommendations;
            result.PerformanceScore = CalculatePerformanceScore(issues);
            result.PerformanceLevel = DeterminePerformanceLevel(result.PerformanceScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during performance analysis for: {CodebasePath}", codebasePath);
        }

        return result;
    }

    #region Private Helper Methods

    private async Task AnalyzeCSharpFileAsync(string filePath, FileAnalysisResult result, FileAnalysisOptions options)
    {
        try
        {
            var code = await File.ReadAllTextAsync(filePath);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = await syntaxTree.GetRootAsync();

            // Extract classes
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var classDecl in classes)
            {
                result.Classes.Add(new DiscoveredClass
                {
                    Name = classDecl.Identifier.ValueText,
                    Namespace = ExtractNamespace(root),
                    AccessModifier = classDecl.Modifiers.ToString(),
                    IsAbstract = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
                    IsStatic = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                    Attributes = classDecl.AttributeLists.SelectMany(a => a.Attributes).Select(a => a.ToString()).ToList(),
                    FilePath = filePath,
                    LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Documentation = ExtractDocumentation(classDecl)
                });
            }

            // Extract interfaces
            var interfaces = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            foreach (var interfaceDecl in interfaces)
            {
                result.Interfaces.Add(new DiscoveredInterface
                {
                    Name = interfaceDecl.Identifier.ValueText,
                    Namespace = ExtractNamespace(root),
                    AccessModifier = interfaceDecl.Modifiers.ToString(),
                    Attributes = interfaceDecl.AttributeLists.SelectMany(a => a.Attributes).Select(a => a.ToString()).ToList(),
                    FilePath = filePath,
                    LineNumber = interfaceDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Documentation = ExtractDocumentation(interfaceDecl)
                });
            }

            // Extract methods
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var methodDecl in methods)
            {
                result.Methods.Add(new DiscoveredMethod
                {
                    Name = methodDecl.Identifier.ValueText,
                    ReturnType = methodDecl.ReturnType.ToString(),
                    Parameters = methodDecl.ParameterList.Parameters.Select(p => new DiscoveredParameter
                    {
                        Name = p.Identifier.ValueText,
                        Type = p.Type?.ToString() ?? "void",
                        IsOptional = p.Default != null,
                        DefaultValue = p.Default?.Value?.ToString(),
                        Attributes = p.AttributeLists.SelectMany(a => a.Attributes).Select(a => a.ToString()).ToList(),
                        Documentation = ExtractDocumentation(p)
                    }).ToList(),
                    AccessModifier = methodDecl.Modifiers.ToString(),
                    IsAsync = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)),
                    IsStatic = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                    IsVirtual = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)),
                    IsAbstract = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
                    Attributes = methodDecl.AttributeLists.SelectMany(a => a.Attributes).Select(a => a.ToString()).ToList(),
                    FilePath = filePath,
                    LineNumber = methodDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Documentation = ExtractDocumentation(methodDecl)
                });
            }

            // Extract properties
            var properties = root.DescendantNodes().OfType<PropertyDeclarationSyntax>();
            foreach (var propDecl in properties)
            {
                result.Properties.Add(new DiscoveredProperty
                {
                    Name = propDecl.Identifier.ValueText,
                    Type = propDecl.Type.ToString(),
                    AccessModifier = propDecl.Modifiers.ToString(),
                    HasGetter = propDecl.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false,
                    HasSetter = propDecl.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false,
                    Attributes = propDecl.AttributeLists.SelectMany(a => a.Attributes).Select(a => a.ToString()).ToList(),
                    Documentation = ExtractDocumentation(propDecl)
                });
            }

            // Extract enums
            var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
            foreach (var enumDecl in enums)
            {
                result.Enums.Add(new DiscoveredEnum
                {
                    Name = enumDecl.Identifier.ValueText,
                    Namespace = ExtractNamespace(root),
                    UnderlyingType = enumDecl.BaseList?.Types.FirstOrDefault()?.ToString() ?? "int",
                    Values = enumDecl.Members.Select(m => new DiscoveredEnumValue
                    {
                        Name = m.Identifier.ValueText,
                        Value = m.EqualsValue?.Value?.ToString() ?? "0",
                        Attributes = m.AttributeLists.SelectMany(a => a.Attributes).Select(a => a.ToString()).ToList(),
                        Documentation = ExtractDocumentation(m)
                    }).ToList(),
                    AccessModifier = enumDecl.Modifiers.ToString(),
                    Attributes = enumDecl.AttributeLists.SelectMany(a => a.Attributes).Select(a => a.ToString()).ToList(),
                    FilePath = filePath,
                    LineNumber = enumDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Documentation = ExtractDocumentation(enumDecl)
                });
            }

            if (options.IncludeImports)
            {
                result.Imports = root.DescendantNodes().OfType<UsingDirectiveSyntax>()
                    .Select(u => u.Name.ToString()).ToList();
            }

            if (options.IncludeComments)
            {
                result.Comments = root.DescendantTrivia()
                    .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia))
                    .Select(t => t.ToString().Trim()).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing C# file: {FilePath}", filePath);
        }
    }

    private async Task AnalyzeJsonFileAsync(string filePath, FileAnalysisResult result, FileAnalysisOptions options)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var document = JsonDocument.Parse(json);
            
            result.Metadata["JsonProperties"] = ExtractJsonProperties(document.RootElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing JSON file: {FilePath}", filePath);
        }
    }

    private async Task AnalyzeXmlFileAsync(string filePath, FileAnalysisResult result, FileAnalysisOptions options)
    {
        try
        {
            var xml = await File.ReadAllTextAsync(filePath);
            var document = XDocument.Parse(xml);
            
            result.Metadata["XmlElements"] = ExtractXmlElements(document.Root);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing XML file: {FilePath}", filePath);
        }
    }

    private async Task AnalyzeTextFileAsync(string filePath, FileAnalysisResult result, FileAnalysisOptions options)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            result.Metadata["LineCount"] = content.Split('\n').Length;
            result.Metadata["WordCount"] = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing text file: {FilePath}", filePath);
        }
    }

    private string ExtractControllerName(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        return fileName.Replace("Controller", "");
    }

    private string ExtractHttpMethod(List<string> attributes)
    {
        if (attributes.Any(a => a.Contains("HttpGet"))) return "GET";
        if (attributes.Any(a => a.Contains("HttpPost"))) return "POST";
        if (attributes.Any(a => a.Contains("HttpPut"))) return "PUT";
        if (attributes.Any(a => a.Contains("HttpDelete"))) return "DELETE";
        return "GET";
    }

    private string ExtractRoute(List<string> attributes)
    {
        var routeAttr = attributes.FirstOrDefault(a => a.Contains("Route"));
        if (routeAttr != null)
        {
            var match = Regex.Match(routeAttr, @"Route\(""([^""]+)""\)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }
        return "";
    }

    private string ExtractInterfaceName(List<string> interfaces)
    {
        return interfaces.FirstOrDefault() ?? "";
    }

    private List<string> ExtractDependencies(List<DiscoveredMethod> methods)
    {
        var dependencies = new List<string>();
        foreach (var method in methods)
        {
            foreach (var param in method.Parameters)
            {
                if (param.Type.Contains("Service") || param.Type.Contains("Repository"))
                {
                    dependencies.Add(param.Type);
                }
            }
        }
        return dependencies.Distinct().ToList();
    }

    private async Task DiscoverJsonConfigurationsAsync(string filePath, List<DiscoveredConfiguration> configurations)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var document = JsonDocument.Parse(json);
            
            ExtractJsonConfigurations(document.RootElement, configurations, filePath, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering JSON configurations: {FilePath}", filePath);
        }
    }

    private void ExtractJsonConfigurations(JsonElement element, List<DiscoveredConfiguration> configurations, string filePath, string prefix)
    {
        foreach (var property in element.EnumerateObject())
        {
            var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";
            
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                ExtractJsonConfigurations(property.Value, configurations, filePath, key);
            }
            else
            {
                configurations.Add(new DiscoveredConfiguration
                {
                    Name = key,
                    Type = property.Value.ValueKind.ToString(),
                    Value = property.Value.ToString(),
                    FilePath = filePath,
                    Description = $"Configuration value from {Path.GetFileName(filePath)}"
                });
            }
        }
    }

    private async Task DiscoverXmlConfigurationsAsync(string filePath, List<DiscoveredConfiguration> configurations)
    {
        try
        {
            var xml = await File.ReadAllTextAsync(filePath);
            var document = XDocument.Parse(xml);
            
            ExtractXmlConfigurations(document.Root, configurations, filePath, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering XML configurations: {FilePath}", filePath);
        }
    }

    private void ExtractXmlConfigurations(XElement element, List<DiscoveredConfiguration> configurations, string filePath, string prefix)
    {
        foreach (var child in element.Elements())
        {
            var key = string.IsNullOrEmpty(prefix) ? child.Name.LocalName : $"{prefix}:{child.Name.LocalName}";
            
            if (child.HasElements)
            {
                ExtractXmlConfigurations(child, configurations, filePath, key);
            }
            else
            {
                configurations.Add(new DiscoveredConfiguration
                {
                    Name = key,
                    Type = "string",
                    Value = child.Value,
                    FilePath = filePath,
                    Description = $"Configuration value from {Path.GetFileName(filePath)}"
                });
            }
        }
    }

    private async Task DiscoverProjectDependenciesAsync(string projectFile, List<DiscoveredDependency> dependencies)
    {
        try
        {
            var content = await File.ReadAllTextAsync(projectFile);
            var document = XDocument.Parse(content);
            
            var packageReferences = document.Descendants("PackageReference");
            foreach (var packageRef in packageReferences)
            {
                dependencies.Add(new DiscoveredDependency
                {
                    Name = packageRef.Attribute("Include")?.Value ?? "",
                    Version = packageRef.Attribute("Version")?.Value ?? "",
                    Type = "NuGet",
                    FilePath = projectFile,
                    IsDevelopment = packageRef.Attribute("PrivateAssets")?.Value == "all"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering project dependencies: {ProjectFile}", projectFile);
        }
    }

    private async Task DiscoverPackagesConfigDependenciesAsync(string packagesFile, List<DiscoveredDependency> dependencies)
    {
        try
        {
            var content = await File.ReadAllTextAsync(packagesFile);
            var document = XDocument.Parse(content);
            
            var packages = document.Descendants("package");
            foreach (var package in packages)
            {
                dependencies.Add(new DiscoveredDependency
                {
                    Name = package.Attribute("id")?.Value ?? "",
                    Version = package.Attribute("version")?.Value ?? "",
                    Type = "NuGet",
                    FilePath = packagesFile,
                    IsDevelopment = package.Attribute("developmentDependency")?.Value == "true"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering packages.config dependencies: {PackagesFile}", packagesFile);
        }
    }

    private string ExtractClassName(string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    private string DetectTestFramework(List<string> attributes)
    {
        if (attributes.Any(a => a.Contains("Test"))) return "NUnit";
        if (attributes.Any(a => a.Contains("Fact") || a.Contains("Theory"))) return "xUnit";
        if (attributes.Any(a => a.Contains("TestMethod"))) return "MSTest";
        return "Unknown";
    }

    private List<string> ExtractTestCategories(List<string> attributes)
    {
        var categories = new List<string>();
        foreach (var attr in attributes)
        {
            if (attr.Contains("Category"))
            {
                var match = Regex.Match(attr, @"Category\(""([^""]+)""\)");
                if (match.Success)
                {
                    categories.Add(match.Groups[1].Value);
                }
            }
        }
        return categories;
    }

    private async Task<Dictionary<string, List<string>>> GenerateFileStructureAsync(string codebasePath)
    {
        var structure = new Dictionary<string, List<string>>();
        
        try
        {
            var directories = Directory.GetDirectories(codebasePath, "*", SearchOption.AllDirectories);
            
            foreach (var dir in directories)
            {
                var relativePath = Path.GetRelativePath(codebasePath, dir);
                var files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly)
                    .Select(f => Path.GetFileName(f))
                    .ToList();
                
                structure[relativePath] = files;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating file structure for: {CodebasePath}", codebasePath);
        }

        return structure;
    }

    private async Task AnalyzeSecurityPatternsAsync(string codebasePath, List<SecurityIssue> issues, List<SecurityRecommendation> recommendations)
    {
        // Basic security pattern analysis
        var files = Directory.GetFiles(codebasePath, "*.cs", SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            var lines = content.Split('\n');
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                
                // Check for SQL injection vulnerabilities
                if (line.Contains("SqlCommand") && line.Contains("+"))
                {
                    issues.Add(new SecurityIssue
                    {
                        Type = "SQL Injection",
                        Severity = "High",
                        Description = "Potential SQL injection vulnerability detected",
                        FilePath = file,
                        LineNumber = i + 1,
                        Recommendation = "Use parameterized queries instead of string concatenation"
                    });
                }
                
                // Check for hardcoded passwords
                if (line.Contains("password") && line.Contains("=") && line.Contains("\""))
                {
                    issues.Add(new SecurityIssue
                    {
                        Type = "Hardcoded Password",
                        Severity = "High",
                        Description = "Hardcoded password detected",
                        FilePath = file,
                        LineNumber = i + 1,
                        Recommendation = "Move password to configuration or use secure storage"
                    });
                }
            }
        }
    }

    private async Task AnalyzePerformancePatternsAsync(string codebasePath, List<PerformanceIssue> issues, List<PerformanceRecommendation> recommendations)
    {
        // Basic performance pattern analysis
        var files = Directory.GetFiles(codebasePath, "*.cs", SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            var lines = content.Split('\n');
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                
                // Check for N+1 query problems
                if (line.Contains("foreach") && line.Contains("Select"))
                {
                    issues.Add(new PerformanceIssue
                    {
                        Type = "N+1 Query",
                        Severity = "Medium",
                        Description = "Potential N+1 query problem detected",
                        FilePath = file,
                        LineNumber = i + 1,
                        Recommendation = "Consider using Include() or projection to load related data in a single query"
                    });
                }
                
                // Check for string concatenation in loops
                if (line.Contains("+=") && line.Contains("string"))
                {
                    issues.Add(new PerformanceIssue
                    {
                        Type = "String Concatenation in Loop",
                        Severity = "Low",
                        Description = "String concatenation in loop detected",
                        FilePath = file,
                        LineNumber = i + 1,
                        Recommendation = "Use StringBuilder for string concatenation in loops"
                    });
                }
            }
        }
    }

    private int CalculateRiskScore(List<SecurityIssue> issues)
    {
        var score = 0;
        foreach (var issue in issues)
        {
            score += issue.Severity switch
            {
                "High" => 10,
                "Medium" => 5,
                "Low" => 1,
                _ => 0
            };
        }
        return score;
    }

    private string DetermineRiskLevel(int riskScore)
    {
        return riskScore switch
        {
            >= 20 => "Critical",
            >= 10 => "High",
            >= 5 => "Medium",
            >= 1 => "Low",
            _ => "None"
        };
    }

    private int CalculatePerformanceScore(List<PerformanceIssue> issues)
    {
        var score = 0;
        foreach (var issue in issues)
        {
            score += issue.Severity switch
            {
                "High" => 10,
                "Medium" => 5,
                "Low" => 1,
                _ => 0
            };
        }
        return score;
    }

    private string DeterminePerformanceLevel(int performanceScore)
    {
        return performanceScore switch
        {
            >= 20 => "Poor",
            >= 10 => "Fair",
            >= 5 => "Good",
            >= 1 => "Very Good",
            _ => "Excellent"
        };
    }

    private string ExtractNamespace(SyntaxNode root)
    {
        var namespaceDecl = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        return namespaceDecl?.Name.ToString() ?? "";
    }

    private string ExtractDocumentation(SyntaxNode node)
    {
        var trivia = node.GetLeadingTrivia()
            .Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || 
                       t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            .FirstOrDefault();
        
        return trivia.ToString().Trim();
    }

    private Dictionary<string, object> ExtractJsonProperties(JsonElement element)
    {
        var properties = new Dictionary<string, object>();
        
        foreach (var property in element.EnumerateObject())
        {
            properties[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => property.Value.ToString()
            };
        }
        
        return properties;
    }

    private Dictionary<string, object> ExtractXmlElements(XElement element)
    {
        var elements = new Dictionary<string, object>();
        
        foreach (var child in element.Elements())
        {
            elements[child.Name.LocalName] = child.HasElements ? 
                ExtractXmlElements(child) : 
                child.Value;
        }
        
        return elements;
    }

    #endregion
}
