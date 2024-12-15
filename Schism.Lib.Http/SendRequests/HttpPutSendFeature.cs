using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;

namespace Schism.Lib.Http.SendRequests;
internal class HttpPutSendFeature(IHttpClientProvider httpClientProvider) : ISendFeature
{
    public string Key { get; } = "HTTP_PUT";

    public async Task<SchismResponse> SendAsync(SchismRequest request, ConnectionPoint point)
    {
        ISchismHttpClient httpClient = httpClientProvider.GetHttpClient(request);
        List<string> queryParams = request.GetPropAs<List<string>>(HttpConsts.ConstQueryParams) ?? [];
        string query = string.Join("&", queryParams);
        object? jsonBody = request.GetProp(SchismRequest.ConstBody);
        HttpResponseMessage response = await httpClient.PutAsync($"{request.BaseUri.TrimEnd('/')}/{point.Path}?{query}", jsonBody);

        return new SchismResponse(request.Serializer)
        {
            StatusCode = response.StatusCode,
            Content = await response.Content.ReadAsByteArrayAsync()
        };
    }
}