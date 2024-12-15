using NSubstitute;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.ServiceBus;
using Schism.Lib.ServiceBus.Client;
using Schism.Lib.ServiceBus.Host;

namespace Schism.Tests.ServiceBus;
public class TestServiceBusSendFeature
{
    [Test]
    public void ServiceBusSendFeature_RequireJsonBody()
    {
        ISchismServiceBusClient client = Substitute.For<ISchismServiceBusClient>();
        ISchismServiceBusFactory provider = Substitute.For<ISchismServiceBusFactory>();
        provider.GetClient(Arg.Any<string>(), Arg.Any<string>()).Returns(client);
        ServiceBusSendFeature feature = new(provider);

        ConnectionPoint connection = new()
        {
            Name = "clientid",
            Path = "path",
            Type = SbConsts.ServiceBusKey,
            Props = new() { { SbConsts.ServiceBusTopicOrQueueNameKey, "topic" } }
        };
        SchismRequest req = new("clientid", [connection], "uri", new DefaultJsonSerializer());
        Assert.ThrowsAsync<ArgumentException>(async () => await feature.SendAsync(req, connection));
    }
    [Test]
    public void ServiceBusSendFeature_RequireTopic()
    {
        ISchismServiceBusClient client = Substitute.For<ISchismServiceBusClient>();
        ISchismServiceBusFactory provider = Substitute.For<ISchismServiceBusFactory>();
        provider.GetClient(Arg.Any<string>(), Arg.Any<string>()).Returns(client);
        ServiceBusSendFeature feature = new(provider);

        ConnectionPoint connection = new()
        {
            Name = "clientid",
            Path = "path",
            Type = SbConsts.ServiceBusKey,
        };
        SchismRequest req = new SchismRequest("clientid", [connection], "uri", new DefaultJsonSerializer())
            .WithBody(new { Prop1 = "A", Prop2 = "B" });
        Assert.ThrowsAsync<ArgumentException>(async () => await feature.SendAsync(req, connection));
    }
    [Test]
    public async Task ServiceBusSendFeature_Happy()
    {
        ISchismServiceBusClient client = Substitute.For<ISchismServiceBusClient>();
        ISchismServiceBusFactory provider = Substitute.For<ISchismServiceBusFactory>();
        provider.GetClient(Arg.Any<string>(), Arg.Any<string>()).Returns(client);
        ServiceBusSendFeature feature = new(provider);

        ConnectionPoint connection = new()
        {
            Name = "clientid",
            Path = "path",
            Type = SbConsts.ServiceBusKey,
            Props = new() { { SbConsts.ServiceBusTopicOrQueueNameKey, "topic" } }
        };
        SchismRequest req = new SchismRequest("clientid", [connection], "uri", new DefaultJsonSerializer())
            .WithBody(new { Prop1 = "A", Prop2 = "B" });
        SchismResponse response = await feature.SendAsync(req, connection);
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        _ = client.Received(1).SendMessage("topic", Arg.Any<object>());
    }
}