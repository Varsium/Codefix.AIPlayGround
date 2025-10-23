using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public class StartNodeExecutor : INodeExecutor
{
    private readonly ILogger<StartNodeExecutor> _logger;

    public StartNodeExecutor(ILogger<StartNodeExecutor> logger)
    {
        _logger = logger;
    }

    public string NodeType => "StartNode";

    public async Task<Dictionary<string, object>> ExecuteAsync(EnhancedWorkflowNode node, Dictionary<string, object> inputData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing StartNode {NodeId} ({NodeName})", node.Id, node.Name);

        // Start nodes typically just pass through the input data
        // They can also initialize workflow-specific data
        var outputData = new Dictionary<string, object>(inputData)
        {
            ["workflow_started"] = DateTime.UtcNow,
            ["node_id"] = node.Id,
            ["node_name"] = node.Name,
            ["node_type"] = node.Type
        };

        // Add any node-specific properties
        foreach (var property in node.Properties)
        {
            outputData[$"node_property_{property.Key}"] = property.Value;
        }

        _logger.LogInformation("StartNode {NodeId} completed successfully", node.Id);
        return outputData;
    }

    public async Task<bool> ValidateAsync(EnhancedWorkflowNode node)
    {
        // Start nodes should have at least one output port
        if (!node.OutputPorts.Any())
        {
            _logger.LogWarning("StartNode {NodeId} has no output ports", node.Id);
            return false;
        }

        return true;
    }
}
