# Local AI Models Implementation Guide

## Overview

Your application now has **full support** for OpenLLaMA, local models, and Docker Model Runner integration using the **Microsoft Agent Framework**! 🚀

## What Was Added

### 1. **New AI Provider Types**

Enhanced `AIProviderType` enum with:
- ✅ **DockerModelRunner** - Docker's native AI model hosting
- ✅ **LocalModel** - Generic local model provider (custom Docker containers)
- ✅ **LMStudio** - LM Studio local provider
- ✅ **Ollama** - Enhanced with full implementation

### 2. **New Services**

#### DockerModelRunnerService
**File:** `Services/DockerModelRunnerService.cs`

Full Microsoft Agent Framework integration for Docker Model Runner:
- Model discovery
- Agent creation
- Chat execution (sync & streaming)
- Connection testing
- Model pulling

**Supported Models:**
- OpenLLaMA 7B & 3B
- Llama 2 (7B, 13B)
- Mistral 7B
- Code Llama 7B
- Phi-2 (2.7B)

#### OllamaIntegrationService
**File:** `Services/OllamaIntegrationService.cs`

Complete Ollama integration with Microsoft Agent Framework:
- Automatic model discovery
- Full streaming support
- OpenAI-compatible API integration
- Health monitoring

**Supported Models:**
- **OpenLLaMA** (3B, 7B) ⭐
- Llama 2 (7B, 13B, 70B)
- Mistral (7B, 7B-instruct)
- Code Llama (7B, 13B)
- Phi (2.7B)
- Gemma (7B)

#### LocalModelsController
**File:** `Controllers/LocalModelsController.cs`

New REST API endpoints for local model management:

```http
GET  /api/localmodels/discover              # Discover all local models
GET  /api/localmodels/ollama/discover       # Discover Ollama models
GET  /api/localmodels/docker/discover       # Discover Docker models
GET  /api/localmodels/ollama/test           # Test Ollama connection
GET  /api/localmodels/docker/test           # Test Docker connection
POST /api/localmodels/ollama/pull           # Pull Ollama model
POST /api/localmodels/docker/pull           # Pull Docker model
GET  /api/localmodels/health                # Health check all providers
GET  /api/localmodels/openllama/recommended # Get recommended OpenLLaMA models
POST /api/localmodels/test-model            # Test a specific model
```

### 3. **Updated Services**

#### UnifiedAIProviderService
Enhanced with:
- Full Ollama implementation
- Docker Model Runner implementation
- LocalModel generic support
- LM Studio support
- Automatic model discovery
- Enhanced error handling

## How to Use

### Prerequisites

1. **Install Ollama** (for OpenLLaMA and other local models)
   ```bash
   # Windows
   winget install Ollama.Ollama
   
   # Or download from https://ollama.ai/
   ```

2. **Install Docker Desktop** (for Docker Model Runner)
   ```bash
   # Download from https://www.docker.com/products/docker-desktop/
   ```

3. **Enable Docker Model Runner**
   - Follow: https://docs.docker.com/ai/model-runner/
   - Ensure Docker daemon is accessible at `http://localhost:2375`

### Quick Start: OpenLLaMA with Ollama

#### Step 1: Pull OpenLLaMA Model
```bash
# Pull OpenLLaMA 7B (recommended)
ollama pull openllama:7b

# Or pull the smaller 3B version
ollama pull openllama:3b
```

#### Step 2: Verify Ollama is Running
```bash
# Check if Ollama is running
ollama list

# Expected output should show openllama models
```

#### Step 3: Test via API

```http
### Discover available Ollama models
GET http://localhost:5000/api/localmodels/ollama/discover

### Test Ollama connection
GET http://localhost:5000/api/localmodels/ollama/test

### Get recommended OpenLLaMA models
GET http://localhost:5000/api/localmodels/openllama/recommended
```

#### Step 4: Create an Agent with OpenLLaMA

Using the API:

```http
### Create OpenLLaMA agent
POST http://localhost:5000/api/agents
Content-Type: application/json

{
  "name": "OpenLLaMA Assistant",
  "description": "Local AI assistant using OpenLLaMA 7B",
  "providerType": "Ollama",
  "modelName": "openllama:7b",
  "config": {
    "temperature": 0.7,
    "maxTokens": 2000
  }
}
```

Using C# code:

```csharp
// Inject the service
private readonly IOllamaIntegrationService _ollamaService;

// Create an agent
var agent = await _ollamaService.CreateAgentAsync(
    name: "OpenLLaMA Assistant",
    description: "Local AI assistant",
    modelName: "openllama:7b",
    config: new Dictionary<string, object>
    {
        ["Temperature"] = 0.7,
        ["MaxTokens"] = 2000
    });

// Execute chat
var response = await _ollamaService.ExecuteChatAsync(
    agent.Id, 
    "Hello! Tell me about yourself.");

Console.WriteLine(response.Message);
```

### Quick Start: Docker Model Runner

#### Step 1: Pull a Model via Docker

```bash
# Pull OpenLLaMA via Docker Model Runner
docker model pull openllama-7b
```

#### Step 2: Test via API

```http
### Discover Docker Model Runner models
GET http://localhost:5000/api/localmodels/docker/discover

### Test Docker Model Runner connection
GET http://localhost:5000/api/localmodels/docker/test
```

#### Step 3: Create an Agent with Docker Model Runner

```http
### Create Docker Model Runner agent
POST http://localhost:5000/api/agents
Content-Type: application/json

{
  "name": "Docker OpenLLaMA Assistant",
  "description": "AI assistant using Docker Model Runner",
  "providerType": "DockerModelRunner",
  "modelName": "openllama-7b",
  "config": {
    "temperature": 0.7,
    "maxTokens": 2000
  }
}
```

### Model Management

#### Pull Models Programmatically

```http
### Pull OpenLLaMA via Ollama
POST http://localhost:5000/api/localmodels/ollama/pull
Content-Type: application/json

{
  "modelName": "openllama:7b"
}

### Pull model via Docker
POST http://localhost:5000/api/localmodels/docker/pull
Content-Type: application/json

{
  "modelName": "openllama-7b"
}
```

#### Health Monitoring

```http
### Check health of all local providers
GET http://localhost:5000/api/localmodels/health
```

Response:
```json
{
  "isHealthy": true,
  "providers": {
    "Ollama": {
      "isAvailable": true,
      "message": "Ollama is accessible. Found 5 models.",
      "responseTimeMs": 45.2,
      "modelCount": 5,
      "endpoint": "http://localhost:11434"
    },
    "DockerModelRunner": {
      "isAvailable": true,
      "message": "Docker Model Runner is accessible. Found 3 models.",
      "responseTimeMs": 32.8,
      "modelCount": 3,
      "endpoint": "http://localhost:2375"
    }
  },
  "availableProviders": ["Ollama", "DockerModelRunner"],
  "checkTime": "2025-10-24T12:00:00Z"
}
```

## Recommended OpenLLaMA Models

| Model | Provider | Size | Parameters | Use Case | Min RAM |
|-------|----------|------|------------|----------|---------|
| `openllama:7b` | Ollama | 4.1GB | 7B | General text generation, Q&A | 8GB |
| `openllama:3b` | Ollama | 1.9GB | 3B | Quick responses, simple tasks | 4GB |
| `openllama-7b` | Docker | 4.1GB | 7B | Containerized deployments | 8GB |

## Architecture

### Microsoft Agent Framework Integration

All services follow Microsoft Agent Framework patterns:

```
┌─────────────────────────────────────────────────────────┐
│         Microsoft Agent Framework Core                  │
│  (Workflows, Checkpoints, Memory, Orchestration)        │
└─────────────────────────────────────────────────────────┘
                          ▲
                          │
┌─────────────────────────────────────────────────────────┐
│         UnifiedAIProviderService                        │
│  (Single interface for all AI providers)                │
└─────────────────────────────────────────────────────────┘
                          ▲
           ┌──────────────┼──────────────┐
           │              │              │
     ┌─────────┐   ┌────────────┐  ┌──────────┐
     │ Ollama  │   │  Docker    │  │ PeerLLM  │
     │ Service │   │  Service   │  │ Service  │
     └─────────┘   └────────────┘  └──────────┘
           │              │              │
     ┌─────────┐   ┌────────────┐  ┌──────────┐
     │OpenLLaMA│   │ OpenLLaMA  │  │ Mistral  │
     │ Llama2  │   │ in Docker  │  │ etc.     │
     └─────────┘   └────────────┘  └──────────┘
```

### Key Features

✅ **Microsoft Agent Framework Native**
- All services implement Microsoft Agent Framework patterns
- Full support for workflows, checkpoints, and memory
- Compatible with Microsoft's official samples

✅ **Multiple Provider Support**
- Ollama (local models)
- Docker Model Runner (containerized models)
- PeerLLM (decentralized)
- OpenAI, Azure OpenAI, Anthropic, Google AI

✅ **OpenLLaMA Ready**
- Pre-configured OpenLLaMA 7B and 3B support
- Automatic model discovery
- Pull and deploy with single API call

✅ **Production Ready**
- Health monitoring
- Error handling
- Streaming support
- Connection pooling

## Configuration

### Default Endpoints

```csharp
// Ollama
Endpoint: http://localhost:11434/api/generate
API Key: Not required

// Docker Model Runner
Endpoint: http://localhost:2375/v1/chat/completions
API Key: Not required

// LM Studio
Endpoint: http://localhost:1234/v1/chat/completions
API Key: Not required
```

### Custom Configuration

You can configure custom endpoints via `appsettings.json`:

```json
{
  "AIProviders": {
    "Ollama": {
      "Endpoint": "http://localhost:11434/api/generate"
    },
    "DockerModelRunner": {
      "Endpoint": "http://localhost:2375/v1/chat/completions"
    },
    "LMStudio": {
      "Endpoint": "http://localhost:1234/v1/chat/completions"
    }
  }
}
```

## Testing

### Test Script

Create a file `test-openllama.http` in your project:

```http
### 1. Check health of all providers
GET http://localhost:5000/api/localmodels/health

### 2. Discover Ollama models
GET http://localhost:5000/api/localmodels/ollama/discover

### 3. Get recommended OpenLLaMA models
GET http://localhost:5000/api/localmodels/openllama/recommended

### 4. Pull OpenLLaMA (if not already pulled)
POST http://localhost:5000/api/localmodels/ollama/pull
Content-Type: application/json

{
  "modelName": "openllama:7b"
}

### 5. Create OpenLLaMA agent
POST http://localhost:5000/api/agents
Content-Type: application/json

{
  "name": "OpenLLaMA Test Agent",
  "description": "Testing OpenLLaMA integration",
  "providerType": "Ollama",
  "modelName": "openllama:7b"
}

### 6. Test the agent (use agent ID from step 5)
POST http://localhost:5000/api/agents/{agentId}/chat
Content-Type: application/json

{
  "message": "Hello! Please tell me a short joke about programming."
}
```

## Troubleshooting

### Ollama Not Found

```bash
# Check if Ollama is running
ollama list

# Start Ollama if not running
# On Windows: Launch Ollama from Start Menu
# On Mac/Linux: systemctl start ollama
```

### Docker Not Accessible

```bash
# Check Docker is running
docker ps

# Verify Docker daemon is listening
curl http://localhost:2375/_ping
```

### Model Not Found

```bash
# List available models
ollama list

# Pull the model if missing
ollama pull openllama:7b
```

## Performance Tips

### GPU Acceleration

If you have an NVIDIA GPU:

```bash
# Ollama automatically uses GPU if available
ollama pull openllama:7b

# Docker Model Runner with GPU
docker model pull --gpu openllama-7b
```

### Memory Management

- **OpenLLaMA 3B**: Requires 4GB+ RAM (good for testing)
- **OpenLLaMA 7B**: Requires 8GB+ RAM (recommended for production)
- **Llama 2 13B**: Requires 16GB+ RAM (high quality)

### Quantization

All default models use Q4_0 quantization:
- 4-bit quantization
- 70-80% of original quality
- 4x less memory usage

## Next Steps

1. **Try OpenLLaMA**: Pull and test `openllama:7b`
2. **Explore Docker Model Runner**: Set up containerized models
3. **Build Workflows**: Use Microsoft Agent Framework to create multi-agent workflows
4. **Monitor Performance**: Use the `/health` endpoint to track model availability

## References

- [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)
- [Docker Model Runner](https://docs.docker.com/ai/model-runner/)
- [Ollama](https://ollama.ai/)
- [OpenLLaMA](https://github.com/openlm-research/open_llama)

## Support

For issues or questions:
1. Check the API docs at `/scalar/v1` (when running in development)
2. Review the health endpoint: `GET /api/localmodels/health`
3. Check logs for detailed error messages

---

**Implementation Complete!** ✅

All services are registered in `Program.cs` and ready to use. The application now has full support for OpenLLaMA, local models, and Docker Model Runner using the Microsoft Agent Framework.

