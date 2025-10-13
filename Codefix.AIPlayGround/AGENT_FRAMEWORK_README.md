# Microsoft Agent Framework Visualizer Implementation

This implementation adds comprehensive visualization capabilities for Microsoft Agent Framework workflows to your Blazor application.

## 🚀 Features Implemented

### 1. **Agent Visualization Service** (`AgentVisualizationService.cs`)
- **Python.NET Integration**: Runs Python visualization code from C#
- **Mermaid Diagram Generation**: Creates beautiful flowcharts
- **GraphViz DOT Support**: Generates DOT format diagrams
- **Sample Workflow Creation**: Pre-built workflows for demonstration
- **Custom Workflow Support**: Build workflows programmatically

### 2. **Interactive Workflow Visualizer** (`WorkflowVisualizer.razor`)
- **Real-time Diagram Generation**: See changes instantly
- **Multiple Format Support**: Switch between Mermaid and DOT
- **Node Management**: Add, remove, and configure workflow nodes
- **Export Capabilities**: Download diagrams in various formats
- **Responsive Design**: Works on all screen sizes

### 3. **Visual Workflow Builder** (`WorkflowBuilder.razor`)
- **Drag & Drop Interface**: Intuitive node placement
- **Node Palette**: Pre-defined node types (Start, Agent, Function, Condition, End)
- **Visual Connections**: Draw connections between nodes
- **Property Editor**: Configure node properties
- **Save/Load Workflows**: Persist your workflows

### 4. **Enhanced Home Page**
- **Modern UI**: Beautiful gradient hero section
- **Tabbed Interface**: Easy navigation between features
- **Documentation**: Built-in framework documentation
- **Quick Start**: Get started immediately

## 🛠️ Technical Implementation

### Dependencies Added
```xml
<PackageReference Include="Python.Runtime" Version="3.0.3" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="10.0.0-rc.1.25451.107" />
```

### Key Components
- **Services/AgentVisualizationService.cs**: Core visualization logic
- **Components/AgentWorkflow/WorkflowVisualizer.razor**: Main visualizer component
- **Components/AgentWorkflow/WorkflowBuilder.razor**: Interactive builder
- **wwwroot/js/workflow-builder.js**: JavaScript utilities
- **Scripts/workflow_visualizer.py**: Python visualization script

### JavaScript Integration
- **Mermaid.js**: For rendering flowcharts in the browser
- **Drag & Drop**: Interactive node placement
- **File Operations**: Save/load workflow files
- **Canvas Management**: Visual workflow editing

## 🎯 Usage

### 1. **Viewing Workflows**
1. Navigate to the Home page
2. Click "Workflow Visualizer" tab
3. Click "Load Sample Workflow"
4. Switch between Mermaid and DOT formats
5. Export diagrams as needed

### 2. **Building Workflows**
1. Click "Interactive Builder" tab
2. Drag nodes from the palette to the canvas
3. Click nodes to configure properties
4. Save your workflow as JSON
5. Load existing workflows

### 3. **Programmatic Usage**
```csharp
// Inject the service
@inject IAgentVisualizationService VisualizationService

// Generate Mermaid diagram
var workflow = await VisualizationService.CreateSampleWorkflowAsync();
var mermaidDiagram = await VisualizationService.GenerateMermaidDiagramAsync(workflow);

// Generate DOT diagram
var dotDiagram = await VisualizationService.GenerateDotDiagramAsync(workflow);
```

## 🔧 Configuration

### Python Setup (Optional)
If you want to use the Python visualization features:
1. Install Python 3.8+
2. Install required packages:
   ```bash
   pip install graphviz
   ```
3. The Python.NET integration will handle the rest

### Mermaid.js
Mermaid.js is loaded from CDN and automatically initializes. No additional setup required.

## 🎨 Customization

### Adding New Node Types
1. Update the `WorkflowNode` class in `AgentVisualizationService.cs`
2. Add new node types to the palette in `WorkflowBuilder.razor`
3. Update the visualization logic in the Python script

### Styling
- Modify CSS in component `<style>` sections
- Update Mermaid themes in the JavaScript initialization
- Customize node colors and shapes

### Export Formats
Add new export formats by extending the `AgentVisualizationService`:
```csharp
public async Task<string> GenerateCustomFormatAsync(Workflow workflow)
{
    // Your custom format logic
}
```

## 🚀 Future Enhancements

### Planned Features
- **Real-time Collaboration**: Multiple users editing workflows
- **Workflow Execution**: Run actual agent workflows
- **Advanced Node Types**: More specialized agent types
- **Template Library**: Pre-built workflow templates
- **Version Control**: Track workflow changes
- **Integration APIs**: Connect to external systems

### Possible Integrations
- **Azure DevOps**: Workflow management
- **GitHub Actions**: CI/CD integration
- **Power Platform**: Low-code integration
- **Microsoft Graph**: Enterprise data sources

## 📚 Resources

- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)
- [Mermaid.js Documentation](https://mermaid.js.org/)
- [GraphViz Documentation](https://graphviz.org/)
- [Python.NET Documentation](https://pythonnet.github.io/)

## 🤝 Contributing

To extend this implementation:
1. Fork the repository
2. Create a feature branch
3. Implement your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This implementation follows the same license as the main project.

---

**Note**: This implementation is designed to work with Microsoft Agent Framework in public preview. Some features may change as the framework evolves.
