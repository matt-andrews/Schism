using NSubstitute;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Http;
using Schism.Lib.Http.SendRequests;

namespace Schism.Tests.Http;
internal class TestHttpGetSendFeature
{
    [Test]
    public async Task GetRequestWithQueryParamsString()
    {
        ISchismHttpClient client = Substitute.For<ISchismHttpClient>();
        IHttpClientProvider clientProvider = Substitute.For<IHttpClientProvider>();
        clientProvider.GetHttpClient(Arg.Any<SchismRequest>()).Returns(client);
        client.GetAsync(Arg.Any<string>()).Returns(Task.FromResult(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK }));
        HttpGetSendFeature feature = new(clientProvider);
        ConnectionPoint connection = new()
        {
            Name = "name",
            Path = "path",
            Type = "HTTP_GET",
        };
        SchismRequest request = new SchismRequest("clientid", [connection], "baseuri", new DefaultJsonSerializer())
            .WithQueryParam("A", 12)
            .WithQueryParam("B", 8);

        SchismResponse result = await feature.SendAsync(request, connection);
        Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        _ = client.Received(1).GetAsync(Arg.Is<string>(i => i == "baseuri/path?A=12&B=8"));
    }
    [Test]
    public async Task GetRequestWithQueryParamsJsonBody()
    {
        ISchismHttpClient client = Substitute.For<ISchismHttpClient>();
        IHttpClientProvider clientProvider = Substitute.For<IHttpClientProvider>();
        clientProvider.GetHttpClient(Arg.Any<SchismRequest>()).Returns(client);
        client.GetAsync(Arg.Any<string>()).Returns(Task.FromResult(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK }));
        HttpGetSendFeature feature = new(clientProvider);
        ConnectionPoint connection = new()
        {
            Name = "name",
            Path = "path",
            Type = "HTTP_GET",
        };
        SchismRequest request = new SchismRequest("clientid", [connection], "baseuri", new DefaultJsonSerializer())
            .WithBody(new
            {
                A = 12,
                B = 8
            });

        SchismResponse result = await feature.SendAsync(request, connection);
        Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        _ = client.Received(1).GetAsync(Arg.Is<string>(i => i == "baseuri/path?A=12&B=8"));
    }
    [Test]
    public async Task GetRequestWithQueryParamsJsonBodyAndString()
    {
        ISchismHttpClient client = Substitute.For<ISchismHttpClient>();
        IHttpClientProvider clientProvider = Substitute.For<IHttpClientProvider>();
        clientProvider.GetHttpClient(Arg.Any<SchismRequest>()).Returns(client);
        client.GetAsync(Arg.Any<string>()).Returns(Task.FromResult(new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.OK }));
        HttpGetSendFeature feature = new(clientProvider);
        ConnectionPoint connection = new()
        {
            Name = "name",
            Path = "path",
            Type = "HTTP_GET",
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
        _ = client.Received(1).GetAsync(Arg.Is<string>(i => i == "baseuri/path?C=4&D=2&A=12&B=8"));
    }
}