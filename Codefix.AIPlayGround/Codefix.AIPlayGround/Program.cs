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

// Add Controllers
builder.Services.AddControllers();

// Add OpenAPI and Scalar documentation
builder.Services.AddOpenApi();

// Add Agent Framework services
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IAgentVisualizationService, Codefix.AIPlayGround.Services.AgentVisualizationService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IEnhancedWorkflowService, Codefix.AIPlayGround.Services.EnhancedWorkflowService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IAgentFrameworkService, Codefix.AIPlayGround.Services.AgentFrameworkService>();

// Add Agent Factory service
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IAgentFactory, Codefix.AIPlayGround.Services.AgentFactory>();

// Add Workflow Seeding service
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IWorkflowSeedingService, Codefix.AIPlayGround.Services.WorkflowSeedingService>();

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
    
    // Map Scalar UI
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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Codefix.AIPlayGround.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Map API Controllers
app.MapControllers();

app.Run();
