using System.Text.Json;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Codefix.AIPlayGround.Services;

/// <summary>
/// Demo business service showcasing various patterns for code detection
/// </summary>
public interface IDemoBusinessService
{
    Task<Customer> CreateCustomerAsync(CustomerRequest request);
    Task<Customer?> GetCustomerAsync(int customerId);
    Task<List<Customer>> SearchCustomersAsync(string searchTerm);
    Task<bool> UpdateCustomerAsync(int customerId, CustomerUpdateRequest request);
    Task<bool> DeleteCustomerAsync(int customerId);
    Task<Order> CreateOrderAsync(OrderRequest request);
    Task<List<Order>> GetCustomerOrdersAsync(int customerId);
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    Task<Report> GenerateReportAsync(ReportRequest request);
    
    // Tool methods
    bool ValidateEmail(string email);
    decimal CalculateOrderTotal(decimal subtotal, decimal taxRate = 0.08m);
    string GenerateCustomerId();
}

/// <summary>
/// Demo business service implementation
/// </summary>
public class DemoBusinessService : IDemoBusinessService
{
    private readonly ILogger<DemoBusinessService> _logger;
    private readonly List<Customer> _customers;
    private readonly List<Order> _orders;
    private readonly Random _random;

    public DemoBusinessService(ILogger<DemoBusinessService> logger)
    {
        _logger = logger;
        _customers = new List<Customer>();
        _orders = new List<Order>();
        _random = new Random();
        
        // Seed some demo data
        SeedDemoData();
    }

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="request">Customer creation request</param>
    /// <returns>Created customer</returns>
    public async Task<Customer> CreateCustomerAsync(CustomerRequest request)
    {
        _logger.LogInformation("Creating customer: {Email}", request.Email);

        var customer = new Customer
        {
            Id = _customers.Count + 1,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _customers.Add(customer);
        
        _logger.LogInformation("Customer created successfully: {CustomerId}", customer.Id);
        return await Task.FromResult(customer);
    }

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer if found, null otherwise</returns>
    public async Task<Customer?> GetCustomerAsync(int customerId)
    {
        _logger.LogInformation("Getting customer: {CustomerId}", customerId);
        
        var customer = _customers.FirstOrDefault(c => c.Id == customerId);
        return await Task.FromResult(customer);
    }

    /// <summary>
    /// Searches customers by name or email
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>List of matching customers</returns>
    public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
    {
        _logger.LogInformation("Searching customers with term: {SearchTerm}", searchTerm);

        var results = _customers.Where(c => 
            c.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            c.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return await Task.FromResult(results);
    }

    /// <summary>
    /// Updates an existing customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="request">Update request</param>
    /// <returns>True if updated successfully</returns>
    public async Task<bool> UpdateCustomerAsync(int customerId, CustomerUpdateRequest request)
    {
        _logger.LogInformation("Updating customer: {CustomerId}", customerId);

        var customer = _customers.FirstOrDefault(c => c.Id == customerId);
        if (customer == null)
        {
            _logger.LogWarning("Customer not found: {CustomerId}", customerId);
            return false;
        }

        customer.FirstName = request.FirstName ?? customer.FirstName;
        customer.LastName = request.LastName ?? customer.LastName;
        customer.Email = request.Email ?? customer.Email;
        customer.Phone = request.Phone ?? customer.Phone;
        customer.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Customer updated successfully: {CustomerId}", customerId);
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Deletes a customer (soft delete)
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteCustomerAsync(int customerId)
    {
        _logger.LogInformation("Deleting customer: {CustomerId}", customerId);

        var customer = _customers.FirstOrDefault(c => c.Id == customerId);
        if (customer == null)
        {
            _logger.LogWarning("Customer not found: {CustomerId}", customerId);
            return false;
        }

        customer.IsActive = false;
        customer.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Customer deleted successfully: {CustomerId}", customerId);
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <returns>Created order</returns>
    public async Task<Order> CreateOrderAsync(OrderRequest request)
    {
        _logger.LogInformation("Creating order for customer: {CustomerId}", request.CustomerId);

        var order = new Order
        {
            Id = _orders.Count + 1,
            CustomerId = request.CustomerId,
            Items = request.Items,
            TotalAmount = request.Items.Sum(i => i.Price * i.Quantity),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _orders.Add(order);

        _logger.LogInformation("Order created successfully: {OrderId}", order.Id);
        return await Task.FromResult(order);
    }

    /// <summary>
    /// Gets all orders for a customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of customer orders</returns>
    public async Task<List<Order>> GetCustomerOrdersAsync(int customerId)
    {
        _logger.LogInformation("Getting orders for customer: {CustomerId}", customerId);

        var orders = _orders.Where(o => o.CustomerId == customerId).ToList();
        return await Task.FromResult(orders);
    }

    /// <summary>
    /// Processes a payment for an order
    /// </summary>
    /// <param name="request">Payment request</param>
    /// <returns>Payment result</returns>
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        _logger.LogInformation("Processing payment for order: {OrderId}", request.OrderId);

        // Simulate payment processing
        await Task.Delay(1000); // Simulate network delay

        var success = _random.NextDouble() > 0.1; // 90% success rate

        var result = new PaymentResult
        {
            OrderId = request.OrderId,
            Success = success,
            TransactionId = success ? Guid.NewGuid().ToString() : null,
            ErrorMessage = success ? null : "Payment processing failed",
            ProcessedAt = DateTime.UtcNow
        };

        if (success)
        {
            var order = _orders.FirstOrDefault(o => o.Id == request.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Paid;
                order.UpdatedAt = DateTime.UtcNow;
            }
        }

        _logger.LogInformation("Payment processed: {Success} for order: {OrderId}", success, request.OrderId);
        return result;
    }

    /// <summary>
    /// Generates a business report
    /// </summary>
    /// <param name="request">Report request</param>
    /// <returns>Generated report</returns>
    public async Task<Report> GenerateReportAsync(ReportRequest request)
    {
        _logger.LogInformation("Generating report: {ReportType} for period: {StartDate} to {EndDate}", 
            request.ReportType, request.StartDate, request.EndDate);

        var report = new Report
        {
            Id = Guid.NewGuid().ToString(),
            Type = request.ReportType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            GeneratedAt = DateTime.UtcNow,
            Data = GenerateReportData(request)
        };

        _logger.LogInformation("Report generated successfully: {ReportId}", report.Id);
        return await Task.FromResult(report);
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    /// <param name="email">Email to validate</param>
    /// <returns>True if valid email format</returns>
    [Description("Validates email address format using regex pattern")]
    public bool ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(email);
    }

    /// <summary>
    /// Calculates order total with tax
    /// </summary>
    /// <param name="subtotal">Order subtotal</param>
    /// <param name="taxRate">Tax rate (default: 0.08)</param>
    /// <returns>Total with tax</returns>
    [Description("Calculates order total including tax based on subtotal and tax rate")]
    public decimal CalculateOrderTotal(decimal subtotal, decimal taxRate = 0.08m)
    {
        return subtotal + (subtotal * taxRate);
    }

    /// <summary>
    /// Generates a customer ID
    /// </summary>
    /// <returns>New customer ID</returns>
    [Description("Generates a unique customer ID with timestamp and random number")]
    public string GenerateCustomerId()
    {
        return $"CUST-{DateTime.UtcNow:yyyyMMdd}-{_random.Next(1000, 9999)}";
    }

    private void SeedDemoData()
    {
        _customers.AddRange(new[]
        {
            new Customer { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "555-0101", CreatedAt = DateTime.UtcNow.AddDays(-30), IsActive = true },
            new Customer { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Phone = "555-0102", CreatedAt = DateTime.UtcNow.AddDays(-25), IsActive = true },
            new Customer { Id = 3, FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@example.com", Phone = "555-0103", CreatedAt = DateTime.UtcNow.AddDays(-20), IsActive = true }
        });

        _orders.AddRange(new[]
        {
            new Order { Id = 1, CustomerId = 1, Items = new List<OrderItem> { new() { Name = "Widget A", Price = 29.99m, Quantity = 2 } }, TotalAmount = 59.98m, Status = OrderStatus.Paid, CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new Order { Id = 2, CustomerId = 2, Items = new List<OrderItem> { new() { Name = "Widget B", Price = 19.99m, Quantity = 1 } }, TotalAmount = 19.99m, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow.AddDays(-5) }
        });
    }

    private Dictionary<string, object> GenerateReportData(ReportRequest request)
    {
        var data = new Dictionary<string, object>
        {
            ["TotalCustomers"] = _customers.Count(c => c.CreatedAt >= request.StartDate && c.CreatedAt <= request.EndDate),
            ["TotalOrders"] = _orders.Count(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate),
            ["TotalRevenue"] = _orders.Where(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate && o.Status == OrderStatus.Paid)
                                    .Sum(o => o.TotalAmount),
            ["AverageOrderValue"] = _orders.Where(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate && o.Status == OrderStatus.Paid)
                                          .Average(o => o.TotalAmount)
        };

        return data;
    }
}

// Data Models
public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class OrderItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class CustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class CustomerUpdateRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class OrderRequest
{
    public int CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class PaymentRequest
{
    public int OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PaymentResult
{
    public int OrderId { get; set; }
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class ReportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class Report
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}
