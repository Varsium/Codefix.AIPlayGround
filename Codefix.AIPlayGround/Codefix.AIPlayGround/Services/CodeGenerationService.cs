using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using Codefix.AIPlayGround.Models.DTOs;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Service for generating and compiling agent code at runtime
/// </summary>
public class CodeGenerationService : ICodeGenerationService
{
    private readonly ILogger<CodeGenerationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly List<CodeTemplate> _templates;
    private readonly string _tempDirectory;
    private readonly string _assemblyDirectory;

    public CodeGenerationService(
        ILogger<CodeGenerationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _tempDirectory = Path.Combine(Path.GetTempPath(), "AgentCodeGen", Guid.NewGuid().ToString());
        _assemblyDirectory = Path.Combine(_tempDirectory, "Assemblies");
        
        Directory.CreateDirectory(_tempDirectory);
        Directory.CreateDirectory(_assemblyDirectory);
        
        _templates = InitializeTemplates();
    }

    public async Task<CodeGenerationResult> GenerateAgentCodeAsync(AgentCodeSpecification specification)
    {
        try
        {
            _logger.LogInformation("Generating code for agent: {AgentName}", specification.AgentName);

            var codeBuilder = new StringBuilder();
            
            // Add using statements
            codeBuilder.AppendLine("using System;");
            codeBuilder.AppendLine("using System.Collections.Generic;");
            codeBuilder.AppendLine("using System.Threading.Tasks;");
            codeBuilder.AppendLine("using System.Text.Json;");
            codeBuilder.AppendLine("using Codefix.AIPlayGround.Models.DTOs;");
            codeBuilder.AppendLine("using Codefix.AIPlayGround.Services;");
            
            // Add custom dependencies
            foreach (var dependency in specification.Dependencies)
            {
                codeBuilder.AppendLine($"using {dependency};");
            }
            
            codeBuilder.AppendLine();
            
            // Add namespace and class declaration
            codeBuilder.AppendLine("namespace Codefix.AIPlayGround.GeneratedAgents");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"    /// <summary>");
            codeBuilder.AppendLine($"    /// {specification.Description}");
            codeBuilder.AppendLine($"    /// Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            codeBuilder.AppendLine($"    /// </summary>");
            
            // Add class attributes
            codeBuilder.AppendLine("    [GeneratedAgent]");
            codeBuilder.AppendLine($"    public class {specification.AgentName} : {specification.BaseClass}");
            codeBuilder.AppendLine("    {");
            
            // Add interfaces
            if (specification.Interfaces.Any())
            {
                var interfaceList = string.Join(", ", specification.Interfaces);
                codeBuilder.AppendLine($"        // Implements: {interfaceList}");
            }
            
            codeBuilder.AppendLine();
            
            // Add properties
            foreach (var property in specification.Properties)
            {
                codeBuilder.AppendLine($"        {GenerateProperty(property)}");
            }
            
            // Add constructor
            codeBuilder.AppendLine($"        public {specification.AgentName}()");
            codeBuilder.AppendLine("        {");
            codeBuilder.AppendLine("            // Initialize agent");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine();
            
            // Add methods
            foreach (var method in specification.Methods)
            {
                codeBuilder.AppendLine($"        {GenerateMethod(method)}");
            }
            
            // Add configuration properties
            if (specification.Configuration.Any())
            {
                codeBuilder.AppendLine("        // Configuration properties");
                foreach (var config in specification.Configuration)
                {
                    var value = config.Value is string ? $"\"{config.Value}\"" : config.Value?.ToString() ?? "null";
                    codeBuilder.AppendLine($"        public object {config.Key} => {value};");
                }
                codeBuilder.AppendLine();
            }
            
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine("}");
            
            var generatedCode = codeBuilder.ToString();
            
            // Validate the generated code
            var validationResult = await ValidateGeneratedCodeAsync(generatedCode);
            if (!validationResult.IsValid)
            {
                return new CodeGenerationResult
                {
                    IsSuccess = false,
                    Message = "Code validation failed",
                    Errors = validationResult.Errors
                };
            }
            
            _logger.LogInformation("Successfully generated code for agent: {AgentName}", specification.AgentName);
            
            return new CodeGenerationResult
            {
                IsSuccess = true,
                GeneratedCode = generatedCode,
                Message = "Code generated successfully",
                Metadata = new Dictionary<string, object>
                {
                    { "AgentName", specification.AgentName },
                    { "AgentType", specification.AgentType },
                    { "GeneratedAt", DateTime.UtcNow },
                    { "CodeLength", generatedCode.Length }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating code for agent: {AgentName}", specification.AgentName);
            return new CodeGenerationResult
            {
                IsSuccess = false,
                Message = $"Code generation failed: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<CompilationResult> CompileAgentCodeAsync(string code, string assemblyName)
    {
        try
        {
            _logger.LogInformation("Compiling agent code for assembly: {AssemblyName}", assemblyName);

            // For now, we'll use a simplified approach that doesn't require Roslyn compilation
            // This creates a mock assembly path for demonstration purposes
            // In a production environment, you would implement actual compilation here
            
            var assemblyPath = Path.Combine(_assemblyDirectory, $"{assemblyName}.dll");
            
            // Create a placeholder file to simulate compilation
            await File.WriteAllTextAsync(assemblyPath, $"// Generated assembly: {assemblyName}\n// Generated at: {DateTime.UtcNow}\n// Code length: {code.Length}");

            _logger.LogInformation("Successfully compiled assembly: {AssemblyName}", assemblyName);

            return new CompilationResult
            {
                IsSuccess = true,
                AssemblyPath = assemblyPath,
                Message = "Compilation successful (simulated)",
                Metadata = new Dictionary<string, object>
                {
                    { "AssemblyName", assemblyName },
                    { "CompiledAt", DateTime.UtcNow },
                    { "AssemblySize", new FileInfo(assemblyPath).Length },
                    { "Note", "This is a simulated compilation for demonstration purposes" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compiling agent code for assembly: {AssemblyName}", assemblyName);
            return new CompilationResult
            {
                IsSuccess = false,
                Message = $"Compilation failed: {ex.Message}",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<AgentInstanceResult> CreateAgentInstanceAsync(string assemblyPath, string className, Dictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("Creating agent instance: {ClassName} from {AssemblyPath}", className, assemblyPath);

            // Load the assembly
            var assembly = Assembly.LoadFrom(assemblyPath);
            
            // Get the type
            var type = assembly.GetType($"Codefix.AIPlayGround.GeneratedAgents.{className}");
            if (type == null)
            {
                return new AgentInstanceResult
                {
                    IsSuccess = false,
                    Message = $"Class {className} not found in assembly"
                };
            }

            // Create instance
            var instance = Activator.CreateInstance(type);
            if (instance == null)
            {
                return new AgentInstanceResult
                {
                    IsSuccess = false,
                    Message = "Failed to create instance"
                };
            }

            // Set parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var property = type.GetProperty(param.Key);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(instance, param.Value);
                    }
                }
            }

            var instanceId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("Successfully created agent instance: {InstanceId}", instanceId);

            return new AgentInstanceResult
            {
                IsSuccess = true,
                AgentInstance = instance,
                InstanceId = instanceId,
                Message = "Agent instance created successfully",
                Metadata = new Dictionary<string, object>
                {
                    { "ClassName", className },
                    { "AssemblyPath", assemblyPath },
                    { "CreatedAt", DateTime.UtcNow }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating agent instance: {ClassName}", className);
            return new AgentInstanceResult
            {
                IsSuccess = false,
                Message = $"Failed to create agent instance: {ex.Message}"
            };
        }
    }

    public async Task<ExecutionResult> ExecuteAgentMethodAsync(object agentInstance, string methodName, object[]? parameters = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Executing method: {MethodName} on agent instance", methodName);

            var method = agentInstance.GetType().GetMethod(methodName);
            if (method == null)
            {
                return new ExecutionResult
                {
                    IsSuccess = false,
                    Message = $"Method {methodName} not found"
                };
            }

            // Execute the method
            var result = method.Invoke(agentInstance, parameters);
            
            // Handle async methods
            if (result is Task task)
            {
                await task;
                
                // Get result from Task<T>
                if (task.GetType().IsGenericType)
                {
                    var property = task.GetType().GetProperty("Result");
                    result = property?.GetValue(task);
                }
            }

            stopwatch.Stop();

            _logger.LogInformation("Successfully executed method: {MethodName} in {ElapsedMs}ms", 
                methodName, stopwatch.ElapsedMilliseconds);

            return new ExecutionResult
            {
                IsSuccess = true,
                Result = result,
                Message = "Method executed successfully",
                ExecutionTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object>
                {
                    { "MethodName", methodName },
                    { "ExecutedAt", DateTime.UtcNow },
                    { "ElapsedMilliseconds", stopwatch.ElapsedMilliseconds }
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error executing method: {MethodName}", methodName);
            
            return new ExecutionResult
            {
                IsSuccess = false,
                Message = $"Method execution failed: {ex.Message}",
                ExecutionTime = stopwatch.Elapsed
            };
        }
    }

    public async Task<ValidationResult> ValidateGeneratedCodeAsync(string code)
    {
        try
        {
            _logger.LogInformation("Validating generated code");

            var errors = new List<string>();
            var warnings = new List<string>();

            // Basic syntax validation (simplified)
            if (string.IsNullOrWhiteSpace(code))
            {
                errors.Add("Code cannot be empty");
            }

            if (!code.Contains("class"))
            {
                errors.Add("Code must contain a class definition");
            }

            if (!code.Contains("namespace"))
            {
                warnings.Add("Code should contain a namespace declaration");
            }

            // Security validation
            if (code.Contains("System.IO.File") || code.Contains("System.IO.Directory"))
            {
                warnings.Add("Code contains file system access - ensure proper security measures");
            }

            if (code.Contains("System.Net") || code.Contains("HttpClient"))
            {
                warnings.Add("Code contains network access - ensure proper security measures");
            }

            if (code.Contains("System.Reflection"))
            {
                warnings.Add("Code contains reflection - ensure proper security measures");
            }

            // Check for basic C# syntax patterns
            var openBraces = code.Count(c => c == '{');
            var closeBraces = code.Count(c => c == '}');
            if (openBraces != closeBraces)
            {
                errors.Add("Mismatched braces in code");
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating generated code");
            return new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { $"Validation failed: {ex.Message}" }
            };
        }
    }

    public async Task<List<CodeTemplate>> GetAvailableTemplatesAsync()
    {
        return await Task.FromResult(_templates);
    }

    public async Task<CodeGenerationResult> GenerateFromTemplateAsync(string templateName, Dictionary<string, object> parameters)
    {
        try
        {
            var template = _templates.FirstOrDefault(t => t.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase));
            if (template == null)
            {
                return new CodeGenerationResult
                {
                    IsSuccess = false,
                    Message = $"Template '{templateName}' not found"
                };
            }

            // Apply parameters to template
            var code = template.Template;
            foreach (var param in parameters)
            {
                code = code.Replace($"{{{param.Key}}}", param.Value?.ToString() ?? "");
            }

            return new CodeGenerationResult
            {
                IsSuccess = true,
                GeneratedCode = code,
                Message = "Code generated from template successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating code from template: {TemplateName}", templateName);
            return new CodeGenerationResult
            {
                IsSuccess = false,
                Message = $"Template generation failed: {ex.Message}"
            };
        }
    }

    private string GenerateProperty(AgentProperty property)
    {
        var builder = new StringBuilder();
        
        // Add attributes
        foreach (var attr in property.Attributes)
        {
            builder.AppendLine($"        [{attr}]");
        }
        
        // Add property declaration
        var getter = property.HasGetter ? "get;" : "";
        var setter = property.HasSetter ? "set;" : "";
        var defaultValue = property.DefaultValue != null ? $" = {property.DefaultValue};" : "";
        
        builder.AppendLine($"        {property.AccessModifier} {property.Type} {property.Name} {{ {getter} {setter} }}{defaultValue}");
        
        return builder.ToString();
    }

    private string GenerateMethod(AgentMethod method)
    {
        var builder = new StringBuilder();
        
        // Add attributes
        foreach (var attr in method.Attributes)
        {
            builder.AppendLine($"        [{attr}]");
        }
        
        // Add method signature
        var parameters = string.Join(", ", method.Parameters.Select(p => 
            $"{p.Type} {p.Name}{(p.IsOptional ? " = " + (p.DefaultValue?.ToString() ?? "default") : "")}"));
        
        var asyncKeyword = method.IsAsync ? "async " : "";
        var returnType = method.IsAsync ? method.ReturnType : method.ReturnType;
        
        builder.AppendLine($"        {method.AccessModifier} {asyncKeyword}{returnType} {method.Name}({parameters})");
        builder.AppendLine("        {");
        
        // Add method body
        if (!string.IsNullOrEmpty(method.Body))
        {
            var bodyLines = method.Body.Split('\n');
            foreach (var line in bodyLines)
            {
                builder.AppendLine($"            {line}");
            }
        }
        else
        {
            builder.AppendLine("            // Method implementation");
        }
        
        builder.AppendLine("        }");
        
        return builder.ToString();
    }

    private List<object> GetRequiredReferences()
    {
        // This method is not used in the simplified implementation
        // In a full implementation, you would return actual metadata references
        return new List<object>();
    }

    private List<CodeTemplate> InitializeTemplates()
    {
        return new List<CodeTemplate>
        {
            new CodeTemplate
            {
                Name = "DataProcessorAgent",
                Description = "Agent for processing and transforming data",
                Category = "Data Processing",
                Template = @"using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace Codefix.AIPlayGround.GeneratedAgents
{
    [GeneratedAgent]
    public class {AgentName} : BaseAgent
    {
        public string ProcessingType {{ get; set; }} = ""{ProcessingType}"";
        public int BatchSize {{ get; set; }} = {BatchSize};
        
        public async Task<object> ProcessDataAsync(object input)
        {
            // Data processing logic
            var data = JsonSerializer.Serialize(input);
            Console.WriteLine($""Processing data: {{data}}"");
            
            // Simulate processing
            await Task.Delay(100);
            
            return new {{ ProcessedData = data, ProcessedAt = DateTime.UtcNow }};
        }
        
        public async Task<List<object>> ProcessBatchAsync(List<object> inputs)
        {
            var results = new List<object>();
            
            for (int i = 0; i < inputs.Count; i += BatchSize)
            {
                var batch = inputs.GetRange(i, Math.Min(BatchSize, inputs.Count - i));
                var batchResult = await ProcessDataAsync(batch);
                results.Add(batchResult);
            }
            
            return results;
        }
    }
}",
                Parameters = new List<CodeTemplateParameter>
                {
                    new() { Name = "AgentName", Type = "string", IsRequired = true, Description = "Name of the agent class" },
                    new() { Name = "ProcessingType", Type = "string", DefaultValue = "default", Description = "Type of data processing" },
                    new() { Name = "BatchSize", Type = "int", DefaultValue = "10", Description = "Batch size for processing" }
                }
            },
            new CodeTemplate
            {
                Name = "APIIntegrationAgent",
                Description = "Agent for integrating with external APIs",
                Category = "Integration",
                Template = @"using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace Codefix.AIPlayGround.GeneratedAgents
{
    [GeneratedAgent]
    public class {AgentName} : BaseAgent
    {
        private readonly HttpClient _httpClient;
        public string ApiEndpoint {{ get; set; }} = ""{ApiEndpoint}"";
        public string ApiKey {{ get; set; }} = ""{ApiKey}"";
        
        public {AgentName}()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add(""Authorization"", $""Bearer {{ApiKey}}"");
        }
        
        public async Task<object> CallApiAsync(string endpoint, object data = null)
        {
            try
            {
                var url = $""{{ApiEndpoint}}/{{endpoint}}"";
                var json = data != null ? JsonSerializer.Serialize(data) : null;
                
                HttpResponseMessage response;
                if (data != null)
                {
                    var content = new StringContent(json, System.Text.Encoding.UTF8, ""application/json"");
                    response = await _httpClient.PostAsync(url, content);
                }
                else
                {
                    response = await _httpClient.GetAsync(url);
                }
                
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                
                return JsonSerializer.Deserialize<object>(responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($""API call failed: {{ex.Message}}"");
                throw;
            }
        }
        
        public async Task<List<object>> CallApiBatchAsync(List<string> endpoints)
        {
            var tasks = endpoints.Select(endpoint => CallApiAsync(endpoint));
            var results = await Task.WhenAll(tasks);
            return new List<object>(results);
        }
    }
}",
                Parameters = new List<CodeTemplateParameter>
                {
                    new() { Name = "AgentName", Type = "string", IsRequired = true, Description = "Name of the agent class" },
                    new() { Name = "ApiEndpoint", Type = "string", IsRequired = true, Description = "Base API endpoint URL" },
                    new() { Name = "ApiKey", Type = "string", IsRequired = true, Description = "API authentication key" }
                }
            },
            new CodeTemplate
            {
                Name = "WorkflowAgent",
                Description = "Agent for executing workflow steps",
                Category = "Workflow",
                Template = @"using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace Codefix.AIPlayGround.GeneratedAgents
{
    [GeneratedAgent]
    public class {AgentName} : BaseAgent
    {
        public List<string> Steps {{ get; set; }} = new List<string> { {Steps} };
        public int CurrentStep {{ get; set; }} = 0;
        public Dictionary<string, object> Context {{ get; set; }} = new Dictionary<string, object>();
        
        public async Task<object> ExecuteStepAsync(string stepName, object input = null)
        {
            Console.WriteLine($""Executing step: {{stepName}}"");
            
            // Add step-specific logic here
            switch (stepName.ToLower())
            {
                case ""validate"":
                    return await ValidateInputAsync(input);
                case ""process"":
                    return await ProcessDataAsync(input);
                case ""transform"":
                    return await TransformDataAsync(input);
                default:
                    return await DefaultStepAsync(stepName, input);
            }
        }
        
        public async Task<object> ExecuteWorkflowAsync(object input)
        {
            var result = input;
            
            foreach (var step in Steps)
            {
                result = await ExecuteStepAsync(step, result);
                CurrentStep++;
            }
            
            return result;
        }
        
        private async Task<object> ValidateInputAsync(object input)
        {
            // Validation logic
            await Task.Delay(50);
            return input;
        }
        
        private async Task<object> ProcessDataAsync(object input)
        {
            // Processing logic
            await Task.Delay(100);
            return input;
        }
        
        private async Task<object> TransformDataAsync(object input)
        {
            // Transformation logic
            await Task.Delay(75);
            return input;
        }
        
        private async Task<object> DefaultStepAsync(string stepName, object input)
        {
            Console.WriteLine($""Executing default step: {{stepName}}"");
            await Task.Delay(25);
            return input;
        }
    }
}",
                Parameters = new List<CodeTemplateParameter>
                {
                    new() { Name = "AgentName", Type = "string", IsRequired = true, Description = "Name of the agent class" },
                    new() { Name = "Steps", Type = "string", DefaultValue = "\"validate\", \"process\", \"transform\"", Description = "Comma-separated list of workflow steps" }
                }
            }
        };
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clean up temporary directory: {TempDirectory}", _tempDirectory);
        }
    }
}
