using NSubstitute;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Internal;
using Schism.Lib.Core.Providers;
using System.Text;
using System.Text.Json;

namespace Schism.Tests.Core;
internal class TestSchismClientEmitterDelegate
{
    [Test]
    public async Task TestWithObjReturn()
    {
        ISendFeature sendFeature = Substitute.For<ISendFeature>();
        sendFeature.Key.Returns("HTTP_POST");
        ISendFeatureProvider featureProvider = Substitute.For<ISendFeatureProvider>();
        featureProvider.GetEnumerator().Returns(_ => new List<ISendFeature>() { sendFeature }.GetEnumerator());
        Connection connection = new()
        {
            InstanceId = Guid.NewGuid(),
            BaseUri = "baseuri",
            ClientId = "clientid",
            ConnectionPoints = [
                new ConnectionPoint(){
                    Name = "IMockInterface.Test1",
                    Path = "path",
                    Type = "HTTP_POST"
                }
                ],
            Namespace = "namespace",
            Version = "0.0.0"
        };
        ISchismSerializer jsonSerializer = Substitute.For<ISchismSerializer>();
        jsonSerializer.Deserialize(Arg.Any<string>(), Arg.Any<Type>())
            .Returns(d => JsonSerializer.Deserialize(d.ArgAt<string>(0), d.ArgAt<Type>(1)));

        ISerializationProvider serializationProvider = Substitute.For<ISerializationProvider>();
        serializationProvider.GetSerializer().Returns(jsonSerializer);
        serializationProvider.GetSerializer(Arg.Any<ISchismClient>()).Returns(jsonSerializer);

        InterfaceEmitter interfaceEmitter = new();
        Microsoft.Extensions.Logging.ILogger<SchismClient> logger = TestHelpers.MockLogger<SchismClient>();

        sendFeature.SendAsync(Arg.Any<SchismRequest>(), Arg.Any<ConnectionPoint>())
            .Returns(new SchismResponse(jsonSerializer)
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = JsonSerializer.SerializeToUtf8Bytes(new MockResponse("20"))
            });

        SchismClient client = new(featureProvider, connection, serializationProvider, interfaceEmitter, logger);
        IMockInterface handler = client.CreateFor<IMockInterface>();
        MockResponse result = await handler.Test1(new MockRequest(12, 8));
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data, Is.EqualTo("20"));
    }
    [Test]
    public async Task TestWithValueReturn()
    {
        ISendFeature sendFeature = Substitute.For<ISendFeature>();
        sendFeature.Key.Returns("HTTP_POST");
        ISendFeatureProvider featureProvider = Substitute.For<ISendFeatureProvider>();
        featureProvider.GetEnumerator().Returns(_ => new List<ISendFeature>() { sendFeature }.GetEnumerator());
        Connection connection = new()
        {
            InstanceId = Guid.NewGuid(),
            BaseUri = "baseuri",
            ClientId = "clientid",
            ConnectionPoints = [
                new ConnectionPoint(){
                    Name = "IMockInterface.Test2",
                    Path = "path",
                    Type = "HTTP_POST"
                }
                ],
            Namespace = "namespace",
            Version = "0.0.0"
        };
        ISchismSerializer jsonSerializer = Substitute.For<ISchismSerializer>();
        jsonSerializer.Deserialize(Arg.Any<string>(), Arg.Any<Type>())
            .Returns(d => JsonSerializer.Deserialize(d.ArgAt<string>(0), d.ArgAt<Type>(1)));

        ISerializationProvider serializationProvider = Substitute.For<ISerializationProvider>();
        serializationProvider.GetSerializer().Returns(jsonSerializer);
        serializationProvider.GetSerializer(Arg.Any<ISchismClient>()).Returns(jsonSerializer);

        InterfaceEmitter interfaceEmitter = new();
        Microsoft.Extensions.Logging.ILogger<SchismClient> logger = TestHelpers.MockLogger<SchismClient>();

        sendFeature.SendAsync(Arg.Any<SchismRequest>(), Arg.Any<ConnectionPoint>())
            .Returns(new SchismResponse(jsonSerializer)
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = Encoding.UTF8.GetBytes("20")
            });

        SchismClient client = new(featureProvider, connection, serializationProvider, interfaceEmitter, logger);
        IMockInterface handler = client.CreateFor<IMockInterface>();
        string result = await handler.Test2(new MockRequest(12, 8));
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo("20"));
    }
    [Test]
    public async Task TestWithNoReturn()
    {
        ISendFeature sendFeature = Substitute.For<ISendFeature>();
        sendFeature.Key.Returns("HTTP_POST");
        ISendFeatureProvider featureProvider = Substitute.For<ISendFeatureProvider>();
        featureProvider.GetEnumerator().Returns(_ => new List<ISendFeature>() { sendFeature }.GetEnumerator());
        Connection connection = new()
        {
            InstanceId = Guid.NewGuid(),
            BaseUri = "baseuri",
            ClientId = "clientid",
            ConnectionPoints = [
                new ConnectionPoint(){
                    Name = "IMockInterface.Test3",
                    Path = "path",
                    Type = "HTTP_POST"
                }
                ],
            Namespace = "namespace",
            Version = "0.0.0"
        };
        ISchismSerializer jsonSerializer = Substitute.For<ISchismSerializer>();
        jsonSerializer.Deserialize(Arg.Any<string>(), Arg.Any<Type>())
            .Returns(d => JsonSerializer.Deserialize(d.ArgAt<string>(0), d.ArgAt<Type>(1)));

        ISerializationProvider serializationProvider = Substitute.For<ISerializationProvider>();
        serializationProvider.GetSerializer().Returns(jsonSerializer);
        serializationProvider.GetSerializer(Arg.Any<ISchismClient>()).Returns(jsonSerializer);

        InterfaceEmitter interfaceEmitter = new();
        Microsoft.Extensions.Logging.ILogger<SchismClient> logger = TestHelpers.MockLogger<SchismClient>();

        sendFeature.SendAsync(Arg.Any<SchismRequest>(), Arg.Any<ConnectionPoint>())
            .Returns(new SchismResponse(jsonSerializer)
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        SchismClient client = new(featureProvider, connection, serializationProvider, interfaceEmitter, logger);
        IMockInterface handler = client.CreateFor<IMockInterface>();
        await handler.Test3(new MockRequest(12, 8));
    }
    [Test]
    public async Task TestWithValueReturnNoParam()
    {
        ISendFeature sendFeature = Substitute.For<ISendFeature>();
        sendFeature.Key.Returns("HTTP_POST");
        ISendFeatureProvider featureProvider = Substitute.For<ISendFeatureProvider>();
        featureProvider.GetEnumerator().Returns(_ => new List<ISendFeature>() { sendFeature }.GetEnumerator());
        Connection connection = new()
        {
            InstanceId = Guid.NewGuid(),
            BaseUri = "baseuri",
            ClientId = "clientid",
            ConnectionPoints = [
                new ConnectionPoint(){
                    Name = "IMockInterface.Test4",
                    Path = "path",
                    Type = "HTTP_POST"
                }
                ],
            Namespace = "namespace",
            Version = "0.0.0"
        };
        ISchismSerializer jsonSerializer = Substitute.For<ISchismSerializer>();
        jsonSerializer.Deserialize(Arg.Any<string>(), Arg.Any<Type>())
            .Returns(d => JsonSerializer.Deserialize(d.ArgAt<string>(0), d.ArgAt<Type>(1)));

        ISerializationProvider serializationProvider = Substitute.For<ISerializationProvider>();
        serializationProvider.GetSerializer().Returns(jsonSerializer);
        serializationProvider.GetSerializer(Arg.Any<ISchismClient>()).Returns(jsonSerializer);

        InterfaceEmitter interfaceEmitter = new();
        Microsoft.Extensions.Logging.ILogger<SchismClient> logger = TestHelpers.MockLogger<SchismClient>();

        sendFeature.SendAsync(Arg.Any<SchismRequest>(), Arg.Any<ConnectionPoint>())
            .Returns(new SchismResponse(jsonSerializer)
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = Encoding.UTF8.GetBytes("20")
            });

        SchismClient client = new(featureProvider, connection, serializationProvider, interfaceEmitter, logger);
        IMockInterface handler = client.CreateFor<IMockInterface>();
        string result = await handler.Test4();
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo("20"));
    }
}
public interface IMockInterface : ISchismContract
{
    Task<MockResponse> Test1(MockRequest req);
    Task<string> Test2(MockRequest req);
    Task Test3(MockRequest req);
    Task<string> Test4();
}
public record MockRequest(int A, int B);
public record MockResponse(string Data);