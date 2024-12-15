using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schism.Lib.Core;
using Schism.Lib.ServiceBus;
using Schism.Lib.ServiceBus.Host;
using System.Reflection;

namespace Schism.Tests.ServiceBus;

public class TestServiceBusExtensions
{
    private static Dictionary<string, string?> Config => new()
            {
                { "Schism:HubUri", "http://localhost:30100" },
                { "Schism:ClientId", "testid" },
                { "Schism:Host", "localhost" }
            };
    [Test]
    public void ServiceBusClientRegistration_PostBuildActions_ClientOnly()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(Config)
            .Build();
        ServiceProvider services = new ServiceCollection()
            .AddSchism(typeof(TestServiceBusExtensions).Assembly, config)
            .WithServiceBusClient()
            .Build()
            .BuildServiceProvider();
        ISchismServiceBusFactory? service = services.GetService<ISchismServiceBusFactory>();
        Assert.That(service, Is.Not.Null);
        SchismServiceBusFactory? clientProvider = (SchismServiceBusFactory)service;
        Assert.That(clientProvider.HostConnections, Is.Empty);
    }

    [SchismServiceBusTopic(Connection = "connect", Topic = "topic", Subscription = "subscription")]
    [Test]
    public void ServiceBusClientRegistration_PostBuildActions_ClientAndHost()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(Config)
            .Build();
        ServiceProvider services = new ServiceCollection()
            .AddSchism(typeof(TestServiceBusExtensions).Assembly, config)
            .WithServiceBusClient()
            .WithServiceBusHost(GetMethodsWithConnection())
            .Build()
            .BuildServiceProvider();
        ISchismServiceBusFactory? service = services.GetService<ISchismServiceBusFactory>();
        Assert.That(service, Is.Not.Null);
        SchismServiceBusFactory? clientProvider = (SchismServiceBusFactory)service;
        Assert.That(clientProvider.HostConnections, Is.Not.Empty);
    }

    [Test]
    public void ServiceBusClientRegistration_PostBuildActions_HostOnly()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(Config)
            .Build();
        ServiceProvider services = new ServiceCollection()
            .AddSchism(typeof(TestServiceBusExtensions).Assembly, config)
            .WithServiceBusHost(GetMethodsWithConnection())
            .Build()
            .BuildServiceProvider();
        ISchismServiceBusFactory? service = services.GetService<ISchismServiceBusFactory>();
        Assert.That(service, Is.Not.Null);
        SchismServiceBusFactory? clientProvider = (SchismServiceBusFactory)service;
        Assert.That(clientProvider.HostConnections, Is.Not.Empty);
    }

    [SchismServiceBusTopic(Connection = "%testConnection%", Topic = "%testTopic%", Subscription = "%testSubscription%")]
    [Test]
    public void ServiceBusClientRegistration_TestEnvVarPatternMatching()
    {
        Dictionary<string, string?> configDict = Config;
        configDict.Add("testConnection", "i have passed");
        configDict.Add("testTopic", "i have passed");
        configDict.Add("testSubscription", "i have passed");
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
        ServiceProvider services = new ServiceCollection()
            .AddSchism(typeof(TestServiceBusExtensions).Assembly, config)
            .WithServiceBusClient()
            .WithServiceBusHost(GetMethodsWithConnection())
            .Build()
            .BuildServiceProvider();
        ISchismServiceBusFactory? service = services.GetService<ISchismServiceBusFactory>();
        Assert.That(service, Is.Not.Null);
        SchismServiceBusFactory? clientProvider = (SchismServiceBusFactory)service;
        Assert.That(clientProvider.HostConnections, Is.Not.Empty);
        ServiceBusHostConnection? connection = clientProvider.HostConnections.FirstOrDefault(f => f.ConnectionString == "i have passed");
        Assert.That(connection, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(connection.QueueOrTopic, Is.EqualTo("i have passed"));
            Assert.That(connection.Subscription, Is.EqualTo("i have passed"));
        });
    }

    [SchismServiceBusTopic(Connection = "%testConnection2%", Topic = "%testTopic%", Subscription = "%testSubscription")]
    [Test]
    public void ServiceBusClientRegistration_TestEnvVarPatternMatching_WrongPatterns()
    {
        Dictionary<string, string?> configDict = Config;
        configDict.Add("testConnection2", "i have passed");
        configDict.Add("testSubscription", "i have passed");
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
        ServiceProvider services = new ServiceCollection()
            .AddSchism(typeof(TestServiceBusExtensions).Assembly, config)
            .WithServiceBusClient()
            .WithServiceBusHost(GetMethodsWithConnection())
            .Build()
            .BuildServiceProvider();
        ISchismServiceBusFactory? service = services.GetService<ISchismServiceBusFactory>();
        Assert.That(service, Is.Not.Null);
        SchismServiceBusFactory? clientProvider = (SchismServiceBusFactory)service;
        Assert.That(clientProvider.HostConnections, Is.Not.Empty);
        ServiceBusHostConnection? connection = clientProvider.HostConnections.FirstOrDefault(f => f.ConnectionString == "i have passed");
        Assert.That(connection, Is.Not.Null);
        Assert.Multiple(() =>
        {
            //topic fails to translate since the config above doesn't specify an env variable for %testTopic%
            Assert.That(connection.QueueOrTopic, Is.EqualTo("%testTopic%"));
            //subscription fails because pattern requires % on both sides of the string
            Assert.That(connection.Subscription, Is.EqualTo("%testSubscription"));
        });
    }

    [SchismServiceBusTopic(Topic = "%testTopic%", Subscription = "%testSubscription%")]
    [Test]
    public void ServiceBusClientRegistration_TestEnvVarPatternMatching_Default()
    {
        Dictionary<string, string?> configDict = Config;
        configDict.Add("testConnection", "i have passed");
        configDict.Add("testTopic", "i have passed");
        configDict.Add("testSubscription", "i have passed");
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
        ServiceProvider services = new ServiceCollection()
            .AddSchism(typeof(TestServiceBusExtensions).Assembly, config)
            .WithServiceBusClient()
            .WithServiceBusHost("testConnection")
            .Build()
            .BuildServiceProvider();
        ISchismServiceBusFactory? service = services.GetService<ISchismServiceBusFactory>();
        Assert.That(service, Is.Not.Null);
        SchismServiceBusFactory? clientProvider = (SchismServiceBusFactory)service;
        Assert.That(clientProvider.HostConnections, Is.Not.Empty);
        ServiceBusHostConnection? connection = clientProvider.HostConnections.FirstOrDefault(f => f.ConnectionString == "i have passed");
        Assert.That(connection, Is.Not.Null);
        Assert.Multiple(() =>
        {
            //topic fails to translate since the config above doesn't specify an env variable for %testTopic%
            Assert.That(connection.QueueOrTopic, Is.EqualTo("i have passed"));
            //subscription fails because pattern requires % on both sides of the string
            Assert.That(connection.Subscription, Is.EqualTo("i have passed"));
        });
    }
    [SchismServiceBusTopic(Topic = "%testTopic%", Subscription = "%testSubscription%")]
    [Test]
    public void ServiceBusClientRegistration_TestEnvVarPatternMatching_NoDefaultNoAttr()
    {
        Dictionary<string, string?> configDict = Config;
        configDict.Add("testConnection", "i have passed");
        configDict.Add("testTopic", "i have passed");
        configDict.Add("testSubscription", "i have passed");
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
        Assert.Throws<ArgumentException>(() =>
        {
            ServiceProvider services = new ServiceCollection()
                .AddSchism(typeof(TestServiceBusExtensions).Assembly, config)
                .WithServiceBusClient()
                .WithServiceBusHost()
                .Build()
                .BuildServiceProvider();
        });
    }
    private MethodInfo[] GetMethodsWithConnection()
    {
        MethodInfo[] actions = GetType().Assembly.GetTypes()
            .SelectMany(s => s.GetMethods())
            .Where(w => w.GetCustomAttributes(typeof(SchismServiceBusTopicAttribute), false).Length > 0)
            .Where(w =>
                !string.IsNullOrWhiteSpace((w.GetCustomAttributes(typeof(SchismServiceBusTopicAttribute), false)
                    .First() as SchismServiceBusTopicAttribute)?.Connection))
            .ToArray();
        return actions;
    }
}