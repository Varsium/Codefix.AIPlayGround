using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public class ConditionalAgentExecutor : INodeExecutor
{
    private readonly ILogger<ConditionalAgentExecutor> _logger;

    public ConditionalAgentExecutor(ILogger<ConditionalAgentExecutor> logger)
    {
        _logger = logger;
    }

    public string NodeType => "ConditionalAgent";

    public async Task<Dictionary<string, object>> ExecuteAsync(EnhancedWorkflowNode node, Dictionary<string, object> inputData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing ConditionalAgent {NodeId} ({NodeName})", node.Id, node.Name);

        // TODO: Implement conditional logic
        // This will evaluate conditions and determine which output port to use
        
        var outputData = new Dictionary<string, object>(inputData)
        {
            ["conditional_result"] = "Conditional logic not yet implemented",
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

        _logger.LogInformation("ConditionalAgent {NodeId} completed (placeholder)", node.Id);
        return outputData;
    }

    public async Task<bool> ValidateAsync(EnhancedWorkflowNode node)
    {
        // Conditional agents should have multiple output ports (true/false)
        if (node.OutputPorts.Count < 2)
        {
            _logger.LogWarning("ConditionalAgent {NodeId} should have at least 2 output ports", node.Id);
            return false;
        }

        return true;
    }
}
