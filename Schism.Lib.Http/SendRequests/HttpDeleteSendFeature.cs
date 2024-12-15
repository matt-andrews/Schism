using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using System.Reflection;

namespace Schism.Lib.Http.SendRequests;
internal class HttpDeleteSendFeature(IHttpClientProvider httpClientProvider) : ISendFeature
{
    public string Key { get; } = "HTTP_DELETE";

    public async Task<SchismResponse> SendAsync(SchismRequest request, ConnectionPoint point)
    {
        ISchismHttpClient httpClient = httpClientProvider.GetHttpClient(request);
        List<string> queryParams = request.GetPropAs<List<string>>(HttpConsts.ConstQueryParams) ?? [];
        object? jsonBody = request.GetProp(SchismRequest.ConstBody);
        if (jsonBody is not null)
        {
            queryParams.AddRange(GetJsonBodyAsQuery(jsonBody));
        }
        string query = string.Join("&", queryParams);
        HttpResponseMessage response = await httpClient.DeleteAsync($"{request.BaseUri.TrimEnd('/')}/{point.Path}?{query}");
        return new SchismResponse(request.Serializer)
        {
            StatusCode = response.StatusCode,
            Content = await response.Content.ReadAsByteArrayAsync()
        };
    }

    private static List<string> GetJsonBodyAsQuery(object json)
    {
        List<string> result = [];
        PropertyInfo[] props = json.GetType().GetProperties();
        foreach (PropertyInfo prop in props)
        {
            result.Add($"{prop.Name}={prop.GetValue(json)}");
        }
        return result;
    }
}