# Agent Factory API Documentation

## Overview

The Agent Factory provides a streamlined API for creating AI agents based on the **Microsoft Agent Framework** patterns. This implementation follows the best practices from the [Microsoft Agent Framework .NET samples](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted/Agents).

## Architecture

### Components

1. **IAgentFactory** - Factory interface defining agent creation contracts
2. **AgentFactory** - Implementation with factory methods for different agent types
3. **AgentFactoryController** - REST API controller exposing factory endpoints
4. **Agent Templates** - Pre-configured agent templates for common use cases

## API Endpoints

Base URL: `/api/agentfactory`

### 1. Create LLM Agent

Creates an LLM-based agent with specified configuration.

**Endpoint:** `POST /api/agentfactory/llm`

**Request Body:**
```json
{
  "name": "Research Assistant",
  "instructions": "You are a helpful research assistant.",
  "llmConfiguration": {
    "modelName": "gpt-4",
    "provider": "OpenAI",
    "temperature": 0.7,
    "maxTokens": 2000,
    "topP": 1.0,
    "frequencyPenalty": 0.0,
    "presencePenalty": 0.0
  }
}
```

**Response:** `201 Created`
```json
{
  "id": "agent-id-guid",
  "name": "Research Assistant",
  "description": "LLM-based agent using gpt-4",
  "agentType": "LLMAgent",
  "status": "Active",
  "createdAt": "2025-10-19T...",
  "updatedAt": "2025-10-19T...",
  "createdBy": "System"
}
```

---

### 2. Create Tool Agent

Creates a tool-enabled agent with specific tools.

**Endpoint:** `POST /api/agentfactory/tool`

**Request Body:**
```json
{
  "name": "Data Analyzer",
  "instructions": "Analyze data using available tools.",
  "tools": [
    {
      "name": "DataProcessor",
      "type": "Function",
      "description": "Process and analyze data",
      "parameters": {
        "inputFormat": "json",
        "outputFormat": "csv"
      },
      "isEnabled": true
    },
    {
      "name": "Visualizer",
      "type": "Function",
      "description": "Create data visualizations",
      "isEnabled": true
    }
  ]
}
```

**Response:** `201 Created` (Same structure as LLM Agent)

---

### 3. Create Conditional Agent

Creates a conditional routing agent for workflow branching.

**Endpoint:** `POST /api/agentfactory/conditional`

**Request Body:**
```json
{
  "name": "Content Router",
  "condition": "request.priority === 'high'",
  "instructions": "Route requests based on priority level."
}
```

---

### 4. Create Parallel Agent

Creates a parallel execution agent.

**Endpoint:** `POST /api/agentfactory/parallel`

**Request Body:**
```json
{
  "name": "Batch Processor",
  "maxConcurrency": 5
}
```

---

### 5. Create Checkpoint Agent

Creates an agent with checkpoint and recovery capabilities.

**Endpoint:** `POST /api/agentfactory/checkpoint`

**Request Body:**
```json
{
  "name": "Long Running Task Agent",
  "checkpointConfiguration": {
    "enableCheckpoints": true,
    "checkpointType": "automatic",
    "checkpointInterval": 10,
    "checkpointConditions": [],
    "enableRecovery": true
  }
}
```

---

### 6. Create MCP Agent

Creates an MCP (Model Context Protocol) enabled agent.

**Endpoint:** `POST /api/agentfactory/mcp`

**Request Body:**
```json
{
  "name": "MCP Enhanced Agent",
  "mcpServers": [
    "github-server",
    "filesystem-server",
    "database-server"
  ],
  "instructions": "Use MCP servers for enhanced functionality."
}
```

---

### 7. Get Available Templates

Retrieves all available agent templates.

**Endpoint:** `GET /api/agentfactory/templates`

**Response:** `200 OK`
```json
[
  {
    "id": "template-guid",
    "name": "ResearchAssistant",
    "description": "AI research assistant for gathering and analyzing information",
    "agentType": "LLMAgent",
    "category": "Research",
    "tags": ["research", "analysis", "information-gathering"],
    "parameters": {
      "topic": {
        "name": "topic",
        "type": "string",
        "description": "Research topic or domain",
        "isRequired": true
      }
    }
  }
]
```

**Available Templates:**
- **ResearchAssistant** - Research and analysis
- **CodeReviewer** - Automated code review
- **DataAnalyst** - Data analysis and visualization
- **CustomerSupport** - Customer support and assistance

---

### 8. Create from Template

Creates an agent from a predefined template.

**Endpoint:** `POST /api/agentfactory/from-template`

**Request Body:**
```json
{
  "templateName": "ResearchAssistant",
  "parameters": {
    "topic": "Artificial Intelligence"
  }
}
```

**Response:** `201 Created`

---

### 9. Validate Configuration

Validates agent configuration before creation.

**Endpoint:** `POST /api/agentfactory/validate`

**Request Body:** (Same as CreateAgentDto)
```json
{
  "name": "Test Agent",
  "agentType": "LLMAgent",
  "description": "Test agent",
  "instructions": "Test instructions"
}
```

**Response:** `200 OK`
```json
{
  "isValid": true,
  "errors": [],
  "warnings": [
    "An agent with name 'Test Agent' already exists"
  ],
  "suggestions": {
    "recommendedTemperature": 0.7
  }
}
```

---

### 10. Clone Agent

Clones an existing agent with optional modifications.

**Endpoint:** `POST /api/agentfactory/clone/{sourceAgentId}`

**Request Body:**
```json
{
  "newName": "Cloned Agent v2",
  "modifications": {
    "temperature": 0.5,
    "category": "Production"
  }
}
```

**Response:** `201 Created`

---

## Usage Examples

### Example 1: Create a Simple LLM Agent

```bash
curl -X POST https://localhost:5001/api/agentfactory/llm \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Assistant",
    "instructions": "You are a helpful assistant.",
    "llmConfiguration": {
      "modelName": "gpt-4",
      "temperature": 0.7
    }
  }'
```

### Example 2: Create from Template

```bash
curl -X POST https://localhost:5001/api/agentfactory/from-template \
  -H "Content-Type: application/json" \
  -d '{
    "templateName": "CodeReviewer"
  }'
```

### Example 3: Create Tool Agent with MCP

```bash
curl -X POST https://localhost:5001/api/agentfactory/mcp \
  -H "Content-Type: application/json" \
  -d '{
    "name": "GitHub Agent",
    "mcpServers": ["github-server"],
    "instructions": "Interact with GitHub repositories."
  }'
```

---

## Agent Types

### LLMAgent
- **Purpose:** General-purpose LLM-based agents
- **Use Cases:** Chatbots, assistants, content generation
- **Key Features:** Configurable model, temperature, token limits

### ToolAgent
- **Purpose:** Agents with external tool capabilities
- **Use Cases:** API integration, data processing, automation
- **Key Features:** Multiple tool support, tool chaining

### ConditionalAgent
- **Purpose:** Workflow routing and branching
- **Use Cases:** Decision trees, conditional workflows
- **Key Features:** Condition evaluation, dynamic routing

### ParallelAgent
- **Purpose:** Concurrent task execution
- **Use Cases:** Batch processing, multi-task scenarios
- **Key Features:** Concurrency control, parallel execution

### CheckpointAgent
- **Purpose:** Long-running tasks with recovery
- **Use Cases:** Complex workflows, reliable processing
- **Key Features:** Automatic checkpointing, recovery mechanism

### MCPAgent
- **Purpose:** Model Context Protocol integration
- **Use Cases:** Server integration, extended capabilities
- **Key Features:** MCP server support, protocol handling

---

## Best Practices

### 1. Naming Conventions
- Use descriptive names that indicate agent purpose
- Example: "CustomerSupportAgent" vs "Agent1"

### 2. Configuration
- Set appropriate temperature values:
  - **0.0-0.3:** Deterministic, factual responses
  - **0.4-0.7:** Balanced creativity and accuracy
  - **0.8-1.0:** Creative, varied responses

### 3. Instructions
- Provide clear, specific instructions
- Define agent behavior and boundaries
- Include example interactions when needed

### 4. Templates
- Use templates for common patterns
- Customize templates with parameters
- Create custom templates for recurring needs

### 5. Validation
- Always validate configuration before creation
- Review warnings and suggestions
- Test with sample inputs

### 6. Error Handling
- Handle API errors gracefully
- Check validation results
- Monitor agent status after creation

---

## Integration with Workflows

Agents created via the factory can be integrated into workflows:

```csharp
// Create agent via factory
var agent = await _agentFactory.CreateLLMAgentAsync(
    "Workflow Agent",
    "Process workflow steps"
);

// Add to workflow
await _workflowService.AddAgentToWorkflowAsync(
    workflowId,
    agent.Id
);
```

---

## Error Codes

| Code | Description |
|------|-------------|
| 400 | Bad Request - Invalid configuration |
| 404 | Not Found - Template or agent not found |
| 500 | Internal Server Error - Server error |

---

## Security Considerations

1. **API Keys:** Store LLM API keys securely (use environment variables or Azure Key Vault)
2. **Authentication:** Add authentication middleware for production
3. **Authorization:** Implement role-based access control
4. **Rate Limiting:** Apply rate limits to prevent abuse
5. **Input Validation:** Always validate user inputs

---

## Performance Tips

1. **Caching:** Cache frequently used templates
2. **Batch Operations:** Use batch endpoints when available
3. **Async Operations:** All operations are async for better performance
4. **Connection Pooling:** Database connections are pooled
5. **Monitoring:** Monitor agent creation and execution metrics

---

## References

- [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)
- [.NET Agent Samples](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/GettingStarted/Agents)
- [Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)

---

## Support

For issues or questions:
1. Check the validation endpoint for configuration issues
2. Review agent status after creation
3. Check logs for detailed error messages
4. Consult the Microsoft Agent Framework documentation

---

**Version:** 1.0  
**Last Updated:** October 19, 2025  
**Status:** Production Ready ✅

