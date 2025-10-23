using Codefix.AIPlayGround.Models.DTOs;
using System.Text.Json;
using System.Text;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Enhanced code generation service that integrates with code detection
/// </summary>
public interface IEnhancedCodeGenerationService
{
    /// <summary>
    /// Generates code based on detected patterns and tools
    /// </summary>
    Task<CodeGenerationResult> GenerateFromDetectedPatternsAsync(string codebasePath, CodeGenerationRequest request);
    
    /// <summary>
    /// Generates workflow nodes based on discovered APIs
    /// </summary>
    Task<List<GeneratedWorkflowNode>> GenerateWorkflowNodesFromApisAsync(List<DiscoveredApi> apis);
    
    /// <summary>
    /// Generates agent code based on discovered services
    /// </summary>
    Task<CodeGenerationResult> GenerateAgentFromServiceAsync(DiscoveredService service, AgentGenerationOptions options);
    
    /// <summary>
    /// Generates test code based on discovered patterns
    /// </summary>
    Task<CodeGenerationResult> GenerateTestsFromPatternsAsync(string codebasePath, TestGenerationOptions options);
    
    /// <summary>
    /// Generates documentation based on discovered patterns
    /// </summary>
    Task<CodeGenerationResult> GenerateDocumentationFromPatternsAsync(string codebasePath, DocumentationOptions options);
    
    /// <summary>
    /// Generates configuration files based on discovered patterns
    /// </summary>
    Task<CodeGenerationResult> GenerateConfigurationFromPatternsAsync(string codebasePath, ConfigurationOptions options);
    
    /// <summary>
    /// Generates API client code based on discovered APIs
    /// </summary>
    Task<CodeGenerationResult> GenerateApiClientAsync(List<DiscoveredApi> apis, ApiClientOptions options);
    
    /// <summary>
    /// Generates database migration based on discovered models
    /// </summary>
    Task<CodeGenerationResult> GenerateMigrationFromModelsAsync(List<DiscoveredModel> models, MigrationOptions options);
    
    /// <summary>
    /// Generates Docker configuration based on discovered patterns
    /// </summary>
    Task<CodeGenerationResult> GenerateDockerConfigurationAsync(string codebasePath, DockerOptions options);
    
    /// <summary>
    /// Generates CI/CD pipeline based on discovered patterns
    /// </summary>
    Task<CodeGenerationResult> GenerateCICDPipelineAsync(string codebasePath, CICDOptions options);
}

/// <summary>
/// Enhanced code generation service implementation
/// </summary>
public class EnhancedCodeGenerationService : IEnhancedCodeGenerationService
{
    private readonly ICodeDetectionService _codeDetectionService;
    private readonly ICodeGenerationService _codeGenerationService;
    private readonly ILogger<EnhancedCodeGenerationService> _logger;

    public EnhancedCodeGenerationService(
        ICodeDetectionService codeDetectionService,
        ICodeGenerationService codeGenerationService,
        ILogger<EnhancedCodeGenerationService> logger)
    {
        _codeDetectionService = codeDetectionService;
        _codeGenerationService = codeGenerationService;
        _logger = logger;
    }

    public async Task<CodeGenerationResult> GenerateFromDetectedPatternsAsync(string codebasePath, CodeGenerationRequest request)
    {
        _logger.LogInformation("Generating code from detected patterns for: {CodebasePath}", codebasePath);

        try
        {
            // First, analyze the codebase to understand patterns
            var analysis = await _codeDetectionService.AnalyzeCodebaseAsync(codebasePath);
            
            var generatedCode = new StringBuilder();
            var warnings = new List<string>();
            var errors = new List<string>();

            // Generate code based on request type
            switch (request.Type.ToLowerInvariant())
            {
                case "workflow":
                    var workflowCode = await GenerateWorkflowCodeAsync(analysis, request);
                    generatedCode.AppendLine(workflowCode);
                    break;
                    
                case "agent":
                    var agentCode = await GenerateAgentCodeAsync(analysis, request);
                    generatedCode.AppendLine(agentCode);
                    break;
                    
                case "api":
                    var apiCode = await GenerateApiCodeAsync(analysis, request);
                    generatedCode.AppendLine(apiCode);
                    break;
                    
                case "test":
                    var testCode = await GenerateTestCodeAsync(analysis, request);
                    generatedCode.AppendLine(testCode);
                    break;
                    
                case "documentation":
                    var docCode = await GenerateDocumentationCodeAsync(analysis, request);
                    generatedCode.AppendLine(docCode);
                    break;
                    
                default:
                    errors.Add($"Unknown generation type: {request.Type}");
                    break;
            }

            return new CodeGenerationResult
            {
                IsSuccess = errors.Count == 0,
                GeneratedCode = generatedCode.ToString(),
                Message = errors.Count == 0 ? "Code generated successfully" : "Code generation completed with errors",
                Warnings = warnings,
                Errors = errors,
                Metadata = new Dictionary<string, object>
                {
                    ["CodebasePath"] = codebasePath,
                    ["GenerationType"] = request.Type,
                    ["DiscoveredApis"] = analysis.Apis.Count,
                    ["DiscoveredModels"] = analysis.Models.Count,
                    ["DiscoveredServices"] = analysis.Services.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating code from detected patterns: {CodebasePath}", codebasePath);
            return new CodeGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = string.Empty,
                Message = $"Error generating code: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<List<GeneratedWorkflowNode>> GenerateWorkflowNodesFromApisAsync(List<DiscoveredApi> apis)
    {
        var nodes = new List<GeneratedWorkflowNode>();

        foreach (var api in apis)
        {
            var node = new GeneratedWorkflowNode
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"{api.Controller}_{api.Action}",
                Type = "ApiCall",
                Description = api.Documentation,
                Configuration = new Dictionary<string, object>
                {
                    ["HttpMethod"] = api.Method,
                    ["Path"] = api.Path,
                    ["Controller"] = api.Controller,
                    ["Action"] = api.Action,
                    ["ReturnType"] = api.ReturnType,
                    ["Parameters"] = api.Parameters.Select(p => new
                    {
                        p.Name,
                        p.Type,
                        p.IsOptional,
                        p.DefaultValue
                    }).ToList()
                },
                InputSchema = GenerateInputSchema(api.Parameters),
                OutputSchema = GenerateOutputSchema(api.ReturnType),
                GeneratedAt = DateTime.UtcNow
            };

            nodes.Add(node);
        }

        return nodes;
    }

    public async Task<CodeGenerationResult> GenerateAgentFromServiceAsync(DiscoveredService service, AgentGenerationOptions options)
    {
        _logger.LogInformation("Generating agent from service: {ServiceName}", service.Name);

        try
        {
            var specification = new AgentCodeSpecification
            {
                AgentName = $"{service.Name}Agent",
                AgentType = "ServiceAgent",
                Description = service.Documentation,
                BaseClass = "BaseServiceAgent",
                Methods = service.Methods.Select(m => new AgentMethod
                {
                    Name = m.Name,
                    ReturnType = m.ReturnType,
                    Parameters = m.Parameters.Select(p => new MethodParameter
                    {
                        Name = p.Name,
                        Type = p.Type,
                        IsOptional = p.IsOptional,
                        DefaultValue = p.DefaultValue
                    }).ToList(),
                    Body = GenerateMethodBody(m, service),
                    AccessModifier = m.AccessModifier,
                    IsAsync = m.IsAsync,
                    Attributes = m.Attributes
                }).ToList(),
                Properties = service.Properties.Select(p => new AgentProperty
                {
                    Name = p.Name,
                    Type = p.Type,
                    AccessModifier = p.AccessModifier,
                    HasGetter = p.HasGetter,
                    HasSetter = p.HasSetter,
                    Attributes = p.Attributes
                }).ToList(),
                Dependencies = service.Dependencies,
                Configuration = new Dictionary<string, object>
                {
                    ["ServiceName"] = service.Name,
                    ["Interface"] = service.Interface,
                    ["Namespace"] = service.Namespace
                }
            };

            return await _codeGenerationService.GenerateAgentCodeAsync(specification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating agent from service: {ServiceName}", service.Name);
            return new CodeGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = string.Empty,
                Message = $"Error generating agent: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<CodeGenerationResult> GenerateTestsFromPatternsAsync(string codebasePath, TestGenerationOptions options)
    {
        _logger.LogInformation("Generating tests from patterns for: {CodebasePath}", codebasePath);

        try
        {
            var analysis = await _codeDetectionService.AnalyzeCodebaseAsync(codebasePath);
            var testCode = new StringBuilder();

            // Generate tests for APIs
            foreach (var api in analysis.Apis)
            {
                testCode.AppendLine(GenerateApiTest(api, options));
            }

            // Generate tests for Services
            foreach (var service in analysis.Services)
            {
                testCode.AppendLine(GenerateServiceTest(service, options));
            }

            // Generate tests for Models
            foreach (var model in analysis.Models)
            {
                testCode.AppendLine(GenerateModelTest(model, options));
            }

            return new CodeGenerationResult
            {
                IsSuccess = true,
                GeneratedCode = testCode.ToString(),
                Message = "Tests generated successfully",
                Metadata = new Dictionary<string, object>
                {
                    ["ApiTests"] = analysis.Apis.Count,
                    ["ServiceTests"] = analysis.Services.Count,
                    ["ModelTests"] = analysis.Models.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tests: {CodebasePath}", codebasePath);
            return new CodeGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = string.Empty,
                Message = $"Error generating tests: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<CodeGenerationResult> GenerateDocumentationFromPatternsAsync(string codebasePath, DocumentationOptions options)
    {
        _logger.LogInformation("Generating documentation from patterns for: {CodebasePath}", codebasePath);

        try
        {
            var analysis = await _codeDetectionService.AnalyzeCodebaseAsync(codebasePath);
            var documentation = new StringBuilder();

            // Generate API documentation
            documentation.AppendLine("# API Documentation");
            documentation.AppendLine();
            
            foreach (var api in analysis.Apis)
            {
                documentation.AppendLine($"## {api.Controller}.{api.Action}");
                documentation.AppendLine();
                documentation.AppendLine($"**Method:** {api.Method}");
                documentation.AppendLine($"**Path:** {api.Path}");
                documentation.AppendLine($"**Return Type:** {api.ReturnType}");
                documentation.AppendLine();
                
                if (!string.IsNullOrEmpty(api.Documentation))
                {
                    documentation.AppendLine(api.Documentation);
                    documentation.AppendLine();
                }
                
                if (api.Parameters.Any())
                {
                    documentation.AppendLine("### Parameters");
                    documentation.AppendLine();
                    foreach (var param in api.Parameters)
                    {
                        documentation.AppendLine($"- **{param.Name}** ({param.Type}): {param.Documentation}");
                    }
                    documentation.AppendLine();
                }
            }

            // Generate Service documentation
            documentation.AppendLine("# Service Documentation");
            documentation.AppendLine();
            
            foreach (var service in analysis.Services)
            {
                documentation.AppendLine($"## {service.Name}");
                documentation.AppendLine();
                documentation.AppendLine(service.Documentation);
                documentation.AppendLine();
                
                if (service.Methods.Any())
                {
                    documentation.AppendLine("### Methods");
                    documentation.AppendLine();
                    foreach (var method in service.Methods)
                    {
                        documentation.AppendLine($"- **{method.Name}**: {method.Documentation}");
                    }
                    documentation.AppendLine();
                }
            }

            return new CodeGenerationResult
            {
                IsSuccess = true,
                GeneratedCode = documentation.ToString(),
                Message = "Documentation generated successfully",
                Metadata = new Dictionary<string, object>
                {
                    ["ApiCount"] = analysis.Apis.Count,
                    ["ServiceCount"] = analysis.Services.Count,
                    ["ModelCount"] = analysis.Models.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating documentation: {CodebasePath}", codebasePath);
            return new CodeGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = string.Empty,
                Message = $"Error generating documentation: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<CodeGenerationResult> GenerateConfigurationFromPatternsAsync(string codebasePath, ConfigurationOptions options)
    {
        _logger.LogInformation("Generating configuration from patterns for: {CodebasePath}", codebasePath);

        try
        {
            var analysis = await _codeDetectionService.AnalyzeCodebaseAsync(codebasePath);
            var configCode = new StringBuilder();

            // Generate appsettings.json
            configCode.AppendLine("{");
            configCode.AppendLine("  \"Logging\": {");
            configCode.AppendLine("    \"LogLevel\": {");
            configCode.AppendLine("      \"Default\": \"Information\",");
            configCode.AppendLine("      \"Microsoft.AspNetCore\": \"Warning\"");
            configCode.AppendLine("    }");
            configCode.AppendLine("  },");
            configCode.AppendLine("  \"AllowedHosts\": \"*\",");
            configCode.AppendLine("  \"ConnectionStrings\": {");
            configCode.AppendLine("    \"DefaultConnection\": \"Server=(localdb)\\mssqllocaldb;Database=CodefixAIPlayGround;Trusted_Connection=true;MultipleActiveResultSets=true\"");
            configCode.AppendLine("  },");
            configCode.AppendLine("  \"AgentExecution\": {");
            configCode.AppendLine("    \"AllowFileSystemAccess\": false,");
            configCode.AppendLine("    \"AllowNetworkAccess\": false,");
            configCode.AppendLine("    \"AllowReflection\": false,");
            configCode.AppendLine("    \"AllowDynamicCode\": false,");
            configCode.AppendLine("    \"MaxExecutionTimeSeconds\": 300,");
            configCode.AppendLine("    \"MaxMemoryMB\": 100");
            configCode.AppendLine("  }");

            // Add discovered configurations
            if (analysis.Configurations.Any())
            {
                configCode.AppendLine(",");
                configCode.AppendLine("  \"DiscoveredConfigurations\": {");
                var configItems = analysis.Configurations.Take(10); // Limit to first 10
                var configLines = configItems.Select(c => $"    \"{c.Name}\": \"{c.Value}\"");
                configCode.AppendLine(string.Join(",\n", configLines));
                configCode.AppendLine("  }");
            }

            configCode.AppendLine("}");

            return new CodeGenerationResult
            {
                IsSuccess = true,
                GeneratedCode = configCode.ToString(),
                Message = "Configuration generated successfully",
                Metadata = new Dictionary<string, object>
                {
                    ["ConfigurationCount"] = analysis.Configurations.Count,
                    ["DependencyCount"] = analysis.Dependencies.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating configuration: {CodebasePath}", codebasePath);
            return new CodeGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = string.Empty,
                Message = $"Error generating configuration: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<CodeGenerationResult> GenerateApiClientAsync(List<DiscoveredApi> apis, ApiClientOptions options)
    {
        _logger.LogInformation("Generating API client for {ApiCount} APIs", apis.Count);

        try
        {
            var clientCode = new StringBuilder();
            
            clientCode.AppendLine("using System.Text.Json;");
            clientCode.AppendLine("using System.Net.Http;");
            clientCode.AppendLine();
            clientCode.AppendLine("namespace GeneratedApiClient;");
            clientCode.AppendLine();
            clientCode.AppendLine("public class ApiClient");
            clientCode.AppendLine("{");
            clientCode.AppendLine("    private readonly HttpClient _httpClient;");
            clientCode.AppendLine("    private readonly JsonSerializerOptions _jsonOptions;");
            clientCode.AppendLine();
            clientCode.AppendLine("    public ApiClient(HttpClient httpClient)");
            clientCode.AppendLine("    {");
            clientCode.AppendLine("        _httpClient = httpClient;");
            clientCode.AppendLine("        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };");
            clientCode.AppendLine("    }");
            clientCode.AppendLine();

            // Generate methods for each API
            foreach (var api in apis)
            {
                clientCode.AppendLine(GenerateApiClientMethod(api));
            }

            clientCode.AppendLine("}");

            return new CodeGenerationResult
            {
                IsSuccess = true,
                GeneratedCode = clientCode.ToString(),
                Message = "API client generated successfully",
                Metadata = new Dictionary<string, object>
                {
                    ["ApiCount"] = apis.Count,
                    ["ClientType"] = options.ClientType
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating API client");
            return new CodeGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = string.Empty,
                Message = $"Error generating API client: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<CodeGenerationResult> GenerateMigrationFromModelsAsync(List<DiscoveredModel> models, MigrationOptions options)
    {
        _logger.LogInformation("Generating migration for {ModelCount} models", models.Count);

        try
        {
            var migrationCode = new StringBuilder();
            
            migrationCode.AppendLine("using Microsoft.EntityFrameworkCore.Migrations;");
            migrationCode.AppendLine();
            migrationCode.AppendLine("namespace GeneratedMigrations;");
            migrationCode.AppendLine();
            migrationCode.AppendLine($"public partial class {options.MigrationName} : Migration");
            migrationCode.AppendLine("{");
            migrationCode.AppendLine("    protected override void Up(MigrationBuilder migrationBuilder)");
            migrationCode.AppendLine("    {");

            foreach (var model in models)
            {
                migrationCode.AppendLine($"        migrationBuilder.CreateTable(");
                migrationCode.AppendLine($"            name: \"{model.Name}\",");
                migrationCode.AppendLine($"            columns: table => new");
                migrationCode.AppendLine($"            {{");

                foreach (var property in model.Properties)
                {
                    var columnType = GetColumnType(property.Type);
                    migrationCode.AppendLine($"                {property.Name} = table.Column<{columnType}>(nullable: {property.IsOptional.ToString().ToLowerInvariant()}),");
                }

                migrationCode.AppendLine($"            }},");
                migrationCode.AppendLine($"            constraints: table =>");
                migrationCode.AppendLine($"            {{");
                migrationCode.AppendLine($"                table.PrimaryKey(\"PK_{model.Name}\", x => x.Id);");
                migrationCode.AppendLine($"            }});");
                migrationCode.AppendLine();
            }

            migrationCode.AppendLine("    }");
            migrationCode.AppendLine();
            migrationCode.AppendLine("    protected override void Down(MigrationBuilder migrationBuilder)");
            migrationCode.AppendLine("    {");

            foreach (var model in models.AsEnumerable().Reverse())
            {
                migrationCode.AppendLine($"        migrationBuilder.DropTable(name: \"{model.Name}\");");
            }

            migrationCode.AppendLine("    }");
            migrationCode.AppendLine("}");

            return new CodeGenerationResult
            {
                IsSuccess = true,
                GeneratedCode = migrationCode.ToString(),
                Message = "Migration generated successfully",
                Metadata = new Dictionary<string, object>
                {
                    ["ModelCount"] = models.Count,
                    ["MigrationName"] = options.MigrationName
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating migration");
            return new CodeGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = string.Empty,
                Message = $"Error generating migration: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<CodeGenerationResult> GenerateDockerConfigurationAsync(string codebasePath, DockerOptions options)
    {
        _logger.LogInformation("Generating Docker configuration for: {CodebasePath}", codebasePath);

        try
        {
            var analysis = await _codeDetectionService.AnalyzeCodebaseAsync(codebasePath);
            var dockerfile = new StringBuilder();

            dockerfile.AppendLine("FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base");
            dockerfile.AppendLine("WORKDIR /app");
            dockerfile.AppendLine("EXPOSE 80");
            dockerfile.AppendLine("EXPOSE 443");
            dockerfile.AppendLine();
            dockerfile.AppendLine("FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build");
            dockerfile.AppendLine("WORKDIR /src");
            dockerfile.AppendLine("COPY [\"Codefix.AIPlayGround.csproj\", \"Codefix.AIPlayGround/\"]");
            dockerfile.AppendLine("RUN dotnet restore \"Codefix.AIPlayGround/Codefix.AIPlayGround.csproj\"");
            dockerfile.AppendLine("COPY . .");
            dockerfile.AppendLine("WORKDIR \"/src/Codefix.AIPlayGround\"");
            dockerfile.AppendLine("RUN dotnet build \"Codefix.AIPlayGround.csproj\" -c Release -o /app/build");
            dockerfile.AppendLine();
            dockerfile.AppendLine("FROM build AS publish");
            dockerfile.AppendLine("RUN dotnet publish \"Codefix.AIPlayGround.csproj\" -c Release -o /app/publish /p:UseAppHost=false");
            dockerfile.AppendLine();
            dockerfile.AppendLine("FROM base AS final");
            dockerfile.AppendLine("WORKDIR /app");
            dockerfile.AppendLine("COPY --from=publish /app/publish .");
            dockerfile.AppendLine("ENTRYPOINT [\"dotnet\", \"Codefix.AIPlayGround.dll\"]");

            return new CodeGenerationResult
            {
                IsSuccess = true,
                GeneratedCode = dockerfile.ToString(),
                Message = "Docker configuration generated successfully",
                Metadata = new Dictionary<string, object>
                {
                    ["BaseImage"] = "mcr.microsoft.com/dotnet/aspnet:8.0",
                    ["SdkImage"] = "mcr.microsoft.com/dotnet/sdk:8.0",
                    ["ExposedPorts"] = new[] { 80, 443 }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Docker configuration: {CodebasePath}", codebasePath);
            return new CodeGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = string.Empty,
                Message = $"Error generating Docker configuration: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<CodeGenerationResult> GenerateCICDPipelineAsync(string codebasePath, CICDOptions options)
    {
        _logger.LogInformation("Generating CI/CD pipeline for: {CodebasePath}", codebasePath);

        try
        {
            var pipelineCode = new StringBuilder();

            pipelineCode.AppendLine("name: CI/CD Pipeline");
            pipelineCode.AppendLine();
            pipelineCode.AppendLine("on:");
            pipelineCode.AppendLine("  push:");
            pipelineCode.AppendLine("    branches: [ main, develop ]");
            pipelineCode.AppendLine("  pull_request:");
            pipelineCode.AppendLine("    branches: [ main ]");
            pipelineCode.AppendLine();
            pipelineCode.AppendLine("jobs:");
            pipelineCode.AppendLine("  build:");
            pipelineCode.AppendLine("    runs-on: ubuntu-latest");
            pipelineCode.AppendLine("    steps:");
            pipelineCode.AppendLine("    - uses: actions/checkout@v3");
            pipelineCode.AppendLine("    - name: Setup .NET");
            pipelineCode.AppendLine("      uses: actions/setup-dotnet@v3");
            pipelineCode.AppendLine("      with:");
            pipelineCode.AppendLine("        dotnet-version: '8.0.x'");
            pipelineCode.AppendLine("    - name: Restore dependencies");
            pipelineCode.AppendLine("      run: dotnet restore");
            pipelineCode.AppendLine("    - name: Build");
            pipelineCode.AppendLine("      run: dotnet build --no-restore");
            pipelineCode.AppendLine("    - name: Test");
            pipelineCode.AppendLine("      run: dotnet test --no-build --verbosity normal");
            pipelineCode.AppendLine("    - name: Publish");
            pipelineCode.AppendLine("      run: dotnet publish --no-build -c Release -o ./publish");

            return new CodeGenerationResult
            {
                IsSuccess = true,
                GeneratedCode = pipelineCode.ToString(),
                Message = "CI/CD pipeline generated successfully",
                Metadata = new Dictionary<string, object>
                {
                    ["Platform"] = "GitHub Actions",
                    ["DotNetVersion"] = "8.0.x",
                    ["RunsOn"] = "ubuntu-latest"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CI/CD pipeline: {CodebasePath}", codebasePath);
            return new CodeGenerationResult
            {
                IsSuccess = false,
                GeneratedCode = string.Empty,
                Message = $"Error generating CI/CD pipeline: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    #region Private Helper Methods

    private async Task<string> GenerateWorkflowCodeAsync(CodeAnalysisResult analysis, CodeGenerationRequest request)
    {
        var workflowCode = new StringBuilder();
        
        workflowCode.AppendLine("using Codefix.AIPlayGround.Models;");
        workflowCode.AppendLine("using Codefix.AIPlayGround.Services;");
        workflowCode.AppendLine();
        workflowCode.AppendLine("namespace GeneratedWorkflows;");
        workflowCode.AppendLine();
        workflowCode.AppendLine("public class GeneratedWorkflow");
        workflowCode.AppendLine("{");
        workflowCode.AppendLine("    public static WorkflowDefinition CreateWorkflow()");
        workflowCode.AppendLine("    {");
        workflowCode.AppendLine("        return new WorkflowDefinition");
        workflowCode.AppendLine("        {");
        workflowCode.AppendLine("            Id = Guid.NewGuid().ToString(),");
        workflowCode.AppendLine("            Name = \"Generated Workflow\",");
        workflowCode.AppendLine("            Description = \"Auto-generated workflow from detected patterns\",");
        workflowCode.AppendLine("            CreatedAt = DateTime.UtcNow,");
        workflowCode.AppendLine("            UpdatedAt = DateTime.UtcNow,");
        workflowCode.AppendLine("            Status = WorkflowStatus.Draft");
        workflowCode.AppendLine("        };");
        workflowCode.AppendLine("    }");
        workflowCode.AppendLine("}");

        return workflowCode.ToString();
    }

    private async Task<string> GenerateAgentCodeAsync(CodeAnalysisResult analysis, CodeGenerationRequest request)
    {
        var agentCode = new StringBuilder();
        
        agentCode.AppendLine("using Codefix.AIPlayGround.Models;");
        agentCode.AppendLine("using Codefix.AIPlayGround.Services;");
        agentCode.AppendLine();
        agentCode.AppendLine("namespace GeneratedAgents;");
        agentCode.AppendLine();
        agentCode.AppendLine("public class GeneratedAgent");
        agentCode.AppendLine("{");
        agentCode.AppendLine("    public static AgentDefinition CreateAgent()");
        agentCode.AppendLine("    {");
        agentCode.AppendLine("        return new AgentDefinition");
        agentCode.AppendLine("        {");
        agentCode.AppendLine("            Id = Guid.NewGuid().ToString(),");
        agentCode.AppendLine("            Name = \"Generated Agent\",");
        agentCode.AppendLine("            Type = AgentType.LLMAgent,");
        agentCode.AppendLine("            Description = \"Auto-generated agent from detected patterns\",");
        agentCode.AppendLine("            CreatedAt = DateTime.UtcNow,");
        agentCode.AppendLine("            UpdatedAt = DateTime.UtcNow");
        agentCode.AppendLine("        };");
        agentCode.AppendLine("    }");
        agentCode.AppendLine("}");

        return agentCode.ToString();
    }

    private async Task<string> GenerateApiCodeAsync(CodeAnalysisResult analysis, CodeGenerationRequest request)
    {
        var apiCode = new StringBuilder();
        
        apiCode.AppendLine("using Microsoft.AspNetCore.Mvc;");
        apiCode.AppendLine();
        apiCode.AppendLine("namespace GeneratedApi;");
        apiCode.AppendLine();
        apiCode.AppendLine("[ApiController]");
        apiCode.AppendLine("[Route(\"api/[controller]\")]");
        apiCode.AppendLine("public class GeneratedController : ControllerBase");
        apiCode.AppendLine("{");
        apiCode.AppendLine("    [HttpGet]");
        apiCode.AppendLine("    public IActionResult Get()");
        apiCode.AppendLine("    {");
        apiCode.AppendLine("        return Ok(\"Generated API endpoint\");");
        apiCode.AppendLine("    }");
        apiCode.AppendLine("}");

        return apiCode.ToString();
    }

    private async Task<string> GenerateTestCodeAsync(CodeAnalysisResult analysis, CodeGenerationRequest request)
    {
        var testCode = new StringBuilder();
        
        testCode.AppendLine("using Microsoft.VisualStudio.TestTools.UnitTesting;");
        testCode.AppendLine();
        testCode.AppendLine("namespace GeneratedTests;");
        testCode.AppendLine();
        testCode.AppendLine("[TestClass]");
        testCode.AppendLine("public class GeneratedTests");
        testCode.AppendLine("{");
        testCode.AppendLine("    [TestMethod]");
        testCode.AppendLine("    public void TestMethod1()");
        testCode.AppendLine("    {");
        testCode.AppendLine("        // Arrange");
        testCode.AppendLine("        // Act");
        testCode.AppendLine("        // Assert");
        testCode.AppendLine("        Assert.IsTrue(true);");
        testCode.AppendLine("    }");
        testCode.AppendLine("}");

        return testCode.ToString();
    }

    private async Task<string> GenerateDocumentationCodeAsync(CodeAnalysisResult analysis, CodeGenerationRequest request)
    {
        var docCode = new StringBuilder();
        
        docCode.AppendLine("# Generated Documentation");
        docCode.AppendLine();
        docCode.AppendLine("This documentation was auto-generated from detected patterns.");
        docCode.AppendLine();
        docCode.AppendLine("## APIs");
        docCode.AppendLine();
        foreach (var api in analysis.Apis.Take(5))
        {
            docCode.AppendLine($"- {api.Controller}.{api.Action} ({api.Method})");
        }
        docCode.AppendLine();
        docCode.AppendLine("## Services");
        docCode.AppendLine();
        foreach (var service in analysis.Services.Take(5))
        {
            docCode.AppendLine($"- {service.Name}");
        }

        return docCode.ToString();
    }

    private string GenerateMethodBody(DiscoveredMethod method, DiscoveredService service)
    {
        var body = new StringBuilder();
        body.AppendLine("        {");
        body.AppendLine("            // TODO: Implement method logic");
        body.AppendLine("            throw new NotImplementedException();");
        body.AppendLine("        }");
        return body.ToString();
    }

    private string GenerateInputSchema(List<DiscoveredParameter> parameters)
    {
        var schema = new Dictionary<string, object>();
        foreach (var param in parameters)
        {
            schema[param.Name] = new
            {
                type = param.Type,
                required = !param.IsOptional,
                defaultValue = param.DefaultValue
            };
        }
        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }

    private string GenerateOutputSchema(string returnType)
    {
        return JsonSerializer.Serialize(new
        {
            type = returnType,
            description = "API response"
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    private string GenerateApiTest(DiscoveredApi api, TestGenerationOptions options)
    {
        var testCode = new StringBuilder();
        
        testCode.AppendLine($"[Test]");
        testCode.AppendLine($"public void Test_{api.Controller}_{api.Action}()");
        testCode.AppendLine("{");
        testCode.AppendLine("    // Arrange");
        testCode.AppendLine("    // Act");
        testCode.AppendLine("    // Assert");
        testCode.AppendLine("    Assert.IsTrue(true);");
        testCode.AppendLine("}");

        return testCode.ToString();
    }

    private string GenerateServiceTest(DiscoveredService service, TestGenerationOptions options)
    {
        var testCode = new StringBuilder();
        
        testCode.AppendLine($"[Test]");
        testCode.AppendLine($"public void Test_{service.Name}()");
        testCode.AppendLine("{");
        testCode.AppendLine("    // Arrange");
        testCode.AppendLine("    // Act");
        testCode.AppendLine("    // Assert");
        testCode.AppendLine("    Assert.IsTrue(true);");
        testCode.AppendLine("}");

        return testCode.ToString();
    }

    private string GenerateModelTest(DiscoveredModel model, TestGenerationOptions options)
    {
        var testCode = new StringBuilder();
        
        testCode.AppendLine($"[Test]");
        testCode.AppendLine($"public void Test_{model.Name}()");
        testCode.AppendLine("{");
        testCode.AppendLine("    // Arrange");
        testCode.AppendLine("    // Act");
        testCode.AppendLine("    // Assert");
        testCode.AppendLine("    Assert.IsTrue(true);");
        testCode.AppendLine("}");

        return testCode.ToString();
    }

    private string GenerateApiClientMethod(DiscoveredApi api)
    {
        var methodCode = new StringBuilder();
        
        methodCode.AppendLine($"    public async Task<{api.ReturnType}> {api.Action}Async()");
        methodCode.AppendLine("    {");
        methodCode.AppendLine($"        var response = await _httpClient.{api.Method}Async(\"{api.Path}\");");
        methodCode.AppendLine("        response.EnsureSuccessStatusCode();");
        methodCode.AppendLine($"        var content = await response.Content.ReadAsStringAsync();");
        methodCode.AppendLine($"        return JsonSerializer.Deserialize<{api.ReturnType}>(content, _jsonOptions);");
        methodCode.AppendLine("    }");

        return methodCode.ToString();
    }

    private string GetColumnType(string propertyType)
    {
        return propertyType.ToLowerInvariant() switch
        {
            "string" => "string",
            "int" => "int",
            "long" => "long",
            "double" => "double",
            "decimal" => "decimal",
            "bool" => "bool",
            "datetime" => "DateTime",
            "guid" => "Guid",
            _ => "string"
        };
    }

    #endregion
}

/// <summary>
/// Request for code generation
/// </summary>
public class CodeGenerationRequest
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public CodeGenerationOptions? Options { get; set; }
}

/// <summary>
/// Options for code generation
/// </summary>
public class CodeGenerationOptions
{
    public string OutputDirectory { get; set; } = string.Empty;
    public bool IncludeComments { get; set; } = true;
    public bool IncludeDocumentation { get; set; } = true;
    public string Namespace { get; set; } = "Generated";
    public List<string> ExcludePatterns { get; set; } = new();
}

/// <summary>
/// Generated workflow node
/// </summary>
public class GeneratedWorkflowNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public string InputSchema { get; set; } = string.Empty;
    public string OutputSchema { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Options for agent generation
/// </summary>
public class AgentGenerationOptions
{
    public string BaseClass { get; set; } = "BaseAgent";
    public List<string> Interfaces { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
}

/// <summary>
/// Options for test generation
/// </summary>
public class TestGenerationOptions
{
    public string Framework { get; set; } = "MSTest";
    public bool IncludeIntegrationTests { get; set; } = true;
    public bool IncludeUnitTests { get; set; } = true;
    public string TestNamespace { get; set; } = "GeneratedTests";
}

/// <summary>
/// Options for documentation generation
/// </summary>
public class DocumentationOptions
{
    public string Format { get; set; } = "Markdown";
    public bool IncludeApiDocs { get; set; } = true;
    public bool IncludeServiceDocs { get; set; } = true;
    public bool IncludeModelDocs { get; set; } = true;
    public string OutputFile { get; set; } = "README.md";
}

/// <summary>
/// Options for configuration generation
/// </summary>
public class ConfigurationOptions
{
    public string Format { get; set; } = "JSON";
    public bool IncludeEnvironmentSpecific { get; set; } = true;
    public string OutputFile { get; set; } = "appsettings.Generated.json";
}

/// <summary>
/// Options for API client generation
/// </summary>
public class ApiClientOptions
{
    public string ClientType { get; set; } = "HttpClient";
    public string Namespace { get; set; } = "GeneratedApiClient";
    public bool IncludeAsyncMethods { get; set; } = true;
    public bool IncludeSyncMethods { get; set; } = false;
}

/// <summary>
/// Options for migration generation
/// </summary>
public class MigrationOptions
{
    public string MigrationName { get; set; } = "GeneratedMigration";
    public string Namespace { get; set; } = "GeneratedMigrations";
    public bool IncludeDataSeeding { get; set; } = false;
}

/// <summary>
/// Options for Docker configuration
/// </summary>
public class DockerOptions
{
    public string BaseImage { get; set; } = "mcr.microsoft.com/dotnet/aspnet:8.0";
    public string SdkImage { get; set; } = "mcr.microsoft.com/dotnet/sdk:8.0";
    public int[] ExposedPorts { get; set; } = { 80, 443 };
    public bool MultiStage { get; set; } = true;
}

/// <summary>
/// Options for CI/CD pipeline generation
/// </summary>
public class CICDOptions
{
    public string Platform { get; set; } = "GitHub Actions";
    public string DotNetVersion { get; set; } = "8.0.x";
    public string RunsOn { get; set; } = "ubuntu-latest";
    public bool IncludeTests { get; set; } = true;
    public bool IncludeSecurityScan { get; set; } = true;
}
