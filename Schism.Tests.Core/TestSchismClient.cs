using NSubstitute;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Internal;
using Schism.Lib.Core.Providers;

namespace Schism.Tests.Core;

public class TestSchismClient
{
    [Test]
    public async Task TestSendFeatureFallback()
    {
        ISendFeature onlyFeature = Substitute.For<ISendFeature>();
        onlyFeature.Key.Returns("HTTP_POST");
        onlyFeature.SendAsync(Arg.Any<SchismRequest>(), Arg.Any<ConnectionPoint>()).Returns(Task.FromResult(new SchismResponse(new DefaultJsonSerializer())
        {
            StatusCode = System.Net.HttpStatusCode.OK
        }));
        List<ISendFeature> features = [onlyFeature];
        ISendFeatureProvider feature = Substitute.For<ISendFeatureProvider>();
        feature.GetEnumerator().Returns(_ => features.GetEnumerator());

        Connection connection = new()
        {
            InstanceId = Guid.NewGuid(),
            BaseUri = "baseuri",
            ClientId = "clientid",
            Version = "0.0.0",
            Namespace = "namespace",
            ConnectionPoints = [
                new ConnectionPoint(){
                    Name = "point",
                    Path = "path1",
                    Type = "HTTP_POST",
                    Priority = 1
                },
                new ConnectionPoint(){
                    Name = "point",
                    Path = "path2",
                    Type = "SERVICE_BUS",
                    Priority = 0
                }
                ]
        };
        ISerializationProvider serializationProvider = Substitute.For<ISerializationProvider>();
        serializationProvider.GetSerializer().Returns(new DefaultJsonSerializer());
        serializationProvider.GetSerializer(Arg.Any<ISchismClient>()).Returns(new DefaultJsonSerializer());
        IInterfaceEmitter interfaceEmitter = Substitute.For<IInterfaceEmitter>();
        SchismClient client = new(feature, connection, serializationProvider, interfaceEmitter, TestHelpers.MockLogger<SchismClient>());
        SchismRequest req = client.CreateRequest("point");
        await client.SendRequestAsync(req);

        _ = onlyFeature.Received(1).SendAsync(Arg.Any<SchismRequest>(), Arg.Any<ConnectionPoint>());
    }
    [Test]
    public void TestSendFeatureFallback_NoOptions()
    {
        List<ISendFeature> features = [];
        ISendFeatureProvider feature = Substitute.For<ISendFeatureProvider>();
        feature.GetEnumerator().Returns(_ => features.GetEnumerator());

        Connection connection = new()
        {
            InstanceId = Guid.NewGuid(),
            BaseUri = "baseuri",
            ClientId = "clientid",
            Version = "0.0.0",
            Namespace = "namespace",
            ConnectionPoints = [
                new ConnectionPoint(){
                    Name = "point",
                    Path = "path1",
                    Type = "HTTP_POST",
                    Priority = 1
                },
                new ConnectionPoint(){
                    Name = "point",
                    Path = "path2",
                    Type = "SERVICE_BUS",
                    Priority = 0
                }
                ]
        };
        ISerializationProvider serializationProvider = Substitute.For<ISerializationProvider>();
        serializationProvider.GetSerializer().Returns(new DefaultJsonSerializer());
        serializationProvider.GetSerializer(Arg.Any<ISchismClient>()).Returns(new DefaultJsonSerializer());
        IInterfaceEmitter interfaceEmitter = Substitute.For<IInterfaceEmitter>();
        SchismClient client = new(feature, connection, serializationProvider, interfaceEmitter, TestHelpers.MockLogger<SchismClient>());
        SchismRequest req = client.CreateRequest("point");
        Assert.ThrowsAsync<Exception>(async () => await client.SendRequestAsync(req));
    }
}