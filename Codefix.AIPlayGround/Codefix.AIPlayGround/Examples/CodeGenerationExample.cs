using Codefix.AIPlayGround.Services;

namespace Codefix.AIPlayGround.Examples;

/// <summary>
/// Example demonstrating how to use the code generation service
/// </summary>
public class CodeGenerationExample
{
    private readonly ICodeGenerationService _codeGenerationService;
    private readonly IAgentFactory _agentFactory;
    private readonly ICodeExecutionService _codeExecutionService;

    public CodeGenerationExample(
        ICodeGenerationService codeGenerationService,
        IAgentFactory agentFactory,
        ICodeExecutionService codeExecutionService)
    {
        _codeGenerationService = codeGenerationService;
        _agentFactory = agentFactory;
        _codeExecutionService = codeExecutionService;
    }

    /// <summary>
    /// Example: Create a data processing agent
    /// </summary>
    public async Task<string> CreateDataProcessingAgentAsync()
    {
        var specification = new AgentCodeSpecification
        {
            AgentName = "DataProcessorAgent",
            Description = "Agent for processing and transforming data",
            AgentType = "DataProcessor",
            Methods = new List<AgentMethod>
            {
                new()
                {
                    Name = "ProcessDataAsync",
                    ReturnType = "Task<object>",
                    Parameters = new List<MethodParameter>
                    {
                        new() { Name = "input", Type = "object", IsOptional = false }
                    },
                    Body = @"
                        // Process the input data
                        var data = JsonSerializer.Serialize(input);
                        Log($""Processing data: {{data}}"");
                        
                        // Simulate processing
                        await Task.Delay(100);
                        
                        return new { ProcessedData = data, ProcessedAt = DateTime.UtcNow };
                    ",
                    IsAsync = true
                },
                new()
                {
                    Name = "ProcessBatchAsync",
                    ReturnType = "Task<List<object>>",
                    Parameters = new List<MethodParameter>
                    {
                        new() { Name = "inputs", Type = "List<object>", IsOptional = false }
                    },
                    Body = @"
                        var results = new List<object>();
                        
                        foreach (var input in inputs)
                        {
                            var result = await ProcessDataAsync(input);
                            results.Add(result);
                        }
                        
                        return results;
                    ",
                    IsAsync = true
                }
            },
            Properties = new List<AgentProperty>
            {
                new()
                {
                    Name = "ProcessingType",
                    Type = "string",
                    DefaultValue = "default",
                    HasGetter = true,
                    HasSetter = true
                },
                new()
                {
                    Name = "BatchSize",
                    Type = "int",
                    DefaultValue = 10,
                    HasGetter = true,
                    HasSetter = true
                }
            },
            Dependencies = new List<string>
            {
                "System.Text.Json",
                "System.Collections.Generic"
            },
            Security = new SecuritySettings
            {
                AllowFileSystemAccess = false,
                AllowNetworkAccess = false,
                AllowReflection = false,
                MaxExecutionTimeSeconds = 60
            }
        };

        // Create the agent
        var agent = await _agentFactory.CreateCodeGeneratedAgentAsync(specification);
        return agent.Id;
    }

    /// <summary>
    /// Example: Create an API integration agent from template
    /// </summary>
    public async Task<string> CreateApiIntegrationAgentAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "AgentName", "WeatherApiAgent" },
            { "ApiEndpoint", "https://api.weather.com/v1" },
            { "ApiKey", "your-api-key-here" }
        };

        var agent = await _agentFactory.CreateAgentFromTemplateAsync("APIIntegrationAgent", parameters);
        return agent.Id;
    }

    /// <summary>
    /// Example: Execute a generated agent
    /// </summary>
    public async Task<object> ExecuteAgentExampleAsync(string agentId)
    {
        var input = new { message = "Hello, World!", timestamp = DateTime.UtcNow };
        var context = new Dictionary<string, object>
        {
            { "userId", "user123" },
            { "sessionId", "session456" }
        };

        var result = await _agentFactory.ExecuteCodeGeneratedAgentAsync(agentId, input, context);
        return result.Result;
    }

    /// <summary>
    /// Example: Execute a specific method on an agent
    /// </summary>
    public async Task<object> ExecuteMethodExampleAsync(string agentId)
    {
        var parameters = new object[] { "https://api.example.com/data" };
        var result = await _agentFactory.ExecuteAgentMethodAsync(agentId, "CallApiAsync", parameters);
        return result.Result;
    }

    /// <summary>
    /// Example: Create a custom workflow agent
    /// </summary>
    public async Task<string> CreateWorkflowAgentAsync()
    {
        var specification = new AgentCodeSpecification
        {
            AgentName = "CustomWorkflowAgent",
            Description = "Custom workflow execution agent",
            AgentType = "Workflow",
            Methods = new List<AgentMethod>
            {
                new()
                {
                    Name = "ExecuteWorkflowAsync",
                    ReturnType = "Task<object>",
                    Parameters = new List<MethodParameter>
                    {
                        new() { Name = "workflowData", Type = "object", IsOptional = false }
                    },
                    Body = @"
                        Log(""Starting workflow execution"");
                        
                        // Step 1: Validate input
                        if (!ValidateInput(workflowData))
                        {
                            throw new ArgumentException(""Invalid workflow data"");
                        }
                        
                        // Step 2: Process data
                        var processedData = await ProcessStepAsync(workflowData);
                        
                        // Step 3: Transform data
                        var transformedData = await TransformStepAsync(processedData);
                        
                        // Step 4: Finalize
                        var result = await FinalizeStepAsync(transformedData);
                        
                        Log(""Workflow execution completed"");
                        return result;
                    ",
                    IsAsync = true
                },
                new()
                {
                    Name = "ProcessStepAsync",
                    ReturnType = "Task<object>",
                    Parameters = new List<MethodParameter>
                    {
                        new() { Name = "data", Type = "object", IsOptional = false }
                    },
                    Body = @"
                        Log(""Processing step"");
                        await Task.Delay(50);
                        return data;
                    ",
                    IsAsync = true
                },
                new()
                {
                    Name = "TransformStepAsync",
                    ReturnType = "Task<object>",
                    Parameters = new List<MethodParameter>
                    {
                        new() { Name = "data", Type = "object", IsOptional = false }
                    },
                    Body = @"
                        Log(""Transforming step"");
                        await Task.Delay(75);
                        return new { TransformedData = data, TransformedAt = DateTime.UtcNow };
                    ",
                    IsAsync = true
                },
                new()
                {
                    Name = "FinalizeStepAsync",
                    ReturnType = "Task<object>",
                    Parameters = new List<MethodParameter>
                    {
                        new() { Name = "data", Type = "object", IsOptional = false }
                    },
                    Body = @"
                        Log(""Finalizing step"");
                        await Task.Delay(25);
                        return new { FinalResult = data, CompletedAt = DateTime.UtcNow };
                    ",
                    IsAsync = true
                }
            },
            Security = new SecuritySettings
            {
                AllowFileSystemAccess = false,
                AllowNetworkAccess = false,
                AllowReflection = false,
                MaxExecutionTimeSeconds = 120
            }
        };

        var agent = await _agentFactory.CreateCodeGeneratedAgentAsync(specification);
        return agent.Id;
    }

    /// <summary>
    /// Example: Get available templates
    /// </summary>
    public async Task<List<CodeTemplate>> GetAvailableTemplatesAsync()
    {
        return await _codeGenerationService.GetAvailableTemplatesAsync();
    }

    /// <summary>
    /// Example: Validate generated code
    /// </summary>
    public async Task<bool> ValidateCodeExampleAsync(string code)
    {
        var result = await _codeGenerationService.ValidateGeneratedCodeAsync(code);
        return result.IsValid;
    }
}
