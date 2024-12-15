using System.Net.Http.Json;

namespace Schism.Lib.Http;
public interface ISchismHttpClient
{
    Task<HttpResponseMessage> DeleteAsync(string? requestUri);
    Task<HttpResponseMessage> GetAsync(string? requestUri);
    Task<HttpResponseMessage> PostAsync(string? requestUri, object? value);
    Task<HttpResponseMessage> PutAsync(string? requestUri, object? value);
    Task<HttpResponseMessage> PatchAsync(string? requestUri, object? value);
    Task<HttpResponseMessage> OptionsAsync(string? requestUri);
    Task<HttpResponseMessage> HeadAsync(string? requestUri);
}
internal class SchismHttpClient(HttpClient httpClient) : ISchismHttpClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<HttpResponseMessage> DeleteAsync(string? requestUri)
    {
        return await _httpClient.DeleteAsync(requestUri);
    }
    public async Task<HttpResponseMessage> GetAsync(string? requestUri)
    {
        return await _httpClient.GetAsync(requestUri);
    }
    public async Task<HttpResponseMessage> PostAsync(string? requestUri, object? value)
    {
        return await _httpClient.PostAsJsonAsync(requestUri, value);
    }
    public async Task<HttpResponseMessage> PutAsync(string? requestUri, object? value)
    {
        return await _httpClient.PutAsJsonAsync(requestUri, value);
    }
    public async Task<HttpResponseMessage> PatchAsync(string? requestUri, object? value)
    {
        return await _httpClient.PatchAsJsonAsync(requestUri, value);
    }
    public async Task<HttpResponseMessage> OptionsAsync(string? requestUri)
    {
        HttpRequestMessage req = new(HttpMethod.Options, requestUri);
        return await _httpClient.SendAsync(req);
    }
    public async Task<HttpResponseMessage> HeadAsync(string? requestUri)
    {
        HttpRequestMessage req = new(HttpMethod.Head, requestUri);
        return await _httpClient.SendAsync(req);
    }
}