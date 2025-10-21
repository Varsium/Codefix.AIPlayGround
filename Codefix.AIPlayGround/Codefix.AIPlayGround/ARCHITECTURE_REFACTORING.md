# Architecture Refactoring - Shared Library Pattern

## Overview
We're implementing a clean architecture with:
1. **Shared Library (Client project)** - Contains all DTOs, interfaces, and shared models
2. **API Layer** - For WASM mode (HTTP-based communication)
3. **Direct Services** - For Server mode (direct DB access)

## What's Been Completed

### ✅ Created Shared DTOs in Client Project
- `Codefix.AIPlayGround.Client/Models/DTOs/AgentDtos.cs`
- `Codefix.AIPlayGround.Client/Models/DTOs/DashboardDtos.cs`

### ✅ Created Service Interfaces in Client Project
- `IAgentApiService` - Interface for agent operations
- `IDashboardApiService` - Interface for dashboard operations

### ✅ Created API Implementations (for WASM)
- `AgentApiService` - HTTP-based implementation in Client project
- `DashboardApiService` - HTTP-based implementation in Client project

### ✅ Created Direct Service Implementations (for Server)
- `DirectAgentService` - Direct DB access implementation
- `DirectDashboardService` - Direct DB access implementation

### ✅ Service Registration
- **Server Mode**: Uses `DirectAgentService` and `DirectDashboardService`
- **WASM Mode**: Uses `AgentApiService` and `DashboardApiService`

### ✅ Updated Render Modes
- All pages now use `InteractiveAutoRenderMode` for seamless WASM/Server switching

### ✅ Created API Controllers
- `DashboardController` - Provides dashboard statistics endpoints
- `AgentsController` - Already existed, provides agent CRUD endpoints
- `AgentFactoryController` - Agent factory pattern endpoints

## Remaining Work

### 1. Complete DTO Migration
Need to move these DTOs from Server to Client project:
- `FlowDtos.cs` - Workflow/Flow related DTOs  
- `NodeDtos.cs` - Node/workflow building block DTOs
- `ChatDtos.cs` - Already in Client, may need verification

### 2. Create Additional API Services
For complete coverage, add:
- `IWorkflowApiService` / `WorkflowApiService` - For workflow operations
- `DirectWorkflowService` - Direct implementation for server

### 3. Fix Remaining Type Conflicts
- Remove duplicate DTOs from server after migration
- Ensure all services reference Client DTOs

## Architecture Benefits

### Clean Separation of Concerns
```
Codefix.AIPlayGround.Client (Shared Library)
├── Models/DTOs          → Shared data contracts
├── Services            → Interface definitions & API implementations
│   ├── IAgentApiService
│   ├── AgentApiService (HTTP-based)
│   ├── IDashboardApiService
│   └── DashboardApiService (HTTP-based)

Codefix.AIPlayGround (Server)
├── Controllers         → HTTP API endpoints
├── Services           → Direct implementations
│   ├── DirectAgentService (DB-based)
│   └── DirectDashboardService (DB-based)
└── Data              → Database context & entities
```

### Mode-Specific Implementations
- **WASM Mode**: Components → API Services → HTTP → Controllers → DB
- **Server Mode**: Components → Direct Services → DB

### Benefits
1. **No Code Duplication** - DTOs defined once in shared library
2. **Type Safety** - Same DTOs used across client/server
3. **Flexible Deployment** - Supports both rendering modes seamlessly
4. **Clean Testing** - Easy to mock interfaces for unit tests
5. **Performance** - Server mode skips HTTP serialization overhead

## Implementation Pattern

### For WASM (Browser):
```csharp
// Client calls HTTP endpoint
var agents = await _httpClient.GetFromJsonAsync<List<AgentDto>>("/api/agents");
```

### For Server (SSR):
```csharp
// Direct DB access, no HTTP
var agents = await _context.Agents.Select(a => new AgentDto { ... }).ToListAsync();
```

### Unified Interface:
```csharp
// Components use the same interface regardless of mode
@inject IAgentApiService AgentService

var agents = await AgentService.GetAgentsAsync(filter);
```

## Next Steps

1. **Complete DTO migration** to Client project
2. **Add workflow/flow API services** following the same pattern
3. **Test both render modes** thoroughly
4. **Performance optimization** - Consider caching strategies
5. **Error handling** - Implement consistent error handling across layers

## Conventional Commits
When committing these changes:
```
feat: implement shared library architecture for WASM/Server support

- Move DTOs to shared Client library
- Create API services for WASM mode
- Create Direct services for Server mode  
- Update render modes to InteractiveAuto
- Add Dashboard API controller

BREAKING CHANGE: Service registration pattern changed
```

## Testing Checklist
- [ ] Dashboard loads data in Server mode
- [ ] Dashboard loads data in WASM mode  
- [ ] Agents page works in Server mode
- [ ] Agents page works in WASM mode
- [ ] Navigation between modes works seamlessly
- [ ] No type conflicts in build
- [ ] API endpoints return correct data
- [ ] Direct services query DB correctly

