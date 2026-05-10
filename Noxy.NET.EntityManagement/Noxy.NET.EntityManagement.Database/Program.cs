using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPersistence(x => x.UseSqlite(@"Data Source=..\..\..\..\..\Data\Noxy.NET.EntityManagement.sqlite").EnableSensitiveDataLogging());

using IHost app = builder.Build();

await app.UsePersistence();

// IDbContextFactory<DataContext> factory = app.Services.GetRequiredService<IDbContextFactory<DataContext>>();
// await using DataContext context = await factory.CreateDbContextAsync();
//
// TableSchema? schema = await context.Schema.FirstOrDefaultAsync(x => x.ID == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
// if (schema == null)
// {
//     schema = new TableSchema()
//     {
//         ID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
//         Name = "Restaurant",
//         Note = "This is a restaurant schema.",
//         IsActive = false,
//         TimeCreated = DateTime.UtcNow,
//         TimeActivated = null,
//     };
//     context.Schema.Add(schema);
// }
//
// var seed = new SchemaSeed(context, schema);
// seed.RegisterContext(SchemaSeed.ContextMenu);
//
// await context.SaveChangesAsync();
