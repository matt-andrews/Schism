using Microsoft.Extensions.DependencyInjection;
using Schism.Lib.Core.Providers;
using Schism.Lib.ServiceBus.Client;
using System.Collections.Concurrent;

namespace Schism.Lib.ServiceBus.Host;
public interface ISchismServiceBusFactory
{
    Task<ISchismServiceBusClient> GetClient(string clientId, string connectionString);
}
internal interface ISchismServiceBusExecutor : IAsyncDisposable
{
    Task<List<SchismServiceBusClient>> StartAsync();
    Task StopAsync();
}
internal class SchismServiceBusFactory(List<ServiceBusHostConnection> hostConnections, IServiceProvider serviceProvider)
    : ISchismServiceBusFactory, ISchismServiceBusExecutor
{
    public List<ServiceBusHostConnection> HostConnections { get; } = hostConnections;
    private readonly ConcurrentDictionary<string, SchismServiceBusClient> _clients = [];
    private readonly IStringTranslationProvider _translationProvider = serviceProvider.GetRequiredService<IStringTranslationProvider>();
    public async Task<ISchismServiceBusClient> GetClient(string clientId, string connectionString)
    {
        if (_clients.TryGetValue(connectionString, out SchismServiceBusClient? client))
        {
            return client;
        }
        client = new(await _translationProvider.Translate(connectionString), serviceProvider, clientId);
        _clients.TryAdd(connectionString, client);
        return client;
    }

    public async Task<List<SchismServiceBusClient>> StartAsync()
    {
        foreach (ServiceBusHostConnection host in HostConnections)
        {
            host.ConnectionString = await _translationProvider.Translate(host.ConnectionString);
        }

        List<SchismServiceBusClient> result = [];
        foreach (IGrouping<string, ServiceBusHostConnection> group in HostConnections.GroupBy(g => g.ConnectionString))
        {
            SchismServiceBusClient client = new(group, serviceProvider, group.First().ClientId);
            result.Add(client);
            _clients.TryAdd(group.Key, client);
            await client.StartProcessing();
        }
        return result;
    }

    public async Task StopAsync()
    {
        foreach (KeyValuePair<string, SchismServiceBusClient> client in _clients)
        {
            await client.Value.StopAsync();
        }
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            foreach (KeyValuePair<string, SchismServiceBusClient> client in _clients)
            {
                await client.Value.DisposeAsync();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
}