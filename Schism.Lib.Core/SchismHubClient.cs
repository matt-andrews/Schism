using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using Schism.Hub.Abstractions.Contracts;
using Schism.Hub.Abstractions.Models;
using System.Net.Http.Json;

namespace Schism.Lib.Core;
public interface ISchismHubClient
{
    /// <summary>
    /// Makes a <see cref="RegistrationRequest"/> to the Hub
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    Task<RegistrationResponse> PostRegistrationRequest(RegistrationRequest req);

    /// <summary>
    /// Makes a request to the Hub getting the <see cref="RefreshResponse"/>
    /// </summary>
    /// <returns></returns>
    Task<RefreshResponse> PostRefreshClients();
}

internal class SchismHubHttpClient(HttpClient _httpClient) : ISchismHubClient
{
    public async Task<RegistrationResponse> PostRegistrationRequest(RegistrationRequest req)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("register", req);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<RegistrationResponse>()
                ?? throw new FormatException($"Failed to deserialize content into {nameof(RegistrationResponse)}")
            : throw new HttpRequestException($"Failed to get a successful status code from {nameof(PostRegistrationRequest)}: {response.StatusCode}");
    }

    public async Task<RefreshResponse> PostRefreshClients()
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("refresh", new RefreshRequest());
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<RefreshResponse>()
                ?? throw new FormatException($"Failed to deserialize content into {nameof(RefreshResponse)}")
            : throw new HttpRequestException($"Failed to get a successful status code from {nameof(PostRefreshClients)}: {response.StatusCode}");
    }
}

internal class SchismHubGrpcClient(SchismOptions options) : ISchismHubClient
{
    private readonly GrpcChannel _channel = GrpcChannel.ForAddress(options.HubUri);

    public async Task<RegistrationResponse> PostRegistrationRequest(RegistrationRequest req)
    {
        IRegisterService registerService = _channel.CreateGrpcService<IRegisterService>();
        RegistrationResponse response = await registerService.Register(req);
        return response;
    }

    public async Task<RefreshResponse> PostRefreshClients()
    {
        IRefreshService refreshService = _channel.CreateGrpcService<IRefreshService>();
        RefreshResponse response = await refreshService.Refresh(new RefreshRequest());
        return response;
    }
}