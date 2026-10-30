using OnForkHub.Web.Services.Api;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The embed player reads video metadata from the API backend.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:9000";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<IVideoService, VideoService>();

await builder.Build().RunAsync();
