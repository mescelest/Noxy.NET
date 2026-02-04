using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Noxy.NET.EntityManagement.Administration.Application;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddPresentation(builder.Configuration["Backend:URL"] ?? throw new KeyNotFoundException("Backend:URL"));

WebAssemblyHost app = builder.Build();

await app.RunAsync();
