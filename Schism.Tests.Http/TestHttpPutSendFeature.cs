using NSubstitute;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Http;
using Schism.Lib.Http.SendRequests;
using System.Text.Json;

namespace Schism.Tests.Http;
internal class TestHttpPutSendFeature
{
    [Test]
    public async Task PutRequestWithJsonBody()
    {
        MockResponse responseData = new(12, 8);
        ISchismHttpClient client = Substitute.For<ISchismHttpClient>();
        IHttpClientProvider clientProvider = Substitute.For<IHttpClientProvider>();
        clientProvider.GetHttpClient(Arg.Any<SchismRequest>()).Returns(client);
        client.PutAsync(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseData))
            }));
        HttpPutSendFeature feature = new(clientProvider);
        ConnectionPoint connection = new()
        {
            Name = "name",
            Path = "path",
            Type = "HTTP_PUT",
        };
        SchismRequest request = new SchismRequest("clientid", [connection], "baseuri", new DefaultJsonSerializer())
            .WithBody(responseData);

        SchismResponse result = await feature.SendAsync(request, connection);
        Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        //JsonBody is appended to query string list
        _ = client.Received(1).PutAsync(Arg.Is<string>(i => i == "baseuri/path?"), responseData);

        MockResponse? response = await result.ContentAsJsonAsync<MockResponse>();
        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.A, Is.EqualTo(12));
            Assert.That(response.B, Is.EqualTo(8));
        });
    }
    [Test]
    public async Task PutRequestWithQueryParamsJsonBodyAndString()
    {
        MockResponse responseData = new(12, 8);
        ISchismHttpClient client = Substitute.For<ISchismHttpClient>();
        IHttpClientProvider clientProvider = Substitute.For<IHttpClientProvider>();
        clientProvider.GetHttpClient(Arg.Any<SchismRequest>()).Returns(client);
        client.PutAsync(Arg.Any<string>(), Arg.Any<object>())
            .Returns(Task.FromResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseData))
            }));
        HttpPutSendFeature feature = new(clientProvider);
        ConnectionPoint connection = new()
        {
            Name = "name",
            Path = "path",
            Type = "HTTP_PUT",
        };
        SchismRequest request = new SchismRequest("clientid", [connection], "baseuri", new DefaultJsonSerializer())
            .WithBody(responseData)
            .WithQueryParam("C", 4)
            .WithQueryParam("D", 2);

        SchismResponse result = await feature.SendAsync(request, connection);
        Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        //JsonBody is appended to query string list
        _ = client.Received(1).PutAsync(Arg.Is<string>(i => i == "baseuri/path?C=4&D=2"), responseData);

        MockResponse? response = await result.ContentAsJsonAsync<MockResponse>();
        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.A, Is.EqualTo(12));
            Assert.That(response.B, Is.EqualTo(8));
        });
    }
    private record MockResponse(int A, int B);
}