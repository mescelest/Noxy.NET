using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Noxy.NET.EntityManagement.Frontend.Application;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddPresentation(builder.Configuration["Backend:URL"] ?? throw new KeyNotFoundException("Backend:URL"));
builder.Services.AddBaseToPresentation();

WebAssemblyHost app = builder.Build();

app.UseBaseWithPresentation();

await app.RunAsync();
