using NSubstitute;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Http;
using Schism.Lib.Http.SendRequests;

namespace Schism.Tests.Http;
internal class TestHttpDeleteSendFeature
{
    [Test]
    public async Task DeleteRequestWithQueryParamsString()
    {
        ISchismHttpClient client = Substitute.For<ISchismHttpClient>();
        IHttpClientProvider clientProvider = Substitute.For<IHttpClientProvider>();
        clientProvider.GetHttpClient(Arg.Any<SchismRequest>()).Returns(client);
        client.DeleteAsync(Arg.Any<string>()).Returns(Task.FromResult(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK }));
        HttpDeleteSendFeature feature = new(clientProvider);
        ConnectionPoint connection = new()
        {
            Name = "name",
            Path = "path",
            Type = "HTTP_DELETE",
        };
        SchismRequest request = new SchismRequest("clientid", [connection], "baseuri", new DefaultJsonSerializer())
            .WithQueryParam("A", 12)
            .WithQueryParam("B", 8);

        SchismResponse result = await feature.SendAsync(request, connection);
        Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        _ = client.Received(1).DeleteAsync(Arg.Is<string>(i => i == "baseuri/path?A=12&B=8"));
    }
    [Test]
    public async Task DeleteRequestWithQueryParamsJsonBody()
    {
        ISchismHttpClient client = Substitute.For<ISchismHttpClient>();
        IHttpClientProvider clientProvider = Substitute.For<IHttpClientProvider>();
        clientProvider.GetHttpClient(Arg.Any<SchismRequest>()).Returns(client);
        client.DeleteAsync(Arg.Any<string>()).Returns(Task.FromResult(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK }));
        HttpDeleteSendFeature feature = new(clientProvider);
        ConnectionPoint connection = new()
        {
            Name = "name",
            Path = "path",
            Type = "HTTP_DELETE",
        };
        SchismRequest request = new SchismRequest("clientid", [connection], "baseuri", new DefaultJsonSerializer())
            .WithBody(new
            {
                A = 12,
                B = 8
            });

        SchismResponse result = await feature.SendAsync(request, connection);
        Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        _ = client.Received(1).DeleteAsync(Arg.Is<string>(i => i == "baseuri/path?A=12&B=8"));
    }
    [Test]
    public async Task DeleteRequestWithQueryParamsJsonBodyAndString()
    {
        ISchismHttpClient client = Substitute.For<ISchismHttpClient>();
        IHttpClientProvider clientProvider = Substitute.For<IHttpClientProvider>();
        clientProvider.GetHttpClient(Arg.Any<SchismRequest>()).Returns(client);
        client.DeleteAsync(Arg.Any<string>()).Returns(Task.FromResult(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK }));
        HttpDeleteSendFeature feature = new(clientProvider);
        ConnectionPoint connection = new()
        {
            Name = "name",
            Path = "path",
            Type = "HTTP_DELETE",
        };
        SchismRequest request = new SchismRequest("clientid", [connection], "baseuri", new DefaultJsonSerializer())
            .WithBody(new
            {
                A = 12,
                B = 8
            })
            .WithQueryParam("C", 4)
            .WithQueryParam("D", 2);

        SchismResponse result = await feature.SendAsync(request, connection);
        Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        //JsonBody is appended to query string list
        _ = client.Received(1).DeleteAsync(Arg.Is<string>(i => i == "baseuri/path?C=4&D=2&A=12&B=8"));
    }
}