using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Models.DTOs;
using Microsoft.Extensions.Options;

namespace Codefix.AIPlayGround.Services;

public class LLMAgentExecutor : INodeExecutor
{
    private readonly ILogger<LLMAgentExecutor> _logger;
    private readonly IChatService _chatService;

    public LLMAgentExecutor(ILogger<LLMAgentExecutor> logger, IChatService chatService)
    {
        _logger = logger;
        _chatService = chatService;
    }

    public string NodeType => "LLMAgent";

    public async Task<Dictionary<string, object>> ExecuteAsync(EnhancedWorkflowNode node, Dictionary<string, object> inputData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing LLMAgent {NodeId} ({NodeName})", node.Id, node.Name);

        try
        {
            // Get agent definition from node
            var agentDefinition = node.AgentDefinition;
            if (agentDefinition == null)
            {
                throw new InvalidOperationException($"No agent definition found for node {node.Id}");
            }

            // Prepare the input message
            var inputMessage = PrepareInputMessage(inputData, agentDefinition);
            
            // Create a chat session for this execution
            var sessionId = Guid.NewGuid().ToString();
            
            // Create a send message request
            var sendRequest = new SendMessageRequest
            {
                SessionId = sessionId,
                Message = inputMessage,
                Context = new Dictionary<string, object>()
            };
            
            // Send message to LLM (using a dummy userId for now)
            var response = await _chatService.SendMessageAsync(sendRequest, "workflow-execution");
            
            // Process the response
            var outputData = new Dictionary<string, object>(inputData)
            {
                ["llm_response"] = response.AgentMessage.Text,
                ["node_id"] = node.Id,
                ["node_name"] = node.Name,
                ["node_type"] = node.Type,
                ["agent_name"] = agentDefinition.Name,
                ["execution_time"] = DateTime.UtcNow,
                ["session_id"] = sessionId
            };

            // Add any node-specific properties
            foreach (var property in node.Properties)
            {
                outputData[$"node_property_{property.Key}"] = property.Value;
            }

            _logger.LogInformation("LLMAgent {NodeId} completed successfully", node.Id);
            return outputData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing LLMAgent {NodeId}", node.Id);
            throw;
        }
    }

    public async Task<bool> ValidateAsync(EnhancedWorkflowNode node)
    {
        // LLM agents should have agent definition
        if (node.AgentDefinition == null)
        {
            _logger.LogWarning("LLMAgent {NodeId} has no agent definition", node.Id);
            return false;
        }

        // Should have at least one input and one output port
        if (!node.InputPorts.Any() || !node.OutputPorts.Any())
        {
            _logger.LogWarning("LLMAgent {NodeId} missing required ports", node.Id);
            return false;
        }

        return true;
    }

    private string PrepareInputMessage(Dictionary<string, object> inputData, AgentDefinition agentDefinition)
    {
        var message = new System.Text.StringBuilder();
        
        // Add agent-specific prompt if available
        if (agentDefinition.PromptTemplate != null)
        {
            if (!string.IsNullOrEmpty(agentDefinition.PromptTemplate.SystemPrompt))
            {
                message.AppendLine("System: " + agentDefinition.PromptTemplate.SystemPrompt);
            }
            if (!string.IsNullOrEmpty(agentDefinition.PromptTemplate.UserPrompt))
            {
                message.AppendLine("User: " + agentDefinition.PromptTemplate.UserPrompt);
            }
            if (!string.IsNullOrEmpty(agentDefinition.PromptTemplate.AssistantPrompt))
            {
                message.AppendLine("Assistant: " + agentDefinition.PromptTemplate.AssistantPrompt);
            }
            message.AppendLine();
        }

        // Add input data context
        message.AppendLine("Input Data:");
        foreach (var kvp in inputData)
        {
            message.AppendLine($"- {kvp.Key}: {kvp.Value}");
        }

        // Add agent description
        if (!string.IsNullOrEmpty(agentDefinition.Description))
        {
            message.AppendLine();
            message.AppendLine($"Agent Role: {agentDefinition.Description}");
        }

        return message.ToString();
    }
}
