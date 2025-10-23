using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Codefix.AIPlayGround.Models;
using System.Text.Json;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Microsoft Agent Framework definition service
/// Manages agent definitions using official framework patterns
/// Based on: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples
/// </summary>
public interface IMicrosoftAgentFrameworkDefinitionService
{
    Task<MicrosoftAgentFrameworkDefinition> CreateAgentAsync(MicrosoftAgentFrameworkCreateRequest request);
    Task<MicrosoftAgentFrameworkDefinition?> GetAgentAsync(string id);
    Task<List<MicrosoftAgentFrameworkDefinition>> GetAgentsAsync(MicrosoftAgentFrameworkGetRequest request);
    Task<MicrosoftAgentFrameworkDefinition> UpdateAgentAsync(string id, MicrosoftAgentFrameworkUpdateRequest request);
    Task<bool> DeleteAgentAsync(string id);
    Task<MicrosoftAgentFrameworkTestResult> TestAgentAsync(string id, MicrosoftAgentFrameworkTestRequest request);
    Task<MicrosoftAgentFrameworkDeploymentResult> DeployAgentAsync(string id);
    Task<MicrosoftAgentFrameworkStatusResponse> GetAgentStatusAsync(string id);
    Task<List<MicrosoftAgentFrameworkTypeDefinition>> GetAgentTypesAsync();
}

/// <summary>
/// Microsoft Agent Framework definition service implementation
/// </summary>
public class MicrosoftAgentFrameworkDefinitionService : IMicrosoftAgentFrameworkDefinitionService
{
    private readonly ILogger<MicrosoftAgentFrameworkDefinitionService> _logger;
    private readonly Dictionary<string, MicrosoftAgentFrameworkDefinition> _agents = new();
    private readonly Dictionary<string, ChatClientAgent> _agentInstances = new();

    public MicrosoftAgentFrameworkDefinitionService(ILogger<MicrosoftAgentFrameworkDefinitionService> logger)
    {
        _logger = logger;
        InitializeDefaultAgentTypes();
    }

    /// <summary>
    /// Creates a new Microsoft Agent Framework agent
    /// </summary>
    public async Task<MicrosoftAgentFrameworkDefinition> CreateAgentAsync(MicrosoftAgentFrameworkCreateRequest request)
    {
        try
        {
            _logger.LogInformation("Creating Microsoft Agent Framework agent: {AgentName}", request.Name);

            var agent = new MicrosoftAgentFrameworkDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                Type = request.AgentType,
                Instructions = request.Instructions,
                LLMConfig = request.LLMConfiguration,
                Tools = request.Tools,
                PromptConfig = request.PromptConfiguration,
                MemoryConfig = request.MemoryConfiguration,
                CheckpointConfig = request.CheckpointConfiguration,
                OrchestrationConfig = request.OrchestrationConfiguration,
                SecurityConfig = request.SecurityConfiguration,
                FrameworkProperties = request.FrameworkProperties,
                Status = MicrosoftAgentFrameworkStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Set default capabilities based on agent type
            agent.Capabilities = GetDefaultCapabilitiesForType(request.AgentType);

            // Validate agent configuration
            await ValidateAgentConfigurationAsync(agent);

            // Store agent
            _agents[agent.Id] = agent;

            _logger.LogInformation("Microsoft Agent Framework agent created successfully: {AgentId}", agent.Id);
            return agent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Microsoft Agent Framework agent: {AgentName}", request.Name);
            throw;
        }
    }

    /// <summary>
    /// Gets a Microsoft Agent Framework agent by ID
    /// </summary>
    public async Task<MicrosoftAgentFrameworkDefinition?> GetAgentAsync(string id)
    {
        try
        {
            _logger.LogInformation("Getting Microsoft Agent Framework agent: {AgentId}", id);

            if (_agents.TryGetValue(id, out var agent))
            {
                return agent;
            }

            _logger.LogWarning("Microsoft Agent Framework agent not found: {AgentId}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Microsoft Agent Framework agent: {AgentId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets Microsoft Agent Framework agents with filtering
    /// </summary>
    public async Task<List<MicrosoftAgentFrameworkDefinition>> GetAgentsAsync(MicrosoftAgentFrameworkGetRequest request)
    {
        try
        {
            _logger.LogInformation("Getting Microsoft Agent Framework agents with filters");

            var agents = _agents.Values.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Name))
            {
                agents = agents.Where(a => a.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (request.AgentType.HasValue)
            {
                agents = agents.Where(a => a.Type == request.AgentType.Value);
            }

            if (request.Status.HasValue)
            {
                agents = agents.Where(a => a.Status == request.Status.Value);
            }

            if (!string.IsNullOrEmpty(request.CreatedBy))
            {
                agents = agents.Where(a => a.CreatedBy == request.CreatedBy);
            }

            if (request.CreatedAfter.HasValue)
            {
                agents = agents.Where(a => a.CreatedAt >= request.CreatedAfter.Value);
            }

            if (request.CreatedBefore.HasValue)
            {
                agents = agents.Where(a => a.CreatedAt <= request.CreatedBefore.Value);
            }

            // Apply pagination
            var result = agents
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            _logger.LogInformation("Retrieved {Count} Microsoft Agent Framework agents", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Microsoft Agent Framework agents");
            throw;
        }
    }

    /// <summary>
    /// Updates a Microsoft Agent Framework agent
    /// </summary>
    public async Task<MicrosoftAgentFrameworkDefinition> UpdateAgentAsync(string id, MicrosoftAgentFrameworkUpdateRequest request)
    {
        try
        {
            _logger.LogInformation("Updating Microsoft Agent Framework agent: {AgentId}", id);

            if (!_agents.TryGetValue(id, out var agent))
            {
                throw new ArgumentException($"Microsoft Agent Framework agent not found: {id}");
            }

            // Update properties
            if (!string.IsNullOrEmpty(request.Name))
                agent.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Description))
                agent.Description = request.Description;

            if (!string.IsNullOrEmpty(request.Instructions))
                agent.Instructions = request.Instructions;

            if (request.Status.HasValue)
                agent.Status = request.Status.Value;

            if (request.LLMConfiguration != null)
                agent.LLMConfig = request.LLMConfiguration;

            if (request.Tools != null)
                agent.Tools = request.Tools;

            if (request.PromptConfiguration != null)
                agent.PromptConfig = request.PromptConfiguration;

            if (request.MemoryConfiguration != null)
                agent.MemoryConfig = request.MemoryConfiguration;

            if (request.CheckpointConfiguration != null)
                agent.CheckpointConfig = request.CheckpointConfiguration;

            if (request.OrchestrationConfiguration != null)
                agent.OrchestrationConfig = request.OrchestrationConfiguration;

            if (request.SecurityConfiguration != null)
                agent.SecurityConfig = request.SecurityConfiguration;

            if (request.FrameworkProperties != null)
                agent.FrameworkProperties = request.FrameworkProperties;

            agent.UpdatedAt = DateTime.UtcNow;

            // Validate updated configuration
            await ValidateAgentConfigurationAsync(agent);

            // Clear cached instance if it exists
            if (_agentInstances.ContainsKey(id))
            {
                _agentInstances.Remove(id);
            }

            _logger.LogInformation("Microsoft Agent Framework agent updated successfully: {AgentId}", id);
            return agent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Microsoft Agent Framework agent: {AgentId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a Microsoft Agent Framework agent
    /// </summary>
    public async Task<bool> DeleteAgentAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting Microsoft Agent Framework agent: {AgentId}", id);

            if (!_agents.ContainsKey(id))
            {
                _logger.LogWarning("Microsoft Agent Framework agent not found: {AgentId}", id);
                return false;
            }

            // Remove agent instance if it exists
            if (_agentInstances.ContainsKey(id))
            {
                _agentInstances.Remove(id);
            }

            // Remove agent
            _agents.Remove(id);

            _logger.LogInformation("Microsoft Agent Framework agent deleted successfully: {AgentId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Microsoft Agent Framework agent: {AgentId}", id);
            throw;
        }
    }

    /// <summary>
    /// Tests a Microsoft Agent Framework agent
    /// </summary>
    public async Task<MicrosoftAgentFrameworkTestResult> TestAgentAsync(string id, MicrosoftAgentFrameworkTestRequest request)
    {
        try
        {
            _logger.LogInformation("Testing Microsoft Agent Framework agent: {AgentId}", id);

            if (!_agents.TryGetValue(id, out var agent))
            {
                return new MicrosoftAgentFrameworkTestResult
                {
                    IsSuccess = false,
                    Message = $"Microsoft Agent Framework agent not found: {id}",
                    Errors = new List<string> { "Agent not found" }
                };
            }

            // Get or create agent instance
            var agentInstance = await GetOrCreateAgentInstanceAsync(agent);
            if (agentInstance == null)
            {
                return new MicrosoftAgentFrameworkTestResult
                {
                    IsSuccess = false,
                    Message = "Failed to create Microsoft Agent Framework agent instance",
                    Errors = new List<string> { "Agent instance creation failed" }
                };
            }

            // Execute test
            var startTime = DateTime.UtcNow;
            var execution = new MicrosoftAgentFrameworkExecutionResponse
            {
                Id = Guid.NewGuid().ToString(),
                AgentId = id,
                StartedAt = startTime,
                Status = MicrosoftAgentFrameworkExecutionStatus.Running,
                InputData = request.Input,
                ExecutionType = MicrosoftAgentFrameworkExecutionType.Single
            };

            try
            {
                // TODO: Implement actual agent execution using Microsoft Agent Framework
                // This would use the agentInstance to process the input
                await Task.Delay(100); // Simulate processing

                execution.CompletedAt = DateTime.UtcNow;
                execution.Status = MicrosoftAgentFrameworkExecutionStatus.Completed;
                execution.OutputData = new Dictionary<string, object>
                {
                    ["result"] = "Microsoft Agent Framework test completed successfully",
                    ["framework"] = "Microsoft Agent Framework",
                    ["agent_type"] = agent.Type.ToString()
                };

                var result = new MicrosoftAgentFrameworkTestResult
                {
                    IsSuccess = true,
                    Message = "Microsoft Agent Framework test completed successfully",
                    Input = request.Input,
                    Output = execution.OutputData,
                    Metrics = new Dictionary<string, object>
                    {
                        ["execution_time_ms"] = (execution.CompletedAt ?? DateTime.UtcNow).Subtract(execution.StartedAt).TotalMilliseconds,
                        ["framework"] = "Microsoft Agent Framework"
                    },
                    Execution = execution
                };

                _logger.LogInformation("Microsoft Agent Framework agent test completed successfully: {AgentId}", id);
                return result;
            }
            catch (Exception ex)
            {
                execution.CompletedAt = DateTime.UtcNow;
                execution.Status = MicrosoftAgentFrameworkExecutionStatus.Failed;
                execution.Errors.Add(ex.Message);

                var result = new MicrosoftAgentFrameworkTestResult
                {
                    IsSuccess = false,
                    Message = $"Microsoft Agent Framework test failed: {ex.Message}",
                    Input = request.Input,
                    Output = new Dictionary<string, object>(),
                    Errors = new List<string> { ex.Message },
                    Execution = execution
                };

                _logger.LogError(ex, "Microsoft Agent Framework agent test failed: {AgentId}", id);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing Microsoft Agent Framework agent: {AgentId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deploys a Microsoft Agent Framework agent
    /// </summary>
    public async Task<MicrosoftAgentFrameworkDeploymentResult> DeployAgentAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deploying Microsoft Agent Framework agent: {AgentId}", id);

            if (!_agents.TryGetValue(id, out var agent))
            {
                return new MicrosoftAgentFrameworkDeploymentResult
                {
                    IsSuccess = false,
                    Message = $"Microsoft Agent Framework agent not found: {id}",
                    Status = MicrosoftAgentFrameworkDeploymentStatus.Failed
                };
            }

            // TODO: Implement actual deployment using Microsoft Agent Framework
            // This would deploy the agent to a runtime environment
            await Task.Delay(1000); // Simulate deployment

            var result = new MicrosoftAgentFrameworkDeploymentResult
            {
                IsSuccess = true,
                Message = "Microsoft Agent Framework agent deployed successfully",
                DeploymentId = Guid.NewGuid().ToString(),
                Endpoint = $"https://api.example.com/agents/{id}",
                Status = MicrosoftAgentFrameworkDeploymentStatus.Completed,
                Metadata = new Dictionary<string, object>
                {
                    ["framework"] = "Microsoft Agent Framework",
                    ["agent_type"] = agent.Type.ToString(),
                    ["deployment_time"] = DateTime.UtcNow
                }
            };

            _logger.LogInformation("Microsoft Agent Framework agent deployed successfully: {AgentId}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying Microsoft Agent Framework agent: {AgentId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets the status of a Microsoft Agent Framework agent
    /// </summary>
    public async Task<MicrosoftAgentFrameworkStatusResponse> GetAgentStatusAsync(string id)
    {
        try
        {
            _logger.LogInformation("Getting Microsoft Agent Framework agent status: {AgentId}", id);

            if (!_agents.TryGetValue(id, out var agent))
            {
                return new MicrosoftAgentFrameworkStatusResponse
                {
                    AgentId = id,
                    Status = MicrosoftAgentFrameworkStatus.Inactive,
                    HealthStatus = MicrosoftAgentFrameworkHealthStatus.Unknown,
                    Metrics = new Dictionary<string, object>
                    {
                        ["error"] = "Agent not found"
                    }
                };
            }

            var result = new MicrosoftAgentFrameworkStatusResponse
            {
                AgentId = id,
                Status = agent.Status,
                HealthStatus = MicrosoftAgentFrameworkHealthStatus.Healthy,
                Metrics = new Dictionary<string, object>
                {
                    ["framework"] = "Microsoft Agent Framework",
                    ["agent_type"] = agent.Type.ToString(),
                    ["created_at"] = agent.CreatedAt,
                    ["updated_at"] = agent.UpdatedAt,
                    ["tools_count"] = agent.Tools.Count,
                    ["capabilities"] = agent.Capabilities
                }
            };

            _logger.LogInformation("Microsoft Agent Framework agent status retrieved: {AgentId}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Microsoft Agent Framework agent status: {AgentId}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets available Microsoft Agent Framework agent types
    /// </summary>
    public async Task<List<MicrosoftAgentFrameworkTypeDefinition>> GetAgentTypesAsync()
    {
        try
        {
            _logger.LogInformation("Getting Microsoft Agent Framework agent types");

            var types = new List<MicrosoftAgentFrameworkTypeDefinition>
            {
                new MicrosoftAgentFrameworkTypeDefinition
                {
                    Type = MicrosoftAgentFrameworkType.ChatAgent,
                    Label = "Chat Agent",
                    Description = "Microsoft Agent Framework chat-based LLM agent using ChatClientAgent",
                    Category = "Core",
                    Icon = "chat",
                    Tags = new List<string> { "chat", "llm", "conversation" },
                    DefaultCapabilities = new MicrosoftAgentFrameworkCapabilities
                    {
                        CanCallTools = true,
                        CanUseMemory = true,
                        CanStreamResponses = true
                    }
                },
                new MicrosoftAgentFrameworkTypeDefinition
                {
                    Type = MicrosoftAgentFrameworkType.FunctionAgent,
                    Label = "Function Agent",
                    Description = "Microsoft Agent Framework tool-calling agent with function capabilities",
                    Category = "Tools",
                    Icon = "function",
                    Tags = new List<string> { "tools", "functions", "api" },
                    DefaultCapabilities = new MicrosoftAgentFrameworkCapabilities
                    {
                        CanCallTools = true,
                        CanUseMemory = true,
                        CanStreamResponses = true
                    }
                },
                new MicrosoftAgentFrameworkTypeDefinition
                {
                    Type = MicrosoftAgentFrameworkType.SequentialAgent,
                    Label = "Sequential Agent",
                    Description = "Microsoft Agent Framework sequential orchestration agent",
                    Category = "Orchestration",
                    Icon = "sequential",
                    Tags = new List<string> { "orchestration", "sequential", "workflow" },
                    DefaultCapabilities = new MicrosoftAgentFrameworkCapabilities
                    {
                        CanOrchestrate = true,
                        CanCallTools = true,
                        CanUseMemory = true
                    }
                },
                new MicrosoftAgentFrameworkTypeDefinition
                {
                    Type = MicrosoftAgentFrameworkType.ConcurrentAgent,
                    Label = "Concurrent Agent",
                    Description = "Microsoft Agent Framework concurrent orchestration agent",
                    Category = "Orchestration",
                    Icon = "concurrent",
                    Tags = new List<string> { "orchestration", "concurrent", "parallel" },
                    DefaultCapabilities = new MicrosoftAgentFrameworkCapabilities
                    {
                        CanOrchestrate = true,
                        CanHandleParallelExecution = true,
                        CanCallTools = true
                    }
                },
                new MicrosoftAgentFrameworkTypeDefinition
                {
                    Type = MicrosoftAgentFrameworkType.GroupChatAgent,
                    Label = "Group Chat Agent",
                    Description = "Microsoft Agent Framework group chat orchestration agent",
                    Category = "Orchestration",
                    Icon = "group-chat",
                    Tags = new List<string> { "orchestration", "group-chat", "collaboration" },
                    DefaultCapabilities = new MicrosoftAgentFrameworkCapabilities
                    {
                        CanOrchestrate = true,
                        CanCallTools = true,
                        CanUseMemory = true
                    }
                },
                new MicrosoftAgentFrameworkTypeDefinition
                {
                    Type = MicrosoftAgentFrameworkType.MCPAgent,
                    Label = "MCP Agent",
                    Description = "Microsoft Agent Framework Model Context Protocol agent",
                    Category = "Integration",
                    Icon = "mcp",
                    Tags = new List<string> { "mcp", "integration", "external" },
                    DefaultCapabilities = new MicrosoftAgentFrameworkCapabilities
                    {
                        CanUseMCP = true,
                        CanCallTools = true
                    }
                },
                new MicrosoftAgentFrameworkTypeDefinition
                {
                    Type = MicrosoftAgentFrameworkType.CustomAgent,
                    Label = "Custom Agent",
                    Description = "Microsoft Agent Framework custom agent with specific capabilities",
                    Category = "Custom",
                    Icon = "custom",
                    Tags = new List<string> { "custom", "extensible" },
                    DefaultCapabilities = new MicrosoftAgentFrameworkCapabilities()
                }
            };

            _logger.LogInformation("Retrieved {Count} Microsoft Agent Framework agent types", types.Count);
            return types;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Microsoft Agent Framework agent types");
            throw;
        }
    }

    /// <summary>
    /// Gets or creates a Microsoft Agent Framework agent instance
    /// </summary>
    private async Task<ChatClientAgent?> GetOrCreateAgentInstanceAsync(MicrosoftAgentFrameworkDefinition agent)
    {
        try
        {
            if (_agentInstances.TryGetValue(agent.Id, out var cachedInstance))
            {
                return cachedInstance;
            }

            // Create chat client based on LLM configuration
            var chatClient = await CreateChatClientAsync(agent.LLMConfig);
            if (chatClient == null)
            {
                _logger.LogWarning("Failed to create chat client for agent {AgentId}", agent.Id);
                return null;
            }

            // Create Microsoft Agent Framework ChatClientAgent
            var instance = new ChatClientAgent(
                chatClient: chatClient,
                name: agent.Name,
                instructions: agent.Instructions
            );

            // Register tools if available
            if (agent.Tools?.Any() == true)
            {
                await RegisterToolsForAgentAsync(instance, agent.Tools);
            }

            // Cache the instance
            _agentInstances[agent.Id] = instance;

            _logger.LogInformation("Created Microsoft Agent Framework agent instance: {AgentId}", agent.Id);
            return instance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Microsoft Agent Framework agent instance: {AgentId}", agent.Id);
            return null;
        }
    }

    /// <summary>
    /// Creates a chat client based on LLM configuration
    /// </summary>
    private async Task<IChatClient?> CreateChatClientAsync(Models.MicrosoftAgentFrameworkLLMConfiguration? llmConfig)
    {
        try
        {
            if (llmConfig == null)
            {
                _logger.LogWarning("No LLM configuration provided");
                return null;
            }

            _logger.LogInformation("Creating chat client for model {ModelName}", llmConfig.ModelName);
            
            // TODO: Implement actual chat client creation using Microsoft Agent Framework
            // This would integrate with your existing chat client creation logic
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat client for model {ModelName}", llmConfig?.ModelName);
            return null;
        }
    }

    /// <summary>
    /// Registers tools with Microsoft Agent Framework agent
    /// </summary>
    private async Task RegisterToolsForAgentAsync(ChatClientAgent agent, List<Models.MicrosoftAgentFrameworkToolConfiguration> tools)
    {
        try
        {
            foreach (var tool in tools)
            {
                // Register tools using Microsoft Agent Framework patterns
                _logger.LogInformation("Registering tool {ToolName} with Microsoft Agent Framework agent", tool.Name);
                
                // TODO: Implement tool registration based on Microsoft Agent Framework patterns
                // This would use AIFunctionFactory.Create() and tool registration APIs
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering tools with Microsoft Agent Framework agent");
        }
    }

    /// <summary>
    /// Validates agent configuration
    /// </summary>
    private async Task ValidateAgentConfigurationAsync(MicrosoftAgentFrameworkDefinition agent)
    {
        try
        {
            _logger.LogInformation("Validating Microsoft Agent Framework agent configuration: {AgentId}", agent.Id);

            // Validate required fields
            if (string.IsNullOrEmpty(agent.Name))
                throw new ArgumentException("Agent name is required");

            if (string.IsNullOrEmpty(agent.Instructions))
                throw new ArgumentException("Agent instructions are required");

            // Validate LLM configuration
            if (agent.LLMConfig != null)
            {
                if (string.IsNullOrEmpty(agent.LLMConfig.ModelName))
                    throw new ArgumentException("LLM model name is required");

                if (agent.LLMConfig.Temperature < 0 || agent.LLMConfig.Temperature > 2)
                    throw new ArgumentException("LLM temperature must be between 0 and 2");

                if (agent.LLMConfig.MaxTokens <= 0)
                    throw new ArgumentException("LLM max tokens must be greater than 0");
            }

            // Validate tools
            foreach (var tool in agent.Tools)
            {
                if (string.IsNullOrEmpty(tool.Name))
                    throw new ArgumentException("Tool name is required");

                if (string.IsNullOrEmpty(tool.Description))
                    throw new ArgumentException("Tool description is required");
            }

            _logger.LogInformation("Microsoft Agent Framework agent configuration validated successfully: {AgentId}", agent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Microsoft Agent Framework agent configuration: {AgentId}", agent.Id);
            throw;
        }
    }

    /// <summary>
    /// Gets default capabilities for agent type
    /// </summary>
    private Models.MicrosoftAgentFrameworkCapabilities GetDefaultCapabilitiesForType(Models.MicrosoftAgentFrameworkType type)
    {
        return type switch
        {
            Models.MicrosoftAgentFrameworkType.ChatAgent => new Models.MicrosoftAgentFrameworkCapabilities
            {
                CanCallTools = true,
                CanUseMemory = true,
                CanStreamResponses = true
            },
            Models.MicrosoftAgentFrameworkType.FunctionAgent => new Models.MicrosoftAgentFrameworkCapabilities
            {
                CanCallTools = true,
                CanUseMemory = true,
                CanStreamResponses = true
            },
            Models.MicrosoftAgentFrameworkType.SequentialAgent => new Models.MicrosoftAgentFrameworkCapabilities
            {
                CanOrchestrate = true,
                CanCallTools = true,
                CanUseMemory = true
            },
            Models.MicrosoftAgentFrameworkType.ConcurrentAgent => new Models.MicrosoftAgentFrameworkCapabilities
            {
                CanOrchestrate = true,
                CanHandleParallelExecution = true,
                CanCallTools = true
            },
            Models.MicrosoftAgentFrameworkType.GroupChatAgent => new Models.MicrosoftAgentFrameworkCapabilities
            {
                CanOrchestrate = true,
                CanCallTools = true,
                CanUseMemory = true
            },
            Models.MicrosoftAgentFrameworkType.MCPAgent => new Models.MicrosoftAgentFrameworkCapabilities
            {
                CanUseMCP = true,
                CanCallTools = true
            },
            Models.MicrosoftAgentFrameworkType.CustomAgent => new Models.MicrosoftAgentFrameworkCapabilities(),
            _ => new Models.MicrosoftAgentFrameworkCapabilities()
        };
    }

    /// <summary>
    /// Initializes default agent types
    /// </summary>
    private void InitializeDefaultAgentTypes()
    {
        try
        {
            _logger.LogInformation("Initializing Microsoft Agent Framework default agent types");
            
            // Create sample agents for demonstration
            var sampleAgent = new MicrosoftAgentFrameworkDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Microsoft Agent Framework Sample Agent",
                Description = "Sample agent demonstrating Microsoft Agent Framework capabilities",
                Type = MicrosoftAgentFrameworkType.ChatAgent,
                Instructions = "You are a helpful Microsoft Agent Framework assistant.",
                Status = MicrosoftAgentFrameworkStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _agents[sampleAgent.Id] = sampleAgent;
            _logger.LogInformation("Microsoft Agent Framework default agent types initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Microsoft Agent Framework default agent types");
        }
    }
}
