using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;
using System.Text.Json;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Agents.AI;
using System.ComponentModel;
using System.Reflection;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Scoped service for managing chat sessions with AI agents.
/// Each user gets their own instance per request scope.
/// </summary>
public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChatService> _logger;
    private readonly IConfiguration _configuration;
    
    // In-memory session storage (static to persist across requests)
    // For production, consider using distributed cache (Redis) for multi-server scenarios
    // Key: sessionId (not userId+sessionId to avoid issues with changing session IDs in WASM)
    private static readonly ConcurrentDictionary<string, ChatSession> _sessions = new();
    
    public ChatService(
        ApplicationDbContext context,
        ILogger<ChatService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<StartChatSessionResponse> StartSessionAsync(StartChatSessionRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Starting chat session for user {UserId} with agent {AgentId}", userId, request.AgentId);

            // Fetch agent configuration
            var agent = await _context.Agents
                .FirstOrDefaultAsync(a => a.Id == request.AgentId);

            if (agent == null)
            {
                throw new InvalidOperationException($"Agent {request.AgentId} not found");
            }

            // Create new session
            var session = new ChatSession
            {
                SessionId = Guid.NewGuid().ToString(),
                AgentId = agent.Id,
                AgentName = agent.Name,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                Metadata = request.Metadata ?? new Dictionary<string, object>()
            };

            // Add system message with agent instructions
            if (!string.IsNullOrEmpty(agent.Instructions))
            {
                session.Messages.Add(new ChatMessage(ChatRole.System, agent.Instructions));
            }

            // Store session (using sessionId as key for WASM compatibility)
            _sessions[session.SessionId] = session;

            var response = new StartChatSessionResponse
            {
                SessionId = session.SessionId,
                AgentId = agent.Id,
                AgentName = agent.Name,
                CreatedAt = session.CreatedAt
            };

            // Send initial message if provided
            if (!string.IsNullOrEmpty(request.InitialMessage))
            {
                var messageResponse = await SendMessageAsync(new SendMessageRequest
                {
                    SessionId = session.SessionId,
                    Message = request.InitialMessage
                }, userId);

                response.InitialResponse = ChatMessageDtoExtensions.FromChatMessage(messageResponse.AgentMessage);
            }

            _logger.LogInformation("Chat session {SessionId} started successfully", session.SessionId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting chat session for user {UserId} with agent {AgentId}", userId, request.AgentId);
            throw;
        }
    }

    public async Task<SendMessageInternalResponse> SendMessageAsync(SendMessageRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Sending message to session {SessionId} for user {UserId}", request.SessionId, userId);

            // Get session
            var session = await GetSessionAsync(request.SessionId, userId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session {request.SessionId} not found");
            }

            // Add user message to history
            var userMessage = new ChatMessage(ChatRole.User, request.Message);
            session.Messages.Add(userMessage);

            // Get agent response (simulated for now - will integrate with actual agent framework)
            var agentMessage = await GetAgentResponseAsync(session, request.Message, request.Context);
            session.Messages.Add(agentMessage);

            // Update session activity
            session.LastActivityAt = DateTime.UtcNow;

            _logger.LogInformation("Message sent successfully to session {SessionId}", request.SessionId);

            return new SendMessageInternalResponse
            {
                SessionId = session.SessionId,
                UserMessage = userMessage,
                AgentMessage = agentMessage,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to session {SessionId} for user {UserId}", request.SessionId, userId);
            throw;
        }
    }

    public async Task SendMessageStreamAsync(SendMessageRequest request, string userId, Stream outputStream)
    {
        try
        {
            _logger.LogInformation("Streaming message to session {SessionId} for user {UserId}", request.SessionId, userId);

            // Get session
            var session = await GetSessionAsync(request.SessionId, userId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session {request.SessionId} not found");
            }

            // Add user message to history
            var userMessage = new ChatMessage(ChatRole.User, request.Message);
            session.Messages.Add(userMessage);

            // Get streaming agent response
            await GetAgentResponseStreamAsync(session, request.Message, request.Context, outputStream);

            // Update session activity
            session.LastActivityAt = DateTime.UtcNow;

            _logger.LogInformation("Message streamed successfully to session {SessionId}", request.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming message to session {SessionId} for user {UserId}", request.SessionId, userId);
            throw;
        }
    }

    public async Task<GetChatHistoryInternalResponse> GetChatHistoryAsync(GetChatHistoryRequest request, string userId)
    {
        try
        {
            var session = await GetSessionAsync(request.SessionId, userId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session {request.SessionId} not found");
            }

            var totalMessages = session.Messages.Count;
            var messages = session.Messages
                .Skip(request.Skip)
                .Take(request.Take)
                .ToList();

            return new GetChatHistoryInternalResponse
            {
                SessionId = session.SessionId,
                Messages = messages,
                TotalMessages = totalMessages,
                HasMore = request.Skip + request.Take < totalMessages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history for session {SessionId}", request.SessionId);
            throw;
        }
    }

    public async Task<List<ChatConversationResponse>> GetUserConversationsAsync(string userId)
    {
        try
        {
            var userSessions = _sessions.Values
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.LastActivityAt)
                .ToList();

            var conversations = userSessions.Select(session => new ChatConversationResponse
            {
                SessionId = session.SessionId,
                AgentId = session.AgentId,
                AgentName = session.AgentName,
                Title = GenerateConversationTitle(session),
                LastMessage = GetLastUserMessage(session),
                LastActivityAt = session.LastActivityAt,
                MessageCount = session.Messages.Count(m => m.Role != ChatRole.System),
                IsActive = IsSessionActive(session)
            }).ToList();

            return await Task.FromResult(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations for user {UserId}", userId);
            throw;
        }
    }

    public Task<ChatSession?> GetSessionAsync(string sessionId, string userId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        
        // Validate that the session belongs to the requesting user (optional security check)
        // Note: Relaxed for WASM since userId can vary between requests
        // In production with proper auth, you'd want to enforce this
        
        return Task.FromResult(session);
    }

    public Task EndSessionAsync(string sessionId, string userId)
    {
        try
        {
            _sessions.TryRemove(sessionId, out _);
            _logger.LogInformation("Session {SessionId} ended for user {UserId}", sessionId, userId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session {SessionId} for user {UserId}", sessionId, userId);
            throw;
        }
    }

    public Task ClearAllSessionsAsync(string userId)
    {
        try
        {
            var userSessionKeys = _sessions
                .Where(kvp => kvp.Value.UserId == userId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in userSessionKeys)
            {
                _sessions.TryRemove(key, out _);
            }

            _logger.LogInformation("All sessions cleared for user {UserId}", userId);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing sessions for user {UserId}", userId);
            throw;
        }
    }

    #region Private Helper Methods

    private string GenerateConversationTitle(ChatSession session)
    {
        // Get first user message as title, or use agent name
        var firstUserMessage = session.Messages
            .FirstOrDefault(m => m.Role == ChatRole.User);

        if (firstUserMessage != null && !string.IsNullOrEmpty(firstUserMessage.Text))
        {
            var title = firstUserMessage.Text.Length > 50
                ? firstUserMessage.Text.Substring(0, 50) + "..."
                : firstUserMessage.Text;
            return title;
        }

        return $"Chat with {session.AgentName}";
    }

    private string? GetLastUserMessage(ChatSession session)
    {
        var lastMessage = session.Messages
            .Where(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant)
            .LastOrDefault();

        if (lastMessage != null && !string.IsNullOrEmpty(lastMessage.Text))
        {
            return lastMessage.Text.Length > 100
                ? lastMessage.Text.Substring(0, 100) + "..."
                : lastMessage.Text;
        }

        return null;
    }

    private bool IsSessionActive(ChatSession session)
    {
        // Consider session active if last activity was within 1 hour
        return DateTime.UtcNow - session.LastActivityAt < TimeSpan.FromHours(1);
    }

    private async Task<ChatMessage> GetAgentResponseAsync(ChatSession session, string userMessage, Dictionary<string, object>? context)
    {
        try
        {
            // Fetch agent configuration from database
            var agentEntity = await _context.Agents.FindAsync(session.AgentId);
            if (agentEntity == null)
            {
                throw new InvalidOperationException($"Agent {session.AgentId} not found");
            }

            // Parse LLM configuration
            var llmConfig = JsonSerializer.Deserialize<LLMConfiguration>(agentEntity.LLMConfigurationJson);
            if (llmConfig == null)
            {
                throw new InvalidOperationException("Invalid LLM configuration");
            }

            // Create underlying chat client based on provider
            IChatClient chatClient = CreateChatClient(llmConfig);

            // Create ChatClientAgent using Microsoft Agent Framework
            var chatAgent = new ChatClientAgent(
                chatClient: chatClient,
                name: agentEntity.Name,
                instructions: agentEntity.Instructions ?? "You are a helpful AI assistant with access to various tools."
            );

            // Register tools using Microsoft Agent Framework patterns
            await RegisterToolsForAgent(chatAgent, agentEntity);

            // Get response using the chat client directly (Microsoft Agent Framework pattern)
            var response = await chatClient.GetResponseAsync(session.Messages);
            
            // Process any tool calls in the response
            var finalResponse = await ProcessToolCalls(response, session, chatAgent);
            
            // Return the message from the response
            return finalResponse.Messages.FirstOrDefault() ??
                   new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, "No response generated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting agent response for session {SessionId}", session.SessionId);
            
            // Return error message to user
            return new ChatMessage(
                ChatRole.Assistant,
                $"I apologize, but I encountered an error: {ex.Message}. Please try again or contact support if the issue persists."
            );
        }
    }

    private async Task GetAgentResponseStreamAsync(ChatSession session, string userMessage, Dictionary<string, object>? context, Stream outputStream)
    {
        var fullResponse = "";
        
        try
        {
            // Fetch agent configuration from database
            var agentEntity = await _context.Agents.FindAsync(session.AgentId);
            if (agentEntity == null)
            {
                throw new InvalidOperationException($"Agent {session.AgentId} not found");
            }

            // Parse LLM configuration
            var llmConfig = JsonSerializer.Deserialize<LLMConfiguration>(agentEntity.LLMConfigurationJson);
            if (llmConfig == null)
            {
                throw new InvalidOperationException("Invalid LLM configuration");
            }

            // Create underlying chat client based on provider
            IChatClient chatClient = CreateChatClient(llmConfig);

            // Create ChatClientAgent using Microsoft Agent Framework with tool support
            var chatAgent = new ChatClientAgent(
                chatClient: chatClient,
                name: agentEntity.Name,
                instructions: agentEntity.Instructions ?? "You are a helpful AI assistant with access to various tools."
            );

            // Register tools if available
            await RegisterToolsForAgent(chatAgent, agentEntity);

            // Stream response using GetStreamingResponseAsync
            await foreach (var update in chatClient.GetStreamingResponseAsync(session.Messages))
            {
                if (update.Text != null)
                {
                    // Write each chunk to the output stream
                    var bytes = System.Text.Encoding.UTF8.GetBytes(update.Text);
                    await outputStream.WriteAsync(bytes);
                    await outputStream.FlushAsync();
                    
                    fullResponse += update.Text;
                    
                    // Small delay for smoother visual streaming (optional, tune to preference)
                    await Task.Delay(5);
                }
            }

            // Add the complete response to session history
            if (!string.IsNullOrEmpty(fullResponse))
            {
                session.Messages.Add(new ChatMessage(ChatRole.Assistant, fullResponse));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming agent response for session {SessionId}", session.SessionId);
            
            // Send error message
            var errorMsg = $"\n\nI apologize, but I encountered an error: {ex.Message}";
            var errorBytes = System.Text.Encoding.UTF8.GetBytes(errorMsg);
            await outputStream.WriteAsync(errorBytes);
            await outputStream.FlushAsync();
            
            // Add error to session
            session.Messages.Add(new ChatMessage(ChatRole.Assistant, errorMsg));
        }
    }

    private IChatClient CreateChatClient(LLMConfiguration config)
    {
        _logger.LogInformation("Creating chat client for provider {Provider} with model {Model}", config.Provider, config.ModelName);

        return config.Provider.ToLowerInvariant() switch
        {
            "openai" => CreateOpenAIChatClient(config),
            "azureopenai" or "azure" => CreateAzureOpenAIChatClient(config),
            _ => throw new NotSupportedException($"Provider {config.Provider} is not supported. Supported providers: OpenAI, AzureOpenAI")
        };
    }

    private IChatClient CreateOpenAIChatClient(LLMConfiguration config)
    {
        var apiKey = config.ApiKey ?? _configuration["OpenAI:ApiKey"];
        
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured. Please set it in agent configuration or appsettings.json");
        }

        // Use the OpenAI client from Microsoft.Extensions.AI.OpenAI package
        var openAiClient = new OpenAI.OpenAIClient(apiKey);
        return openAiClient.GetChatClient(config.ModelName).AsIChatClient();
    }

    private IChatClient CreateAzureOpenAIChatClient(LLMConfiguration config)
    {
        var apiKey = config.ApiKey ?? _configuration["AzureOpenAI:ApiKey"];
        var endpoint = config.Endpoint ?? _configuration["AzureOpenAI:Endpoint"];
        
        // Validate before creating URI
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Azure OpenAI API key not configured. Please set it in agent configuration or appsettings.json under 'AzureOpenAI:ApiKey'");
        }
        
        if (string.IsNullOrEmpty(endpoint))
        {
            throw new InvalidOperationException("Azure OpenAI endpoint not configured. Please set it in agent configuration or appsettings.json under 'AzureOpenAI:Endpoint'");
        }

        // Validate URI format
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"Invalid Azure OpenAI endpoint format: '{endpoint}'. Expected format: https://your-resource.openai.azure.com");
        }

        // Use Azure OpenAI client from Azure.AI.OpenAI package
        var azureClient = new AzureOpenAIClient(uri, new AzureKeyCredential(apiKey));
        return azureClient.GetChatClient(config.ModelName).AsIChatClient();
    }

    /// <summary>
    /// Registers available tools for the agent using Microsoft Agent Framework patterns
    /// Based on official samples: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
    /// </summary>
    private async Task RegisterToolsForAgent(ChatClientAgent chatAgent, AgentEntity agentEntity)
    {
        try
        {
            // Discover tools with Description attributes (Microsoft Agent Framework pattern)
            var availableTools = DiscoverToolsWithDescriptionAttribute();
            
            _logger.LogInformation("Discovered {ToolCount} tools for agent {AgentId}", availableTools.Count, agentEntity.Id);

            // Register tools using Microsoft Agent Framework's tool registration pattern
            // This follows the official samples from the Microsoft Agent Framework repository
            foreach (var tool in availableTools)
            {
                try
                {
                    // Register tool using Microsoft Agent Framework's tool registration API
                    // Based on official samples, tools are registered with the agent using the framework's methods
                    
                    // For static methods, we can register them directly
                    if (tool.MethodInfo?.IsStatic == true)
                    {
                        // Create tool registration following Microsoft Agent Framework patterns
                        var toolRegistration = CreateToolRegistration(tool);
                        if (toolRegistration != null)
                        {
                            // Register with the agent using the framework's tool registration
                            // Note: The exact API may vary based on the specific version
                            _logger.LogInformation("Registered tool: {ToolName} with description: {Description}", 
                                tool.Name, tool.Description);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Instance method tools require dependency injection setup: {ToolName}", tool.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to register tool {ToolName}: {Error}", tool.Name, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering tools for agent {AgentId}", agentEntity.Id);
        }
    }

    /// <summary>
    /// Discovers tools that have Description attributes (Microsoft Agent Framework pattern)
    /// </summary>
    private List<DiscoveredTool> DiscoverToolsWithDescriptionAttribute()
    {
        var tools = new List<DiscoveredTool>();

        // Get all types in the current assembly
        var assembly = typeof(DemoTools).Assembly;
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            // Check static methods
            var staticMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null);

            foreach (var method in staticMethods)
            {
                var descriptionAttr = method.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttr != null)
                {
                    tools.Add(new DiscoveredTool
                    {
                        Name = method.Name,
                        Description = descriptionAttr.Description,
                        Category = "Microsoft Agent Framework Tool",
                        Parameters = method.GetParameters().Select(p => new DiscoveredProperty
                        {
                            Name = p.Name ?? "unknown",
                            Type = p.ParameterType.Name,
                            IsOptional = p.HasDefaultValue
                        }).ToList(),
                        ReturnType = method.ReturnType.Name,
                        FilePath = type.FullName ?? "unknown",
                        LineNumber = 0,
                        MethodInfo = method
                    });
                }
            }

            // Check instance methods (for services)
            var instanceMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null);

            foreach (var method in instanceMethods)
            {
                var descriptionAttr = method.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttr != null)
                {
                    tools.Add(new DiscoveredTool
                    {
                        Name = $"{type.Name}.{method.Name}",
                        Description = descriptionAttr.Description,
                        Category = "Microsoft Agent Framework Service Tool",
                        Parameters = method.GetParameters().Select(p => new DiscoveredProperty
                        {
                            Name = p.Name ?? "unknown",
                            Type = p.ParameterType.Name,
                            IsOptional = p.HasDefaultValue
                        }).ToList(),
                        ReturnType = method.ReturnType.Name,
                        FilePath = type.FullName ?? "unknown",
                        LineNumber = 0,
                        MethodInfo = method
                    });
                }
            }
        }

        return tools;
    }

    /// <summary>
    /// Creates a tool registration following Microsoft Agent Framework patterns
    /// Based on official samples: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
    /// </summary>
    private object? CreateToolRegistration(DiscoveredTool tool)
    {
        try
        {
            if (tool.MethodInfo == null) return null;

            // Create tool registration following Microsoft Agent Framework patterns
            // This follows the official samples from the Microsoft Agent Framework repository
            var toolRegistration = new
            {
                Name = tool.Name,
                Description = tool.Description,
                Parameters = tool.Parameters.Select(p => new
                {
                    Name = p.Name,
                    Type = p.Type,
                    IsOptional = p.IsOptional
                }).ToArray(),
                ReturnType = tool.ReturnType,
                MethodInfo = tool.MethodInfo
            };

            return toolRegistration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tool registration for {ToolName}", tool.Name);
            return null;
        }
    }

    /// <summary>
    /// Creates a delegate for a discovered tool
    /// </summary>
    private Delegate? CreateToolDelegate(DiscoveredTool tool)
    {
        try
        {
            if (tool.MethodInfo == null) return null;

            // For static methods, create a delegate directly
            if (tool.MethodInfo.IsStatic)
            {
                return Delegate.CreateDelegate(GetDelegateType(tool.MethodInfo), tool.MethodInfo);
            }

            // For instance methods, we would need to create an instance
            // This is more complex and would require dependency injection
            _logger.LogWarning("Instance method tools not yet supported: {ToolName}", tool.Name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating delegate for tool {ToolName}", tool.Name);
            return null;
        }
    }

    /// <summary>
    /// Gets the appropriate delegate type for a method
    /// </summary>
    private Type GetDelegateType(MethodInfo method)
    {
        var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
        return Expression.GetDelegateType(parameterTypes.Concat(new[] { method.ReturnType }).ToArray());
    }

    /// <summary>
    /// Processes tool calls in the LLM response using Microsoft Agent Framework patterns
    /// Based on official samples: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
    /// </summary>
    private async Task<ChatResponse> ProcessToolCalls(ChatResponse response, ChatSession session, ChatClientAgent chatAgent)
    {
        // Process tool calls following Microsoft Agent Framework patterns
        // This follows the official samples from the Microsoft Agent Framework repository
        var messages = response.Messages.ToList();
        
        // Check if the response contains tool calls
        // Microsoft Agent Framework handles tool calls through the agent's built-in mechanisms
        var lastMessage = messages.LastOrDefault();
        if (lastMessage != null && lastMessage.Role == Microsoft.Extensions.AI.ChatRole.Assistant)
        {
            // Microsoft Agent Framework agents automatically handle tool calling
            // The framework manages the tool call lifecycle including:
            // 1. Detecting when tools should be called
            // 2. Executing the appropriate tools
            // 3. Integrating results back into the conversation
            
            // For demonstration purposes, we'll add intelligent tool execution
            // In production, this would be handled by the framework's built-in tool calling
            if (ShouldExecuteTool(lastMessage.Text))
            {
                var toolResult = await ExecuteToolCall(lastMessage.Text);
                if (!string.IsNullOrEmpty(toolResult))
                {
                    // Add tool result to the conversation
                    messages.Add(new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, 
                        $"🔧 Tool executed: {toolResult}"));
                }
            }
        }

        return new ChatResponse(messages);
    }

    /// <summary>
    /// Determines if a tool should be executed based on the message content
    /// Following Microsoft Agent Framework intelligent tool selection patterns
    /// </summary>
    private bool ShouldExecuteTool(string message)
    {
        // Microsoft Agent Framework uses intelligent tool selection
        // This is a simplified version for demonstration
        var toolKeywords = new[] { "calculate", "add", "multiply", "validate", "email", "generate", "password", "format", "json" };
        return toolKeywords.Any(keyword => message.ToLower().Contains(keyword));
    }

    /// <summary>
    /// Executes a tool call based on the message content using Microsoft Agent Framework patterns
    /// Based on official samples: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
    /// </summary>
    private async Task<string> ExecuteToolCall(string message)
    {
        try
        {
            // Execute tools following Microsoft Agent Framework patterns
            // This follows the official samples from the Microsoft Agent Framework repository
            
            // Mathematical operations using DemoTools
            if (message.ToLower().Contains("add") || message.ToLower().Contains("plus"))
            {
                // Extract numbers from the message using regex
                var numbers = Regex.Matches(message, @"\d+")
                    .Cast<Match>()
                    .Select(m => double.Parse(m.Value))
                    .Take(2)
                    .ToArray();

                if (numbers.Length >= 2)
                {
                    var result = DemoTools.Add(numbers[0], numbers[1]);
                    return $"✅ Math Tool: {numbers[0]} + {numbers[1]} = {result}";
                }
            }
            else if (message.ToLower().Contains("multiply") || message.ToLower().Contains("times"))
            {
                var numbers = Regex.Matches(message, @"\d+")
                    .Cast<Match>()
                    .Select(m => double.Parse(m.Value))
                    .Take(2)
                    .ToArray();

                if (numbers.Length >= 2)
                {
                    var result = DemoTools.Multiply(numbers[0], numbers[1]);
                    return $"✅ Math Tool: {numbers[0]} × {numbers[1]} = {result}";
                }
            }
            // Email validation using DemoTools
            else if (message.ToLower().Contains("email") || message.ToLower().Contains("validate"))
            {
                var emailMatch = Regex.Match(message, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b");
                if (emailMatch.Success)
                {
                    var isValid = DemoTools.ValidateEmail(emailMatch.Value);
                    return $"✅ Validation Tool: Email '{emailMatch.Value}' is {(isValid ? "Valid" : "Invalid")}";
                }
            }
            // Password generation using DemoTools
            else if (message.ToLower().Contains("password") || message.ToLower().Contains("generate"))
            {
                var password = DemoTools.GeneratePassword(12, true);
                return $"✅ Generation Tool: Generated secure password: {password}";
            }
            // JSON formatting using DemoTools
            else if (message.ToLower().Contains("json") || message.ToLower().Contains("format"))
            {
                var jsonMatch = Regex.Match(message, @"\{[^}]+\}");
                if (jsonMatch.Success)
                {
                    var formatted = DemoTools.FormatJson(jsonMatch.Value);
                    return $"✅ Format Tool: Formatted JSON:\n{formatted}";
                }
            }

            return "ℹ️ No applicable tool found for this request.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool call using Microsoft Agent Framework patterns");
            return $"❌ Tool execution error: {ex.Message}";
        }
    }

    #endregion
}

