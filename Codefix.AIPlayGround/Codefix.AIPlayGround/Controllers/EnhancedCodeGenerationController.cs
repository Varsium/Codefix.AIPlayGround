using Microsoft.AspNetCore.Mvc;
using Codefix.AIPlayGround.Services;

namespace Codefix.AIPlayGround.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnhancedCodeGenerationController : ControllerBase
{
    private readonly IEnhancedCodeGenerationService _enhancedCodeGenerationService;
    private readonly ICodeDetectionService _codeDetectionService;
    private readonly ILogger<EnhancedCodeGenerationController> _logger;

    public EnhancedCodeGenerationController(
        IEnhancedCodeGenerationService enhancedCodeGenerationService,
        ICodeDetectionService codeDetectionService,
        ILogger<EnhancedCodeGenerationController> logger)
    {
        _enhancedCodeGenerationService = enhancedCodeGenerationService;
        _codeDetectionService = codeDetectionService;
        _logger = logger;
    }

    /// <summary>
    /// Generates code based on detected patterns and tools
    /// </summary>
    [HttpPost("generate-from-patterns")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateFromDetectedPatternsAsync([FromBody] GenerateFromPatternsRequest request)
    {
        try
        {
            _logger.LogInformation("Generating code from patterns for: {CodebasePath}", request.CodebasePath);
            
            var result = await _enhancedCodeGenerationService.GenerateFromDetectedPatternsAsync(
                request.CodebasePath, 
                request.Request);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating code from patterns: {CodebasePath}", request.CodebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates workflow nodes based on discovered APIs
    /// </summary>
    [HttpPost("generate-workflow-nodes")]
    public async Task<ActionResult<List<GeneratedWorkflowNode>>> GenerateWorkflowNodesFromApisAsync([FromBody] GenerateWorkflowNodesRequest request)
    {
        try
        {
            _logger.LogInformation("Generating workflow nodes from {ApiCount} APIs", request.Apis.Count);
            
            var nodes = await _enhancedCodeGenerationService.GenerateWorkflowNodesFromApisAsync(request.Apis);
            
            return Ok(nodes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating workflow nodes");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates agent code based on discovered services
    /// </summary>
    [HttpPost("generate-agent-from-service")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateAgentFromServiceAsync([FromBody] GenerateAgentFromServiceRequest request)
    {
        try
        {
            _logger.LogInformation("Generating agent from service: {ServiceName}", request.Service.Name);
            
            var result = await _enhancedCodeGenerationService.GenerateAgentFromServiceAsync(
                request.Service, 
                request.Options);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating agent from service: {ServiceName}", request.Service.Name);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates test code based on discovered patterns
    /// </summary>
    [HttpPost("generate-tests")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateTestsFromPatternsAsync([FromBody] GenerateTestsRequest request)
    {
        try
        {
            _logger.LogInformation("Generating tests for: {CodebasePath}", request.CodebasePath);
            
            var result = await _enhancedCodeGenerationService.GenerateTestsFromPatternsAsync(
                request.CodebasePath, 
                request.Options);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tests: {CodebasePath}", request.CodebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates documentation based on discovered patterns
    /// </summary>
    [HttpPost("generate-documentation")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateDocumentationFromPatternsAsync([FromBody] GenerateDocumentationRequest request)
    {
        try
        {
            _logger.LogInformation("Generating documentation for: {CodebasePath}", request.CodebasePath);
            
            var result = await _enhancedCodeGenerationService.GenerateDocumentationFromPatternsAsync(
                request.CodebasePath, 
                request.Options);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating documentation: {CodebasePath}", request.CodebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates configuration files based on discovered patterns
    /// </summary>
    [HttpPost("generate-configuration")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateConfigurationFromPatternsAsync([FromBody] GenerateConfigurationRequest request)
    {
        try
        {
            _logger.LogInformation("Generating configuration for: {CodebasePath}", request.CodebasePath);
            
            var result = await _enhancedCodeGenerationService.GenerateConfigurationFromPatternsAsync(
                request.CodebasePath, 
                request.Options);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating configuration: {CodebasePath}", request.CodebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates API client code based on discovered APIs
    /// </summary>
    [HttpPost("generate-api-client")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateApiClientAsync([FromBody] GenerateApiClientRequest request)
    {
        try
        {
            _logger.LogInformation("Generating API client for {ApiCount} APIs", request.Apis.Count);
            
            var result = await _enhancedCodeGenerationService.GenerateApiClientAsync(
                request.Apis, 
                request.Options);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating API client");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates database migration based on discovered models
    /// </summary>
    [HttpPost("generate-migration")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateMigrationFromModelsAsync([FromBody] GenerateMigrationRequest request)
    {
        try
        {
            _logger.LogInformation("Generating migration for {ModelCount} models", request.Models.Count);
            
            var result = await _enhancedCodeGenerationService.GenerateMigrationFromModelsAsync(
                request.Models, 
                request.Options);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating migration");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates Docker configuration based on discovered patterns
    /// </summary>
    [HttpPost("generate-docker")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateDockerConfigurationAsync([FromBody] GenerateDockerRequest request)
    {
        try
        {
            _logger.LogInformation("Generating Docker configuration for: {CodebasePath}", request.CodebasePath);
            
            var result = await _enhancedCodeGenerationService.GenerateDockerConfigurationAsync(
                request.CodebasePath, 
                request.Options);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Docker configuration: {CodebasePath}", request.CodebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates CI/CD pipeline based on discovered patterns
    /// </summary>
    [HttpPost("generate-cicd")]
    public async Task<ActionResult<CodeGenerationResult>> GenerateCICDPipelineAsync([FromBody] GenerateCICDRequest request)
    {
        try
        {
            _logger.LogInformation("Generating CI/CD pipeline for: {CodebasePath}", request.CodebasePath);
            
            var result = await _enhancedCodeGenerationService.GenerateCICDPipelineAsync(
                request.CodebasePath, 
                request.Options);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CI/CD pipeline: {CodebasePath}", request.CodebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes codebase and generates comprehensive code based on all discovered patterns
    /// </summary>
    [HttpPost("generate-comprehensive")]
    public async Task<ActionResult<ComprehensiveGenerationResult>> GenerateComprehensiveAsync([FromBody] ComprehensiveGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("Generating comprehensive code for: {CodebasePath}", request.CodebasePath);
            
            // First analyze the codebase
            var analysis = await _codeDetectionService.AnalyzeCodebaseAsync(request.CodebasePath);
            
            var results = new Dictionary<string, CodeGenerationResult>();
            
            // Generate different types of code
            if (request.GenerateTests)
            {
                var testRequest = new GenerateTestsRequest
                {
                    CodebasePath = request.CodebasePath,
                    Options = request.TestOptions ?? new TestGenerationOptions()
                };
                results["Tests"] = await _enhancedCodeGenerationService.GenerateTestsFromPatternsAsync(
                    testRequest.CodebasePath, testRequest.Options);
            }
            
            if (request.GenerateDocumentation)
            {
                var docRequest = new GenerateDocumentationRequest
                {
                    CodebasePath = request.CodebasePath,
                    Options = request.DocumentationOptions ?? new DocumentationOptions()
                };
                results["Documentation"] = await _enhancedCodeGenerationService.GenerateDocumentationFromPatternsAsync(
                    docRequest.CodebasePath, docRequest.Options);
            }
            
            if (request.GenerateConfiguration)
            {
                var configRequest = new GenerateConfigurationRequest
                {
                    CodebasePath = request.CodebasePath,
                    Options = request.ConfigurationOptions ?? new ConfigurationOptions()
                };
                results["Configuration"] = await _enhancedCodeGenerationService.GenerateConfigurationFromPatternsAsync(
                    configRequest.CodebasePath, configRequest.Options);
            }
            
            if (request.GenerateApiClient && analysis.Apis.Any())
            {
                var apiClientRequest = new GenerateApiClientRequest
                {
                    Apis = analysis.Apis,
                    Options = request.ApiClientOptions ?? new ApiClientOptions()
                };
                results["ApiClient"] = await _enhancedCodeGenerationService.GenerateApiClientAsync(
                    apiClientRequest.Apis, apiClientRequest.Options);
            }
            
            if (request.GenerateMigration && analysis.Models.Any())
            {
                var migrationRequest = new GenerateMigrationRequest
                {
                    Models = analysis.Models,
                    Options = request.MigrationOptions ?? new MigrationOptions()
                };
                results["Migration"] = await _enhancedCodeGenerationService.GenerateMigrationFromModelsAsync(
                    migrationRequest.Models, migrationRequest.Options);
            }
            
            if (request.GenerateDocker)
            {
                var dockerRequest = new GenerateDockerRequest
                {
                    CodebasePath = request.CodebasePath,
                    Options = request.DockerOptions ?? new DockerOptions()
                };
                results["Docker"] = await _enhancedCodeGenerationService.GenerateDockerConfigurationAsync(
                    dockerRequest.CodebasePath, dockerRequest.Options);
            }
            
            if (request.GenerateCICD)
            {
                var cicdRequest = new GenerateCICDRequest
                {
                    CodebasePath = request.CodebasePath,
                    Options = request.CICDOptions ?? new CICDOptions()
                };
                results["CICD"] = await _enhancedCodeGenerationService.GenerateCICDPipelineAsync(
                    cicdRequest.CodebasePath, cicdRequest.Options);
            }

            return Ok(new ComprehensiveGenerationResult
            {
                CodebasePath = request.CodebasePath,
                GeneratedAt = DateTime.UtcNow,
                Results = results,
                Analysis = analysis,
                SuccessCount = results.Count(r => r.Value.IsSuccess),
                TotalCount = results.Count,
                Message = $"Generated {results.Count(r => r.Value.IsSuccess)} out of {results.Count} code types successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comprehensive code: {CodebasePath}", request.CodebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request to generate code from patterns
/// </summary>
public class GenerateFromPatternsRequest
{
    public string CodebasePath { get; set; } = string.Empty;
    public CodeGenerationRequest Request { get; set; } = new();
}

/// <summary>
/// Request to generate workflow nodes
/// </summary>
public class GenerateWorkflowNodesRequest
{
    public List<DiscoveredApi> Apis { get; set; } = new();
}

/// <summary>
/// Request to generate agent from service
/// </summary>
public class GenerateAgentFromServiceRequest
{
    public DiscoveredService Service { get; set; } = new();
    public AgentGenerationOptions Options { get; set; } = new();
}

/// <summary>
/// Request to generate tests
/// </summary>
public class GenerateTestsRequest
{
    public string CodebasePath { get; set; } = string.Empty;
    public TestGenerationOptions Options { get; set; } = new();
}

/// <summary>
/// Request to generate documentation
/// </summary>
public class GenerateDocumentationRequest
{
    public string CodebasePath { get; set; } = string.Empty;
    public DocumentationOptions Options { get; set; } = new();
}

/// <summary>
/// Request to generate configuration
/// </summary>
public class GenerateConfigurationRequest
{
    public string CodebasePath { get; set; } = string.Empty;
    public ConfigurationOptions Options { get; set; } = new();
}

/// <summary>
/// Request to generate API client
/// </summary>
public class GenerateApiClientRequest
{
    public List<DiscoveredApi> Apis { get; set; } = new();
    public ApiClientOptions Options { get; set; } = new();
}

/// <summary>
/// Request to generate migration
/// </summary>
public class GenerateMigrationRequest
{
    public List<DiscoveredModel> Models { get; set; } = new();
    public MigrationOptions Options { get; set; } = new();
}

/// <summary>
/// Request to generate Docker configuration
/// </summary>
public class GenerateDockerRequest
{
    public string CodebasePath { get; set; } = string.Empty;
    public DockerOptions Options { get; set; } = new();
}

/// <summary>
/// Request to generate CI/CD pipeline
/// </summary>
public class GenerateCICDRequest
{
    public string CodebasePath { get; set; } = string.Empty;
    public CICDOptions Options { get; set; } = new();
}

/// <summary>
/// Request for comprehensive code generation
/// </summary>
public class ComprehensiveGenerationRequest
{
    public string CodebasePath { get; set; } = string.Empty;
    public bool GenerateTests { get; set; } = true;
    public bool GenerateDocumentation { get; set; } = true;
    public bool GenerateConfiguration { get; set; } = true;
    public bool GenerateApiClient { get; set; } = true;
    public bool GenerateMigration { get; set; } = true;
    public bool GenerateDocker { get; set; } = true;
    public bool GenerateCICD { get; set; } = true;
    public TestGenerationOptions? TestOptions { get; set; }
    public DocumentationOptions? DocumentationOptions { get; set; }
    public ConfigurationOptions? ConfigurationOptions { get; set; }
    public ApiClientOptions? ApiClientOptions { get; set; }
    public MigrationOptions? MigrationOptions { get; set; }
    public DockerOptions? DockerOptions { get; set; }
    public CICDOptions? CICDOptions { get; set; }
}

/// <summary>
/// Result of comprehensive code generation
/// </summary>
public class ComprehensiveGenerationResult
{
    public string CodebasePath { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, CodeGenerationResult> Results { get; set; } = new();
    public CodeAnalysisResult Analysis { get; set; } = new();
    public int SuccessCount { get; set; }
    public int TotalCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
