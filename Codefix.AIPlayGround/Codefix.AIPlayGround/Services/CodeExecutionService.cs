using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models;
using Microsoft.EntityFrameworkCore;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Service for executing generated agent code with security and monitoring
/// </summary>
public class CodeExecutionService : ICodeExecutionService, IDisposable
{
    private readonly ILogger<CodeExecutionService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly ICodeGenerationService _codeGenerationService;
    private readonly ConcurrentDictionary<string, AgentExecutionResult> _runningExecutions;
    private readonly ConcurrentDictionary<string, object> _agentInstances;
    private readonly Timer _cleanupTimer;
    private readonly SecuritySettings _securitySettings;

    public CodeExecutionService(
        ILogger<CodeExecutionService> logger,
        ApplicationDbContext context,
        ICodeGenerationService codeGenerationService,
        IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _codeGenerationService = codeGenerationService;
        _runningExecutions = new ConcurrentDictionary<string, AgentExecutionResult>();
        _agentInstances = new ConcurrentDictionary<string, object>();
        
        _securitySettings = new SecuritySettings
        {
            AllowFileSystemAccess = configuration.GetValue<bool>("AgentExecution:AllowFileSystemAccess", false),
            AllowNetworkAccess = configuration.GetValue<bool>("AgentExecution:AllowNetworkAccess", false),
            AllowReflection = configuration.GetValue<bool>("AgentExecution:AllowReflection", false),
            AllowDynamicCode = configuration.GetValue<bool>("AgentExecution:AllowDynamicCode", false),
            MaxExecutionTimeSeconds = configuration.GetValue<int>("AgentExecution:MaxExecutionTimeSeconds", 300),
            MaxMemoryMB = configuration.GetValue<int>("AgentExecution:MaxMemoryMB", 100)
        };
        
        // Setup cleanup timer to run every 5 minutes
        _cleanupTimer = new Timer(OnCleanupTimer, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public async Task<AgentExecutionResult> ExecuteAgentAsync(string agentId, object input, Dictionary<string, object>? context = null)
    {
        var executionId = Guid.NewGuid().ToString();
        var execution = new AgentExecutionResult
        {
            ExecutionId = executionId,
            AgentId = agentId,
            Status = AgentExecutionStatus.Running,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting agent execution: {AgentId} with execution {ExecutionId}", agentId, executionId);
            
            // Add to running executions
            _runningExecutions.TryAdd(executionId, execution);

            // Get agent from database
            var agent = await _context.Agents.FindAsync(agentId);
            if (agent == null)
            {
                execution.Status = AgentExecutionStatus.Failed;
                execution.Message = "Agent not found";
                execution.CompletedAt = DateTime.UtcNow;
                return execution;
            }

            // Check if agent is a code-generated agent
            if (agent.AgentType != "CodeGeneratedAgent")
            {
                execution.Status = AgentExecutionStatus.Failed;
                execution.Message = "Agent is not a code-generated agent";
                execution.CompletedAt = DateTime.UtcNow;
                return execution;
            }

            // Get or create agent instance
            var agentInstance = await GetOrCreateAgentInstanceAsync(agentId, agent);
            if (agentInstance == null)
            {
                execution.Status = AgentExecutionStatus.Failed;
                execution.Message = "Failed to create agent instance";
                execution.CompletedAt = DateTime.UtcNow;
                return execution;
            }

            // Execute the agent with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_securitySettings.MaxExecutionTimeSeconds));
            var task = ExecuteWithTimeoutAsync(agentInstance, input, cts.Token);
            
            var result = await task;
            
            execution.Status = AgentExecutionStatus.Completed;
            execution.Result = result;
            execution.Message = "Execution completed successfully";
            execution.CompletedAt = DateTime.UtcNow;

            // Save execution to database
            await SaveExecutionToDatabaseAsync(execution, agent);

            _logger.LogInformation("Agent execution completed: {ExecutionId} in {Duration}ms", 
                executionId, execution.Duration.TotalMilliseconds);
        }
        catch (OperationCanceledException)
        {
            execution.Status = AgentExecutionStatus.Timeout;
            execution.Message = "Execution timed out";
            execution.CompletedAt = DateTime.UtcNow;
            _logger.LogWarning("Agent execution timed out: {ExecutionId}", executionId);
        }
        catch (Exception ex)
        {
            execution.Status = AgentExecutionStatus.Failed;
            execution.Message = $"Execution failed: {ex.Message}";
            execution.Errors.Add(ex.Message);
            execution.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Agent execution failed: {ExecutionId}", executionId);
        }
        finally
        {
            // Remove from running executions
            _runningExecutions.TryRemove(executionId, out _);
        }

        return execution;
    }

    public async Task<AgentExecutionResult> ExecuteAgentMethodAsync(string agentId, string methodName, object[]? parameters = null)
    {
        var executionId = Guid.NewGuid().ToString();
        var execution = new AgentExecutionResult
        {
            ExecutionId = executionId,
            AgentId = agentId,
            Status = AgentExecutionStatus.Running,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting agent method execution: {AgentId}.{MethodName} with execution {ExecutionId}", 
                agentId, methodName, executionId);
            
            _runningExecutions.TryAdd(executionId, execution);

            // Get agent instance
            var agentInstance = await GetAgentInstanceAsync(agentId);
            if (agentInstance == null)
            {
                execution.Status = AgentExecutionStatus.Failed;
                execution.Message = "Agent instance not found";
                execution.CompletedAt = DateTime.UtcNow;
                return execution;
            }

            // Execute the method with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_securitySettings.MaxExecutionTimeSeconds));
            var result = await _codeGenerationService.ExecuteAgentMethodAsync(agentInstance, methodName, parameters);
            
            execution.Status = result.IsSuccess ? AgentExecutionStatus.Completed : AgentExecutionStatus.Failed;
            execution.Result = result.Result;
            execution.Message = result.Message;
            execution.CompletedAt = DateTime.UtcNow;

            if (!result.IsSuccess)
            {
                execution.Errors.Add(result.Message);
            }

            _logger.LogInformation("Agent method execution completed: {ExecutionId} in {Duration}ms", 
                executionId, execution.Duration.TotalMilliseconds);
        }
        catch (OperationCanceledException)
        {
            execution.Status = AgentExecutionStatus.Timeout;
            execution.Message = "Method execution timed out";
            execution.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            execution.Status = AgentExecutionStatus.Failed;
            execution.Message = $"Method execution failed: {ex.Message}";
            execution.Errors.Add(ex.Message);
            execution.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Agent method execution failed: {ExecutionId}", executionId);
        }
        finally
        {
            _runningExecutions.TryRemove(executionId, out _);
        }

        return execution;
    }

    public async Task<AgentExecutionStatus> GetExecutionStatusAsync(string executionId)
    {
        if (_runningExecutions.TryGetValue(executionId, out var execution))
        {
            return execution.Status;
        }

        // Check database for completed executions
        var dbExecution = await _context.AgentExecutions
            .FirstOrDefaultAsync(e => e.Id == executionId);

        if (dbExecution != null)
        {
            return dbExecution.Status switch
            {
                ExecutionStatus.Running => AgentExecutionStatus.Running,
                ExecutionStatus.Completed => AgentExecutionStatus.Completed,
                ExecutionStatus.Failed => AgentExecutionStatus.Failed,
                ExecutionStatus.Cancelled => AgentExecutionStatus.Cancelled,
                _ => AgentExecutionStatus.Pending
            };
        }

        return AgentExecutionStatus.Pending;
    }

    public async Task<bool> CancelExecutionAsync(string executionId)
    {
        if (_runningExecutions.TryGetValue(executionId, out var execution))
        {
            execution.Status = AgentExecutionStatus.Cancelled;
            execution.CompletedAt = DateTime.UtcNow;
            execution.Message = "Execution cancelled by user";
            
            _runningExecutions.TryRemove(executionId, out _);
            
            _logger.LogInformation("Execution cancelled: {ExecutionId}", executionId);
            return true;
        }

        return false;
    }

    public async Task<List<AgentExecutionResult>> GetExecutionHistoryAsync(string agentId, int limit = 50)
    {
        var executions = await _context.AgentExecutions
            .Where(e => e.AgentId == agentId)
            .OrderByDescending(e => e.StartedAt)
            .Take(limit)
            .ToListAsync();

        return executions.Select(e => new AgentExecutionResult
        {
            ExecutionId = e.Id,
            AgentId = e.AgentId,
            IsSuccess = e.Status == ExecutionStatus.Completed,
            Status = e.Status switch
            {
                ExecutionStatus.Running => AgentExecutionStatus.Running,
                ExecutionStatus.Completed => AgentExecutionStatus.Completed,
                ExecutionStatus.Failed => AgentExecutionStatus.Failed,
                ExecutionStatus.Cancelled => AgentExecutionStatus.Cancelled,
                _ => AgentExecutionStatus.Pending
            },
            StartedAt = e.StartedAt,
            CompletedAt = e.CompletedAt,
            Message = e.Status.ToString()
        }).ToList();
    }

    public async Task<bool> ValidateExecutionPermissionsAsync(string agentId, string methodName)
    {
        // Basic security validation
        if (!_securitySettings.AllowDynamicCode)
        {
            return false;
        }

        // Check if method is in allowed list
        var allowedMethods = new[] { "ExecuteAsync", "ProcessDataAsync", "CallApiAsync", "ExecuteStepAsync" };
        if (!allowedMethods.Contains(methodName))
        {
            _logger.LogWarning("Method {MethodName} not in allowed list for agent {AgentId}", methodName, agentId);
            return false;
        }

        return true;
    }

    public async Task<AgentPerformanceMetrics> GetPerformanceMetricsAsync(string agentId)
    {
        var executions = await _context.AgentExecutions
            .Where(e => e.AgentId == agentId)
            .ToListAsync();

        var successfulExecutions = executions.Count(e => e.Status == ExecutionStatus.Completed);
        var failedExecutions = executions.Count(e => e.Status == ExecutionStatus.Failed);
        
        var executionTimes = executions
            .Where(e => e.CompletedAt.HasValue)
            .Select(e => e.CompletedAt!.Value.Subtract(e.StartedAt).TotalMilliseconds)
            .ToList();

        return new AgentPerformanceMetrics
        {
            AgentId = agentId,
            TotalExecutions = executions.Count,
            SuccessfulExecutions = successfulExecutions,
            FailedExecutions = failedExecutions,
            AverageExecutionTimeMs = executionTimes.Any() ? executionTimes.Average() : 0,
            MinExecutionTimeMs = executionTimes.Any() ? executionTimes.Min() : 0,
            MaxExecutionTimeMs = executionTimes.Any() ? executionTimes.Max() : 0,
            LastExecutionAt = executions.Any() ? executions.Max(e => e.StartedAt) : DateTime.MinValue
        };
    }

    public async Task CleanupAsync()
    {
        try
        {
            // Clean up old executions from database (older than 7 days)
            var cutoffDate = DateTime.UtcNow.AddDays(-7);
            var oldExecutions = await _context.AgentExecutions
                .Where(e => e.StartedAt < cutoffDate)
                .ToListAsync();

            if (oldExecutions.Any())
            {
                _context.AgentExecutions.RemoveRange(oldExecutions);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} old executions", oldExecutions.Count);
            }

            // Clean up completed running executions
            var completedExecutions = _runningExecutions.Values
                .Where(e => e.Status == AgentExecutionStatus.Completed || e.Status == AgentExecutionStatus.Failed)
                .ToList();

            foreach (var execution in completedExecutions)
            {
                _runningExecutions.TryRemove(execution.ExecutionId, out _);
            }

            _logger.LogInformation("Cleaned up {Count} completed executions from memory", completedExecutions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
        }
    }

    private async Task<object?> GetOrCreateAgentInstanceAsync(string agentId, AgentEntity agent)
    {
        if (_agentInstances.TryGetValue(agentId, out var existingInstance))
        {
            return existingInstance;
        }

        // Create new instance
        var assemblyPath = agent.PropertiesJson; // Assuming assembly path is stored in properties
        var className = agent.Name;
        
        var instanceResult = await _codeGenerationService.CreateAgentInstanceAsync(assemblyPath, className);
        
        if (instanceResult.IsSuccess && instanceResult.AgentInstance != null)
        {
            _agentInstances.TryAdd(agentId, instanceResult.AgentInstance);
            return instanceResult.AgentInstance;
        }

        return null;
    }

    private async Task<object?> GetAgentInstanceAsync(string agentId)
    {
        if (_agentInstances.TryGetValue(agentId, out var instance))
        {
            return instance;
        }

        // Try to get from database and create
        var agent = await _context.Agents.FindAsync(agentId);
        if (agent != null)
        {
            return await GetOrCreateAgentInstanceAsync(agentId, agent);
        }

        return null;
    }

    private async Task<object> ExecuteWithTimeoutAsync(object agentInstance, object input, CancellationToken cancellationToken)
    {
        // Use reflection to find and call the ExecuteAsync method
        var method = agentInstance.GetType().GetMethod("ExecuteAsync");
        if (method == null)
        {
            throw new InvalidOperationException("ExecuteAsync method not found on agent");
        }

        var task = (Task)method.Invoke(agentInstance, new[] { input })!;
        
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_securitySettings.MaxExecutionTimeSeconds));
        
        try
        {
            await task.WaitAsync(timeoutCts.Token);
            
            // Get result if it's Task<T>
            if (task.GetType().IsGenericType)
            {
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task) ?? task;
            }
            
            return task;
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            throw new TimeoutException("Agent execution timed out");
        }
    }

    private async Task SaveExecutionToDatabaseAsync(AgentExecutionResult execution, AgentEntity agent)
    {
        try
        {
            var dbExecution = new AgentExecutionEntity
            {
                Id = execution.ExecutionId,
                AgentId = execution.AgentId,
                StartedAt = execution.StartedAt,
                CompletedAt = execution.CompletedAt,
                Status = execution.Status switch
                {
                    AgentExecutionStatus.Completed => ExecutionStatus.Completed,
                    AgentExecutionStatus.Failed => ExecutionStatus.Failed,
                    AgentExecutionStatus.Cancelled => ExecutionStatus.Cancelled,
                    AgentExecutionStatus.Timeout => ExecutionStatus.Failed,
                    _ => ExecutionStatus.Running
                },
                InputDataJson = System.Text.Json.JsonSerializer.Serialize(execution.Result),
                OutputDataJson = System.Text.Json.JsonSerializer.Serialize(execution.Result),
                MetricsJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Duration = execution.Duration.TotalMilliseconds,
                    LogCount = execution.Logs.Count,
                    ErrorCount = execution.Errors.Count
                }),
                ErrorsJson = System.Text.Json.JsonSerializer.Serialize(execution.Errors)
            };

            _context.AgentExecutions.Add(dbExecution);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save execution to database: {ExecutionId}", execution.ExecutionId);
        }
    }

    private void OnCleanupTimer(object? state)
    {
        _ = Task.Run(async () => await CleanupAsync());
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}
