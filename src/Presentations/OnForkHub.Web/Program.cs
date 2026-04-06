using Microsoft.AspNetCore.Components.Authorization;
using OnForkHub.Web.Auth;
using OnForkHub.Web.Services;
using OnForkHub.Web.Services.Api;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// SECURITY: Configure HttpClient to point to the API backend
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:9000";

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// Register authentication services
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

builder.Services.AddComponentServices();
builder.Services.AddApiServices();

await builder.Build().RunAsync();
