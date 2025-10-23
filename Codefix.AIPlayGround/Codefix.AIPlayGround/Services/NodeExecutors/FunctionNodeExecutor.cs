using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public class FunctionNodeExecutor : INodeExecutor
{
    private readonly ILogger<FunctionNodeExecutor> _logger;

    public FunctionNodeExecutor(ILogger<FunctionNodeExecutor> logger)
    {
        _logger = logger;
    }

    public string NodeType => "FunctionNode";

    public async Task<Dictionary<string, object>> ExecuteAsync(EnhancedWorkflowNode node, Dictionary<string, object> inputData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing FunctionNode {NodeId} ({NodeName})", node.Id, node.Name);

        // TODO: Implement custom function execution
        // This will execute user-defined functions or scripts
        
        var outputData = new Dictionary<string, object>(inputData)
        {
            ["function_result"] = "Custom function execution not yet implemented",
            ["node_id"] = node.Id,
            ["node_name"] = node.Name,
            ["node_type"] = node.Type,
            ["execution_time"] = DateTime.UtcNow,
            ["status"] = "placeholder"
        };

        // Add any node-specific properties
        foreach (var property in node.Properties)
        {
            outputData[$"node_property_{property.Key}"] = property.Value;
        }

        _logger.LogInformation("FunctionNode {NodeId} completed (placeholder)", node.Id);
        return outputData;
    }

    public async Task<bool> ValidateAsync(EnhancedWorkflowNode node)
    {
        // Function nodes should have at least one input and one output port
        if (!node.InputPorts.Any() || !node.OutputPorts.Any())
        {
            _logger.LogWarning("FunctionNode {NodeId} missing required ports", node.Id);
            return false;
        }

        return true;
    }
}
