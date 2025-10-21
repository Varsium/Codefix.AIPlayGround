using System.Text.Json;
using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Factory implementation for creating agents following Microsoft Agent Framework patterns
/// Reference: https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted/Agents
/// </summary>
public class AgentFactory : IAgentFactory
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AgentFactory> _logger;
    private readonly List<AgentTemplate> _templates;

    public AgentFactory(ApplicationDbContext context, ILogger<AgentFactory> logger)
    {
        _context = context;
        _logger = logger;
        _templates = InitializeTemplates();
    }

    public async Task<AgentEntity> CreateLLMAgentAsync(string name, string instructions, LLMConfiguration? config = null)
    {
        _logger.LogInformation("Creating LLM Agent: {Name}", name);

        var llmConfig = config ?? new LLMConfiguration
        {
            ModelName = "gpt-4",
            Provider = "OpenAI",
            Temperature = 0.7,
            MaxTokens = 2000
        };

        var agent = new AgentEntity
        {
            Name = name,
            Description = $"LLM-based agent using {llmConfig.ModelName}",
            AgentType = "LLMAgent",
            Instructions = instructions,
            LLMConfigurationJson = JsonSerializer.Serialize(llmConfig),
            ToolsConfigurationJson = JsonSerializer.Serialize(new List<ToolConfiguration>()),
            PromptTemplateJson = JsonSerializer.Serialize(new PromptTemplate
            {
                SystemPrompt = "You are a helpful AI assistant.",
                UserPrompt = "{user_input}",
                Template = "System: {system_prompt}\nUser: {user_prompt}"
            }),
            MemoryConfigurationJson = JsonSerializer.Serialize(new MemoryConfiguration
            {
                EnableMemory = true,
                MaxMemoryItems = 100,
                MemoryType = "conversation"
            }),
            CheckpointConfigurationJson = JsonSerializer.Serialize(new CheckpointConfiguration()),
            PropertiesJson = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "CreatedBy", "AgentFactory" },
                { "FactoryMethod", "CreateLLMAgent" }
            }),
            CreatedBy = "System"
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created LLM Agent: {AgentId}", agent.Id);
        return agent;
    }

    public async Task<AgentEntity> CreateToolAgentAsync(string name, List<ToolConfiguration> tools, string? instructions = null)
    {
        _logger.LogInformation("Creating Tool Agent: {Name} with {ToolCount} tools", name, tools.Count);

        var agent = new AgentEntity
        {
            Name = name,
            Description = $"Tool-enabled agent with {tools.Count} tools",
            AgentType = "ToolAgent",
            Instructions = instructions ?? "You are an agent equipped with specialized tools. Use them effectively to accomplish tasks.",
            LLMConfigurationJson = JsonSerializer.Serialize(new LLMConfiguration
            {
                ModelName = "gpt-4",
                Provider = "OpenAI",
                Temperature = 0.5
            }),
            ToolsConfigurationJson = JsonSerializer.Serialize(tools),
            PromptTemplateJson = JsonSerializer.Serialize(new PromptTemplate
            {
                SystemPrompt = "You have access to the following tools. Use them when needed.",
                UserPrompt = "{user_input}"
            }),
            MemoryConfigurationJson = JsonSerializer.Serialize(new MemoryConfiguration()),
            CheckpointConfigurationJson = JsonSerializer.Serialize(new CheckpointConfiguration()),
            PropertiesJson = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "CreatedBy", "AgentFactory" },
                { "FactoryMethod", "CreateToolAgent" },
                { "ToolCount", tools.Count }
            }),
            CreatedBy = "System"
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        return agent;
    }

    public async Task<AgentEntity> CreateConditionalAgentAsync(string name, string condition, string? instructions = null)
    {
        _logger.LogInformation("Creating Conditional Agent: {Name}", name);

        var agent = new AgentEntity
        {
            Name = name,
            Description = "Conditional routing agent for workflow branching",
            AgentType = "ConditionalAgent",
            Instructions = instructions ?? "Evaluate conditions and route workflows accordingly.",
            LLMConfigurationJson = JsonSerializer.Serialize(new LLMConfiguration()),
            ToolsConfigurationJson = JsonSerializer.Serialize(new List<ToolConfiguration>()),
            PromptTemplateJson = JsonSerializer.Serialize(new PromptTemplate()),
            MemoryConfigurationJson = JsonSerializer.Serialize(new MemoryConfiguration()),
            CheckpointConfigurationJson = JsonSerializer.Serialize(new CheckpointConfiguration()),
            PropertiesJson = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "CreatedBy", "AgentFactory" },
                { "FactoryMethod", "CreateConditionalAgent" },
                { "Condition", condition },
                { "RoutingLogic", "Evaluates condition and routes to appropriate path" }
            }),
            CreatedBy = "System"
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        return agent;
    }

    public async Task<AgentEntity> CreateParallelAgentAsync(string name, int maxConcurrency = 5)
    {
        _logger.LogInformation("Creating Parallel Agent: {Name} with concurrency {MaxConcurrency}", name, maxConcurrency);

        var agent = new AgentEntity
        {
            Name = name,
            Description = $"Parallel execution agent (max concurrency: {maxConcurrency})",
            AgentType = "ParallelAgent",
            Instructions = "Execute multiple tasks in parallel for improved performance.",
            LLMConfigurationJson = JsonSerializer.Serialize(new LLMConfiguration()),
            ToolsConfigurationJson = JsonSerializer.Serialize(new List<ToolConfiguration>()),
            PromptTemplateJson = JsonSerializer.Serialize(new PromptTemplate()),
            MemoryConfigurationJson = JsonSerializer.Serialize(new MemoryConfiguration()),
            CheckpointConfigurationJson = JsonSerializer.Serialize(new CheckpointConfiguration()),
            PropertiesJson = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "CreatedBy", "AgentFactory" },
                { "FactoryMethod", "CreateParallelAgent" },
                { "MaxConcurrency", maxConcurrency },
                { "ExecutionMode", "Parallel" }
            }),
            CreatedBy = "System"
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        return agent;
    }

    public async Task<AgentEntity> CreateCheckpointAgentAsync(string name, CheckpointConfiguration? config = null)
    {
        _logger.LogInformation("Creating Checkpoint Agent: {Name}", name);

        var checkpointConfig = config ?? new CheckpointConfiguration
        {
            EnableCheckpoints = true,
            CheckpointType = "automatic",
            CheckpointInterval = 10,
            EnableRecovery = true
        };

        var agent = new AgentEntity
        {
            Name = name,
            Description = "Agent with checkpoint and recovery capabilities",
            AgentType = "CheckpointAgent",
            Instructions = "Execute tasks with automatic checkpointing for reliability.",
            LLMConfigurationJson = JsonSerializer.Serialize(new LLMConfiguration()),
            ToolsConfigurationJson = JsonSerializer.Serialize(new List<ToolConfiguration>()),
            PromptTemplateJson = JsonSerializer.Serialize(new PromptTemplate()),
            MemoryConfigurationJson = JsonSerializer.Serialize(new MemoryConfiguration()),
            CheckpointConfigurationJson = JsonSerializer.Serialize(checkpointConfig),
            PropertiesJson = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "CreatedBy", "AgentFactory" },
                { "FactoryMethod", "CreateCheckpointAgent" },
                { "CheckpointEnabled", true }
            }),
            CreatedBy = "System"
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        return agent;
    }

    public async Task<AgentEntity> CreateMCPAgentAsync(string name, List<string> mcpServers, string? instructions = null)
    {
        _logger.LogInformation("Creating MCP Agent: {Name} with {ServerCount} MCP servers", name, mcpServers.Count);

        var tools = mcpServers.Select(server => new ToolConfiguration
        {
            Name = $"MCP_{server}",
            Type = "MCP",
            Description = $"MCP server: {server}",
            IsEnabled = true,
            Parameters = new Dictionary<string, object>
            {
                { "server", server },
                { "protocol", "MCP" }
            }
        }).ToList();

        var agent = new AgentEntity
        {
            Name = name,
            Description = $"MCP-enabled agent with {mcpServers.Count} servers",
            AgentType = "MCPAgent",
            Instructions = instructions ?? "You are an agent with Model Context Protocol capabilities. Use MCP servers for enhanced functionality.",
            LLMConfigurationJson = JsonSerializer.Serialize(new LLMConfiguration
            {
                ModelName = "gpt-4",
                Provider = "OpenAI"
            }),
            ToolsConfigurationJson = JsonSerializer.Serialize(tools),
            PromptTemplateJson = JsonSerializer.Serialize(new PromptTemplate()),
            MemoryConfigurationJson = JsonSerializer.Serialize(new MemoryConfiguration()),
            CheckpointConfigurationJson = JsonSerializer.Serialize(new CheckpointConfiguration()),
            PropertiesJson = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "CreatedBy", "AgentFactory" },
                { "FactoryMethod", "CreateMCPAgent" },
                { "MCPServers", mcpServers },
                { "Protocol", "MCP" }
            }),
            CreatedBy = "System"
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        return agent;
    }

    public async Task<AgentEntity> CreateFromTemplateAsync(string templateName, Dictionary<string, object>? parameters = null)
    {
        _logger.LogInformation("Creating agent from template: {TemplateName}", templateName);

        var template = _templates.FirstOrDefault(t => t.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase));
        if (template == null)
        {
            throw new ArgumentException($"Template '{templateName}' not found");
        }

        // Apply parameters to template
        var agentDto = ApplyTemplateParameters(template, parameters);
        
        return await CreateCustomAgentAsync(agentDto);
    }

    public async Task<AgentEntity> CreateCustomAgentAsync(CreateAgentRequest agentDto)
    {
        _logger.LogInformation("Creating custom agent: {Name}", agentDto.Name);

        var agent = new AgentEntity
        {
            Name = agentDto.Name,
            Description = agentDto.Description,
            AgentType = agentDto.AgentType,
            Instructions = agentDto.Instructions,
            LLMConfigurationJson = JsonSerializer.Serialize(agentDto.LLMConfiguration ?? new LLMConfiguration()),
            ToolsConfigurationJson = JsonSerializer.Serialize(agentDto.Tools ?? new List<ToolConfiguration>()),
            PromptTemplateJson = JsonSerializer.Serialize(agentDto.PromptTemplate ?? new PromptTemplate()),
            MemoryConfigurationJson = JsonSerializer.Serialize(agentDto.MemoryConfiguration ?? new MemoryConfiguration()),
            CheckpointConfigurationJson = JsonSerializer.Serialize(agentDto.CheckpointConfiguration ?? new CheckpointConfiguration()),
            PropertiesJson = JsonSerializer.Serialize(agentDto.Properties ?? new Dictionary<string, object>()),
            CreatedBy = "System"
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        return agent;
    }

    public async Task<List<AgentTemplate>> GetAvailableTemplatesAsync()
    {
        return await Task.FromResult(_templates);
    }

    public async Task<ValidationResult> ValidateAgentConfigurationAsync(CreateAgentRequest agentDto)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate required fields
        if (string.IsNullOrWhiteSpace(agentDto.Name))
        {
            result.IsValid = false;
            result.Errors.Add("Agent name is required");
        }

        if (string.IsNullOrWhiteSpace(agentDto.AgentType))
        {
            result.IsValid = false;
            result.Errors.Add("Agent type is required");
        }

        // Validate agent type
        var validTypes = new[] { "LLMAgent", "ToolAgent", "ConditionalAgent", "ParallelAgent", "CheckpointAgent", "MCPAgent" };
        if (!string.IsNullOrWhiteSpace(agentDto.AgentType) && !validTypes.Contains(agentDto.AgentType))
        {
            result.IsValid = false;
            result.Errors.Add($"Invalid agent type. Valid types: {string.Join(", ", validTypes)}");
        }

        // Validate LLM configuration for LLM agents
        if (agentDto.AgentType == "LLMAgent" && agentDto.LLMConfiguration != null)
        {
            if (agentDto.LLMConfiguration.Temperature < 0 || agentDto.LLMConfiguration.Temperature > 2)
            {
                result.Warnings.Add("Temperature should be between 0 and 2");
            }

            if (agentDto.LLMConfiguration.MaxTokens < 1)
            {
                result.IsValid = false;
                result.Errors.Add("MaxTokens must be greater than 0");
            }
        }

        // Check for duplicate names
        var existingAgent = await _context.Agents
            .FirstOrDefaultAsync(a => a.Name == agentDto.Name);
        
        if (existingAgent != null)
        {
            result.Warnings.Add($"An agent with name '{agentDto.Name}' already exists");
        }

        return result;
    }

    public async Task<AgentEntity> CloneAgentAsync(string sourceAgentId, string newName, Dictionary<string, object>? modifications = null)
    {
        _logger.LogInformation("Cloning agent: {SourceId} to {NewName}", sourceAgentId, newName);

        var sourceAgent = await _context.Agents.FindAsync(sourceAgentId);
        if (sourceAgent == null)
        {
            throw new ArgumentException($"Source agent '{sourceAgentId}' not found");
        }

        var clonedAgent = new AgentEntity
        {
            Name = newName,
            Description = sourceAgent.Description + " (Clone)",
            AgentType = sourceAgent.AgentType,
            Instructions = sourceAgent.Instructions,
            LLMConfigurationJson = sourceAgent.LLMConfigurationJson,
            ToolsConfigurationJson = sourceAgent.ToolsConfigurationJson,
            PromptTemplateJson = sourceAgent.PromptTemplateJson,
            MemoryConfigurationJson = sourceAgent.MemoryConfigurationJson,
            CheckpointConfigurationJson = sourceAgent.CheckpointConfigurationJson,
            PropertiesJson = sourceAgent.PropertiesJson,
            CreatedBy = "System"
        };

        // Apply modifications if provided
        if (modifications != null && modifications.Any())
        {
            var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(clonedAgent.PropertiesJson) ?? new();
            foreach (var mod in modifications)
            {
                properties[mod.Key] = mod.Value;
            }
            clonedAgent.PropertiesJson = JsonSerializer.Serialize(properties);
        }

        _context.Agents.Add(clonedAgent);
        await _context.SaveChangesAsync();

        return clonedAgent;
    }

    private List<AgentTemplate> InitializeTemplates()
    {
        return new List<AgentTemplate>
        {
            new AgentTemplate
            {
                Name = "ResearchAssistant",
                Description = "AI research assistant for gathering and analyzing information",
                AgentType = "LLMAgent",
                Category = "Research",
                Tags = new List<string> { "research", "analysis", "information-gathering" },
                Parameters = new Dictionary<string, TemplateParameter>
                {
                    ["topic"] = new TemplateParameter
                    {
                        Name = "topic",
                        Type = "string",
                        Description = "Research topic or domain",
                        IsRequired = true
                    }
                },
                BaseConfiguration = new CreateAgentRequest
                {
                    AgentType = "LLMAgent",
                    Instructions = "You are a research assistant. Gather, analyze, and synthesize information on the given topic.",
                    LLMConfiguration = new LLMConfiguration
                    {
                        ModelName = "gpt-4",
                        Temperature = 0.7,
                        MaxTokens = 3000
                    }
                }
            },
            new AgentTemplate
            {
                Name = "CodeReviewer",
                Description = "Automated code review agent with best practices",
                AgentType = "ToolAgent",
                Category = "Development",
                Tags = new List<string> { "code-review", "development", "quality" },
                BaseConfiguration = new CreateAgentRequest
                {
                    AgentType = "ToolAgent",
                    Instructions = "Review code for best practices, potential bugs, and improvements.",
                    Tools = new List<ToolConfiguration>
                    {
                        new() { Name = "StaticAnalysis", Type = "Function", Description = "Perform static code analysis" },
                        new() { Name = "SecurityScan", Type = "Function", Description = "Security vulnerability scanning" }
                    }
                }
            },
            new AgentTemplate
            {
                Name = "DataAnalyst",
                Description = "Data analysis and visualization agent",
                AgentType = "LLMAgent",
                Category = "Analytics",
                Tags = new List<string> { "data", "analytics", "visualization" },
                BaseConfiguration = new CreateAgentRequest
                {
                    AgentType = "LLMAgent",
                    Instructions = "Analyze data, identify patterns, and provide insights.",
                    LLMConfiguration = new LLMConfiguration
                    {
                        ModelName = "gpt-4",
                        Temperature = 0.3
                    }
                }
            },
            new AgentTemplate
            {
                Name = "CustomerSupport",
                Description = "Customer support and assistance agent",
                AgentType = "LLMAgent",
                Category = "Support",
                Tags = new List<string> { "support", "customer-service", "help-desk" },
                BaseConfiguration = new CreateAgentRequest
                {
                    AgentType = "LLMAgent",
                    Instructions = "Provide helpful, friendly, and professional customer support.",
                    LLMConfiguration = new LLMConfiguration
                    {
                        ModelName = "gpt-4",
                        Temperature = 0.8
                    },
                    MemoryConfiguration = new MemoryConfiguration
                    {
                        EnableMemory = true,
                        MemoryType = "conversation",
                        MaxMemoryItems = 50
                    }
                }
            }
        };
    }

    private CreateAgentRequest ApplyTemplateParameters(AgentTemplate template, Dictionary<string, object>? parameters)
    {
        var dto = template.BaseConfiguration;
        
        if (parameters != null)
        {
            // Apply parameters to name and instructions
            foreach (var param in parameters)
            {
                if (string.IsNullOrEmpty(dto.Name))
                {
                    dto.Name = template.Name;
                }
                
                if (!string.IsNullOrEmpty(dto.Instructions))
                {
                    dto.Instructions = dto.Instructions.Replace($"{{{param.Key}}}", param.Value?.ToString() ?? "");
                }
            }
        }

        // Ensure name is set
        if (string.IsNullOrEmpty(dto.Name))
        {
            dto.Name = $"{template.Name}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        return dto;
    }
}

