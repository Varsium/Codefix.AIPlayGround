using System.Text.Json;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Demo tools for showcasing code detection and tool integration capabilities
/// </summary>
public static class DemoTools
{
    /// <summary>
    /// Calculates the sum of two numbers
    /// </summary>
    /// <param name="a">First number</param>
    /// <param name="b">Second number</param>
    /// <returns>The sum of a and b</returns>
    [Description("Adds two numbers together and returns the result")]
    public static double Add(double a, double b)
    {
        return a + b;
    }

    /// <summary>
    /// Calculates the product of two numbers
    /// </summary>
    /// <param name="a">First number</param>
    /// <param name="b">Second number</param>
    /// <returns>The product of a and b</returns>
    [Description("Multiplies two numbers together and returns the result")]
    public static double Multiply(double a, double b)
    {
        return a * b;
    }

    /// <summary>
    /// Validates an email address using regex
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email is valid, false otherwise</returns>
    [Description("Validates an email address format using regex pattern matching")]
    public static bool ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(email);
    }

    /// <summary>
    /// Formats a string to title case
    /// </summary>
    /// <param name="input">Input string to format</param>
    /// <returns>String formatted to title case</returns>
    [Description("Converts a string to title case format (first letter of each word capitalized)")]
    public static string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleCaseWords = words.Select(word => 
            char.ToUpper(word[0]) + word.Substring(1).ToLower());
        
        return string.Join(" ", titleCaseWords);
    }

    /// <summary>
    /// Generates a random password with specified length
    /// </summary>
    /// <param name="length">Length of the password (default: 12)</param>
    /// <param name="includeSpecialChars">Include special characters (default: true)</param>
    /// <returns>Generated password</returns>
    [Description("Generates a secure random password with specified length and character options")]
    public static string GeneratePassword(int length = 12, bool includeSpecialChars = true)
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        var chars = lowercase + uppercase + digits;
        if (includeSpecialChars)
            chars += special;

        var random = new Random();
        var password = new char[length];

        for (int i = 0; i < length; i++)
        {
            password[i] = chars[random.Next(chars.Length)];
        }

        return new string(password);
    }

    /// <summary>
    /// Converts a JSON string to a formatted, indented JSON string
    /// </summary>
    /// <param name="json">JSON string to format</param>
    /// <returns>Formatted JSON string</returns>
    [Description("Formats a JSON string with proper indentation for readability")]
    public static string FormatJson(string json)
    {
        try
        {
            var jsonDocument = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (JsonException)
        {
            return "Invalid JSON format";
        }
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings
    /// </summary>
    /// <param name="source">Source string</param>
    /// <param name="target">Target string</param>
    /// <returns>Levenshtein distance</returns>
    [Description("Calculates the Levenshtein distance (edit distance) between two strings")]
    public static int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        var distance = new int[source.Length + 1, target.Length + 1];

        for (int i = 0; i <= source.Length; i++)
            distance[i, 0] = i;

        for (int j = 0; j <= target.Length; j++)
            distance[0, j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[source.Length, target.Length];
    }

    /// <summary>
    /// Gets the current system information
    /// </summary>
    /// <returns>System information as JSON</returns>
    [Description("Retrieves current system information including machine name, OS version, and hardware details")]
    public static string GetSystemInfo()
    {
        var systemInfo = new
        {
            MachineName = Environment.MachineName,
            OSVersion = Environment.OSVersion.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            WorkingSet = Environment.WorkingSet,
            TickCount = Environment.TickCount,
            UserName = Environment.UserName,
            CurrentDirectory = Environment.CurrentDirectory,
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(systemInfo, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Encrypts a string using a simple Caesar cipher
    /// </summary>
    /// <param name="text">Text to encrypt</param>
    /// <param name="shift">Shift amount (default: 3)</param>
    /// <returns>Encrypted text</returns>
    [Description("Encrypts text using Caesar cipher with specified shift amount")]
    public static string EncryptCaesar(string text, int shift = 3)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var result = new char[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (char.IsLetter(c))
            {
                char baseChar = char.IsUpper(c) ? 'A' : 'a';
                result[i] = (char)(((c - baseChar + shift) % 26) + baseChar);
            }
            else
            {
                result[i] = c;
            }
        }

        return new string(result);
    }

    /// <summary>
    /// Decrypts a string using Caesar cipher
    /// </summary>
    /// <param name="encryptedText">Encrypted text</param>
    /// <param name="shift">Shift amount (default: 3)</param>
    /// <returns>Decrypted text</returns>
    [Description("Decrypts text that was encrypted using Caesar cipher")]
    public static string DecryptCaesar(string encryptedText, int shift = 3)
    {
        return EncryptCaesar(encryptedText, -shift);
    }

    /// <summary>
    /// Calculates the factorial of a number
    /// </summary>
    /// <param name="n">Number to calculate factorial for</param>
    /// <returns>Factorial of n</returns>
    [Description("Calculates the factorial of a given number")]
    public static long Factorial(int n)
    {
        if (n < 0)
            throw new ArgumentException("Factorial is not defined for negative numbers");
        
        if (n <= 1)
            return 1;

        long result = 1;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }

        return result;
    }

    /// <summary>
    /// Checks if a number is prime
    /// </summary>
    /// <param name="number">Number to check</param>
    /// <returns>True if number is prime, false otherwise</returns>
    [Description("Determines if a number is prime using efficient algorithm")]
    public static bool IsPrime(int number)
    {
        if (number < 2)
            return false;

        if (number == 2)
            return true;

        if (number % 2 == 0)
            return false;

        for (int i = 3; i * i <= number; i += 2)
        {
            if (number % i == 0)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Converts temperature between Celsius and Fahrenheit
    /// </summary>
    /// <param name="temperature">Temperature value</param>
    /// <param name="fromUnit">Source unit (C or F)</param>
    /// <param name="toUnit">Target unit (C or F)</param>
    /// <returns>Converted temperature</returns>
    [Description("Converts temperature between Celsius and Fahrenheit")]
    public static double ConvertTemperature(double temperature, string fromUnit, string toUnit)
    {
        if (fromUnit.ToUpper() == toUnit.ToUpper())
            return temperature;

        if (fromUnit.ToUpper() == "C" && toUnit.ToUpper() == "F")
        {
            return (temperature * 9.0 / 5.0) + 32;
        }
        else if (fromUnit.ToUpper() == "F" && toUnit.ToUpper() == "C")
        {
            return (temperature - 32) * 5.0 / 9.0;
        }

        throw new ArgumentException($"Unsupported conversion from {fromUnit} to {toUnit}");
    }

    /// <summary>
    /// Generates a UUID/GUID
    /// </summary>
    /// <returns>New GUID as string</returns>
    [Description("Generates a new globally unique identifier (GUID)")]
    public static string GenerateGuid()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Calculates the hash of a string using MD5
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>MD5 hash as hexadecimal string</returns>
    [Description("Calculates MD5 hash of input string")]
    public static string CalculateMD5Hash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        using var md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        
        return Convert.ToHexString(hashBytes).ToLower();
    }

    /// <summary>
    /// Formats a number as currency
    /// </summary>
    /// <param name="amount">Amount to format</param>
    /// <param name="currencyCode">Currency code (default: USD)</param>
    /// <param name="culture">Culture code (default: en-US)</param>
    /// <returns>Formatted currency string</returns>
    [Description("Formats a number as currency with specified culture and currency code")]
    public static string FormatCurrency(decimal amount, string currencyCode = "USD", string culture = "en-US")
    {
        var cultureInfo = new System.Globalization.CultureInfo(culture);
        return amount.ToString("C", cultureInfo);
    }

    /// <summary>
    /// Validates a credit card number using Luhn algorithm
    /// </summary>
    /// <param name="cardNumber">Credit card number</param>
    /// <returns>True if card number is valid, false otherwise</returns>
    [Description("Validates credit card number using Luhn algorithm")]
    public static bool ValidateCreditCard(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber))
            return false;

        // Remove spaces and non-digits
        var cleanNumber = Regex.Replace(cardNumber, @"\D", "");
        
        if (cleanNumber.Length < 13 || cleanNumber.Length > 19)
            return false;

        var sum = 0;
        var isEven = false;

        // Process digits from right to left
        for (int i = cleanNumber.Length - 1; i >= 0; i--)
        {
            var digit = int.Parse(cleanNumber[i].ToString());

            if (isEven)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            isEven = !isEven;
        }

        return sum % 10 == 0;
    }
}

