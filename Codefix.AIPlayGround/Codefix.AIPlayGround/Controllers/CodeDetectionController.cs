using Microsoft.AspNetCore.Mvc;
using Codefix.AIPlayGround.Services;

namespace Codefix.AIPlayGround.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CodeDetectionController : ControllerBase
{
    private readonly ICodeDetectionService _codeDetectionService;
    private readonly ILogger<CodeDetectionController> _logger;

    public CodeDetectionController(ICodeDetectionService codeDetectionService, ILogger<CodeDetectionController> logger)
    {
        _codeDetectionService = codeDetectionService;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes a codebase to detect available tools and capabilities
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<CodeAnalysisResult>> AnalyzeCodebaseAsync([FromBody] AnalyzeCodebaseRequest request)
    {
        try
        {
            _logger.LogInformation("Analyzing codebase: {CodebasePath}", request.CodebasePath);
            
            var result = await _codeDetectionService.AnalyzeCodebaseAsync(request.CodebasePath, request.Options);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing codebase: {CodebasePath}", request.CodebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes a specific file for patterns and capabilities
    /// </summary>
    [HttpPost("analyze-file")]
    public async Task<ActionResult<FileAnalysisResult>> AnalyzeFileAsync([FromBody] AnalyzeFileRequest request)
    {
        try
        {
            _logger.LogInformation("Analyzing file: {FilePath}", request.FilePath);
            
            var result = await _codeDetectionService.AnalyzeFileAsync(request.FilePath, request.Options);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing file: {FilePath}", request.FilePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Discovers available APIs and endpoints
    /// </summary>
    [HttpGet("apis")]
    public async Task<ActionResult<List<DiscoveredApi>>> DiscoverApisAsync([FromQuery] string codebasePath)
    {
        try
        {
            _logger.LogInformation("Discovering APIs in: {CodebasePath}", codebasePath);
            
            var apis = await _codeDetectionService.DiscoverApisAsync(codebasePath);
            
            return Ok(apis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering APIs: {CodebasePath}", codebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Discovers data models and entities
    /// </summary>
    [HttpGet("models")]
    public async Task<ActionResult<List<DiscoveredModel>>> DiscoverModelsAsync([FromQuery] string codebasePath)
    {
        try
        {
            _logger.LogInformation("Discovering models in: {CodebasePath}", codebasePath);
            
            var models = await _codeDetectionService.DiscoverModelsAsync(codebasePath);
            
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering models: {CodebasePath}", codebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Discovers service classes and their methods
    /// </summary>
    [HttpGet("services")]
    public async Task<ActionResult<List<DiscoveredService>>> DiscoverServicesAsync([FromQuery] string codebasePath)
    {
        try
        {
            _logger.LogInformation("Discovering services in: {CodebasePath}", codebasePath);
            
            var services = await _codeDetectionService.DiscoverServicesAsync(codebasePath);
            
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering services: {CodebasePath}", codebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Discovers configuration patterns and settings
    /// </summary>
    [HttpGet("configurations")]
    public async Task<ActionResult<List<DiscoveredConfiguration>>> DiscoverConfigurationsAsync([FromQuery] string codebasePath)
    {
        try
        {
            _logger.LogInformation("Discovering configurations in: {CodebasePath}", codebasePath);
            
            var configurations = await _codeDetectionService.DiscoverConfigurationsAsync(codebasePath);
            
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering configurations: {CodebasePath}", codebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Discovers dependencies and package references
    /// </summary>
    [HttpGet("dependencies")]
    public async Task<ActionResult<List<DiscoveredDependency>>> DiscoverDependenciesAsync([FromQuery] string codebasePath)
    {
        try
        {
            _logger.LogInformation("Discovering dependencies in: {CodebasePath}", codebasePath);
            
            var dependencies = await _codeDetectionService.DiscoverDependenciesAsync(codebasePath);
            
            return Ok(dependencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering dependencies: {CodebasePath}", codebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Discovers test patterns and testing frameworks
    /// </summary>
    [HttpGet("tests")]
    public async Task<ActionResult<List<DiscoveredTest>>> DiscoverTestsAsync([FromQuery] string codebasePath)
    {
        try
        {
            _logger.LogInformation("Discovering tests in: {CodebasePath}", codebasePath);
            
            var tests = await _codeDetectionService.DiscoverTestsAsync(codebasePath);
            
            return Ok(tests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering tests: {CodebasePath}", codebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Generates a comprehensive codebase map
    /// </summary>
    [HttpGet("map")]
    public async Task<ActionResult<CodebaseMap>> GenerateCodebaseMapAsync([FromQuery] string codebasePath)
    {
        try
        {
            _logger.LogInformation("Generating codebase map for: {CodebasePath}", codebasePath);
            
            var map = await _codeDetectionService.GenerateCodebaseMapAsync(codebasePath);
            
            return Ok(map);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating codebase map: {CodebasePath}", codebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes code patterns for security issues
    /// </summary>
    [HttpGet("security")]
    public async Task<ActionResult<SecurityAnalysisResult>> AnalyzeSecurityAsync([FromQuery] string codebasePath)
    {
        try
        {
            _logger.LogInformation("Analyzing security for: {CodebasePath}", codebasePath);
            
            var result = await _codeDetectionService.AnalyzeSecurityAsync(codebasePath);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing security: {CodebasePath}", codebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes code patterns for performance issues
    /// </summary>
    [HttpGet("performance")]
    public async Task<ActionResult<PerformanceAnalysisResult>> AnalyzePerformanceAsync([FromQuery] string codebasePath)
    {
        try
        {
            _logger.LogInformation("Analyzing performance for: {CodebasePath}", codebasePath);
            
            var result = await _codeDetectionService.AnalyzePerformanceAsync(codebasePath);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing performance: {CodebasePath}", codebasePath);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request to analyze a codebase
/// </summary>
public class AnalyzeCodebaseRequest
{
    public string CodebasePath { get; set; } = string.Empty;
    public CodeAnalysisOptions? Options { get; set; }
}

/// <summary>
/// Request to analyze a file
/// </summary>
public class AnalyzeFileRequest
{
    public string FilePath { get; set; } = string.Empty;
    public FileAnalysisOptions? Options { get; set; }
}
