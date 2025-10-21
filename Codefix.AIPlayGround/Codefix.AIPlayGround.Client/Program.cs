using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Configure HttpClient for API calls to the server
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Register API services for WASM mode
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IAgentApiService, Codefix.AIPlayGround.Services.AgentApiService>();
builder.Services.AddScoped<Codefix.AIPlayGround.Services.IDashboardApiService, Codefix.AIPlayGround.Services.DashboardApiService>();

await builder.Build().RunAsync();
