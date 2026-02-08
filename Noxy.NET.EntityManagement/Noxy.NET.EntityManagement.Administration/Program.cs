using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Noxy.NET.EntityManagement.Administration.Application;
using Noxy.NET.EntityManagement.Administration.Features;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddPresentation(builder.Configuration["Backend:URL"] ?? throw new KeyNotFoundException("Backend:URL"));
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UseReduxDevTools(); // optional
});

WebAssemblyHost app = builder.Build();

var featureBase = typeof(Feature<>);

foreach (var type in typeof(FeatureSchemaParameterDataParameterListState).Assembly.GetTypes())
{
    if (type.BaseType != null &&
        type.BaseType.IsGenericType &&
        type.BaseType.GetGenericTypeDefinition() == typeof(Feature<>))
    {
        Console.WriteLine("FEATURE FOUND: " + type.FullName);
    }
}

await app.RunAsync();
