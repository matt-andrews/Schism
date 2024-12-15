using NSubstitute;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Internal;
using Schism.Lib.Core.Providers;

namespace Schism.Tests.Core;
internal class TestSchismClientFactory
{
    [Test]
    public async Task TestClientInsertAndGet()
    {
        ISchismHubClient httpClient = Substitute.For<ISchismHubClient>();
        RefreshResponse postResponse = new()
        {
            Data = [new Connection() {
                    InstanceId = Guid.NewGuid(),
                    BaseUri = "uri",
                    ClientId = "client",
                    Namespace = "namespace",
                    ConnectionPoints = [],
                    Version = "0.0.0"
                }]
        };
        httpClient.PostRefreshClients().Returns(postResponse);
        ISendFeatureProvider featureProvider = Substitute.For<ISendFeatureProvider>();
        IInterfaceEmitter interfaceEmitter = Substitute.For<IInterfaceEmitter>();
        ISerializationProvider serializationProvider = Substitute.For<ISerializationProvider>();
        serializationProvider.GetSerializer().Returns(new DefaultJsonSerializer());
        serializationProvider.GetSerializer(Arg.Any<ISchismClient>()).Returns(new DefaultJsonSerializer());

        SchismClientFactory factory = new(httpClient, featureProvider, serializationProvider, interfaceEmitter, TestHelpers.MockLogger<SchismClient>(), TestHelpers.BuildOptions());
        await factory.RefreshClients();
        ISchismClient client = factory.GetClient("client");
        Assert.Multiple(() =>
        {
            Assert.That(client, Is.Not.Null);
            Assert.That(factory.IsExpired(), Is.False);
        });
    }
    [Test]
    public async Task TestClientInsert_FailedHubResponse()
    {
        ISchismHubClient httpClient = Substitute.For<ISchismHubClient>();
        httpClient.PostRefreshClients().Returns(Task.FromException<RefreshResponse>(new Exception("some error")));
        ISendFeatureProvider featureProvider = Substitute.For<ISendFeatureProvider>();
        IInterfaceEmitter interfaceEmitter = Substitute.For<IInterfaceEmitter>();
        ISerializationProvider serializationProvider = Substitute.For<ISerializationProvider>();
        serializationProvider.GetSerializer().Returns(new DefaultJsonSerializer());
        serializationProvider.GetSerializer(Arg.Any<ISchismClient>()).Returns(new DefaultJsonSerializer());

        SchismClientFactory factory = new(httpClient, featureProvider, serializationProvider, interfaceEmitter, TestHelpers.MockLogger<SchismClient>(), TestHelpers.BuildOptions());
        await factory.RefreshClients();

        Assert.Throws<KeyNotFoundException>(() =>
        {
            ISchismClient client = factory.GetClient("client");
        });

        Assert.That(factory.IsExpired(), Is.True);
    }
}