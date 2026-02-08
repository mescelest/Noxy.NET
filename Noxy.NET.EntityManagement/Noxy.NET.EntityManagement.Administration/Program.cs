using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Noxy.NET.EntityManagement.Administration.Application;
using Noxy.NET.EntityManagement.Presentation;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.ScanAssemblies(typeof(PresentationAssemblyMarker).Assembly);
    options.UseReduxDevTools();
});
builder.Services.AddPresentation(builder.Configuration["Backend:URL"] ?? throw new KeyNotFoundException("Backend:URL"));

WebAssemblyHost app = builder.Build();

await app.RunAsync();
