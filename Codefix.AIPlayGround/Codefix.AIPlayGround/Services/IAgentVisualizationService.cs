using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public interface IAgentVisualizationService
{
    Task<string> GenerateWorkflowDiagramAsync(List<WorkflowNode> nodes, List<WorkflowConnection> connections);
    Task<List<WorkflowNode>> GetWorkflowNodesAsync();
    Task<List<WorkflowConnection>> GetWorkflowConnectionsAsync();
    Task SaveWorkflowAsync(string name, List<EnhancedWorkflowNode> nodes, List<EnhancedWorkflowConnection> connections);
    Task<WorkflowDefinition> LoadWorkflowAsync(string id);
}
