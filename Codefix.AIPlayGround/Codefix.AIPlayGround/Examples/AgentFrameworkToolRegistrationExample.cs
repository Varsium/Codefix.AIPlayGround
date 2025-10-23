using Microsoft.Agents.AI;
using Codefix.AIPlayGround.Services;
using System.ComponentModel;
using System.Reflection;

namespace Codefix.AIPlayGround.Examples;

/// <summary>
/// Example showing how to properly register Microsoft Agent Framework tools
/// </summary>
public class AgentFrameworkToolRegistrationExample
{
    /// <summary>
    /// Demonstrates proper Microsoft Agent Framework tool registration
    /// </summary>
    public static void RegisterToolsWithAgent()
    {
        // Example of how to register tools using Microsoft Agent Framework
        // Note: This is a conceptual example - actual implementation depends on your agent setup
        
        Console.WriteLine("🤖 Microsoft Agent Framework Tool Registration Example");
        Console.WriteLine("=====================================================");
        Console.WriteLine();

        // Method 1: Register individual tools using Microsoft Agent Framework
        Console.WriteLine("📋 Method 1: Individual Tool Registration");
        Console.WriteLine("Using Microsoft Agent Framework tool registration:");
        Console.WriteLine();

        // Note: The actual Microsoft Agent Framework API may vary
        // This is a conceptual example of how tools would be registered
        var tools = new List<string>
        {
            // Register DemoTools methods as agent tools
            "DemoTools.Add",
            "DemoTools.Multiply", 
            "DemoTools.ValidateEmail",
            "DemoTools.GeneratePassword",
            "DemoTools.FormatJson",
            "DemoTools.CalculateLevenshteinDistance",
            "DemoTools.GetSystemInfo",
            "DemoTools.EncryptCaesar",
            "DemoTools.DecryptCaesar",
            "DemoTools.Factorial",
            "DemoTools.IsPrime",
            "DemoTools.ConvertTemperature",
            "DemoTools.GenerateGuid",
            "DemoTools.CalculateMD5Hash",
            "DemoTools.FormatCurrency",
            "DemoTools.ValidateCreditCard"
        };

        Console.WriteLine($"✅ Registered {tools.Count} tools from DemoTools class");
        Console.WriteLine();

        // Method 2: Register tools from a service instance
        Console.WriteLine("📋 Method 2: Service Instance Tool Registration");
        Console.WriteLine("Registering tools from DemoBusinessService:");
        Console.WriteLine();

        // This would be done with dependency injection in a real application
        // var businessService = serviceProvider.GetRequiredService<IDemoBusinessService>();
        // var businessTools = new List<string>
        // {
        //     "DemoBusinessService.ValidateEmail",
        //     "DemoBusinessService.CalculateOrderTotal",
        //     "DemoBusinessService.GenerateCustomerId"
        // };

        Console.WriteLine("✅ Would register 3 tools from DemoBusinessService");
        Console.WriteLine();

        // Method 3: Dynamic tool discovery and registration
        Console.WriteLine("📋 Method 3: Dynamic Tool Discovery");
        Console.WriteLine("Automatically discovering and registering tools:");
        Console.WriteLine();

        var discoveredTools = DiscoverToolsWithDescriptionAttribute();
        Console.WriteLine($"✅ Discovered {discoveredTools.Count} tools with Description attributes");
        Console.WriteLine();

        // Display discovered tools
        foreach (var tool in discoveredTools.Take(5))
        {
            Console.WriteLine($"   • {tool.Name}: {tool.Description}");
        }
        
        if (discoveredTools.Count > 5)
        {
            Console.WriteLine($"   ... and {discoveredTools.Count - 5} more tools");
        }
        Console.WriteLine();

        Console.WriteLine("🎉 Tool registration example completed!");
        Console.WriteLine();
        Console.WriteLine("💡 Key Points:");
        Console.WriteLine("   • Use [Description] attributes on methods");
        Console.WriteLine("   • Register tools with Microsoft Agent Framework APIs");
        Console.WriteLine("   • Tools are automatically discovered by the framework");
        Console.WriteLine("   • Description attributes provide tool metadata");
        Console.WriteLine();
    }

    /// <summary>
    /// Discovers tools that have Description attributes (Microsoft Agent Framework pattern)
    /// </summary>
    private static List<DiscoveredTool> DiscoverToolsWithDescriptionAttribute()
    {
        var tools = new List<DiscoveredTool>();

        // Get all types in the current assembly
        var assembly = typeof(DemoTools).Assembly;
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            // Check static methods
            var staticMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null);

            foreach (var method in staticMethods)
            {
                var descriptionAttr = method.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttr != null)
                {
                    tools.Add(new DiscoveredTool
                    {
                        Name = method.Name,
                        Description = descriptionAttr.Description,
                        Category = "Microsoft Agent Framework Tool",
                        Parameters = method.GetParameters().Select(p => new DiscoveredProperty
                        {
                            Name = p.Name ?? "unknown",
                            Type = p.ParameterType.Name,
                            IsOptional = p.HasDefaultValue
                        }).ToList(),
                        ReturnType = method.ReturnType.Name,
                        FilePath = type.FullName ?? "unknown",
                        LineNumber = 0
                    });
                }
            }

            // Check instance methods (for services)
            var instanceMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null);

            foreach (var method in instanceMethods)
            {
                var descriptionAttr = method.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttr != null)
                {
                    tools.Add(new DiscoveredTool
                    {
                        Name = $"{type.Name}.{method.Name}",
                        Description = descriptionAttr.Description,
                        Category = "Microsoft Agent Framework Service Tool",
                        Parameters = method.GetParameters().Select(p => new DiscoveredProperty
                        {
                            Name = p.Name ?? "unknown",
                            Type = p.ParameterType.Name,
                            IsOptional = p.HasDefaultValue
                        }).ToList(),
                        ReturnType = method.ReturnType.Name,
                        FilePath = type.FullName ?? "unknown",
                        LineNumber = 0
                    });
                }
            }
        }

        return tools;
    }
}

/// <summary>
/// Represents a discovered tool for demonstration purposes
/// </summary>
public class DiscoveredTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<DiscoveredProperty> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

/// <summary>
/// Represents a tool parameter for demonstration purposes
/// </summary>
public class DiscoveredProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}
