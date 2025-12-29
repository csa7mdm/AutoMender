using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AutoMender.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Point to the Azure Functions backend (default local port)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:7071") });

await builder.Build().RunAsync();
