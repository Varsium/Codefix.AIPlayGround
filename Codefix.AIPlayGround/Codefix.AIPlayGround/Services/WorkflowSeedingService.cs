using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Services;

public interface IWorkflowSeedingService
{
    Task SeedWorkflowsAsync();
}

public class WorkflowSeedingService : IWorkflowSeedingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WorkflowSeedingService> _logger;
    private readonly IWebHostEnvironment _environment;

    public WorkflowSeedingService(
        ApplicationDbContext context, 
        ILogger<WorkflowSeedingService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    public async Task SeedWorkflowsAsync()
    {
        try
        {
            _logger.LogInformation("Starting workflow seeding process...");

            // Check if workflows already exist
            if (await _context.Workflows.AnyAsync())
            {
                _logger.LogInformation("Workflows already exist in database. Skipping seeding.");
                return;
            }

            // Try both possible paths
            var workflowsPath = Path.Combine(_environment.ContentRootPath, "Data", "Workflows");
            
            if (!Directory.Exists(workflowsPath))
            {
                _logger.LogWarning("Workflows directory not found at: {WorkflowsPath}. Skipping seeding.", workflowsPath);
                return;
            }

            var jsonFiles = Directory.GetFiles(workflowsPath, "*.json");
            _logger.LogInformation("Found {Count} JSON workflow files", jsonFiles.Length);

            if (jsonFiles.Length == 0)
            {
                _logger.LogInformation("No workflow files to seed. Skipping.");
                return;
            }

            var seededCount = 0;
            var errorCount = 0;

            // Process each file individually without transaction (to allow partial success)
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    // Clear the change tracker before processing each file to avoid entity state conflicts
                    _context.ChangeTracker.Clear();
                    
                    await SeedWorkflowFromFileAsync(jsonFile);
                    
                    seededCount++;
                    _logger.LogInformation("Successfully seeded workflow from: {FileName}", Path.GetFileName(jsonFile));
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogError(ex, "Failed to seed workflow from file: {FileName}. Error: {Message}", 
                        Path.GetFileName(jsonFile), ex.InnerException?.Message ?? ex.Message);
                    
                    // Clear the change tracker on error to prevent state conflicts
                    _context.ChangeTracker.Clear();
                }
            }
            
            _logger.LogInformation("Workflow seeding completed. Seeded: {SeededCount}, Errors: {ErrorCount}", 
                seededCount, errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during workflow seeding: {Message}", ex.Message);
            // Don't throw - allow app to continue even if seeding fails
        }
    }

    private async Task SeedWorkflowFromFileAsync(string jsonFilePath)
    {
        var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
        
        // Deserialize using JsonDocument to handle flexible schema
        using var document = JsonDocument.Parse(jsonContent);
        var root = document.RootElement;

        var workflowId = root.GetProperty("Id").GetString() ?? Guid.NewGuid().ToString();

        // Check if workflow already exists
        if (await _context.Workflows.AnyAsync(w => w.Id == workflowId))
        {
            _logger.LogDebug("Workflow {WorkflowId} already exists, skipping", workflowId);
            return;
        }

        // Create workflow entity
        var workflowEntity = new WorkflowEntity
        {
            Id = workflowId,
            Name = root.GetProperty("Name").GetString() ?? "Untitled Workflow",
            Description = root.TryGetProperty("Description", out var desc) ? desc.GetString() ?? "" : "",
            Version = root.TryGetProperty("Version", out var ver) ? ver.GetString() ?? "1.0.0" : "1.0.0",
            CreatedAt = root.TryGetProperty("CreatedAt", out var created) ? created.GetDateTime() : DateTime.UtcNow,
            UpdatedAt = root.TryGetProperty("UpdatedAt", out var updated) ? updated.GetDateTime() : DateTime.UtcNow,
            CreatedBy = root.TryGetProperty("CreatedBy", out var createdBy) ? createdBy.GetString() ?? "System" : "System",
            Status = WorkflowStatus.Draft
        };

        // Add the workflow entity to the context first
        _context.Workflows.Add(workflowEntity);
        
        // Save the workflow first to ensure it exists before adding child entities
        await _context.SaveChangesAsync();

        // Create nodes
        if (root.TryGetProperty("Nodes", out var nodesArray))
        {
            foreach (var nodeElement in nodesArray.EnumerateArray())
            {
                var nodeId = nodeElement.GetProperty("Id").GetString() ?? Guid.NewGuid().ToString();
                
                // Convert simple string arrays to ConnectionPort JSON
                var inputPortsJson = "[]";
                var outputPortsJson = "[]";
                
                if (nodeElement.TryGetProperty("InputPorts", out var inputPorts))
                {
                    var ports = new List<ConnectionPort>();
                    foreach (var port in inputPorts.EnumerateArray())
                    {
                        ports.Add(new ConnectionPort { Name = port.GetString() ?? "input", Type = "input" });
                    }
                    inputPortsJson = JsonSerializer.Serialize(ports);
                }
                
                if (nodeElement.TryGetProperty("OutputPorts", out var outputPorts))
                {
                    var ports = new List<ConnectionPort>();
                    foreach (var port in outputPorts.EnumerateArray())
                    {
                        ports.Add(new ConnectionPort { Name = port.GetString() ?? "output", Type = "output" });
                    }
                    outputPortsJson = JsonSerializer.Serialize(ports);
                }

                var nodeEntity = new WorkflowNodeEntity
                {
                    Id = nodeId,
                    Name = nodeElement.GetProperty("Name").GetString() ?? "Untitled Node",
                    Type = nodeElement.TryGetProperty("Type", out var type) ? type.GetString() ?? "unknown" : "unknown",
                    X = nodeElement.TryGetProperty("X", out var x) ? x.GetDouble() : 0,
                    Y = nodeElement.TryGetProperty("Y", out var y) ? y.GetDouble() : 0,
                    Width = nodeElement.TryGetProperty("Width", out var w) ? w.GetDouble() : 150,
                    Height = nodeElement.TryGetProperty("Height", out var h) ? h.GetDouble() : 80,
                    Status = nodeElement.TryGetProperty("Status", out var status) ? status.GetString() ?? "idle" : "idle",
                    IsSelected = nodeElement.TryGetProperty("IsSelected", out var selected) && selected.GetBoolean(),
                    PropertiesJson = nodeElement.TryGetProperty("Properties", out var props) ? props.GetRawText() : "{}",
                    InputPortsJson = inputPortsJson,
                    OutputPortsJson = outputPortsJson,
                    WorkflowId = workflowEntity.Id
                };
                
                _context.WorkflowNodes.Add(nodeEntity);
            }
        }

        // Create connections
        if (root.TryGetProperty("Connections", out var connectionsArray))
        {
            foreach (var connElement in connectionsArray.EnumerateArray())
            {
                var connId = connElement.GetProperty("Id").GetString() ?? Guid.NewGuid().ToString();
                
                var connectionEntity = new WorkflowConnectionEntity
                {
                    Id = connId,
                    FromNodeId = connElement.TryGetProperty("FromNodeId", out var fromNode) ? fromNode.GetString() ?? "" : "",
                    ToNodeId = connElement.TryGetProperty("ToNodeId", out var toNode) ? toNode.GetString() ?? "" : "",
                    FromPort = connElement.TryGetProperty("FromPort", out var fromPort) ? fromPort.GetString() ?? "" : "",
                    ToPort = connElement.TryGetProperty("ToPort", out var toPort) ? toPort.GetString() ?? "" : "",
                    Label = connElement.TryGetProperty("Label", out var label) ? label.GetString() ?? "" : "",
                    Type = connElement.TryGetProperty("Type", out var connType) ? connType.GetString() ?? "default" : "default",
                    IsSelected = connElement.TryGetProperty("IsSelected", out var connSelected) && connSelected.GetBoolean(),
                    From = connElement.TryGetProperty("From", out var from) ? from.GetString() ?? "" : "",
                    To = connElement.TryGetProperty("To", out var to) ? to.GetString() ?? "" : "",
                    PropertiesJson = connElement.TryGetProperty("Properties", out var connProps) ? connProps.GetRawText() : "{}",
                    WorkflowId = workflowEntity.Id
                };
                
                _context.WorkflowConnections.Add(connectionEntity);
            }
        }

        // Create metadata
        if (root.TryGetProperty("Metadata", out var metadataElement))
        {
            var metadataEntity = new WorkflowMetadataEntity
            {
                WorkflowId = workflowEntity.Id,
                Category = metadataElement.TryGetProperty("Category", out var cat) ? cat.GetString() ?? "" : "",
                Author = metadataElement.TryGetProperty("Author", out var auth) ? auth.GetString() ?? "" : "",
                License = metadataElement.TryGetProperty("License", out var lic) ? lic.GetString() ?? "" : "",
                TagsJson = metadataElement.TryGetProperty("Tags", out var tags) ? tags.GetRawText() : "[]",
                CustomPropertiesJson = metadataElement.TryGetProperty("CustomProperties", out var custom) ? custom.GetRawText() : "{}"
            };
            
            _context.WorkflowMetadata.Add(metadataEntity);
        }

        // Create settings
        if (root.TryGetProperty("Settings", out var settingsElement))
        {
            var settingsEntity = new WorkflowSettingsEntity
            {
                WorkflowId = workflowEntity.Id,
                EnableCheckpoints = settingsElement.TryGetProperty("EnableCheckpoints", out var chk) && chk.GetBoolean(),
                EnableLogging = settingsElement.TryGetProperty("EnableLogging", out var log) && log.GetBoolean(),
                EnableMetrics = settingsElement.TryGetProperty("EnableMetrics", out var met) && met.GetBoolean(),
                MaxExecutionTimeMinutes = settingsElement.TryGetProperty("MaxExecutionTimeMinutes", out var maxTime) ? maxTime.GetInt32() : 60,
                MaxRetryAttempts = settingsElement.TryGetProperty("MaxRetryAttempts", out var retry) ? retry.GetInt32() : 3,
                ExecutionMode = settingsElement.TryGetProperty("ExecutionMode", out var mode) ? mode.GetString() ?? "sequential" : "sequential",
                EnvironmentVariablesJson = settingsElement.TryGetProperty("EnvironmentVariables", out var env) ? env.GetRawText() : "{}"
            };
            
            _context.WorkflowSettings.Add(settingsEntity);
        }
        
        // Save all child entities
        await _context.SaveChangesAsync();
    }
}
