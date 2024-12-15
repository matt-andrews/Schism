using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc.Server;
using Schism.Hub.Abstractions.Contracts;
using Schism.Hub.Data;
using Schism.Hub.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IRefreshService, RefreshService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddDbContext<Context>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddCodeFirstGrpc(config => config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal);

WebApplication app = builder.Build();

app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<RefreshService>();
app.MapGrpcService<RegisterService>();

app.Run();