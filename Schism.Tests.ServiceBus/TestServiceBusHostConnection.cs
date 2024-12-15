using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Providers;
using Schism.Lib.ServiceBus.Host;
using System.Reflection;
using System.Text.Json;

namespace Schism.Tests.ServiceBus;
internal class TestServiceBusHostConnection
{
    [Test]
    public async Task VerifyMiddlewarePipelineTriggered()
    {
        IMiddlewareDelegationProvider middleware = Substitute.For<IMiddlewareDelegationProvider>();
        ServiceProvider provider = new ServiceCollection()
            .AddScoped(_ => middleware)
            .AddScoped(_ => this)
            .AddSingleton(provider => new SerializationProvider.SerializationCollection().Build(provider, new DefaultJsonSerializer()))
            .BuildServiceProvider();
        ServiceBusHostConnection conn = new()
        {
            ClientId = "clientId",
            Action = typeof(TestServiceBusHostConnection).GetMethod(nameof(TestMethod), BindingFlags.Instance | BindingFlags.NonPublic)!,
            ConnectionString = "connection-string",
            Subscription = "subscription",
            QueueOrTopic = "topic",
            ServiceProvider = provider
        };
        object? result = await conn.ProcessMessage(string.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That((bool)result, Is.True);
        });
        _ = middleware.Received(1).Invoke();
    }
    [Test]
    public async Task VerifyMethodWithParameterIsTriggered()
    {
        IMiddlewareDelegationProvider middleware = Substitute.For<IMiddlewareDelegationProvider>();
        ServiceProvider provider = new ServiceCollection()
            .AddScoped(_ => middleware)
            .AddScoped(_ => this)
            .AddSingleton(provider => new SerializationProvider.SerializationCollection().Build(provider, new DefaultJsonSerializer()))
            .BuildServiceProvider();
        ServiceBusHostConnection conn = new()
        {
            ClientId = "clientId",
            Action = typeof(TestServiceBusHostConnection).GetMethod(nameof(TestMethodWithPackage), BindingFlags.Instance | BindingFlags.NonPublic)!,
            ConnectionString = "connection-string",
            Subscription = "subscription",
            QueueOrTopic = "topic",
            ServiceProvider = provider
        };
        TestMethodPackage content = new();
        object? result = await conn.ProcessMessage(JsonSerializer.Serialize(content));
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That((bool)result, Is.True);
        });
    }

#pragma warning disable CA1822 // Mark members as static // we want instance types for these tests
    private bool TestMethod()
    {
        return true;
    }

    private bool TestMethodWithPackage(TestMethodPackage package)
    {
        package.HasTriggered = true;
        return true;
    }
#pragma warning restore CA1822 // Mark members as static
    private class TestMethodPackage
    {
        public bool HasTriggered { get; set; }
    }
}