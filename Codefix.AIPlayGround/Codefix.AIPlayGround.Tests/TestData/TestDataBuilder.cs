using System.Text.Json;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Tests.TestData;

public static class TestDataBuilder
{
    public static AgentEntity CreateTestAgent(
        string name = "Test Agent",
        string agentType = "LLMAgent",
        string instructions = "You are a helpful assistant",
        AgentStatus status = AgentStatus.Draft)
    {
        return new AgentEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"Test description for {name}",
            AgentType = agentType,
            Instructions = instructions,
            LLMConfigurationJson = JsonSerializer.Serialize(new
            {
                modelName = "gpt-4",
                provider = "OpenAI",
                temperature = 0.7,
                maxTokens = 2000
            }),
            ToolsConfigurationJson = JsonSerializer.Serialize(new List<object>()),
            PromptTemplateJson = JsonSerializer.Serialize(new
            {
                systemPrompt = "You are a helpful assistant",
                userPrompt = "User: {message}",
                assistantPrompt = "Assistant: {response}"
            }),
            MemoryConfigurationJson = JsonSerializer.Serialize(new
            {
                enableMemory = true,
                maxMemoryItems = 100,
                memoryType = "conversation"
            }),
            CheckpointConfigurationJson = JsonSerializer.Serialize(new
            {
                enableCheckpoints = true,
                checkpointType = "automatic",
                checkpointInterval = 10
            }),
            PropertiesJson = JsonSerializer.Serialize(new Dictionary<string, object>()),
            Status = status,
            CreatedBy = "test-user"
        };
    }

    public static CreateAgentDto CreateTestAgentDto(
        string name = "Test Agent",
        string agentType = "LLMAgent",
        string instructions = "You are a helpful assistant")
    {
        return new CreateAgentDto
        {
            Name = name,
            Description = $"Test description for {name}",
            AgentType = agentType,
            Instructions = instructions,
            LLMConfiguration = new LLMConfigurationDto
            {
                ModelName = "gpt-4",
                Provider = "OpenAI",
                Temperature = 0.7,
                MaxTokens = 2000,
                TopP = 1.0,
                FrequencyPenalty = 0.0,
                PresencePenalty = 0.0,
                StopSequences = new List<string>()
            },
            Tools = new List<ToolConfigurationDto>(),
            PromptTemplate = new PromptTemplateDto
            {
                SystemPrompt = "You are a helpful assistant",
                UserPrompt = "User: {message}",
                AssistantPrompt = "Assistant: {response}",
                Variables = new List<PromptVariableDto>()
            },
            MemoryConfiguration = new MemoryConfigurationDto
            {
                EnableMemory = true,
                MaxMemoryItems = 100,
                MemoryType = "conversation",
                RelevanceThreshold = 0.7,
                MemoryRetention = TimeSpan.FromDays(30)
            },
            CheckpointConfiguration = new CheckpointConfigurationDto
            {
                EnableCheckpoints = true,
                CheckpointType = "automatic",
                CheckpointInterval = 10,
                CheckpointConditions = new List<string>(),
                EnableRecovery = true
            },
            Properties = new Dictionary<string, object>()
        };
    }

    public static FlowEntity CreateTestFlow(
        string name = "Test Flow",
        string flowType = "Sequential",
        FlowStatus status = FlowStatus.Draft)
    {
        return new FlowEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"Test description for {name}",
            Version = "1.0.0",
            FlowType = flowType,
            ConfigurationJson = JsonSerializer.Serialize(new
            {
                timeout = 300,
                retryAttempts = 3,
                enableLogging = true,
                enableMetrics = true
            }),
            Status = status,
            CreatedBy = "test-user"
        };
    }

    public static NodeEntity CreateTestNode(
        string name = "Test Node",
        string nodeType = "Function",
        NodeStatus status = NodeStatus.Draft)
    {
        return new NodeEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"Test description for {name}",
            NodeType = nodeType,
            ConfigurationJson = JsonSerializer.Serialize(new
            {
                functionName = "testFunction",
                timeout = 30,
                retryCount = 2,
                parameters = new Dictionary<string, object>()
            }),
            InputSchemaJson = JsonSerializer.Serialize(new
            {
                type = "object",
                properties = new
                {
                    input = new { type = "string", description = "Input data" }
                },
                required = new[] { "input" }
            }),
            OutputSchemaJson = JsonSerializer.Serialize(new
            {
                type = "object",
                properties = new
                {
                    output = new { type = "string", description = "Output data" }
                }
            }),
            PropertiesJson = JsonSerializer.Serialize(new Dictionary<string, object>()),
            Status = status,
            CreatedBy = "test-user"
        };
    }

    public static TestInputDto CreateTestInput(
        string message = "Hello, how are you?",
        Dictionary<string, object>? context = null)
    {
        return new TestInputDto
        {
            Input = new Dictionary<string, object>
            {
                { "message", message },
                { "context", context ?? new Dictionary<string, object> { { "userId", "test-user" } } }
            }
        };
    }

    public static List<AgentEntity> CreateMultipleTestAgents(int count)
    {
        var agents = new List<AgentEntity>();
        for (int i = 1; i <= count; i++)
        {
            agents.Add(CreateTestAgent(
                name: $"Test Agent {i}",
                agentType: i % 2 == 0 ? "LLMAgent" : "ToolAgent",
                status: (AgentStatus)(i % 3)
            ));
        }
        return agents;
    }
}
