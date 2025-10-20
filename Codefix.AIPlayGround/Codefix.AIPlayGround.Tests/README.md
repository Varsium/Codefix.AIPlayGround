# Agent Framework API Tests

This directory contains comprehensive tests for the Agent Framework API, including unit tests, integration tests, and test utilities.

## Test Structure

### Unit Tests
- **Services/AgentFrameworkServiceTests.cs**: Tests for the core Agent Framework service
- **Controllers/AgentsControllerTests.cs**: Tests for the Agents API controller

### Integration Tests
- **Integration/AgentFrameworkIntegrationTests.cs**: End-to-end API tests using TestServer

### Test Utilities
- **TestData/TestDataBuilder.cs**: Helper methods for creating test data

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Categories
```bash
# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"

# Service tests only
dotnet test --filter "FullyQualifiedName~Services"

# Controller tests only
dotnet test --filter "FullyQualifiedName~Controllers"
```

### Run with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Categories

### Unit Tests
- **AgentFrameworkService**: Tests the core business logic
  - Agent creation and validation
  - Flow execution
  - Configuration validation
  - Status monitoring
  - Error handling

- **AgentsController**: Tests the API endpoints
  - CRUD operations
  - Input validation
  - Response formatting
  - Error handling
  - Authentication/authorization

### Integration Tests
- **End-to-End API Testing**: Tests the complete request/response cycle
  - HTTP status codes
  - JSON serialization/deserialization
  - Database operations
  - Service integration
  - Error scenarios

## Test Data

The `TestDataBuilder` class provides factory methods for creating test entities:

```csharp
// Create a test agent
var agent = TestDataBuilder.CreateTestAgent(
    name: "My Test Agent",
    agentType: "LLMAgent",
    instructions: "You are a helpful assistant"
);

// Create a test agent DTO
var agentDto = TestDataBuilder.CreateTestAgentDto(
    name: "My Test Agent",
    agentType: "LLMAgent"
);

// Create multiple test agents
var agents = TestDataBuilder.CreateMultipleTestAgents(5);
```

## Test Configuration

### In-Memory Database
Tests use Entity Framework's in-memory database provider for fast, isolated testing:

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("TestDb");
});
```

### Mock Services
External dependencies are mocked using Moq:

```csharp
var mockService = new Mock<IAgentFrameworkService>();
mockService.Setup(s => s.CreateAgentAsync(It.IsAny<AgentEntity>()))
    .ReturnsAsync(AgentFrameworkResult.Success("Agent created"));
```

## Test Scenarios

### Agent Management
- ✅ Create agent with valid data
- ✅ Create agent with invalid data
- ✅ Get agent by ID
- ✅ Get non-existent agent
- ✅ Update agent
- ✅ Delete agent
- ✅ Filter agents by type/status
- ✅ Pagination

### Agent Operations
- ✅ Deploy agent
- ✅ Test agent execution
- ✅ Get agent status
- ✅ Update LLM configuration
- ✅ Update tools
- ✅ Update prompt template

### Flow Management
- ✅ Create flow
- ✅ Execute flow
- ✅ Add agents to flow
- ✅ Add nodes to flow
- ✅ Nested flows

### Node Management
- ✅ Create node
- ✅ Update node
- ✅ Delete node
- ✅ Node connections
- ✅ Node templates

### Error Handling
- ✅ Invalid input validation
- ✅ Not found scenarios
- ✅ Service failures
- ✅ Database errors
- ✅ Serialization errors

## Test Coverage

The test suite aims for comprehensive coverage of:

- **Controllers**: All endpoints and error scenarios
- **Services**: Core business logic and edge cases
- **Models**: Data validation and serialization
- **Database**: CRUD operations and relationships
- **Integration**: End-to-end workflows

## Continuous Integration

Tests are designed to run in CI/CD pipelines:

- Fast execution (< 30 seconds for full suite)
- No external dependencies
- Deterministic results
- Clear failure messages
- Coverage reporting

## Debugging Tests

### Visual Studio
1. Set breakpoints in test methods
2. Right-click test → "Debug Selected Tests"
3. Use Test Explorer for test management

### VS Code
1. Install C# extension
2. Use Test Explorer panel
3. Set breakpoints and debug individual tests

### Command Line
```bash
# Run with detailed output
dotnet test --verbosity detailed

# Run specific test
dotnet test --filter "FullyQualifiedName~CreateAgent_WithValidData_ReturnsCreatedResult"

# Run with logger
dotnet test --logger "console;verbosity=detailed"
```

## Best Practices

### Test Naming
- Use descriptive test method names
- Follow pattern: `Method_Scenario_ExpectedResult`
- Example: `CreateAgent_WithValidData_ReturnsCreatedResult`

### Test Organization
- One test class per class under test
- Group related tests in test methods
- Use test data builders for complex objects

### Assertions
- Use specific assertion methods
- Provide meaningful failure messages
- Test both success and failure scenarios

### Test Data
- Use realistic test data
- Avoid hardcoded values where possible
- Create reusable test data builders

### Cleanup
- Dispose of resources properly
- Use `IDisposable` pattern for test fixtures
- Clean up database state between tests
