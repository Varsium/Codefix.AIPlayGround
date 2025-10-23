using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public class EndNodeExecutor : INodeExecutor
{
    private readonly ILogger<EndNodeExecutor> _logger;

    public EndNodeExecutor(ILogger<EndNodeExecutor> logger)
    {
        _logger = logger;
    }

    public string NodeType => "EndNode";

    public async Task<Dictionary<string, object>> ExecuteAsync(EnhancedWorkflowNode node, Dictionary<string, object> inputData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing EndNode {NodeId} ({NodeName})", node.Id, node.Name);

        // End nodes finalize the workflow execution
        var outputData = new Dictionary<string, object>(inputData)
        {
            ["workflow_completed"] = DateTime.UtcNow,
            ["node_id"] = node.Id,
            ["node_name"] = node.Name,
            ["node_type"] = node.Type,
            ["execution_summary"] = new
            {
                total_inputs = inputData.Count,
                execution_time = DateTime.UtcNow,
                final_node = true
            }
        };

        // Add any node-specific properties
        foreach (var property in node.Properties)
        {
            outputData[$"node_property_{property.Key}"] = property.Value;
        }

        _logger.LogInformation("EndNode {NodeId} completed successfully - Workflow execution finished", node.Id);
        return outputData;
    }

    public async Task<bool> ValidateAsync(EnhancedWorkflowNode node)
    {
        // End nodes should have at least one input port
        if (!node.InputPorts.Any())
        {
            _logger.LogWarning("EndNode {NodeId} has no input ports", node.Id);
            return false;
        }

        return true;
    }
}
