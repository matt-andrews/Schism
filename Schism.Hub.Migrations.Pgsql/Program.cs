using Microsoft.EntityFrameworkCore;
using Schism.Hub.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Context>(o => o.UseNpgsql(
        builder.Configuration.GetConnectionString("Database"),
        b => b.MigrationsAssembly("Schism.Hub.Migrations.Pgsql")));

WebApplication app = builder.Build();

app.MapGet("/", () => "Hello World!");

#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
Context context = builder.Services.BuildServiceProvider().GetRequiredService<Context>();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
await context.Database.MigrateAsync();

//app.Run();