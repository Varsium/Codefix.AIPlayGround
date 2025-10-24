using Microsoft.Extensions.Logging;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Unified AI Provider service for Microsoft Agent Framework
/// Supports multiple AI providers through a single interface
/// Based on: https://github.com/microsoft/agent-framework/tree/main/dotnet
/// </summary>
public class UnifiedAIProviderService : IUnifiedAIProviderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnifiedAIProviderService> _logger;
    private readonly IDockerModelRunnerService _dockerModelRunner;
    private readonly IOllamaIntegrationService _ollamaIntegration;
    private readonly Dictionary<string, object> _providerClients = new();

    public UnifiedAIProviderService(
        ApplicationDbContext context,
        ILogger<UnifiedAIProviderService> logger,
        IDockerModelRunnerService dockerModelRunner,
        IOllamaIntegrationService ollamaIntegration)
    {
        _context = context;
        _logger = logger;
        _dockerModelRunner = dockerModelRunner;
        _ollamaIntegration = ollamaIntegration;
    }

    /// <summary>
    /// Create AI agent with specified provider
    /// </summary>
    public async Task<AgentEntity> CreateAgentAsync(
        string name, 
        string description, 
        AIProviderType providerType,
        string modelName,
        Dictionary<string, object>? providerConfig = null)
    {
        try
        {
            _logger.LogInformation("Creating AI agent {AgentName} with provider {ProviderType}", name, providerType);

            var agentDefinition = new MicrosoftAgentFrameworkDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                Type = MicrosoftAgentFrameworkType.ChatAgent,
                Instructions = $"AI agent using {providerType} provider with model {modelName}",
                LLMConfig = new MicrosoftAgentFrameworkLLMConfiguration
                {
                    ModelName = modelName,
                    Provider = providerType.ToString(),
                    ProviderType = providerType,
                    Endpoint = GetProviderEndpoint(providerType),
                    ApiKey = GetProviderApiKey(providerType),
                    Temperature = 0.7,
                    MaxTokens = 2000
                },
                Capabilities = new MicrosoftAgentFrameworkCapabilities
                {
                    CanCallTools = true,
                    CanUseMemory = true,
                    CanUseCheckpoints = true,
                    CanOrchestrate = false,
                    CanUseMCP = providerType == AIProviderType.PeerLLM,
                    CanStreamResponses = true,
                    CanHandleParallelExecution = false
                },
                Status = MicrosoftAgentFrameworkStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Version = "1.0.0"
            };

            // Add provider-specific configuration
            if (providerConfig != null)
            {
                agentDefinition.LLMConfig.FrameworkSettings = providerConfig;
            }

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

            _logger.LogInformation("Successfully created AI agent {AgentId} with provider {ProviderType}", entity.Id, providerType);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AI agent {AgentName} with provider {ProviderType}", name, providerType);
            throw;
        }
    }

    /// <summary>
    /// Execute chat with AI agent using specified provider
    /// </summary>
    public async Task<AIProviderResponse> ExecuteChatAsync(
        string agentId, 
        string message, 
        Dictionary<string, object>? context = null)
    {
        try
        {
            _logger.LogInformation("Executing chat with agent {AgentId}", agentId);

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

            // Execute based on provider type
            var response = await ExecuteWithProviderAsync(llmConfig.ProviderType, llmConfig, message, context);

            // Store conversation in database
            await StoreConversationAsync(agentId, message, response.Message, llmConfig.ProviderType);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing chat with agent {AgentId}", agentId);
            throw;
        }
    }

    /// <summary>
    /// Stream chat response from AI agent
    /// </summary>
    public async IAsyncEnumerable<string> StreamChatAsync(
        string agentId, 
        string message, 
        Dictionary<string, object>? context = null)
    {
        _logger.LogInformation("Streaming chat with agent {AgentId}", agentId);

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

        // Stream based on provider type
        var stream = StreamWithProviderAsync(llmConfig.ProviderType, llmConfig, message, context);
        
        await foreach (var token in stream)
        {
            yield return token;
        }
    }

    /// <summary>
    /// Get available models for provider
    /// </summary>
    public async Task<List<string>> GetAvailableModelsAsync(AIProviderType providerType)
    {
        try
        {
            return providerType switch
            {
                AIProviderType.OpenAI => new List<string> { "gpt-4", "gpt-3.5-turbo", "gpt-4-turbo", "gpt-4o" },
                AIProviderType.AzureOpenAI => new List<string> { "gpt-4", "gpt-3.5-turbo", "gpt-4-turbo", "gpt-4o" },
                AIProviderType.Anthropic => new List<string> { "claude-3-opus", "claude-3-sonnet", "claude-3-haiku", "claude-3.5-sonnet" },
                AIProviderType.PeerLLM => new List<string> 
                { 
                    "mistral-7b-instruct-v0.1.Q8_0", 
                    "mistral-7b-instruct-v0.2.Q8_0", 
                    "llama-2-7b-chat.Q8_0", 
                    "llama-2-13b-chat.Q8_0",
                    "codellama-7b-instruct.Q8_0",
                    "codellama-13b-instruct.Q8_0"
                },
                AIProviderType.Ollama => await GetOllamaModelsAsync(),
                AIProviderType.DockerModelRunner => await GetDockerModelRunnerModelsAsync(),
                AIProviderType.LocalModel => new List<string> { "custom-local-model" },
                AIProviderType.LMStudio => new List<string> { "local-model" },
                AIProviderType.GoogleAI => new List<string> { "gemini-pro", "gemini-pro-vision", "gemini-1.5-pro" },
                _ => new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available models for provider {ProviderType}", providerType);
            return new List<string>();
        }
    }

    /// <summary>
    /// Get Ollama models with OpenLLaMA support
    /// </summary>
    private async Task<List<string>> GetOllamaModelsAsync()
    {
        try
        {
            var models = await _ollamaIntegration.DiscoverModelsAsync();
            return models.Select(m => m.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to discover Ollama models, returning defaults");
            return new List<string> 
            { 
                // OpenLLaMA models
                "openllama:7b",
                "openllama:3b",
                // Llama 2 models
                "llama2:7b",
                "llama2:13b",
                "llama2:70b",
                // Mistral models
                "mistral:7b",
                "mistral:7b-instruct",
                // Code Llama models
                "codellama:7b",
                "codellama:13b",
                // Phi models
                "phi:2.7b",
                // Gemma models
                "gemma:7b"
            };
        }
    }

    /// <summary>
    /// Get Docker Model Runner models with OpenLLaMA support
    /// </summary>
    private async Task<List<string>> GetDockerModelRunnerModelsAsync()
    {
        try
        {
            var models = await _dockerModelRunner.DiscoverModelsAsync();
            return models.Select(m => m.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to discover Docker Model Runner models, returning defaults");
            return new List<string> 
            { 
                // OpenLLaMA models
                "openllama-7b",
                "openllama-3b",
                // Llama 2 models
                "llama2-7b",
                "llama2-13b",
                // Mistral models
                "mistral-7b",
                // Code Llama models
                "codellama-7b",
                // Phi models
                "phi-2"
            };
        }
    }

    /// <summary>
    /// Test AI provider connection
    /// </summary>
    public async Task<AIProviderTestResult> TestProviderAsync(AIProviderType providerType, string modelName)
    {
        try
        {
            _logger.LogInformation("Testing {ProviderType} provider with model {ModelName}", providerType, modelName);

            var config = new MicrosoftAgentFrameworkLLMConfiguration
            {
                ProviderType = providerType,
                ModelName = modelName,
                Endpoint = GetProviderEndpoint(providerType),
                ApiKey = GetProviderApiKey(providerType)
            };

            var startTime = DateTime.UtcNow;
            var response = await ExecuteWithProviderAsync(providerType, config, "Hello, this is a test message.", null);
            var endTime = DateTime.UtcNow;

            return new AIProviderTestResult
            {
                IsSuccess = !response.Message.Contains("Error"),
                Message = response.Message,
                ResponseTimeMs = (endTime - startTime).TotalMilliseconds,
                TokenCount = response.TokenCount,
                ProviderType = providerType,
                ModelName = modelName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing {ProviderType} provider", providerType);
            return new AIProviderTestResult
            {
                IsSuccess = false,
                Message = ex.Message,
                ProviderType = providerType,
                ModelName = modelName
            };
        }
    }

    // Private helper methods

    private async Task<AIProviderResponse> ExecuteWithProviderAsync(
        AIProviderType providerType, 
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        return providerType switch
        {
            AIProviderType.PeerLLM => await ExecuteWithPeerLLMAsync(config, message, context),
            AIProviderType.Ollama => await ExecuteWithOllamaAsync(config, message, context),
            AIProviderType.DockerModelRunner => await ExecuteWithDockerModelRunnerAsync(config, message, context),
            AIProviderType.LocalModel => await ExecuteWithLocalModelAsync(config, message, context),
            AIProviderType.LMStudio => await ExecuteWithLMStudioAsync(config, message, context),
            AIProviderType.OpenAI => await ExecuteWithOpenAIAsync(config, message, context),
            AIProviderType.AzureOpenAI => await ExecuteWithAzureOpenAIAsync(config, message, context),
            AIProviderType.Anthropic => await ExecuteWithAnthropicAsync(config, message, context),
            AIProviderType.GoogleAI => await ExecuteWithGoogleAIAsync(config, message, context),
            _ => new AIProviderResponse { Message = $"Provider {providerType} not implemented yet", TokenCount = 0 }
        };
    }

    private async IAsyncEnumerable<string> StreamWithProviderAsync(
        AIProviderType providerType, 
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        // For now, simulate streaming for all providers
        // TODO: Implement actual streaming for each provider
        var response = await ExecuteWithProviderAsync(providerType, config, message, context);
        var words = response.Message.Split(' ');
        
        foreach (var word in words)
        {
            yield return word + " ";
            await Task.Delay(50); // Simulate streaming delay
        }
    }

    private async Task<AIProviderResponse> ExecuteWithPeerLLMAsync(
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        // TODO: Implement actual PeerLLM integration
        await Task.Delay(100); // Simulate API call
        return new AIProviderResponse
        {
            Message = $"PeerLLM Response: {message} (simulated with {config.ModelName})",
            TokenCount = message.Length + 50,
            ProviderType = AIProviderType.PeerLLM,
            ModelName = config.ModelName
        };
    }

    private async Task<AIProviderResponse> ExecuteWithOllamaAsync(
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        try
        {
            // Create a temporary agent for execution
            var tempAgent = await _ollamaIntegration.CreateAgentAsync(
                $"temp-ollama-{Guid.NewGuid()}",
                "Temporary Ollama agent",
                config.ModelName,
                new Dictionary<string, object>
                {
                    ["Endpoint"] = config.Endpoint ?? "http://localhost:11434/api/generate",
                    ["Temperature"] = config.Temperature,
                    ["MaxTokens"] = config.MaxTokens,
                    ["TopP"] = config.TopP
                });

            var response = await _ollamaIntegration.ExecuteChatAsync(tempAgent.Id, message, context);

            // Clean up temporary agent
            _context.MicrosoftAgentFrameworkAgents.Remove(tempAgent);
            await _context.SaveChangesAsync();

            return new AIProviderResponse
            {
                Message = response.Message,
                TokenCount = response.TokenCount,
                ProviderType = AIProviderType.Ollama,
                ModelName = config.ModelName,
                Metadata = response.Metadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Ollama request");
            return new AIProviderResponse
            {
                Message = $"Error: {ex.Message}",
                TokenCount = 0,
                ProviderType = AIProviderType.Ollama,
                ModelName = config.ModelName
            };
        }
    }

    private async Task<AIProviderResponse> ExecuteWithDockerModelRunnerAsync(
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        try
        {
            // Create a temporary agent for execution
            var tempAgent = await _dockerModelRunner.CreateAgentAsync(
                $"temp-docker-{Guid.NewGuid()}",
                "Temporary Docker Model Runner agent",
                config.ModelName,
                new Dictionary<string, object>
                {
                    ["Endpoint"] = config.Endpoint ?? "http://localhost:2375/v1/chat/completions",
                    ["Temperature"] = config.Temperature,
                    ["MaxTokens"] = config.MaxTokens,
                    ["DockerEndpoint"] = config.FrameworkSettings?.GetValueOrDefault("dockerEndpoint", "http://localhost:2375") ?? "http://localhost:2375"
                });

            var response = await _dockerModelRunner.ExecuteChatAsync(tempAgent.Id, message, context);

            // Clean up temporary agent
            _context.MicrosoftAgentFrameworkAgents.Remove(tempAgent);
            await _context.SaveChangesAsync();

            return new AIProviderResponse
            {
                Message = response.Message,
                TokenCount = response.TokenCount,
                ProviderType = AIProviderType.DockerModelRunner,
                ModelName = config.ModelName,
                Metadata = response.Metadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Docker Model Runner request");
            return new AIProviderResponse
            {
                Message = $"Error: {ex.Message}",
                TokenCount = 0,
                ProviderType = AIProviderType.DockerModelRunner,
                ModelName = config.ModelName
            };
        }
    }

    private async Task<AIProviderResponse> ExecuteWithLocalModelAsync(
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        // Generic local model execution (OpenAI-compatible API)
        try
        {
            var httpClient = new HttpClient();
            var requestPayload = new
            {
                model = config.ModelName,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = message }
                },
                temperature = config.Temperature,
                max_tokens = config.MaxTokens
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestPayload),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(config.Endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new AIProviderResponse
                {
                    Message = $"Error: {responseContent}",
                    TokenCount = 0,
                    ProviderType = AIProviderType.LocalModel,
                    ModelName = config.ModelName
                };
            }

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var assistantMessage = jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "No response";
            var tokenCount = jsonResponse.TryGetProperty("usage", out var usage) ? usage.GetProperty("total_tokens").GetInt32() : 0;

            return new AIProviderResponse
            {
                Message = assistantMessage,
                TokenCount = tokenCount,
                ProviderType = AIProviderType.LocalModel,
                ModelName = config.ModelName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing local model request");
            return new AIProviderResponse
            {
                Message = $"Error: {ex.Message}",
                TokenCount = 0,
                ProviderType = AIProviderType.LocalModel,
                ModelName = config.ModelName
            };
        }
    }

    private async Task<AIProviderResponse> ExecuteWithLMStudioAsync(
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        // LM Studio uses OpenAI-compatible API on http://localhost:1234/v1
        config.Endpoint ??= "http://localhost:1234/v1/chat/completions";
        return await ExecuteWithLocalModelAsync(config, message, context);
    }

    private async Task<AIProviderResponse> ExecuteWithOpenAIAsync(
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        // TODO: Implement actual OpenAI integration using Microsoft Agent Framework
        await Task.Delay(100); // Simulate API call
        return new AIProviderResponse
        {
            Message = $"OpenAI Response: {message} (simulated with {config.ModelName})",
            TokenCount = message.Length + 50,
            ProviderType = AIProviderType.OpenAI,
            ModelName = config.ModelName
        };
    }

    private async Task<AIProviderResponse> ExecuteWithAzureOpenAIAsync(
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        // TODO: Implement actual Azure OpenAI integration
        await Task.Delay(100); // Simulate API call
        return new AIProviderResponse
        {
            Message = $"Azure OpenAI Response: {message} (simulated with {config.ModelName})",
            TokenCount = message.Length + 50,
            ProviderType = AIProviderType.AzureOpenAI,
            ModelName = config.ModelName
        };
    }

    private async Task<AIProviderResponse> ExecuteWithAnthropicAsync(
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        // TODO: Implement actual Anthropic integration
        await Task.Delay(100); // Simulate API call
        return new AIProviderResponse
        {
            Message = $"Anthropic Response: {message} (simulated with {config.ModelName})",
            TokenCount = message.Length + 50,
            ProviderType = AIProviderType.Anthropic,
            ModelName = config.ModelName
        };
    }

    private async Task<AIProviderResponse> ExecuteWithGoogleAIAsync(
        MicrosoftAgentFrameworkLLMConfiguration config, 
        string message, 
        Dictionary<string, object>? context)
    {
        // TODO: Implement actual Google AI integration
        await Task.Delay(100); // Simulate API call
        return new AIProviderResponse
        {
            Message = $"Google AI Response: {message} (simulated with {config.ModelName})",
            TokenCount = message.Length + 50,
            ProviderType = AIProviderType.GoogleAI,
            ModelName = config.ModelName
        };
    }

    private string GetProviderEndpoint(AIProviderType providerType)
    {
        return providerType switch
        {
            AIProviderType.PeerLLM => "https://api.peerllm.com",
            AIProviderType.Ollama => "http://localhost:11434/api/generate",
            AIProviderType.DockerModelRunner => "http://localhost:2375/v1/chat/completions",
            AIProviderType.LocalModel => "http://localhost:8080/v1/chat/completions",
            AIProviderType.LMStudio => "http://localhost:1234/v1/chat/completions",
            AIProviderType.OpenAI => "https://api.openai.com/v1",
            AIProviderType.AzureOpenAI => "https://your-resource.openai.azure.com/",
            AIProviderType.Anthropic => "https://api.anthropic.com",
            AIProviderType.GoogleAI => "https://generativelanguage.googleapis.com/v1beta",
            _ => ""
        };
    }

    private string GetProviderApiKey(AIProviderType providerType)
    {
        // TODO: Get from configuration or environment variables
        return providerType switch
        {
            AIProviderType.PeerLLM => "peerllm-api-key",
            AIProviderType.Ollama => "", // Ollama doesn't require API key
            AIProviderType.DockerModelRunner => "", // Docker Model Runner doesn't require API key
            AIProviderType.LocalModel => "", // Local models typically don't require API key
            AIProviderType.LMStudio => "", // LM Studio doesn't require API key
            AIProviderType.OpenAI => "openai-api-key",
            AIProviderType.AzureOpenAI => "azure-openai-api-key",
            AIProviderType.Anthropic => "anthropic-api-key",
            AIProviderType.GoogleAI => "google-ai-api-key",
            _ => ""
        };
    }

    private async Task StoreConversationAsync(string agentId, string userMessage, string assistantMessage, AIProviderType providerType)
    {
        try
        {
            // Store conversation in database
            var conversation = new PeerLLMConversationEntity
            {
                Id = Guid.NewGuid().ToString(),
                AgentId = agentId,
                UserId = "system", // TODO: Get from authentication context
                Title = $"Chat with {providerType} agent",
                Status = PeerLLMConversationStatus.Active,
                MessagesJson = JsonSerializer.Serialize(new[] { userMessage, assistantMessage }),
                MessageCount = 2,
                TotalTokens = userMessage.Length + assistantMessage.Length,
                StartedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            _context.PeerLLMConversations.Add(conversation);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing conversation for agent {AgentId}", agentId);
        }
    }
}

/// <summary>
/// AI Provider response
/// </summary>
public class AIProviderResponse
{
    public string Message { get; set; } = string.Empty;
    public int TokenCount { get; set; } = 0;
    public AIProviderType ProviderType { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// AI Provider test result
/// </summary>
public class AIProviderTestResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public double ResponseTimeMs { get; set; } = 0.0;
    public int TokenCount { get; set; } = 0;
    public AIProviderType ProviderType { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Unified AI Provider service interface
/// </summary>
public interface IUnifiedAIProviderService
{
    Task<AgentEntity> CreateAgentAsync(string name, string description, AIProviderType providerType, string modelName, Dictionary<string, object>? providerConfig = null);
    Task<AIProviderResponse> ExecuteChatAsync(string agentId, string message, Dictionary<string, object>? context = null);
    IAsyncEnumerable<string> StreamChatAsync(string agentId, string message, Dictionary<string, object>? context = null);
    Task<List<string>> GetAvailableModelsAsync(AIProviderType providerType);
    Task<AIProviderTestResult> TestProviderAsync(AIProviderType providerType, string modelName);
}
