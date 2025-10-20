using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models;
using Codefix.AIPlayGround.Services;
using Xunit;

namespace Codefix.AIPlayGround.Tests.Services;

public class AgentFrameworkServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<AgentFrameworkService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AgentFrameworkService _service;

    public AgentFrameworkServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<AgentFrameworkService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        _service = new AgentFrameworkService(_mockLogger.Object, _mockConfiguration.Object, _context);
    }

    [Fact]
    public async Task CreateAgentAsync_WithValidLLMAgent_ReturnsSuccess()
    {
        // Arrange
        var agent = new AgentEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test LLM Agent",
            AgentType = "LLMAgent",
            Instructions = "You are a helpful assistant",
            LLMConfigurationJson = JsonSerializer.Serialize(new
            {
                modelName = "gpt-4",
                provider = "OpenAI",
                temperature = 0.7
            })
        };

        // Act
        var result = await _service.CreateAgentAsync(agent);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("LLM Agent created successfully", result.Message);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task CreateAgentAsync_WithInvalidAgentType_ReturnsFailure()
    {
        // Arrange
        var agent = new AgentEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Agent",
            AgentType = "InvalidType",
            Instructions = "Test instructions"
        };

        // Act
        var result = await _service.CreateAgentAsync(agent);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not supported", result.Message);
    }

    [Fact]
    public async Task TestAgentAsync_WithValidAgent_ReturnsSuccess()
    {
        // Arrange
        var agent = new AgentEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Agent",
            AgentType = "LLMAgent",
            Instructions = "Test instructions"
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        var testInput = new { message = "Hello, how are you?" };

        // Act
        var result = await _service.TestAgentAsync(agent.Id, testInput);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Agent test completed successfully", result.Message);
        Assert.NotNull(result.Data);

        // Verify execution was created
        var execution = await _context.AgentExecutions.FirstOrDefaultAsync(e => e.AgentId == agent.Id);
        Assert.NotNull(execution);
        Assert.Equal(ExecutionStatus.Completed, execution.Status);
    }

    [Fact]
    public async Task TestAgentAsync_WithInvalidAgentId_ReturnsFailure()
    {
        // Arrange
        var invalidAgentId = Guid.NewGuid().ToString();
        var testInput = new { message = "Hello" };

        // Act
        var result = await _service.TestAgentAsync(invalidAgentId, testInput);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Agent not found", result.Message);
    }

    [Fact]
    public async Task ValidateAgentConfigurationAsync_WithValidAgent_ReturnsSuccess()
    {
        // Arrange
        var agent = new AgentEntity
        {
            Name = "Test Agent",
            AgentType = "LLMAgent",
            Instructions = "Test instructions",
            LLMConfigurationJson = JsonSerializer.Serialize(new
            {
                modelName = "gpt-4",
                provider = "OpenAI"
            }),
            ToolsConfigurationJson = JsonSerializer.Serialize(new List<object>())
        };

        // Act
        var result = await _service.ValidateAgentConfigurationAsync(agent);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Agent configuration is valid", result.Message);
    }

    [Fact]
    public async Task ValidateAgentConfigurationAsync_WithMissingName_ReturnsFailure()
    {
        // Arrange
        var agent = new AgentEntity
        {
            Name = "",
            AgentType = "LLMAgent",
            Instructions = "Test instructions"
        };

        // Act
        var result = await _service.ValidateAgentConfigurationAsync(agent);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Agent name is required", result.Errors);
    }

    [Fact]
    public async Task GetAgentStatusAsync_WithValidAgent_ReturnsStatus()
    {
        // Arrange
        var agent = new AgentEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Agent",
            AgentType = "LLMAgent",
            Status = AgentStatus.Active
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAgentStatusAsync(agent.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetAgentStatusAsync_WithInvalidAgentId_ReturnsFailure()
    {
        // Arrange
        var invalidAgentId = Guid.NewGuid().ToString();

        // Act
        var result = await _service.GetAgentStatusAsync(invalidAgentId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Agent not found", result.Message);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
