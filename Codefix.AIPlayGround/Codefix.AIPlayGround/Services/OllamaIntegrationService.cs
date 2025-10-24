using Microsoft.Extensions.Logging;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Http;
using System.Text;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Ollama integration service with full Microsoft Agent Framework support
/// Provides local model hosting with models like Llama2, Mistral, CodeLlama, and OpenLLaMA
/// Based on: https://ollama.ai/
/// Integrated with Microsoft Agent Framework
/// </summary>
public class OllamaIntegrationService : IOllamaIntegrationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OllamaIntegrationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, OllamaModelInfo> _cachedModels = new();

    public OllamaIntegrationService(
        ApplicationDbContext context,
        ILogger<OllamaIntegrationService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(10); // Ollama can be slow for large models
    }

    /// <summary>
    /// Discover available Ollama models
    /// </summary>
    public async Task<List<OllamaModelInfo>> DiscoverModelsAsync(string ollamaEndpoint = "http://localhost:11434")
    {
        try
        {
            _logger.LogInformation("Discovering Ollama models at {Endpoint}", ollamaEndpoint);

            var tagsEndpoint = $"{ollamaEndpoint}/api/tags";
            var response = await _httpClient.GetAsync(tagsEndpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to discover Ollama models: {StatusCode}", response.StatusCode);
                return GetDefaultOllamaModels();
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content);

            var models = new List<OllamaModelInfo>();

            if (jsonResponse.TryGetProperty("models", out var modelsArray))
            {
                foreach (var modelElement in modelsArray.EnumerateArray())
                {
                    var modelInfo = ParseOllamaModel(modelElement);
                    models.Add(modelInfo);
                    _cachedModels[modelInfo.Name] = modelInfo;
                }
            }

            _logger.LogInformation("Discovered {Count} Ollama models", models.Count);
            return models.Count > 0 ? models : GetDefaultOllamaModels();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering Ollama models");
            return GetDefaultOllamaModels();
        }
    }

    /// <summary>
    /// Parse Ollama model from API response
    /// </summary>
    private OllamaModelInfo ParseOllamaModel(JsonElement modelElement)
    {
        var name = modelElement.GetProperty("name").GetString() ?? "unknown";
        var size = modelElement.TryGetProperty("size", out var sizeElement) ? sizeElement.GetInt64() : 0;
        var modifiedAt = modelElement.TryGetProperty("modified_at", out var modifiedElement) ? modifiedElement.GetString() : null;

        return new OllamaModelInfo
        {
            Name = name,
            DisplayName = FormatDisplayName(name),
            Size = FormatSize(size),
            SizeBytes = size,
            ModifiedAt = modifiedAt != null ? DateTime.Parse(modifiedAt) : DateTime.UtcNow,
            Family = ExtractFamily(name),
            ParameterSize = ExtractParameterSize(name),
            IsAvailable = true,
            Endpoint = "http://localhost:11434/api/generate"
        };
    }

    /// <summary>
    /// Get default Ollama models (common models)
    /// </summary>
    private List<OllamaModelInfo> GetDefaultOllamaModels()
    {
        return new List<OllamaModelInfo>
        {
            // OpenLLaMA models
            new OllamaModelInfo
            {
                Name = "openllama:7b",
                DisplayName = "OpenLLaMA 7B",
                Description = "Open-source large language model (7 billion parameters)",
                Size = "4.1GB",
                Family = "llama",
                ParameterSize = "7B",
                Tags = new List<string> { "text-generation", "open-source", "llama", "openllama" },
                Endpoint = "http://localhost:11434/api/generate",
                IsAvailable = false,
                PullCommand = "ollama pull openllama:7b"
            },
            new OllamaModelInfo
            {
                Name = "openllama:3b",
                DisplayName = "OpenLLaMA 3B",
                Description = "Smaller open-source large language model (3 billion parameters)",
                Size = "1.9GB",
                Family = "llama",
                ParameterSize = "3B",
                Tags = new List<string> { "text-generation", "open-source", "llama", "openllama" },
                Endpoint = "http://localhost:11434/api/generate",
                IsAvailable = false,
                PullCommand = "ollama pull openllama:3b"
            },
            // Llama 2 models
            new OllamaModelInfo
            {
                Name = "llama2:7b",
                DisplayName = "Llama 2 7B",
                Description = "Meta's Llama 2 model (7 billion parameters)",
                Size = "3.8GB",
                Family = "llama",
                ParameterSize = "7B",
                Tags = new List<string> { "text-generation", "meta", "llama" },
                Endpoint = "http://localhost:11434/api/generate",
                IsAvailable = false,
                PullCommand = "ollama pull llama2:7b"
            },
            new OllamaModelInfo
            {
                Name = "llama2:13b",
                DisplayName = "Llama 2 13B",
                Description = "Meta's Llama 2 model (13 billion parameters)",
                Size = "7.4GB",
                Family = "llama",
                ParameterSize = "13B",
                Tags = new List<string> { "text-generation", "meta", "llama" },
                Endpoint = "http://localhost:11434/api/generate",
                IsAvailable = false,
                PullCommand = "ollama pull llama2:13b"
            },
            // Mistral models
            new OllamaModelInfo
            {
                Name = "mistral:7b",
                DisplayName = "Mistral 7B Instruct",
                Description = "Mistral AI's 7B instruction-tuned model",
                Size = "4.1GB",
                Family = "mistral",
                ParameterSize = "7B",
                Tags = new List<string> { "text-generation", "instruction", "mistral" },
                Endpoint = "http://localhost:11434/api/generate",
                IsAvailable = false,
                PullCommand = "ollama pull mistral:7b"
            },
            // Code Llama models
            new OllamaModelInfo
            {
                Name = "codellama:7b",
                DisplayName = "Code Llama 7B",
                Description = "Meta's specialized code generation model",
                Size = "3.8GB",
                Family = "llama",
                ParameterSize = "7B",
                Tags = new List<string> { "code-generation", "meta", "llama" },
                Endpoint = "http://localhost:11434/api/generate",
                IsAvailable = false,
                PullCommand = "ollama pull codellama:7b"
            },
            new OllamaModelInfo
            {
                Name = "codellama:13b",
                DisplayName = "Code Llama 13B",
                Description = "Meta's specialized code generation model (13B)",
                Size = "7.4GB",
                Family = "llama",
                ParameterSize = "13B",
                Tags = new List<string> { "code-generation", "meta", "llama" },
                Endpoint = "http://localhost:11434/api/generate",
                IsAvailable = false,
                PullCommand = "ollama pull codellama:13b"
            },
            // Phi models
            new OllamaModelInfo
            {
                Name = "phi:2.7b",
                DisplayName = "Phi-2 (2.7B)",
                Description = "Microsoft's small language model optimized for performance",
                Size = "1.6GB",
                Family = "phi",
                ParameterSize = "2.7B",
                Tags = new List<string> { "text-generation", "microsoft", "small-model" },
                Endpoint = "http://localhost:11434/api/generate",
                IsAvailable = false,
                PullCommand = "ollama pull phi:2.7b"
            },
            // Gemma models
            new OllamaModelInfo
            {
                Name = "gemma:7b",
                DisplayName = "Gemma 7B",
                Description = "Google's Gemma model (7 billion parameters)",
                Size = "4.8GB",
                Family = "gemma",
                ParameterSize = "7B",
                Tags = new List<string> { "text-generation", "google", "gemma" },
                Endpoint = "http://localhost:11434/api/generate",
                IsAvailable = false,
                PullCommand = "ollama pull gemma:7b"
            }
        };
    }

    /// <summary>
    /// Create an agent using Ollama
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
            _logger.LogInformation("Creating Ollama agent {AgentName} with model {ModelName}", name, modelName);

            var agentDefinition = new MicrosoftAgentFrameworkDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                Type = MicrosoftAgentFrameworkType.ChatAgent,
                Instructions = $"AI agent using Ollama with model {modelName}",
                LLMConfig = new MicrosoftAgentFrameworkLLMConfiguration
                {
                    ModelName = modelName,
                    Provider = "Ollama",
                    ProviderType = AIProviderType.Ollama,
                    Endpoint = config?.GetValueOrDefault("Endpoint", "http://localhost:11434/api/generate") as string ?? "http://localhost:11434/api/generate",
                    Temperature = (double)(config?.GetValueOrDefault("Temperature", 0.7) ?? 0.7),
                    MaxTokens = (int)(config?.GetValueOrDefault("MaxTokens", 2000) ?? 2000),
                    TopP = (double)(config?.GetValueOrDefault("TopP", 1.0) ?? 1.0),
                    FrameworkSettings = new Dictionary<string, object>
                    {
                        ["ollamaEndpoint"] = config?.GetValueOrDefault("OllamaEndpoint", "http://localhost:11434") ?? "http://localhost:11434",
                        ["stream"] = config?.GetValueOrDefault("Stream", true) ?? true,
                        ["context"] = config?.GetValueOrDefault("Context", new int[0]) ?? new int[0],
                        ["num_predict"] = config?.GetValueOrDefault("NumPredict", 2000) ?? 2000
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

            _logger.LogInformation("Successfully created Ollama agent {AgentId}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Ollama agent {AgentName}", name);
            throw;
        }
    }

    /// <summary>
    /// Execute chat with Ollama
    /// </summary>
    public async Task<OllamaResponse> ExecuteChatAsync(
        string agentId,
        string message,
        Dictionary<string, object>? context = null)
    {
        try
        {
            _logger.LogInformation("Executing chat with Ollama agent {AgentId}", agentId);

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

            // Build Ollama API request
            var requestPayload = new
            {
                model = llmConfig.ModelName,
                prompt = $"{agent.Instructions}\n\nUser: {message}\nAssistant:",
                stream = false,
                options = new
                {
                    temperature = llmConfig.Temperature,
                    num_predict = llmConfig.MaxTokens,
                    top_p = llmConfig.TopP
                }
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
                _logger.LogError("Ollama request failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return new OllamaResponse
                {
                    Message = $"Error: Ollama request failed with status {response.StatusCode}",
                    TokenCount = 0,
                    ProcessingTimeMs = processingTime,
                    IsSuccess = false,
                    ErrorMessage = responseContent
                };
            }

            // Parse Ollama response
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var assistantMessage = jsonResponse.GetProperty("response").GetString() ?? "No response";
            
            var tokenCount = 0;
            if (jsonResponse.TryGetProperty("eval_count", out var evalCount))
            {
                tokenCount = evalCount.GetInt32();
            }

            return new OllamaResponse
            {
                Message = assistantMessage,
                TokenCount = tokenCount,
                ProcessingTimeMs = processingTime,
                IsSuccess = true,
                Metadata = new Dictionary<string, object>
                {
                    ["model"] = llmConfig.ModelName,
                    ["provider"] = "Ollama",
                    ["endpoint"] = llmConfig.Endpoint ?? "unknown"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing chat with Ollama agent {AgentId}", agentId);
            throw;
        }
    }

    /// <summary>
    /// Stream chat response from Ollama
    /// </summary>
    public async IAsyncEnumerable<string> StreamChatAsync(
        string agentId,
        string message,
        Dictionary<string, object>? context = null)
    {
        _logger.LogInformation("Streaming chat with Ollama agent {AgentId}", agentId);

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

        // Build streaming request
        var requestPayload = new
        {
            model = llmConfig.ModelName,
            prompt = $"{agent.Instructions}\n\nUser: {message}\nAssistant:",
            stream = true,
            options = new
            {
                temperature = llmConfig.Temperature,
                num_predict = llmConfig.MaxTokens,
                top_p = llmConfig.TopP
            }
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
            if (string.IsNullOrWhiteSpace(line))
                continue;

            
                var jsonData = JsonSerializer.Deserialize<JsonElement>(line);
                
                if (jsonData.TryGetProperty("response", out var responseElement))
                {
                    var token = responseElement.GetString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        yield return token;
                    }
                }

                // Check if done
                if (jsonData.TryGetProperty("done", out var doneElement) && doneElement.GetBoolean())
                {
                    break;
                }
        }
    }

    /// <summary>
    /// Test Ollama connection
    /// </summary>
    public async Task<OllamaTestResult> TestConnectionAsync(string ollamaEndpoint = "http://localhost:11434")
    {
        try
        {
            _logger.LogInformation("Testing Ollama connection at {Endpoint}", ollamaEndpoint);

            var startTime = DateTime.UtcNow;
            
            // Test if Ollama is accessible
            var healthEndpoint = $"{ollamaEndpoint}/api/tags";
            var response = await _httpClient.GetAsync(healthEndpoint);
            
            var endTime = DateTime.UtcNow;
            var responseTime = (endTime - startTime).TotalMilliseconds;

            if (!response.IsSuccessStatusCode)
            {
                return new OllamaTestResult
                {
                    IsSuccess = false,
                    Message = $"Ollama not accessible at {ollamaEndpoint}",
                    ResponseTimeMs = responseTime,
                    ErrorMessage = $"HTTP {response.StatusCode}"
                };
            }

            // Try to discover models
            var models = await DiscoverModelsAsync(ollamaEndpoint);

            return new OllamaTestResult
            {
                IsSuccess = true,
                Message = $"Ollama is accessible. Found {models.Count} models.",
                ResponseTimeMs = responseTime,
                AvailableModels = models.Select(m => m.Name).ToList(),
                Metrics = new Dictionary<string, object>
                {
                    ["ollamaEndpoint"] = ollamaEndpoint,
                    ["modelCount"] = models.Count,
                    ["responseTimeMs"] = responseTime
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Ollama connection");
            return new OllamaTestResult
            {
                IsSuccess = false,
                Message = "Failed to connect to Ollama",
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Pull an Ollama model
    /// </summary>
    public async Task<OllamaPullResult> PullModelAsync(string modelName, string ollamaEndpoint = "http://localhost:11434")
    {
        try
        {
            _logger.LogInformation("Pulling Ollama model {ModelName}", modelName);

            var pullEndpoint = $"{ollamaEndpoint}/api/pull";
            var requestPayload = new
            {
                name = modelName
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestPayload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(pullEndpoint, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new OllamaPullResult
                {
                    IsSuccess = false,
                    ModelName = modelName,
                    Message = $"Failed to pull model: {response.StatusCode}",
                    ErrorMessage = responseContent
                };
            }

            return new OllamaPullResult
            {
                IsSuccess = true,
                ModelName = modelName,
                Message = $"Successfully pulled model {modelName}",
                Details = responseContent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pulling Ollama model {ModelName}", modelName);
            return new OllamaPullResult
            {
                IsSuccess = false,
                ModelName = modelName,
                Message = "Failed to pull model",
                ErrorMessage = ex.Message
            };
        }
    }

    // Helper methods

    private string FormatDisplayName(string name)
    {
        // Convert "llama2:7b" to "Llama 2 7B"
        var parts = name.Split(':');
        var modelName = parts[0].Replace("-", " ").Replace("_", " ");
        modelName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(modelName);
        
        if (parts.Length > 1)
        {
            modelName += $" {parts[1].ToUpper()}";
        }

        return modelName;
    }

    private string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
    }

    private string ExtractFamily(string name)
    {
        var lower = name.ToLower();
        if (lower.Contains("llama")) return "llama";
        if (lower.Contains("mistral")) return "mistral";
        if (lower.Contains("phi")) return "phi";
        if (lower.Contains("gemma")) return "gemma";
        if (lower.Contains("codellama")) return "llama";
        return "other";
    }

    private string ExtractParameterSize(string name)
    {
        var parts = name.Split(':');
        if (parts.Length > 1)
        {
            return parts[1].ToUpper();
        }

        // Try to extract from name (e.g., "llama2-7b")
        if (name.Contains("7b", StringComparison.OrdinalIgnoreCase)) return "7B";
        if (name.Contains("13b", StringComparison.OrdinalIgnoreCase)) return "13B";
        if (name.Contains("70b", StringComparison.OrdinalIgnoreCase)) return "70B";
        if (name.Contains("3b", StringComparison.OrdinalIgnoreCase)) return "3B";
        if (name.Contains("2.7b", StringComparison.OrdinalIgnoreCase)) return "2.7B";

        return "unknown";
    }
}

/// <summary>
/// Ollama integration service interface
/// </summary>
public interface IOllamaIntegrationService
{
    Task<List<OllamaModelInfo>> DiscoverModelsAsync(string ollamaEndpoint = "http://localhost:11434");
    Task<AgentEntity> CreateAgentAsync(string name, string description, string modelName, Dictionary<string, object>? config = null);
    Task<OllamaResponse> ExecuteChatAsync(string agentId, string message, Dictionary<string, object>? context = null);
    IAsyncEnumerable<string> StreamChatAsync(string agentId, string message, Dictionary<string, object>? context = null);
    Task<OllamaTestResult> TestConnectionAsync(string ollamaEndpoint = "http://localhost:11434");
    Task<OllamaPullResult> PullModelAsync(string modelName, string ollamaEndpoint = "http://localhost:11434");
}

/// <summary>
/// Ollama model information
/// </summary>
public class OllamaModelInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public long SizeBytes { get; set; } = 0;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public string Family { get; set; } = string.Empty;
    public string ParameterSize { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Endpoint { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = false;
    public string PullCommand { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Ollama response
/// </summary>
public class OllamaResponse
{
    public string Message { get; set; } = string.Empty;
    public int TokenCount { get; set; } = 0;
    public double ProcessingTimeMs { get; set; } = 0.0;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Ollama test result
/// </summary>
public class OllamaTestResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public double ResponseTimeMs { get; set; } = 0.0;
    public string? ErrorMessage { get; set; }
    public List<string> AvailableModels { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Ollama pull result
/// </summary>
public class OllamaPullResult
{
    public bool IsSuccess { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
}

