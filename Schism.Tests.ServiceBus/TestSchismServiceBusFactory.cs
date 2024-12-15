using Microsoft.Extensions.DependencyInjection;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Providers;
using Schism.Lib.ServiceBus.Host;

namespace Schism.Tests.ServiceBus;
internal class TestSchismServiceBusFactory
{
    [Test]
    public void TestWithNewClient()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton(provider => new StringTranslationProvider.StringTranslationCollection().Build(provider))
            .AddSingleton(provider => new SerializationProvider.SerializationCollection().Build(provider, new DefaultJsonSerializer()))
            .BuildServiceProvider();
        SchismServiceBusFactory factory = new([], serviceProvider);

        //we use an emulator connection string since the ServiceBusClient() does some parsing on construction
        Task<Lib.ServiceBus.Client.ISchismServiceBusClient> client = factory.GetClient("clientId", "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;");
        Assert.That(client, Is.Not.Null);
    }

    [Test]
    public async Task TestWithOldClient()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton(provider => new StringTranslationProvider.StringTranslationCollection().Build(provider))
            .AddSingleton(provider => new SerializationProvider.SerializationCollection().Build(provider, new DefaultJsonSerializer()))
            .BuildServiceProvider();
        SchismServiceBusFactory factory = new([], serviceProvider);

        Lib.ServiceBus.Client.ISchismServiceBusClient client = await factory.GetClient("clientId", "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;");
        Assert.That(client, Is.Not.Null);
        Lib.ServiceBus.Client.ISchismServiceBusClient client2 = await factory.GetClient("clientId", "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;");
        Assert.That(client, Is.EqualTo(client2));
    }

    [Test]
    public async Task TestExecuteHost()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton(provider => new StringTranslationProvider.StringTranslationCollection().Build(provider))
            .AddSingleton(provider => new SerializationProvider.SerializationCollection().Build(provider, new DefaultJsonSerializer()))
            .BuildServiceProvider();
        SchismServiceBusFactory factory = new([
            new ServiceBusHostConnection(){
                ClientId = "clientId",
                Action = GetType().GetMethod(nameof(TestExecuteHost))!,
                ConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;",
                Subscription = "sub",
                QueueOrTopic = "topic"
            },
            new ServiceBusHostConnection(){
                ClientId = "clientId",
                Action = GetType().GetMethod(nameof(TestExecuteHost))!,
                ConnectionString = "Endpoint=sb://localhost2;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;",
                Subscription = "sub",
                QueueOrTopic = "topic"
            },
            ], serviceProvider);

        List<Lib.ServiceBus.Client.SchismServiceBusClient> result = await factory.StartAsync();

        Assert.Multiple(async () =>
        {
            Assert.That(result.First(), Is.EqualTo(await factory.GetClient("clientId", "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;")));
            Assert.That(result.Last(), Is.EqualTo(await factory.GetClient("clientId", "Endpoint=sb://localhost2;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;")));
        });
    }
}