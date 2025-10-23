using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public interface INodeExecutor
{
    /// <summary>
    /// Executes a workflow node
    /// </summary>
    /// <param name="node">The node to execute</param>
    /// <param name="inputData">Input data for the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Output data from the node execution</returns>
    Task<Dictionary<string, object>> ExecuteAsync(EnhancedWorkflowNode node, Dictionary<string, object> inputData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the node can be executed
    /// </summary>
    /// <param name="node">The node to validate</param>
    /// <returns>True if the node can be executed, false otherwise</returns>
    Task<bool> ValidateAsync(EnhancedWorkflowNode node);

    /// <summary>
    /// Gets the node type this executor handles
    /// </summary>
    string NodeType { get; }
}
