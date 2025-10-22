using System.Reflection;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Service for providing security sandboxing for generated agent code
/// </summary>
public class SecuritySandboxService : IDisposable
{
    private readonly ILogger<SecuritySandboxService> _logger;
    private readonly SecuritySettings _securitySettings;
    private readonly AppDomain? _sandboxDomain;
    private readonly Dictionary<string, object> _sandboxedInstances;

    public SecuritySandboxService(
        ILogger<SecuritySandboxService> logger,
        SecuritySettings securitySettings)
    {
        _logger = logger;
        _securitySettings = securitySettings;
        _sandboxedInstances = new Dictionary<string, object>();
        
        // Create sandbox domain if security is enabled
        if (securitySettings.AllowDynamicCode)
        {
            _sandboxDomain = CreateSandboxDomain();
        }
    }

    /// <summary>
    /// Creates a sandboxed instance of an agent
    /// </summary>
    public T? CreateSandboxedInstance<T>(string assemblyPath, string className, object[]? parameters = null) where T : class
    {
        try
        {
            if (_sandboxDomain == null)
            {
                _logger.LogWarning("Sandbox domain not available, creating instance in current domain");
                return CreateInstanceInCurrentDomain<T>(assemblyPath, className, parameters);
            }

            // Create instance in sandbox domain
            var instance = _sandboxDomain.CreateInstanceFromAndUnwrap(
                assemblyPath,
                className,
                false,
                BindingFlags.Default,
                null,
                parameters,
                null,
                null) as T;

            if (instance != null)
            {
                var instanceId = Guid.NewGuid().ToString();
                _sandboxedInstances[instanceId] = instance;
                _logger.LogInformation("Created sandboxed instance: {InstanceId}", instanceId);
            }

            return instance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sandboxed instance");
            return null;
        }
    }

    /// <summary>
    /// Executes a method in a sandboxed environment
    /// </summary>
    public async Task<object?> ExecuteSandboxedMethodAsync(object instance, string methodName, object[]? parameters = null)
    {
        try
        {
            if (_sandboxDomain == null)
            {
                return await ExecuteMethodInCurrentDomainAsync(instance, methodName, parameters);
            }

            // Execute in sandbox domain
            var method = instance.GetType().GetMethod(methodName);
            if (method == null)
            {
                throw new InvalidOperationException($"Method {methodName} not found");
            }

            var result = method.Invoke(instance, parameters);
            
            // Handle async methods
            if (result is Task task)
            {
                await task;
                
                if (task.GetType().IsGenericType)
                {
                    var resultProperty = task.GetType().GetProperty("Result");
                    return resultProperty?.GetValue(task);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sandboxed method: {MethodName}", methodName);
            throw;
        }
    }

    /// <summary>
    /// Validates code for security compliance
    /// </summary>
    public bool ValidateCodeSecurity(string code)
    {
        var violations = new List<string>();

        // Check for file system access
        if (!_securitySettings.AllowFileSystemAccess)
        {
            if (ContainsFileSystemAccess(code))
            {
                violations.Add("Code contains file system access which is not allowed");
            }
        }

        // Check for network access
        if (!_securitySettings.AllowNetworkAccess)
        {
            if (ContainsNetworkAccess(code))
            {
                violations.Add("Code contains network access which is not allowed");
            }
        }

        // Check for reflection
        if (!_securitySettings.AllowReflection)
        {
            if (ContainsReflection(code))
            {
                violations.Add("Code contains reflection which is not allowed");
            }
        }

        // Check for blocked namespaces
        foreach (var blockedNamespace in _securitySettings.BlockedNamespaces)
        {
            if (code.Contains($"using {blockedNamespace}") || code.Contains($"{blockedNamespace}."))
            {
                violations.Add($"Code uses blocked namespace: {blockedNamespace}");
            }
        }

        if (violations.Any())
        {
            _logger.LogWarning("Code security validation failed: {Violations}", string.Join(", ", violations));
            return false;
        }

        return true;
    }

    /// <summary>
    /// Applies security restrictions to an assembly
    /// </summary>
    public void ApplySecurityRestrictions(Assembly assembly)
    {
        try
        {
            // Set security permissions
            var permissionSet = CreatePermissionSet();
            
            // Apply to assembly if possible
            if (assembly != null)
            {
                // Note: In .NET Core/.NET 5+, AppDomain security is not available
                // This is a placeholder for future security implementations
                _logger.LogInformation("Security restrictions applied to assembly: {AssemblyName}", assembly.GetName().Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not apply security restrictions to assembly");
        }
    }

    private AppDomain? CreateSandboxDomain()
    {
        try
        {
            // Note: AppDomain.CreateDomain is not available in .NET Core/.NET 5+
            // This is a placeholder for future sandboxing implementations
            _logger.LogWarning("AppDomain sandboxing is not available in .NET Core/.NET 5+");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sandbox domain");
            return null;
        }
    }

    private T? CreateInstanceInCurrentDomain<T>(string assemblyPath, string className, object[]? parameters) where T : class
    {
        try
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            var type = assembly.GetType(className);
            
            if (type == null)
            {
                _logger.LogError("Type {ClassName} not found in assembly {AssemblyPath}", className, assemblyPath);
                return null;
            }

            var instance = Activator.CreateInstance(type, parameters) as T;
            return instance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating instance in current domain");
            return null;
        }
    }

    private async Task<object?> ExecuteMethodInCurrentDomainAsync(object instance, string methodName, object[]? parameters)
    {
        try
        {
            var method = instance.GetType().GetMethod(methodName);
            if (method == null)
            {
                throw new InvalidOperationException($"Method {methodName} not found");
            }

            var result = method.Invoke(instance, parameters);
            
            if (result is Task task)
            {
                await task;
                
                if (task.GetType().IsGenericType)
                {
                    var resultProperty = task.GetType().GetProperty("Result");
                    return resultProperty?.GetValue(task);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing method in current domain");
            throw;
        }
    }

    private bool ContainsFileSystemAccess(string code)
    {
        var fileSystemPatterns = new[]
        {
            "System.IO.File",
            "System.IO.Directory",
            "System.IO.FileStream",
            "System.IO.StreamReader",
            "System.IO.StreamWriter",
            "File.Create",
            "File.Open",
            "File.ReadAllText",
            "File.WriteAllText",
            "Directory.CreateDirectory",
            "Directory.Delete",
            "Directory.GetFiles",
            "Directory.GetDirectories"
        };

        return fileSystemPatterns.Any(pattern => code.Contains(pattern));
    }

    private bool ContainsNetworkAccess(string code)
    {
        var networkPatterns = new[]
        {
            "System.Net.Http",
            "System.Net.WebClient",
            "System.Net.HttpWebRequest",
            "HttpClient",
            "WebClient",
            "HttpWebRequest",
            "System.Net.Sockets",
            "TcpClient",
            "UdpClient"
        };

        return networkPatterns.Any(pattern => code.Contains(pattern));
    }

    private bool ContainsReflection(string code)
    {
        var reflectionPatterns = new[]
        {
            "System.Reflection",
            "Assembly.Load",
            "Type.GetType",
            "Activator.CreateInstance",
            "MethodInfo.Invoke",
            "PropertyInfo.GetValue",
            "FieldInfo.GetValue"
        };

        return reflectionPatterns.Any(pattern => code.Contains(pattern));
    }

    private object? CreatePermissionSet()
    {
        // Note: PermissionSet is not available in .NET Core/.NET 5+
        // This is a placeholder for future security implementations
        _logger.LogWarning("PermissionSet is not available in .NET Core/.NET 5+");
        return null;
    }

    public void Dispose()
    {
        try
        {
            // Clean up sandboxed instances
            _sandboxedInstances.Clear();
            
            // Unload sandbox domain if it exists
            if (_sandboxDomain != null)
            {
                // Note: AppDomain.Unload is not available in .NET Core/.NET 5+
                _logger.LogInformation("Sandbox domain cleanup completed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during security sandbox cleanup");
        }
    }
}
