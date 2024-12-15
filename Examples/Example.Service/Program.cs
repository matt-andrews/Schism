using Example.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using Schism.Lib.Core;
using Schism.Lib.Http;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSchism(typeof(Program).Assembly, builder.Configuration)
    .WithHttpClient()
    .WithClientBuilder("Example.Service.Math", httpBuilder => httpBuilder.ConfigureHttpClient(httpClient => httpClient.DefaultRequestHeaders.Add("x-test-key", "i-am-a-secret-key")))
    .Build();

WebApplication app = builder.Build();
app.UseSchism();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/add", async ([FromQuery] string a, [FromQuery] string b, [FromServices] ISchismClientFactory factory) =>
{
    IMathContract client = factory.GetClientFor<IMathContract>();
    AdditionResponse result = await client.Addition(new AdditionRequest(a, b));
    return result.Data;
})
    .WithOpenApi();

app.MapGet("/subtract", async ([FromQuery] string a, [FromQuery] string b, [FromServices] ISchismClientFactory factory) =>
{
    IMathContract client = factory.GetClientFor<IMathContract>();
    SubtractionResponse result = await client.Subtraction(new SubtractionRequest(a, b));
    return result.Data;
})
    .WithOpenApi();

app.MapGet("/divide", async ([FromQuery] int a, [FromQuery] int b, [FromServices] ISchismClientFactory factory) =>
{
    IMathContract client = factory.GetClientFor<IMathContract>();
    DivisionResponse result = await client.Division(new DivisionRequest(a, b));
    return result.Data;
})
    .WithOpenApi();

app.MapGet("/multiply", async ([FromQuery] int a, [FromQuery] int b, [FromServices] ISchismClientFactory factory) =>
{
    IMathContract client = factory.GetClientFor<IMathContract>();
    MultiplicationResponse result = await client.Multiplication(new MultiplicationRequest(a, b));
    return result.Data;
})
    .WithOpenApi();

app.MapGet("/get-store", async ([FromServices] ISchismClientFactory factory) =>
{
    IDataCollectionContract client = factory.GetClientFor<IDataCollectionContract>();
    GetDataResponse result = await client.GetData();
    return result;
})
    .WithOpenApi();

app.MapDelete("/reset-store", async ([FromServices] ISchismClientFactory factory) =>
{
    IDataCollectionContract client = factory.GetClientFor<IDataCollectionContract>();
    await client.ResetStore();
    return Results.NoContent();
})
    .WithOpenApi();

app.Run();