using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schism.Lib.Core;
using Schism.Lib.Http;
using System.Diagnostics.CodeAnalysis;

namespace Schism.Tests.Http;

public class TestHttpExtensions
{
    private static Dictionary<string, string?> Config => new()
            {
                { "Schism:HubUri", "http://localhost:30100" },
                { "Schism:ClientId", "testid" },
                { "Schism:Host", "localhost" }
            };
    [Test]
    public void VerifyRoutesConstructed_ConnectionPointCount()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(Config)
            .Build();
        ServiceCollection services = new();
        services.AddMvcCore()
            .AddApplicationPart(typeof(TestHttpExtensions).Assembly)
            .AddControllersAsServices();
        SchismBuilder builder = services
            .AddSchism(typeof(TestHttpExtensions).Assembly, config)
            .WithHubHttpClient()
            .WithHttpHost();
        Assert.That(builder.ConnectionPoints, Has.Count.EqualTo(4));
    }
    [Test]
    public void VerifyRoutesConstructed_WithControllerActionRoute()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(Config)
            .Build();
        ServiceCollection services = new();
        services.AddMvcCore()
            .AddApplicationPart(typeof(TestHttpExtensions).Assembly)
            .AddControllersAsServices();
        SchismBuilder builder = services
            .AddSchism(typeof(TestHttpExtensions).Assembly, config)
            .WithHubHttpClient()
            .WithHttpHost();
        Assert.That(builder.ConnectionPoints.FirstOrDefault(f => f.Name == "MockController.GetTest"), Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(builder.ConnectionPoints.FirstOrDefault(f => f.Name == "MockController.GetTest").Type, Is.EqualTo("HTTP_GET"));
            Assert.That(builder.ConnectionPoints.FirstOrDefault(f => f.Name == "MockController.PostTest"), Is.Not.Null);
        });
        Assert.That(builder.ConnectionPoints.FirstOrDefault(f => f.Name == "MockController.PostTest").Type, Is.EqualTo("HTTP_POST"));
    }
    [Test]
    public void VerifyRoutesConstructed_WithControllerRoute()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(Config)
            .Build();
        ServiceCollection services = new();
        services.AddMvcCore()
            .AddApplicationPart(typeof(TestHttpExtensions).Assembly)
            .AddControllersAsServices();
        SchismBuilder builder = services
            .AddSchism(typeof(TestHttpExtensions).Assembly, config)
            .WithHubHttpClient()
            .WithHttpHost();
        Assert.Multiple(() =>
        {
            Assert.That(builder.ConnectionPoints.FirstOrDefault(f => f.Name == "Mock2Controller.GetTest" && f.Type == "HTTP_GET"), Is.Not.Null);
            Assert.That(builder.ConnectionPoints.FirstOrDefault(f => f.Name == "Mock2Controller.PostTest" && f.Type == "HTTP_POST"), Is.Not.Null);
        });
    }
}

[ExcludeFromCodeCoverage]
[ApiController]
[Route("[controller]/[action]")]
public class MockController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetTest()
    {
        return Task.FromResult<IActionResult>(Ok());
    }
    [HttpPost]
    public Task<IActionResult> PostTest()
    {
        return Task.FromResult<IActionResult>(Ok());
    }
}
[ExcludeFromCodeCoverage]
[ApiController]
[Route("[controller]")]
public class Mock2Controller : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetTest()
    {
        return Task.FromResult<IActionResult>(Ok());
    }
    [HttpPost]
    public Task<IActionResult> PostTest()
    {
        return Task.FromResult<IActionResult>(Ok());
    }
}