using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schism.Lib.Core;
using Schism.Lib.ServiceBus;

namespace Schism.Tests.ServiceBus;
internal class TestDI
{
    private static Dictionary<string, string?> Config => new()
            {
                { "Schism:HubUri", "http://localhost:30100" },
                { "Schism:ClientId", "testid" },
                { "Schism:Host", "localhost" }
            };
    [Test]
    public void VerifyNoDuplicateServices()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(Config)
            .Build();
        IEnumerable<IGrouping<Type, ServiceDescriptor>> collection = new ServiceCollection()
            .AddSchism(typeof(TestDI).Assembly, config)
            .WithServiceBusClient()
            .WithServiceBusHost("defaultsbConnection")
            .Build()
            .Where(w => w.ServiceType.Namespace?.StartsWith("Schism.Lib.ServiceBus") ?? false)
            .GroupBy(g => g.ServiceType)
            ;

        Assert.That(collection.Any(a => a.Count() > 1), Is.False);
    }
}