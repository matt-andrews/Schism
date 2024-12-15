using Microsoft.Extensions.Logging;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Internal;
using Schism.Lib.Core.Providers;
using System.Collections.Concurrent;
using System.Reflection;

namespace Schism.Lib.Core;
public interface ISchismClientFactory
{
    /// <summary>
    /// Get an <see cref="ISchismClient"/> from the client id
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Throws when a <paramref name="clientId"/> is not found</exception>
    ISchismClient GetClient(string clientId);

    /// <summary>
    /// Get the contract for the given <typeparamref name="TContract"/>
    /// </summary>
    /// <typeparam name="TContract"></typeparam>
    /// <returns></returns>
    TContract GetClientFor<TContract>()
        where TContract : ISchismContract;
}
internal class SchismClientFactory(
    ISchismHubClient httpClient,
    ISendFeatureProvider featureProvider,
    ISerializationProvider serializationProvider,
    IInterfaceEmitter interfaceEmitter,
    ILogger<SchismClient> logger,
    SchismOptions options) : ISchismClientFactory, IMiddlewareDelegationFeature
{
    private readonly IInterfaceEmitter _interfaceEmitter = interfaceEmitter;
    private readonly ILogger<SchismClient> _logger = logger;
    private readonly SchismOptions _options = options;
    private readonly ISchismHubClient _httpClient = httpClient;
    private readonly ConcurrentDictionary<string, SchismClient> _clients = [];
    private DateTimeOffset _nextRefresh;

    public ISchismClient GetClient(string clientId)
    {
        if (_clients.TryGetValue(clientId, out SchismClient? client))
        {
            return client;
        }
        _logger.LogError("{Client} has not been registered with the client factory", clientId);
        throw new KeyNotFoundException($"Client {clientId} has not been registered");
    }

    public async Task RefreshClients()
    {
        try
        {
            RefreshResponse? content = await _httpClient.PostRefreshClients();
            if (content is not null)
            {
                foreach (Connection data in content.Data)
                {
                    _logger.LogDebug("Refreshed client {ClientId}", data.ClientId);
                    _clients.AddOrUpdate(data.ClientId,
                        new SchismClient(featureProvider, data, serializationProvider, _interfaceEmitter, _logger),
                        (k, v) => v.Initialize(data));
                }
            }
            else
            {
                _logger.LogWarning("RefreshResponse failed to deserialize, indicating a failure at the hub");
            }
            _nextRefresh = DateTimeOffset.UtcNow.AddSeconds(_options.Refresh);
            _logger.LogDebug("Next hub refresh happens at: {Time}", _nextRefresh);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshClients failed to process: {Message}", ex.Message);
        }
    }

    public TContract GetClientFor<TContract>()
        where TContract : ISchismContract
    {
        SchismContractAttribute meta = typeof(TContract).GetCustomAttribute<SchismContractAttribute>()
            ?? new SchismContractAttribute()
            {
                ClientId = typeof(TContract).Namespace,
                Type = typeof(TContract).Name
            };
        string typeNamespace = string.IsNullOrWhiteSpace(meta.ClientId) ? typeof(TContract).Namespace! : meta.ClientId;
        SchismClient client = _clients[typeNamespace];
        return client.CreateFor<TContract>(meta);
    }

    public bool IsExpired()
    {
        return DateTimeOffset.UtcNow > _nextRefresh;
    }

    public async Task<bool> Invoke()
    {
        if (DateTimeOffset.UtcNow > _nextRefresh)
        {
            await RefreshClients();
            return true;
        }
        return false;
    }
}