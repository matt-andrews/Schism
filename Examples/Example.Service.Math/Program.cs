using Schism.Lib.Core;
using Schism.Lib.Http;
using Schism.Lib.ServiceBus;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSchism(typeof(Program).Assembly, builder.Configuration)
    .WithHttpClient()
    .WithHttpHost()
    .WithServiceBusClient()
    .Build();

WebApplication app = builder.Build();
app.UseSchism();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

await app.EarlyInitializeSchism();

app.Run();