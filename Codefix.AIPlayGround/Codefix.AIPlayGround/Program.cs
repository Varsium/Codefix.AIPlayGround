using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Codefix.AIPlayGround.Components;
using Codefix.AIPlayGround.Components.Account;
using Codefix.AIPlayGround.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

// Add controllers for API endpoints
builder.Services.AddControllers();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add DbContextFactory for parallel operations in Direct Services
// Use Scoped lifetime to avoid singleton/scoped service lifetime conflicts
builder.Services.AddDbContextFactory<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Add OpenAPI and Scalar documentation
builder.Services.AddOpenApi();

// Add Microsoft Agent Framework visualization service (replaces custom AgentVisualizationService)
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IAgentVisualizationService, Codefix.AIPlayGround.Services.MicrosoftAgentFrameworkVisualizationService>();

// Keep legacy visualization service for comparison/fallback
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.IAgentVisualizationService, Codefix.AIPlayGround.Services.AgentVisualizationService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IEnhancedWorkflowService, Codefix.AIPlayGround.Services.EnhancedWorkflowService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IAgentFrameworkService, Codefix.AIPlayGround.Services.AgentFrameworkService>();

// Add Microsoft Agent Framework definition service
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IMicrosoftAgentFrameworkDefinitionService, Codefix.AIPlayGround.Services.MicrosoftAgentFrameworkDefinitionService>();

// Add Microsoft Agent Framework orchestration service
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IMicrosoftAgentFrameworkOrchestrationService, Codefix.AIPlayGround.Services.MicrosoftAgentFrameworkOrchestrationService>();

// Add MCP (Model Context Protocol) integration service
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IMCPIntegrationService, Codefix.AIPlayGround.Services.MCPIntegrationService>();

// Add unified AI provider service (replaces separate PeerLLM service)
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IUnifiedAIProviderService, Codefix.AIPlayGround.Services.UnifiedAIProviderService>();

// Add local model integration services
// Docker Model Runner - for Docker-based AI models
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IDockerModelRunnerService, Codefix.AIPlayGround.Services.DockerModelRunnerService>();

// Ollama - for local models (Llama2, Mistral, CodeLlama, OpenLLaMA, etc.)
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IOllamaIntegrationService, Codefix.AIPlayGround.Services.OllamaIntegrationService>();

// PeerLLM - for decentralized AI models
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IPeerLLMIntegrationService, Codefix.AIPlayGround.Services.PeerLLMIntegrationService>();

// HttpClient factory for making HTTP requests to local model providers
builder.Services.AddHttpClient();

// Add Workflow Seeding service
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IWorkflowSeedingService, Codefix.AIPlayGround.Services.WorkflowSeedingService>();

// Add Chat service (scoped to maintain agent instances per session)
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IChatService, Codefix.AIPlayGround.Services.ChatService>();

// Add Direct Services for Server-side rendering (direct DB/service access)
// These will be used when components are rendered on the server
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IAgentApiService, Codefix.AIPlayGround.Services.DirectAgentService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IDashboardApiService, Codefix.AIPlayGround.Services.DirectDashboardService>();

// Add code generation services
builder.Services.AddScoped<Codefix.AIPlayGround.Services.ICodeGenerationService, Codefix.AIPlayGround.Services.CodeGenerationService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.ICodeExecutionService, Codefix.AIPlayGround.Services.CodeExecutionService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.ICodeDetectionService, Codefix.AIPlayGround.Services.CodeDetectionService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IEnhancedCodeGenerationService, Codefix.AIPlayGround.Services.EnhancedCodeGenerationService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.SecuritySettings>();

// Add demo services for testing code detection
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IDemoBusinessService, Codefix.AIPlayGround.Services.DemoBusinessService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.SecuritySandboxService>();

// Add workflow execution services - Microsoft Agent Framework implementation
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IWorkflowExecutionService, Codefix.AIPlayGround.Services.MicrosoftAgentFrameworkWorkflowService>();

// Keep legacy workflow execution service for comparison/fallback
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.IWorkflowExecutionService, Codefix.AIPlayGround.Services.WorkflowExecutionService>();

// Add Microsoft Agent Framework node executor (replaces all custom node executors)
builder.Services.AddScoped<Codefix.AIPlayGround.Services.MicrosoftAgentFrameworkNodeExecutor>();

// Keep legacy node executors for comparison/fallback (commented out)
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.StartNodeExecutor>();
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.EndNodeExecutor>();
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.LLMAgentExecutor>();
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.ToolAgentExecutor>();
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.ConditionalAgentExecutor>();
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.ParallelAgentExecutor>();
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.CheckpointAgentExecutor>();
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.MCPAgentExecutor>();
// builder.Services.AddScoped<Codefix.AIPlayGround.Services.FunctionNodeExecutor>();

// Add session support for session management
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Seed workflows on startup
using (var scope = app.Services.CreateScope())
{
    var seedingService = scope.ServiceProvider.GetRequiredService<Codefix.AIPlayGround.Services.IWorkflowSeedingService>();
    await seedingService.SeedWorkflowsAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
    
    // Map OpenAPI endpoint
    app.MapOpenApi();
    
    // Map Scalar API documentation UI
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseSession();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
    // .AddInteractiveWebAssemblyRenderMode()
    // .AddAdditionalAssemblies(typeof(Codefix.AIPlayGround.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Map API controllers
app.MapControllers();

app.Run();
