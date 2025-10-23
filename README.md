# Codefix AI PlayGround 🚀

> **The Future of AI Agent Development: Where Low-Code Meets Pro-Code**

A revolutionary platform that bridges the gap between visual workflow design and professional code development, enabling you to create sophisticated AI agents and workflows through both intuitive drag-and-drop interfaces and traditional programming approaches.

## 🌟 Vision

**The Dream**: Create a hybrid development environment where you can build AI agents and workflows without writing a single line of code, while seamlessly integrating with professional codebases. When you add new methods, tools, or capabilities in your code, they automatically become available in the visual interface, creating a true **hybrid low-code/pro-code development experience**.

## 🎯 What Makes This Special

### 🎨 **Visual Flow Builder**
- **Drag-and-drop workflow creation** - Build complex AI agent workflows without coding
- **Real-time visual feedback** - See your workflows come to life as you design them
- **Intuitive node-based interface** - Connect agents, tools, and logic flows visually
- **Mermaid diagram generation** - Automatically generate documentation from your workflows

### 🔧 **Pro-Code Integration**
- **Automatic code detection** - New methods and tools in your codebase are automatically discovered
- **Seamless tool integration** - Your custom functions become available in the visual builder
- **Hybrid development** - Switch between visual and code approaches as needed
- **Code-generated agents** - Generate complete agent implementations from visual workflows

### 🤖 **AI Agent Framework**
- **Multiple agent types** - LLM agents, tool agents, conditional agents, parallel processing
- **Flexible configuration** - Customize prompts, memory, checkpoints, and execution modes
- **Real-time execution** - Run and test your workflows immediately
- **Extensible architecture** - Add new agent types and capabilities easily

## 🏗️ Architecture

### Core Components

```
┌─────────────────────────────────────────────────────────────┐
│                    AI PlayGround Platform                  │
├─────────────────────────────────────────────────────────────┤
│  Visual Workflow Builder  │  Pro-Code Integration Layer   │
│  ┌─────────────────────┐  │  ┌──────────────────────────┐  │
│  │ • Drag & Drop UI    │  │  │ • Code Detection         │  │
│  │ • Node Connections  │  │  │ • Tool Discovery         │  │
│  │ • Real-time Preview │  │  │ • Method Integration     │  │
│  │ • Mermaid Export    │  │  │ • Auto-sync Capabilities │  │
│  └─────────────────────┘  │  └──────────────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│              Agent Framework & Execution Engine            │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │ • LLM Agents    • Tool Agents    • Conditional Logic   │ │
│  │ • Parallel Exec • Checkpoints    • Memory Management   │ │
│  │ • MCP Support   • Custom Nodes   • Real-time Execution │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Technology Stack

- **Frontend**: Blazor Server/WASM with modern UI components
- **Backend**: .NET 10 with Entity Framework Core
- **Database**: SQL Server with workflow persistence
- **AI Integration**: OpenAI and Azure OpenAI with Microsoft Agent Framework
- **Visualization**: Custom canvas with drag-and-drop functionality
- **Documentation**: Mermaid diagram generation
- **Authentication**: ASP.NET Core Identity

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- OpenAI API key or Azure OpenAI credentials

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/Codefix.AIPlayGround.git
   cd Codefix.AIPlayGround
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure database**
   ```bash
   dotnet ef database update --project Codefix.AIPlayGround
   ```

4. **Configure API keys** in `appsettings.json`:
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-openai-api-key"
     },
     "AzureOpenAI": {
       "ApiKey": "your-azure-openai-key",
       "Endpoint": "https://your-resource.openai.azure.com"
     }
   }
   ```

5. **Run the application**
   ```bash
   dotnet run --project Codefix.AIPlayGround
   ```

6. **Open your browser** to `https://localhost:5001`

## 🎮 How It Works

### 1. **Low-Code Approach** - Visual Workflow Creation

Start with the visual workflow builder:

1. **Drag nodes** from the palette onto the canvas ✅
2. **Connect nodes** to define data flow and execution order ✅
3. **Configure agents** with prompts, tools, and settings ✅
4. **Test workflows** in real-time (limited - chat interface available)
5. **Export to Mermaid** for documentation ✅

**Example Workflow Types:**
- Customer service chatbots
- Data processing pipelines
- Content generation workflows
- Decision trees and conditional logic
- Parallel processing tasks

### 2. **Pro-Code Approach** - Traditional Development

Develop custom capabilities in code:

```csharp
// Add a new tool to your codebase
public class CustomDataProcessor
{
    [Tool("ProcessCustomerData")]
    public async Task<ProcessedData> ProcessCustomerDataAsync(CustomerData input)
    {
        // Your custom logic here
        return new ProcessedData { /* processed data */ };
    }
}
```

### 3. **Hybrid Magic** - Best of Both Worlds (Planned)

The platform will automatically:
- **Detect** new methods and tools in your code (not yet implemented)
- **Make them available** in the visual builder (not yet implemented)
- **Generate** visual nodes for your custom functions (not yet implemented)
- **Enable** seamless integration between visual and code approaches (not yet implemented)

## 🔧 Key Features

### Visual Workflow Builder
- **Drag-and-drop interface** for creating complex workflows ✅
- **Real-time execution** and testing (limited - chat interface available)
- **Mermaid diagram generation** for documentation ✅
- **Node-based architecture** with multiple agent types ✅
- **Connection management** with data flow visualization ✅

### Agent Types
- **LLM Agents** - Large language model integration ✅ (with OpenAI/Azure OpenAI)
- **Tool Agents** - Execute specific tools and functions (framework ready)
- **Conditional Agents** - Branch logic based on conditions (framework ready)
- **Parallel Agents** - Execute multiple tasks simultaneously (framework ready)
- **Checkpoint Agents** - Save and restore workflow state (framework ready)
- **MCP Agents** - Model Context Protocol integration (framework ready)
- **Function Nodes** - Custom code execution (framework ready)

### Pro-Code Integration (Planned)
- **Automatic tool discovery** from codebase (not yet implemented)
- **Method signature analysis** for parameter mapping (not yet implemented)
- **Real-time synchronization** between code and UI (not yet implemented)
- **Custom node generation** for new capabilities (not yet implemented)
- **Code generation** from visual workflows (not yet implemented)

### Workflow Management
- **Version control** for workflows (basic - database timestamps)
- **Import/export** capabilities (basic - JSON serialization)
- **Template system** for common patterns ✅
- **Execution monitoring** and logging (limited - basic logging)
- **Performance metrics** and optimization (not yet implemented)

## 🎯 Use Cases

### For Business Users (Low-Code)
- **Process automation** without technical knowledge (visual design ready)
- **Customer service workflows** with AI agents (chat interface available)
- **Data processing pipelines** with visual design (visual design ready)
- **Decision trees** for business logic (visual design ready)
- **Content generation** workflows (chat interface available)

### For Developers (Pro-Code)
- **Custom tool development** with automatic UI integration (planned)
- **Complex algorithm implementation** with visual testing (visual design ready)
- **API integration** with workflow orchestration (planned)
- **Performance optimization** through hybrid approaches (planned)
- **Code generation** from business requirements (planned)

### For Teams (Hybrid)
- **Business-Developer collaboration** on the same platform (visual design ready)
- **Rapid prototyping** with visual tools ✅
- **Production deployment** with code-based optimization (planned)
- **Knowledge transfer** through visual documentation ✅
- **Iterative development** with both approaches (visual design ready)

## 🔮 The Future: Code-Generated Agents

Our ultimate vision is to enable **code generation from visual workflows**:

1. **Design** your agent workflow visually
2. **Configure** agents, tools, and connections
3. **Generate** complete, production-ready code
4. **Deploy** as standalone applications or integrate into existing systems

This creates a true **no-code to pro-code pipeline** where visual design becomes executable code.

## 🎯 Current Implementation Status

### ✅ What's Working Now
- **Visual Workflow Designer**: Complete drag-and-drop interface with modern UI
- **Workflow Persistence**: Full database storage with Entity Framework and SQL Server
- **Agent Chat Interface**: Real-time chat with LLM agents (OpenAI/Azure OpenAI)
- **Workflow Templates**: Pre-built templates for common workflow patterns
- **Mermaid Integration**: Generate documentation diagrams from workflows
- **Authentication**: User management with ASP.NET Core Identity
- **Modern UI**: Responsive Blazor components with Bootstrap styling

### 🚧 What's In Progress/Planned
- **Workflow Execution Engine**: Execute workflows step-by-step (framework ready)
- **Code Detection**: Automatically discover tools from your codebase
- **Code Generation**: Generate executable code from visual workflows
- **Real-time Monitoring**: Live execution tracking and performance metrics

## 🚧 Roadmap to Vision: What's Missing

### Current Status ✅
- **Visual Workflow Builder**: ✅ Complete drag-and-drop interface with modern UI
- **Workflow Persistence**: ✅ Full database storage and retrieval with Entity Framework
- **Mermaid Integration**: ✅ Basic diagram generation from workflows
- **Agent Framework**: ✅ Core agent types and configuration models with Microsoft Agent Framework
- **UI Components**: ✅ Modern Blazor components with drag-and-drop functionality
- **Chat Service**: ✅ LLM integration with OpenAI/Azure OpenAI support
- **Workflow Templates**: ✅ Template system with workflow seeding
- **Database Integration**: ✅ SQL Server with full CRUD operations

### 🎯 Critical Missing Features (High Priority)

#### 1. **Code Detection & Tool Integration** 🔍
- [ ] **Reflection-based tool discovery** - Scan codebase for methods with `[Tool]` attributes
- [ ] **Automatic node generation** - Create visual nodes from discovered tools
- [ ] **Parameter mapping UI** - Visual interface for mapping tool parameters
- [ ] **Real-time code sync** - Watch file system for changes and update UI
- [ ] **Tool metadata extraction** - Parse method signatures, XML docs, and attributes
- [ ] **Dependency resolution** - Handle tool dependencies and imports

#### 2. **Workflow Execution Engine** ⚡
- [ ] **Real-time workflow execution** - Execute workflows step by step
- [ ] **Node execution logic** - Implement actual execution for each agent type (currently placeholder)
- [ ] **Data flow management** - Pass data between nodes during execution
- [ ] **Error handling & recovery** - Robust error handling and retry mechanisms
- [ ] **Execution monitoring** - Real-time status updates and progress tracking
- [ ] **Performance metrics** - Execution time, memory usage, success rates

#### 3. **LLM Integration & Agent Execution** 🤖
- [x] **OpenAI/Azure OpenAI integration** - ✅ Connected to actual LLM providers
- [ ] **Prompt template system** - Dynamic prompt generation and management
- [x] **Memory management** - ✅ Context and conversation memory for agents
- [ ] **Tool calling** - Execute discovered tools from LLM agents
- [x] **Streaming responses** - ✅ Real-time streaming of LLM responses
- [x] **Multi-provider support** - ✅ Support for OpenAI and Azure OpenAI providers

#### 4. **Code Generation Pipeline** 🏗️
- [ ] **Workflow-to-code generator** - Convert visual workflows to executable code
- [ ] **Agent code templates** - Generate complete agent implementations
- [ ] **Deployment packages** - Create deployable applications from workflows
- [ ] **Code optimization** - Optimize generated code for performance
- [ ] **Testing framework** - Generate unit tests for created agents
- [ ] **Documentation generation** - Auto-generate docs from workflows

### 🔧 Enhancement Features (Medium Priority)

#### 5. **Advanced Visual Builder** 🎨
- [ ] **Conditional logic UI** - Visual conditional branching and loops
- [ ] **Parallel execution UI** - Visual parallel processing configuration
- [ ] **Data transformation nodes** - Visual data mapping and transformation
- [ ] **Custom node creation** - Allow users to create custom node types
- [ ] **Workflow templates** - Pre-built workflow templates for common patterns
- [ ] **Version control** - Workflow versioning and branching

#### 6. **Pro-Code Integration** 💻
- [ ] **IDE extensions** - Visual Studio Code extension for workflow editing
- [ ] **Git integration** - Version control for workflows and generated code
- [ ] **CI/CD integration** - Deploy workflows through CI/CD pipelines
- [ ] **Code analysis** - Analyze code quality and suggest improvements
- [ ] **Refactoring tools** - Refactor workflows and generated code
- [ ] **Debugging support** - Debug workflows and generated agents

#### 7. **Enterprise Features** 🏢
- [ ] **Multi-tenant support** - Support for multiple organizations
- [ ] **Role-based access control** - Granular permissions and security
- [ ] **Audit logging** - Complete audit trail of all actions
- [ ] **API management** - RESTful APIs for workflow management
- [ ] **Scalability** - Horizontal scaling and load balancing
- [ ] **Monitoring & alerting** - Production monitoring and alerting

### 🚀 Future Vision Features (Long Term)

#### 8. **AI-Powered Development** 🧠
- [ ] **AI workflow suggestions** - AI suggests workflow improvements
- [ ] **Natural language to workflow** - Convert text descriptions to workflows
- [ ] **Auto-optimization** - AI optimizes workflows for performance
- [ ] **Intelligent testing** - AI generates test cases for workflows
- [ ] **Predictive analytics** - Predict workflow performance and issues
- [ ] **Smart debugging** - AI helps debug workflow issues

#### 9. **Ecosystem & Marketplace** 🌐
- [ ] **Workflow marketplace** - Share and discover workflows
- [ ] **Plugin system** - Third-party extensions and integrations
- [ ] **Community features** - Forums, ratings, and reviews
- [ ] **Monetization** - Paid workflows and premium features
- [ ] **API ecosystem** - Public APIs for third-party integrations
- [ ] **Mobile app** - Mobile interface for workflow management

### 📊 Implementation Phases

#### Phase 1: Core Execution (Months 1-3)
- Implement workflow execution engine
- Add LLM integration and agent execution
- Build basic code detection system
- Create real-time execution monitoring

#### Phase 2: Pro-Code Integration (Months 4-6)
- Complete code detection and tool integration
- Build code generation pipeline
- Add IDE extensions and Git integration
- Implement advanced visual builder features

#### Phase 3: Enterprise & AI (Months 7-12)
- Add enterprise features and security
- Implement AI-powered development tools
- Build ecosystem and marketplace
- Create mobile and advanced interfaces

## 🤝 Contributing

We welcome contributions! Here's how you can help:

### For Developers
- **Implement missing features** from the roadmap above
- **Add new agent types** and capabilities
- **Improve code detection** and tool integration
- **Enhance the visual builder** with new features
- **Create workflow templates** for common use cases

### For Designers
- **Improve the UI/UX** of the visual builder
- **Create workflow templates** and examples
- **Design new node types** and visual elements
- **Enhance the user experience** for non-technical users

### For Documentation
- **Write tutorials** and guides
- **Create video demonstrations** of key features
- **Improve API documentation** and examples
- **Translate** the interface to other languages

## 📚 Documentation

- [Getting Started Guide](docs/getting-started.md)
- [Agent Development Guide](docs/agent-development.md)
- [Workflow Design Patterns](docs/workflow-patterns.md)
- [API Reference](docs/api-reference.md)
- [Troubleshooting](docs/troubleshooting.md)

## 🐛 Issues & Support

- **Report bugs** on our [GitHub Issues](https://github.com/yourusername/Codefix.AIPlayGround/issues)
- **Request features** through [GitHub Discussions](https://github.com/yourusername/Codefix.AIPlayGround/discussions)
- **Get help** in our [Discord community](https://discord.gg/your-invite)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [Blazor](https://blazor.net/) and [.NET 8](https://dotnet.microsoft.com/)
- Inspired by modern workflow automation platforms
- Community feedback and contributions

---

**Ready to revolutionize how you build AI agents?** 🚀

[Get Started Now](https://github.com/yourusername/Codefix.AIPlayGround) | [View Documentation](docs/) | [Join Community](https://discord.gg/your-invite)
