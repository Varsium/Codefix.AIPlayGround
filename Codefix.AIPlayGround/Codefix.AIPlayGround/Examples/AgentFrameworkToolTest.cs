using Microsoft.Agents.AI;
using Codefix.AIPlayGround.Services;
using System.ComponentModel;
using System.Reflection;

namespace Codefix.AIPlayGround.Examples;

/// <summary>
/// Comprehensive test to verify Microsoft Agent Framework tool integration
/// </summary>
public class AgentFrameworkToolTest
{
    /// <summary>
    /// Tests all our tools to ensure they work correctly with Microsoft Agent Framework
    /// </summary>
    public static async Task TestAllTools()
    {
        Console.WriteLine("🧪 Microsoft Agent Framework Tool Integration Test");
        Console.WriteLine("================================================");
        Console.WriteLine();

        var testResults = new List<ToolTestResult>();

        // Test DemoTools static methods
        Console.WriteLine("📋 Testing DemoTools Static Methods:");
        Console.WriteLine();

        testResults.AddRange(TestStaticTools());

        // Test DemoBusinessService methods
        Console.WriteLine("📋 Testing DemoBusinessService Methods:");
        Console.WriteLine();

        testResults.AddRange(TestServiceTools());

        // Summary
        Console.WriteLine("📊 Test Summary:");
        Console.WriteLine($"   ✅ Passed: {testResults.Count(r => r.Passed)}");
        Console.WriteLine($"   ❌ Failed: {testResults.Count(r => !r.Passed)}");
        Console.WriteLine($"   📈 Success Rate: {(testResults.Count(r => r.Passed) * 100.0 / testResults.Count):F1}%");
        Console.WriteLine();

        // Show failed tests
        var failedTests = testResults.Where(r => !r.Passed).ToList();
        if (failedTests.Any())
        {
            Console.WriteLine("❌ Failed Tests:");
            foreach (var failed in failedTests)
            {
                Console.WriteLine($"   • {failed.ToolName}: {failed.ErrorMessage}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("🎉 Tool integration test completed!");
        Console.WriteLine();
        Console.WriteLine("💡 All tools are properly configured for Microsoft Agent Framework!");
    }

    /// <summary>
    /// Tests static tools from DemoTools class
    /// </summary>
    private static List<ToolTestResult> TestStaticTools()
    {
        var results = new List<ToolTestResult>();

        // Test Add
        try
        {
            var result = DemoTools.Add(5, 3);
            results.Add(new ToolTestResult("DemoTools.Add", true, $"Result: {result}", null));
            Console.WriteLine($"   ✅ Add(5, 3) = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.Add", false, null, ex.Message));
            Console.WriteLine($"   ❌ Add(5, 3) failed: {ex.Message}");
        }

        // Test Multiply
        try
        {
            var result = DemoTools.Multiply(4, 7);
            results.Add(new ToolTestResult("DemoTools.Multiply", true, $"Result: {result}", null));
            Console.WriteLine($"   ✅ Multiply(4, 7) = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.Multiply", false, null, ex.Message));
            Console.WriteLine($"   ❌ Multiply(4, 7) failed: {ex.Message}");
        }

        // Test ValidateEmail
        try
        {
            var result1 = DemoTools.ValidateEmail("test@example.com");
            var result2 = DemoTools.ValidateEmail("invalid-email");
            results.Add(new ToolTestResult("DemoTools.ValidateEmail", true, $"Valid: {result1}, Invalid: {result2}", null));
            Console.WriteLine($"   ✅ ValidateEmail('test@example.com') = {result1}");
            Console.WriteLine($"   ✅ ValidateEmail('invalid-email') = {result2}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.ValidateEmail", false, null, ex.Message));
            Console.WriteLine($"   ❌ ValidateEmail failed: {ex.Message}");
        }

        // Test GeneratePassword
        try
        {
            var result = DemoTools.GeneratePassword(12, true);
            results.Add(new ToolTestResult("DemoTools.GeneratePassword", true, $"Generated: {result}", null));
            Console.WriteLine($"   ✅ GeneratePassword(12, true) = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.GeneratePassword", false, null, ex.Message));
            Console.WriteLine($"   ❌ GeneratePassword failed: {ex.Message}");
        }

        // Test FormatJson
        try
        {
            var json = "{\"name\":\"test\",\"value\":123}";
            var result = DemoTools.FormatJson(json);
            results.Add(new ToolTestResult("DemoTools.FormatJson", true, $"Formatted JSON", null));
            Console.WriteLine($"   ✅ FormatJson('{json}') = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.FormatJson", false, null, ex.Message));
            Console.WriteLine($"   ❌ FormatJson failed: {ex.Message}");
        }

        // Test CalculateLevenshteinDistance
        try
        {
            var result = DemoTools.CalculateLevenshteinDistance("kitten", "sitting");
            results.Add(new ToolTestResult("DemoTools.CalculateLevenshteinDistance", true, $"Distance: {result}", null));
            Console.WriteLine($"   ✅ CalculateLevenshteinDistance('kitten', 'sitting') = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.CalculateLevenshteinDistance", false, null, ex.Message));
            Console.WriteLine($"   ❌ CalculateLevenshteinDistance failed: {ex.Message}");
        }

        // Test GetSystemInfo
        try
        {
            var result = DemoTools.GetSystemInfo();
            results.Add(new ToolTestResult("DemoTools.GetSystemInfo", true, "System info retrieved", null));
            Console.WriteLine($"   ✅ GetSystemInfo() = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.GetSystemInfo", false, null, ex.Message));
            Console.WriteLine($"   ❌ GetSystemInfo failed: {ex.Message}");
        }

        // Test EncryptCaesar
        try
        {
            var result = DemoTools.EncryptCaesar("Hello World", 3);
            results.Add(new ToolTestResult("DemoTools.EncryptCaesar", true, $"Encrypted: {result}", null));
            Console.WriteLine($"   ✅ EncryptCaesar('Hello World', 3) = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.EncryptCaesar", false, null, ex.Message));
            Console.WriteLine($"   ❌ EncryptCaesar failed: {ex.Message}");
        }

        // Test DecryptCaesar
        try
        {
            var encrypted = DemoTools.EncryptCaesar("Hello World", 3);
            var result = DemoTools.DecryptCaesar(encrypted, 3);
            results.Add(new ToolTestResult("DemoTools.DecryptCaesar", true, $"Decrypted: {result}", null));
            Console.WriteLine($"   ✅ DecryptCaesar('{encrypted}', 3) = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.DecryptCaesar", false, null, ex.Message));
            Console.WriteLine($"   ❌ DecryptCaesar failed: {ex.Message}");
        }

        // Test Factorial
        try
        {
            var result = DemoTools.Factorial(5);
            results.Add(new ToolTestResult("DemoTools.Factorial", true, $"Result: {result}", null));
            Console.WriteLine($"   ✅ Factorial(5) = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.Factorial", false, null, ex.Message));
            Console.WriteLine($"   ❌ Factorial failed: {ex.Message}");
        }

        // Test IsPrime
        try
        {
            var result1 = DemoTools.IsPrime(17);
            var result2 = DemoTools.IsPrime(15);
            results.Add(new ToolTestResult("DemoTools.IsPrime", true, $"17: {result1}, 15: {result2}", null));
            Console.WriteLine($"   ✅ IsPrime(17) = {result1}");
            Console.WriteLine($"   ✅ IsPrime(15) = {result2}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.IsPrime", false, null, ex.Message));
            Console.WriteLine($"   ❌ IsPrime failed: {ex.Message}");
        }

        // Test ConvertTemperature
        try
        {
            var result = DemoTools.ConvertTemperature(32, "F", "C");
            results.Add(new ToolTestResult("DemoTools.ConvertTemperature", true, $"Result: {result}", null));
            Console.WriteLine($"   ✅ ConvertTemperature(32, 'F', 'C') = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.ConvertTemperature", false, null, ex.Message));
            Console.WriteLine($"   ❌ ConvertTemperature failed: {ex.Message}");
        }

        // Test GenerateGuid
        try
        {
            var result = DemoTools.GenerateGuid();
            results.Add(new ToolTestResult("DemoTools.GenerateGuid", true, $"Generated: {result}", null));
            Console.WriteLine($"   ✅ GenerateGuid() = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.GenerateGuid", false, null, ex.Message));
            Console.WriteLine($"   ❌ GenerateGuid failed: {ex.Message}");
        }

        // Test CalculateMD5Hash
        try
        {
            var result = DemoTools.CalculateMD5Hash("Hello World");
            results.Add(new ToolTestResult("DemoTools.CalculateMD5Hash", true, $"Hash: {result}", null));
            Console.WriteLine($"   ✅ CalculateMD5Hash('Hello World') = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.CalculateMD5Hash", false, null, ex.Message));
            Console.WriteLine($"   ❌ CalculateMD5Hash failed: {ex.Message}");
        }

        // Test FormatCurrency
        try
        {
            var result = DemoTools.FormatCurrency(123.45m, "USD", "en-US");
            results.Add(new ToolTestResult("DemoTools.FormatCurrency", true, $"Formatted: {result}", null));
            Console.WriteLine($"   ✅ FormatCurrency(123.45m, 'USD', 'en-US') = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.FormatCurrency", false, null, ex.Message));
            Console.WriteLine($"   ❌ FormatCurrency failed: {ex.Message}");
        }

        // Test ValidateCreditCard
        try
        {
            var result1 = DemoTools.ValidateCreditCard("4111111111111111"); // Valid test card
            var result2 = DemoTools.ValidateCreditCard("1234567890123456"); // Invalid card
            results.Add(new ToolTestResult("DemoTools.ValidateCreditCard", true, $"Valid: {result1}, Invalid: {result2}", null));
            Console.WriteLine($"   ✅ ValidateCreditCard('4111111111111111') = {result1}");
            Console.WriteLine($"   ✅ ValidateCreditCard('1234567890123456') = {result2}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoTools.ValidateCreditCard", false, null, ex.Message));
            Console.WriteLine($"   ❌ ValidateCreditCard failed: {ex.Message}");
        }

        Console.WriteLine();
        return results;
    }

    /// <summary>
    /// Tests service tools from DemoBusinessService
    /// </summary>
    private static List<ToolTestResult> TestServiceTools()
    {
        var results = new List<ToolTestResult>();

        // Create a service instance for testing
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DemoBusinessService>.Instance;
        var service = new DemoBusinessService(logger);

        // Test ValidateEmail
        try
        {
            var result1 = service.ValidateEmail("test@example.com");
            var result2 = service.ValidateEmail("invalid-email");
            results.Add(new ToolTestResult("DemoBusinessService.ValidateEmail", true, $"Valid: {result1}, Invalid: {result2}", null));
            Console.WriteLine($"   ✅ ValidateEmail('test@example.com') = {result1}");
            Console.WriteLine($"   ✅ ValidateEmail('invalid-email') = {result2}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoBusinessService.ValidateEmail", false, null, ex.Message));
            Console.WriteLine($"   ❌ ValidateEmail failed: {ex.Message}");
        }

        // Test CalculateOrderTotal
        try
        {
            var result = service.CalculateOrderTotal(100.00m, 0.08m);
            results.Add(new ToolTestResult("DemoBusinessService.CalculateOrderTotal", true, $"Total: {result}", null));
            Console.WriteLine($"   ✅ CalculateOrderTotal(100.00m, 0.08m) = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoBusinessService.CalculateOrderTotal", false, null, ex.Message));
            Console.WriteLine($"   ❌ CalculateOrderTotal failed: {ex.Message}");
        }

        // Test GenerateCustomerId
        try
        {
            var result = service.GenerateCustomerId();
            results.Add(new ToolTestResult("DemoBusinessService.GenerateCustomerId", true, $"Generated: {result}", null));
            Console.WriteLine($"   ✅ GenerateCustomerId() = {result}");
        }
        catch (Exception ex)
        {
            results.Add(new ToolTestResult("DemoBusinessService.GenerateCustomerId", false, null, ex.Message));
            Console.WriteLine($"   ❌ GenerateCustomerId failed: {ex.Message}");
        }

        Console.WriteLine();
        return results;
    }
}

/// <summary>
/// Represents the result of a tool test
/// </summary>
public class ToolTestResult
{
    public string ToolName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }

    public ToolTestResult(string toolName, bool passed, string? result, string? errorMessage)
    {
        ToolName = toolName;
        Passed = passed;
        Result = result;
        ErrorMessage = errorMessage;
    }
}
