using Microsoft.AspNetCore.Mvc;
using Codefix.AIPlayGround.Services;

namespace Codefix.AIPlayGround.Controllers;

/// <summary>
/// Demo controller showcasing various API patterns for code detection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DemoController : ControllerBase
{
    private readonly IDemoBusinessService _businessService;
    private readonly ILogger<DemoController> _logger;

    public DemoController(IDemoBusinessService businessService, ILogger<DemoController> logger)
    {
        _businessService = businessService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all customers
    /// </summary>
    /// <returns>List of customers</returns>
    [HttpGet("customers")]
    [ProducesResponseType(typeof(List<Customer>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<List<Customer>>> GetCustomers()
    {
        try
        {
            _logger.LogInformation("Getting all customers");
            // This would normally call a service method
            return Ok(new List<Customer>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers");
            return BadRequest("Error retrieving customers");
        }
    }

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details</returns>
    [HttpGet("customers/{id}")]
    [ProducesResponseType(typeof(Customer), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        try
        {
            _logger.LogInformation("Getting customer: {CustomerId}", id);
            
            var customer = await _businessService.GetCustomerAsync(id);
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer: {CustomerId}", id);
            return BadRequest("Error retrieving customer");
        }
    }

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="request">Customer creation request</param>
    /// <returns>Created customer</returns>
    [HttpPost("customers")]
    [ProducesResponseType(typeof(Customer), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Customer>> CreateCustomer([FromBody] CustomerRequest request)
    {
        try
        {
            _logger.LogInformation("Creating customer: {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _businessService.CreateCustomerAsync(request);
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer: {Email}", request.Email);
            return BadRequest("Error creating customer");
        }
    }

    /// <summary>
    /// Updates an existing customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Update result</returns>
    [HttpPut("customers/{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> UpdateCustomer(int id, [FromBody] CustomerUpdateRequest request)
    {
        try
        {
            _logger.LogInformation("Updating customer: {CustomerId}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _businessService.UpdateCustomerAsync(id, request);
            if (!success)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer: {CustomerId}", id);
            return BadRequest("Error updating customer");
        }
    }

    /// <summary>
    /// Deletes a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Delete result</returns>
    [HttpDelete("customers/{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> DeleteCustomer(int id)
    {
        try
        {
            _logger.LogInformation("Deleting customer: {CustomerId}", id);

            var success = await _businessService.DeleteCustomerAsync(id);
            if (!success)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer: {CustomerId}", id);
            return BadRequest("Error deleting customer");
        }
    }

    /// <summary>
    /// Searches customers by name or email
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>List of matching customers</returns>
    [HttpGet("customers/search")]
    [ProducesResponseType(typeof(List<Customer>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<List<Customer>>> SearchCustomers([FromQuery] string searchTerm)
    {
        try
        {
            _logger.LogInformation("Searching customers with term: {SearchTerm}", searchTerm);

            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Search term is required");
            }

            var customers = await _businessService.SearchCustomersAsync(searchTerm);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers: {SearchTerm}", searchTerm);
            return BadRequest("Error searching customers");
        }
    }

    /// <summary>
    /// Gets orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of customer orders</returns>
    [HttpGet("customers/{customerId}/orders")]
    [ProducesResponseType(typeof(List<Order>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<List<Order>>> GetCustomerOrders(int customerId)
    {
        try
        {
            _logger.LogInformation("Getting orders for customer: {CustomerId}", customerId);

            var orders = await _businessService.GetCustomerOrdersAsync(customerId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer orders: {CustomerId}", customerId);
            return BadRequest("Error retrieving customer orders");
        }
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <returns>Created order</returns>
    [HttpPost("orders")]
    [ProducesResponseType(typeof(Order), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] OrderRequest request)
    {
        try
        {
            _logger.LogInformation("Creating order for customer: {CustomerId}", request.CustomerId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _businessService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetCustomerOrders), new { customerId = order.CustomerId }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer: {CustomerId}", request.CustomerId);
            return BadRequest("Error creating order");
        }
    }

    /// <summary>
    /// Processes a payment for an order
    /// </summary>
    /// <param name="request">Payment request</param>
    /// <returns>Payment result</returns>
    [HttpPost("payments")]
    [ProducesResponseType(typeof(PaymentResult), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<PaymentResult>> ProcessPayment([FromBody] PaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing payment for order: {OrderId}", request.OrderId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _businessService.ProcessPaymentAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order: {OrderId}", request.OrderId);
            return BadRequest("Error processing payment");
        }
    }

    /// <summary>
    /// Generates a business report
    /// </summary>
    /// <param name="request">Report request</param>
    /// <returns>Generated report</returns>
    [HttpPost("reports")]
    [ProducesResponseType(typeof(Report), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<Report>> GenerateReport([FromBody] ReportRequest request)
    {
        try
        {
            _logger.LogInformation("Generating report: {ReportType}", request.ReportType);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var report = await _businessService.GenerateReportAsync(request);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report: {ReportType}", request.ReportType);
            return BadRequest("Error generating report");
        }
    }

    /// <summary>
    /// Validates an email address
    /// </summary>
    /// <param name="email">Email to validate</param>
    /// <returns>Validation result</returns>
    [HttpGet("validation/email")]
    [ProducesResponseType(typeof(ValidationResult), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ValidationResult>> ValidateEmail([FromQuery] string email)
    {
        try
        {
            _logger.LogInformation("Validating email: {Email}", email);

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required");
            }

            var isValid = _businessService.ValidateEmail(email);
            var result = new ValidationResult
            {
                Field = "email",
                Value = email,
                IsValid = isValid,
                Message = isValid ? "Valid email format" : "Invalid email format"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating email: {Email}", email);
            return BadRequest("Error validating email");
        }
    }

    /// <summary>
    /// Calculates order total with tax
    /// </summary>
    /// <param name="subtotal">Order subtotal</param>
    /// <param name="taxRate">Tax rate (optional)</param>
    /// <returns>Calculated total</returns>
    [HttpGet("calculator/order-total")]
    [ProducesResponseType(typeof(CalculationResult), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<CalculationResult>> CalculateOrderTotal([FromQuery] decimal subtotal, [FromQuery] decimal? taxRate = null)
    {
        try
        {
            _logger.LogInformation("Calculating order total: {Subtotal}, Tax Rate: {TaxRate}", subtotal, taxRate);

            var rate = taxRate ?? 0.08m;
            var total = _businessService.CalculateOrderTotal(subtotal, rate);
            
            var result = new CalculationResult
            {
                Subtotal = subtotal,
                TaxRate = rate,
                TaxAmount = total - subtotal,
                Total = total
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating order total: {Subtotal}", subtotal);
            return BadRequest("Error calculating order total");
        }
    }

    /// <summary>
    /// Generates a new customer ID
    /// </summary>
    /// <returns>Generated customer ID</returns>
    [HttpGet("generator/customer-id")]
    [ProducesResponseType(typeof(GeneratorResult), 200)]
    public async Task<ActionResult<GeneratorResult>> GenerateCustomerId()
    {
        try
        {
            _logger.LogInformation("Generating customer ID");

            var customerId = _businessService.GenerateCustomerId();
            var result = new GeneratorResult
            {
                Type = "customer-id",
                Value = customerId,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer ID");
            return BadRequest("Error generating customer ID");
        }
    }
}

// Response DTOs
public class ValidationResult
{
    public string Field { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CalculationResult
{
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}

public class GeneratorResult
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
