using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Codefix.AIPlayGround.Data;
using Codefix.AIPlayGround.Models.DTOs;
using Xunit;

namespace Codefix.AIPlayGround.Tests.Integration;

public class AgentFrameworkIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;

    public AgentFrameworkIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
        _context = _factory.Services.GetRequiredService<ApplicationDbContext>();
    }

    [Fact]
    public async Task GetAgents_ReturnsOkWithEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/agents");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var agents = JsonSerializer.Deserialize<List<AgentDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.Empty(agents);
    }

    [Fact]
    public async Task CreateAgent_ReturnsCreatedResult()
    {
        // Arrange
        var createAgentDto = new CreateAgentDto
        {
            Name = "Integration Test Agent",
            Description = "Agent created during integration test",
            AgentType = "LLMAgent",
            Instructions = "You are a test agent for integration testing",
            LLMConfiguration = new LLMConfigurationDto
            {
                ModelName = "gpt-4",
                Provider = "OpenAI",
                Temperature = 0.7,
                MaxTokens = 2000
            },
            Tools = new List<ToolConfigurationDto>(),
            Properties = new Dictionary<string, object>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/agents", createAgentDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var agent = JsonSerializer.Deserialize<AgentDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.Equal("Integration Test Agent", agent.Name);
        Assert.Equal("LLMAgent", agent.AgentType);
        Assert.Equal("Draft", agent.Status);
    }

    [Fact]
    public async Task CreateAndGetAgent_ReturnsCorrectAgent()
    {
        // Arrange
        var createAgentDto = new CreateAgentDto
        {
            Name = "Test Agent",
            Description = "Test description",
            AgentType = "ToolAgent",
            Instructions = "Test instructions"
        };

        // Act - Create agent
        var createResponse = await _client.PostAsJsonAsync("/api/agents", createAgentDto);
        var createdAgent = await createResponse.Content.ReadFromJsonAsync<AgentDto>();

        // Act - Get agent
        var getResponse = await _client.GetAsync($"/api/agents/{createdAgent.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var agent = await getResponse.Content.ReadFromJsonAsync<AgentDetailDto>();
        
        Assert.Equal(createdAgent.Id, agent.Id);
        Assert.Equal("Test Agent", agent.Name);
        Assert.Equal("ToolAgent", agent.AgentType);
        Assert.Equal("Test instructions", agent.Instructions);
    }

    [Fact]
    public async Task GetNonExistentAgent_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/agents/non-existent-id");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TestAgent_ReturnsTestResult()
    {
        // Arrange
        var createAgentDto = new CreateAgentDto
        {
            Name = "Test Agent",
            AgentType = "LLMAgent",
            Instructions = "Test instructions"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/agents", createAgentDto);
        var createdAgent = await createResponse.Content.ReadFromJsonAsync<AgentDto>();

        var testInput = new TestInputDto
        {
            Input = new Dictionary<string, object>
            {
                { "message", "Hello, how are you?" },
                { "context", new { userId = "test-user" } }
            }
        };

        // Act
        var testResponse = await _client.PostAsJsonAsync($"/api/agents/{createdAgent.Id}/test", testInput);

        // Assert
        Assert.Equal(HttpStatusCode.OK, testResponse.StatusCode);
        var testResult = await testResponse.Content.ReadFromJsonAsync<TestResultDto>();
        
        Assert.True(testResult.IsSuccess);
        Assert.Equal("Agent test completed successfully", testResult.Message);
        Assert.NotNull(testResult.Input);
        Assert.NotNull(testResult.Output);
    }

    public void Dispose()
    {
        _context.Dispose();
        _client.Dispose();
    }
}
