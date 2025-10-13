using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public interface IEnhancedWorkflowService
{
    // Workflow Management
    Task<Models.WorkflowDefinition> CreateWorkflowAsync(string name, string description = "");
    Task<Models.WorkflowDefinition> GetWorkflowAsync(string id);
    Task<List<Models.WorkflowDefinition>> GetAllWorkflowsAsync();
    Task<Models.WorkflowDefinition> UpdateWorkflowAsync(Models.WorkflowDefinition workflow);
    Task<bool> DeleteWorkflowAsync(string id);
    
    // Node Management
    Task<EnhancedWorkflowNode> AddNodeAsync(string workflowId, AgentType nodeType, double x, double y);
    Task<EnhancedWorkflowNode> UpdateNodeAsync(string workflowId, EnhancedWorkflowNode node);
    Task<bool> RemoveNodeAsync(string workflowId, string nodeId);
    Task<List<EnhancedWorkflowNode>> GetWorkflowNodesAsync(string workflowId);
    
    // Connection Management
    Task<EnhancedWorkflowConnection> AddConnectionAsync(string workflowId, string fromNodeId, string toNodeId, ConnectionType connectionType = ConnectionType.DataFlow);
    Task<EnhancedWorkflowConnection> UpdateConnectionAsync(string workflowId, EnhancedWorkflowConnection connection);
    Task<bool> RemoveConnectionAsync(string workflowId, string connectionId);
    Task<List<EnhancedWorkflowConnection>> GetWorkflowConnectionsAsync(string workflowId);
    
    // Mermaid Integration
    Task<string> GenerateMermaidDiagramAsync(string workflowId);
    Task<Models.WorkflowDefinition> ParseMermaidDiagramAsync(string mermaidContent);
    
    // File Persistence
    Task SaveWorkflowToFileAsync(string workflowId, string filePath);
    Task<Models.WorkflowDefinition> LoadWorkflowFromFileAsync(string filePath);
    Task<List<string>> GetSavedWorkflowFilesAsync();
    
    // Validation
    Task<List<string>> ValidateWorkflowAsync(string workflowId);
    Task<bool> ValidateConnectionAsync(string workflowId, string fromNodeId, string toNodeId);
}
