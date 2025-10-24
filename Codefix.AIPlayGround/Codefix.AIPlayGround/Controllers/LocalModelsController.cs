using Microsoft.AspNetCore.Mvc;
using Codefix.AIPlayGround.Services;
using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Controllers;

/// <summary>
/// Local Models API Controller
/// Provides endpoints for discovering, testing, and managing local AI models
/// Supports Ollama, Docker Model Runner, and other local model providers
/// Integrated with Microsoft Agent Framework
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LocalModelsController : ControllerBase
{
    private readonly ILogger<LocalModelsController> _logger;
    private readonly IDockerModelRunnerService _dockerModelRunner;
    private readonly IOllamaIntegrationService _ollamaIntegration;
    private readonly IUnifiedAIProviderService _unifiedProvider;

    public LocalModelsController(
        ILogger<LocalModelsController> _logger,
        IDockerModelRunnerService dockerModelRunner,
        IOllamaIntegrationService ollamaIntegration,
        IUnifiedAIProviderService unifiedProvider)
    {
        this._logger = _logger;
        _dockerModelRunner = dockerModelRunner;
        _ollamaIntegration = ollamaIntegration;
        _unifiedProvider = unifiedProvider;
    }

    /// <summary>
    /// Discover all available local models across all providers
    /// GET /api/localmodels/discover
    /// </summary>
    [HttpGet("discover")]
    public async Task<ActionResult<LocalModelDiscoveryResponse>> DiscoverAllModels()
    {
        try
        {
            _logger.LogInformation("Discovering all local models");

            var response = new LocalModelDiscoveryResponse
            {
                DiscoveryTime = DateTime.UtcNow
            };

            // Discover Ollama models
            try
            {
                var ollamaModels = await _ollamaIntegration.DiscoverModelsAsync();
                response.OllamaModels = ollamaModels;
                response.Providers.Add("Ollama");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to discover Ollama models");
                response.Errors.Add($"Ollama: {ex.Message}");
            }

            // Discover Docker Model Runner models
            try
            {
                var dockerModels = await _dockerModelRunner.DiscoverModelsAsync();
                response.DockerModels = dockerModels;
                response.Providers.Add("DockerModelRunner");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to discover Docker Model Runner models");
                response.Errors.Add($"Docker Model Runner: {ex.Message}");
            }

            response.TotalModels = response.OllamaModels.Count + response.DockerModels.Count;

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering local models");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Discover Ollama models
    /// GET /api/localmodels/ollama/discover
    /// </summary>
    [HttpGet("ollama/discover")]
    public async Task<ActionResult<List<OllamaModelInfo>>> DiscoverOllamaModels([FromQuery] string? endpoint = null)
    {
        try
        {
            _logger.LogInformation("Discovering Ollama models at {Endpoint}", endpoint ?? "default");
            var models = endpoint != null 
                ? await _ollamaIntegration.DiscoverModelsAsync(endpoint)
                : await _ollamaIntegration.DiscoverModelsAsync();

            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering Ollama models");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Discover Docker Model Runner models
    /// GET /api/localmodels/docker/discover
    /// </summary>
    [HttpGet("docker/discover")]
    public async Task<ActionResult<List<DockerModelInfo>>> DiscoverDockerModels([FromQuery] string? endpoint = null)
    {
        try
        {
            _logger.LogInformation("Discovering Docker Model Runner models at {Endpoint}", endpoint ?? "default");
            var models = endpoint != null
                ? await _dockerModelRunner.DiscoverModelsAsync(endpoint)
                : await _dockerModelRunner.DiscoverModelsAsync();

            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering Docker Model Runner models");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test Ollama connection
    /// GET /api/localmodels/ollama/test
    /// </summary>
    [HttpGet("ollama/test")]
    public async Task<ActionResult<OllamaTestResult>> TestOllama([FromQuery] string? endpoint = null)
    {
        try
        {
            _logger.LogInformation("Testing Ollama connection");
            var result = endpoint != null
                ? await _ollamaIntegration.TestConnectionAsync(endpoint)
                : await _ollamaIntegration.TestConnectionAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Ollama connection");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test Docker Model Runner connection
    /// GET /api/localmodels/docker/test
    /// </summary>
    [HttpGet("docker/test")]
    public async Task<ActionResult<DockerModelTestResult>> TestDocker([FromQuery] string? endpoint = null)
    {
        try
        {
            _logger.LogInformation("Testing Docker Model Runner connection");
            var result = endpoint != null
                ? await _dockerModelRunner.TestConnectionAsync(endpoint)
                : await _dockerModelRunner.TestConnectionAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Docker Model Runner connection");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pull an Ollama model
    /// POST /api/localmodels/ollama/pull
    /// </summary>
    [HttpPost("ollama/pull")]
    public async Task<ActionResult<OllamaPullResult>> PullOllamaModel([FromBody] PullModelRequest request)
    {
        try
        {
            _logger.LogInformation("Pulling Ollama model {ModelName}", request.ModelName);
            var result = request.Endpoint != null
                ? await _ollamaIntegration.PullModelAsync(request.ModelName, request.Endpoint)
                : await _ollamaIntegration.PullModelAsync(request.ModelName);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling Ollama model {ModelName}", request.ModelName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Pull a Docker model
    /// POST /api/localmodels/docker/pull
    /// </summary>
    [HttpPost("docker/pull")]
    public async Task<ActionResult<DockerModelPullResult>> PullDockerModel([FromBody] PullModelRequest request)
    {
        try
        {
            _logger.LogInformation("Pulling Docker model {ModelName}", request.ModelName);
            var result = request.Endpoint != null
                ? await _dockerModelRunner.PullModelAsync(request.ModelName, request.Endpoint)
                : await _dockerModelRunner.PullModelAsync(request.ModelName);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling Docker model {ModelName}", request.ModelName);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get health status of all local model providers
    /// GET /api/localmodels/health
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult<LocalModelHealthResponse>> GetHealthStatus()
    {
        try
        {
            _logger.LogInformation("Checking health status of local model providers");

            var response = new LocalModelHealthResponse
            {
                CheckTime = DateTime.UtcNow
            };

            // Check Ollama
            var ollamaTest = await _ollamaIntegration.TestConnectionAsync();
            response.Providers["Ollama"] = new ProviderHealth
            {
                IsAvailable = ollamaTest.IsSuccess,
                Message = ollamaTest.Message,
                ResponseTimeMs = ollamaTest.ResponseTimeMs,
                ModelCount = ollamaTest.AvailableModels.Count,
                Endpoint = "http://localhost:11434"
            };

            // Check Docker Model Runner
            var dockerTest = await _dockerModelRunner.TestConnectionAsync();
            response.Providers["DockerModelRunner"] = new ProviderHealth
            {
                IsAvailable = dockerTest.IsSuccess,
                Message = dockerTest.Message,
                ResponseTimeMs = dockerTest.ResponseTimeMs,
                ModelCount = dockerTest.AvailableModels.Count,
                Endpoint = "http://localhost:2375"
            };

            response.IsHealthy = response.Providers.Values.Any(p => p.IsAvailable);
            response.AvailableProviders = response.Providers.Where(p => p.Value.IsAvailable).Select(p => p.Key).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health status");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get recommended OpenLLaMA models
    /// GET /api/localmodels/openllama/recommended
    /// </summary>
    [HttpGet("openllama/recommended")]
    public ActionResult<List<RecommendedModelInfo>> GetRecommendedOpenLLaMAModels()
    {
        var models = new List<RecommendedModelInfo>
        {
            new RecommendedModelInfo
            {
                Name = "openllama:7b",
                Provider = "Ollama",
                DisplayName = "OpenLLaMA 7B",
                Description = "Open-source large language model (7 billion parameters). Good balance of quality and performance.",
                Size = "4.1GB",
                ParameterSize = "7B",
                PullCommand = "ollama pull openllama:7b",
                IsRecommended = true,
                RecommendedFor = new List<string> { "General text generation", "Q&A", "Summarization" },
                RequiresGPU = false,
                MinimumRAM = "8GB"
            },
            new RecommendedModelInfo
            {
                Name = "openllama:3b",
                Provider = "Ollama",
                DisplayName = "OpenLLaMA 3B",
                Description = "Smaller open-source model (3 billion parameters). Faster, requires less resources.",
                Size = "1.9GB",
                ParameterSize = "3B",
                PullCommand = "ollama pull openllama:3b",
                IsRecommended = true,
                RecommendedFor = new List<string> { "Quick responses", "Simple tasks", "Testing" },
                RequiresGPU = false,
                MinimumRAM = "4GB"
            },
            new RecommendedModelInfo
            {
                Name = "openllama-7b",
                Provider = "DockerModelRunner",
                DisplayName = "OpenLLaMA 7B (Docker)",
                Description = "OpenLLaMA 7B running in Docker Model Runner. Easy deployment.",
                Size = "4.1GB",
                ParameterSize = "7B",
                PullCommand = "docker model pull openllama-7b",
                IsRecommended = true,
                RecommendedFor = new List<string> { "Containerized environments", "Production deployments" },
                RequiresGPU = false,
                MinimumRAM = "8GB"
            }
        };

        return Ok(models);
    }

    /// <summary>
    /// Test a specific model
    /// POST /api/localmodels/test-model
    /// </summary>
    [HttpPost("test-model")]
    public async Task<ActionResult<ModelTestResponse>> TestModel([FromBody] TestModelRequest request)
    {
        try
        {
            _logger.LogInformation("Testing model {ModelName} with provider {Provider}", request.ModelName, request.Provider);

            var startTime = DateTime.UtcNow;

            // Create a test agent
            var agent = await _unifiedProvider.CreateAgentAsync(
                $"test-{request.ModelName}",
                "Test agent",
                request.Provider,
                request.ModelName);

            // Execute a test message
            var testMessage = request.TestMessage ?? "Hello! Please respond with 'Test successful' if you can read this.";
            var response = await _unifiedProvider.ExecuteChatAsync(agent.Id, testMessage);

            var endTime = DateTime.UtcNow;

            return Ok(new ModelTestResponse
            {
                IsSuccess = !response.Message.Contains("Error"),
                ModelName = request.ModelName,
                Provider = request.Provider.ToString(),
                TestMessage = testMessage,
                Response = response.Message,
                TokenCount = response.TokenCount,
                ResponseTimeMs = (endTime - startTime).TotalMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing model {ModelName}", request.ModelName);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Local model discovery response
/// </summary>
public class LocalModelDiscoveryResponse
{
    public List<OllamaModelInfo> OllamaModels { get; set; } = new();
    public List<DockerModelInfo> DockerModels { get; set; } = new();
    public int TotalModels { get; set; } = 0;
    public List<string> Providers { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime DiscoveryTime { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Local model health response
/// </summary>
public class LocalModelHealthResponse
{
    public bool IsHealthy { get; set; } = false;
    public Dictionary<string, ProviderHealth> Providers { get; set; } = new();
    public List<string> AvailableProviders { get; set; } = new();
    public DateTime CheckTime { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Provider health information
/// </summary>
public class ProviderHealth
{
    public bool IsAvailable { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public double ResponseTimeMs { get; set; } = 0.0;
    public int ModelCount { get; set; } = 0;
    public string Endpoint { get; set; } = string.Empty;
}

/// <summary>
/// Pull model request
/// </summary>
public class PullModelRequest
{
    public string ModelName { get; set; } = string.Empty;
    public string? Endpoint { get; set; }
}

/// <summary>
/// Test model request
/// </summary>
public class TestModelRequest
{
    public string ModelName { get; set; } = string.Empty;
    public AIProviderType Provider { get; set; }
    public string? TestMessage { get; set; }
}

/// <summary>
/// Model test response
/// </summary>
public class ModelTestResponse
{
    public bool IsSuccess { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string TestMessage { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public int TokenCount { get; set; } = 0;
    public double ResponseTimeMs { get; set; } = 0.0;
}

/// <summary>
/// Recommended model information
/// </summary>
public class RecommendedModelInfo
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string ParameterSize { get; set; } = string.Empty;
    public string PullCommand { get; set; } = string.Empty;
    public bool IsRecommended { get; set; } = false;
    public List<string> RecommendedFor { get; set; } = new();
    public bool RequiresGPU { get; set; } = false;
    public string MinimumRAM { get; set; } = string.Empty;
}

