using Blazor.Diagrams.Core.Geometry;
using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Docker Model Runner integration service
/// Provides integration with Docker's native AI model hosting
/// Based on: https://docs.docker.com/ai/model-runner/
/// Integrated with Microsoft Agent Framework
/// </summary>
public class DockerModelRunnerService : IDockerModelRunnerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DockerModelRunnerService> _logger;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, DockerModelInfo> _discoveredModels = new();

    public DockerModelRunnerService(
        ApplicationDbContext context,
        ILogger<DockerModelRunnerService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Discover available models from Docker Model Runner
    /// </summary>
    public async Task<List<DockerModelInfo>> DiscoverModelsAsync(string dockerEndpoint = "http://localhost:2375")
    {
        try
        {
            _logger.LogInformation("Discovering Docker Model Runner models at {Endpoint}", dockerEndpoint);

            // Docker Model Runner exposes models through the Docker API
            // Format: docker model ls
            var modelsEndpoint = $"{dockerEndpoint}/v1/models";
            
            var response = await _httpClient.GetAsync(modelsEndpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to discover models from Docker Model Runner: {StatusCode}", response.StatusCode);
                return GetDefaultDockerModels();
            }

            var content = await response.Content.ReadAsStringAsync();
            var models = JsonSerializer.Deserialize<List<DockerModelInfo>>(content) ?? new List<DockerModelInfo>();

            // Cache discovered models
            foreach (var model in models)
            {
                _discoveredModels[model.Name] = model;
            }

            _logger.LogInformation("Discovered {Count} models from Docker Model Runner", models.Count);
            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering Docker Model Runner models");
            return GetDefaultDockerModels();
        }
    }

    /// <summary>
    /// Get default Docker Model Runner models
    /// These are commonly available models through Docker Model Runner
    /// </summary>
    private List<DockerModelInfo> GetDefaultDockerModels()
    {
        return new List<DockerModelInfo>
        {
            new DockerModelInfo
            {
                Name = "openllama-7b",
                DisplayName = "OpenLLaMA 7B",
                Description = "Open-source large language model (7 billion parameters)",
                Size = "4.1GB",
                Family = "llama",
                ParameterSize = "7B",
                Quantization = "Q4_0",
                Tags = new List<string> { "text-generation", "open-source", "llama" },
                Endpoint = "http://localhost:2375/v1/chat/completions",
                IsAvailable = true
            },
            new DockerModelInfo
            {
                Name = "openllama-3b",
                DisplayName = "OpenLLaMA 3B",
                Description = "Smaller open-source large language model (3 billion parameters)",
                Size = "1.9GB",
                Family = "llama",
                ParameterSize = "3B",
                Quantization = "Q4_0",
                Tags = new List<string> { "text-generation", "open-source", "llama" },
                Endpoint = "http://localhost:2375/v1/chat/completions",
                IsAvailable = true
            },
            new DockerModelInfo
            {
                Name = "llama2-7b",
                DisplayName = "Llama 2 7B",
                Description = "Meta's Llama 2 model (7 billion parameters)",
                Size = "3.8GB",
                Family = "llama",
                ParameterSize = "7B",
                Quantization = "Q4_0",
                Tags = new List<string> { "text-generation", "meta", "llama" },
                Endpoint = "http://localhost:2375/v1/chat/completions",
                IsAvailable = true
            },
            new DockerModelInfo
            {
                Name = "llama2-13b",
                DisplayName = "Llama 2 13B",
                Description = "Meta's Llama 2 model (13 billion parameters)",
                Size = "7.4GB",
                Family = "llama",
                ParameterSize = "13B",
                Quantization = "Q4_0",
                Tags = new List<string> { "text-generation", "meta", "llama" },
                Endpoint = "http://localhost:2375/v1/chat/completions",
                IsAvailable = false
            },
            new DockerModelInfo
            {
                Name = "mistral-7b",
                DisplayName = "Mistral 7B Instruct",
                Description = "Mistral AI's 7B instruction-tuned model",
                Size = "4.1GB",
                Family = "mistral",
                ParameterSize = "7B",
                Quantization = "Q4_0",
                Tags = new List<string> { "text-generation", "instruction", "mistral" },
                Endpoint = "http://localhost:2375/v1/chat/completions",
                IsAvailable = true
            },
            new DockerModelInfo
            {
                Name = "codellama-7b",
                DisplayName = "Code Llama 7B",
                Description = "Meta's specialized code generation model",
                Size = "3.8GB",
                Family = "llama",
                ParameterSize = "7B",
                Quantization = "Q4_0",
                Tags = new List<string> { "code-generation", "meta", "llama" },
                Endpoint = "http://localhost:2375/v1/chat/completions",
                IsAvailable = true
            },
            new DockerModelInfo
            {
                Name = "phi-2",
                DisplayName = "Phi-2 (2.7B)",
                Description = "Microsoft's small language model optimized for performance",
                Size = "1.6GB",
                Family = "phi",
                ParameterSize = "2.7B",
                Quantization = "Q4_0",
                Tags = new List<string> { "text-generation", "microsoft", "small-model" },
                Endpoint = "http://localhost:2375/v1/chat/completions",
                IsAvailable = true
            }
        };
    }

    /// <summary>
    /// Create an agent using Docker Model Runner
    /// Integrated with Microsoft Agent Framework
    /// </summary>
    public async Task<AgentEntity> CreateAgentAsync(
        string name,
        string description,
        string modelName,
        Dictionary<string, object>? config = null)
    {
        try
        {
            _logger.LogInformation("Creating Docker Model Runner agent {AgentName} with model {ModelName}", name, modelName);

            var agentDefinition = new MicrosoftAgentFrameworkDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                Type = MicrosoftAgentFrameworkType.ChatAgent,
                Instructions = $"AI agent using Docker Model Runner with model {modelName}",
                LLMConfig = new MicrosoftAgentFrameworkLLMConfiguration
                {
                    ModelName = modelName,
                    Provider = "DockerModelRunner",
                    ProviderType = AIProviderType.DockerModelRunner,
                    Endpoint = config?.GetValueOrDefault("Endpoint", "http://localhost:2375/v1/chat/completions") as string ?? "http://localhost:2375/v1/chat/completions",
                    Temperature = (double)(config?.GetValueOrDefault("Temperature", 0.7) ?? 0.7),
                    MaxTokens = (int)(config?.GetValueOrDefault("MaxTokens", 2000) ?? 2000),
                    FrameworkSettings = new Dictionary<string, object>
                    {
                        ["dockerEndpoint"] = config?.GetValueOrDefault("DockerEndpoint", "http://localhost:2375") ?? "http://localhost:2375",
                        ["stream"] = config?.GetValueOrDefault("Stream", true) ?? true,
                        ["useGpu"] = config?.GetValueOrDefault("UseGpu", false) ?? false
                    }
                },
                Capabilities = new MicrosoftAgentFrameworkCapabilities
                {
                    CanCallTools = true,
                    CanUseMemory = true,
                    CanUseCheckpoints = true,
                    CanOrchestrate = false,
                    CanUseMCP = false,
                    CanStreamResponses = true,
                    CanHandleParallelExecution = false
                },
                Status = MicrosoftAgentFrameworkStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Version = "1.0.0"
            };

            // Store in database
            var entity = new AgentEntity
            {
                Id = agentDefinition.Id,
                Name = agentDefinition.Name,
                AgentType = agentDefinition.Type.ToString(),
                Description = agentDefinition.Description,
                Instructions = agentDefinition.Instructions,
                LLMConfigurationJson = JsonSerializer.Serialize(agentDefinition.LLMConfig),
                PropertiesJson = JsonSerializer.Serialize(new
                {
                    Capabilities = agentDefinition.Capabilities,
                    SecurityConfig = agentDefinition.SecurityConfig,
                    FrameworkProperties = agentDefinition.FrameworkProperties,
                    Status = agentDefinition.Status,
                    Version = agentDefinition.Version
                }),
                Status = AgentStatus.Active,
                CreatedBy = "system",
                CreatedAt = agentDefinition.CreatedAt,
                UpdatedAt = agentDefinition.UpdatedAt
            };

            _context.MicrosoftAgentFrameworkAgents.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created Docker Model Runner agent {AgentId}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Docker Model Runner agent {AgentName}", name);
            throw;
        }
    }

    /// <summary>
    /// Execute chat with Docker Model Runner
    /// </summary>
    public async Task<DockerModelResponse> ExecuteChatAsync(
        string agentId,
        string message,
        Dictionary<string, object>? context = null)
    {
        try
        {
            _logger.LogInformation("Executing chat with Docker Model Runner agent {AgentId}", agentId);

            var agent = await _context.MicrosoftAgentFrameworkAgents.FindAsync(agentId);
            if (agent == null)
            {
                throw new ArgumentException($"Agent {agentId} not found");
            }

            var llmConfig = JsonSerializer.Deserialize<MicrosoftAgentFrameworkLLMConfiguration>(agent.LLMConfigurationJson);
            if (llmConfig == null)
            {
                throw new InvalidOperationException($"Invalid LLM configuration for agent {agentId}");
            }

            var startTime = DateTime.UtcNow;

            // Build request payload for Docker Model Runner (OpenAI-compatible API)
            var requestPayload = new
            {
                model = llmConfig.ModelName,
                messages = new[]
                {
                    new { role = "system", content = agent.Instructions },
                    new { role = "user", content = message }
                },
                temperature = llmConfig.Temperature,
                max_tokens = llmConfig.MaxTokens,
                stream = false
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestPayload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(llmConfig.Endpoint, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            var endTime = DateTime.UtcNow;
            var processingTime = (endTime - startTime).TotalMilliseconds;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Docker Model Runner request failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return new DockerModelResponse
                {
                    Message = $"Error: Docker Model Runner request failed with status {response.StatusCode}",
                    TokenCount = 0,
                    ProcessingTimeMs = processingTime,
                    IsSuccess = false,
                    ErrorMessage = responseContent
                };
            }

            // Parse OpenAI-compatible response
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var assistantMessage = jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "No response";
            var tokenCount = jsonResponse.GetProperty("usage").GetProperty("total_tokens").GetInt32();

            return new DockerModelResponse
            {
                Message = assistantMessage,
                TokenCount = tokenCount,
                ProcessingTimeMs = processingTime,
                IsSuccess = true,
                Metadata = new Dictionary<string, object>
                {
                    ["model"] = llmConfig.ModelName,
                    ["provider"] = "DockerModelRunner",
                    ["endpoint"] = llmConfig.Endpoint ?? "unknown"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing chat with Docker Model Runner agent {AgentId}", agentId);
            throw;
        }
    }

    /// <summary>
    /// Stream chat response from Docker Model Runner
    /// </summary>
    public async IAsyncEnumerable<string> StreamChatAsync(
        string agentId,
        string message,
        Dictionary<string, object>? context = null)
    {
        _logger.LogInformation("Streaming chat with Docker Model Runner agent {AgentId}", agentId);

        var agent = await _context.MicrosoftAgentFrameworkAgents.FindAsync(agentId);
        if (agent == null)
        {
            yield return $"Error: Agent {agentId} not found";
            yield break;
        }

        var llmConfig = JsonSerializer.Deserialize<MicrosoftAgentFrameworkLLMConfiguration>(agent.LLMConfigurationJson);
        if (llmConfig == null)
        {
            yield return "Error: Invalid LLM configuration";
            yield break;
        }

        // Build streaming request payload
        var requestPayload = new
        {
            model = llmConfig.ModelName,
            messages = new[]
            {
                new { role = "system", content = agent.Instructions },
                new { role = "user", content = message }
            },
            temperature = llmConfig.Temperature,
            max_tokens = llmConfig.MaxTokens,
            stream = true
        };

        var requestContent = new StringContent(
            JsonSerializer.Serialize(requestPayload),
            Encoding.UTF8,
            "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, llmConfig.Endpoint)
        {
            Content = requestContent
        };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
       
            while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var data = line.Substring(6);
            if (data == "[DONE]")
                break;

           
                var jsonData = JsonSerializer.Deserialize<JsonElement>(data);
                var delta = jsonData.GetProperty("choices")[0].GetProperty("delta");
                
                if (delta.TryGetProperty("content", out var content))
                {
                    var token = content.GetString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        yield return token;
                    }
                }
            }
           
        }

    /// <summary>
    /// Test Docker Model Runner connection
    /// </summary>
    public async Task<DockerModelTestResult> TestConnectionAsync(string dockerEndpoint = "http://localhost:2375")
    {
        try
        {
            _logger.LogInformation("Testing Docker Model Runner connection at {Endpoint}", dockerEndpoint);

            var startTime = DateTime.UtcNow;
            
            // Test if Docker is accessible
            var healthEndpoint = $"{dockerEndpoint}/_ping";
            var response = await _httpClient.GetAsync(healthEndpoint);
            
            var endTime = DateTime.UtcNow;
            var responseTime = (endTime - startTime).TotalMilliseconds;

            if (!response.IsSuccessStatusCode)
            {
                return new DockerModelTestResult
                {
                    IsSuccess = false,
                    Message = $"Docker Model Runner not accessible at {dockerEndpoint}",
                    ResponseTimeMs = responseTime,
                    ErrorMessage = $"HTTP {response.StatusCode}"
                };
            }

            // Try to discover models
            var models = await DiscoverModelsAsync(dockerEndpoint);

            return new DockerModelTestResult
            {
                IsSuccess = true,
                Message = $"Docker Model Runner is accessible. Found {models.Count} models.",
                ResponseTimeMs = responseTime,
                AvailableModels = models.Select(m => m.Name).ToList(),
                Metrics = new Dictionary<string, object>
                {
                    ["dockerEndpoint"] = dockerEndpoint,
                    ["modelCount"] = models.Count,
                    ["responseTimeMs"] = responseTime
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Docker Model Runner connection");
            return new DockerModelTestResult
            {
                IsSuccess = false,
                Message = "Failed to connect to Docker Model Runner",
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Pull a model from Docker Hub to Docker Model Runner
    /// </summary>
    public async Task<DockerModelPullResult> PullModelAsync(string modelName, string dockerEndpoint = "http://localhost:2375")
    {
        try
        {
            _logger.LogInformation("Pulling model {ModelName} to Docker Model Runner", modelName);

            var pullEndpoint = $"{dockerEndpoint}/v1/models/pull";
            var requestPayload = new
            {
                model = modelName
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestPayload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(pullEndpoint, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new DockerModelPullResult
                {
                    IsSuccess = false,
                    ModelName = modelName,
                    Message = $"Failed to pull model: {response.StatusCode}",
                    ErrorMessage = responseContent
                };
            }

            return new DockerModelPullResult
            {
                IsSuccess = true,
                ModelName = modelName,
                Message = $"Successfully pulled model {modelName}",
                Details = responseContent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling model {ModelName}", modelName);
            return new DockerModelPullResult
            {
                IsSuccess = false,
                ModelName = modelName,
                Message = "Failed to pull model",
                ErrorMessage = ex.Message
            };
        }
    }
}

/// <summary>
/// Docker Model Runner service interface
/// </summary>
public interface IDockerModelRunnerService
{
    Task<List<DockerModelInfo>> DiscoverModelsAsync(string dockerEndpoint = "http://localhost:2375");
    Task<AgentEntity> CreateAgentAsync(string name, string description, string modelName, Dictionary<string, object>? config = null);
    Task<DockerModelResponse> ExecuteChatAsync(string agentId, string message, Dictionary<string, object>? context = null);
    IAsyncEnumerable<string> StreamChatAsync(string agentId, string message, Dictionary<string, object>? context = null);
    Task<DockerModelTestResult> TestConnectionAsync(string dockerEndpoint = "http://localhost:2375");
    Task<DockerModelPullResult> PullModelAsync(string modelName, string dockerEndpoint = "http://localhost:2375");
}

/// <summary>
/// Docker Model information
/// </summary>
public class DockerModelInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Family { get; set; } = string.Empty;
    public string ParameterSize { get; set; } = string.Empty;
    public string Quantization { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Endpoint { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = false;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Docker Model Runner response
/// </summary>
public class DockerModelResponse
{
    public string Message { get; set; } = string.Empty;
    public int TokenCount { get; set; } = 0;
    public double ProcessingTimeMs { get; set; } = 0.0;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Docker Model Runner test result
/// </summary>
public class DockerModelTestResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public double ResponseTimeMs { get; set; } = 0.0;
    public string? ErrorMessage { get; set; }
    public List<string> AvailableModels { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Docker Model pull result
/// </summary>
public class DockerModelPullResult
{
    public bool IsSuccess { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
}

