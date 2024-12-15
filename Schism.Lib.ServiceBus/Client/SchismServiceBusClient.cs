using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Providers;
using Schism.Lib.ServiceBus.Host;
using System.Collections.Concurrent;

namespace Schism.Lib.ServiceBus.Client;

public interface ISchismServiceBusClient : IAsyncDisposable
{
    Task SendMessage(string queueOrTopic, object message);
    Task StopAsync();
}
internal class SchismServiceBusClient : ISchismServiceBusClient
{
    private readonly ServiceBusClient _client;
    private readonly SenderCache _senders;
    private readonly List<ServiceBusProcessor> _processors = [];
    private readonly ISchismSerializer _serializer;
    private readonly string _clientId;

    /// <summary>
    /// Constructor for initialization of sender clients
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="serviceProvider"></param>
    public SchismServiceBusClient(string connectionString, IServiceProvider serviceProvider, string clientId)
    {
        _clientId = clientId;
        _client = new(connectionString);
        _senders = new(_client);
        _serializer = serviceProvider.GetRequiredService<ISerializationProvider>().GetSerializer(_clientId);
    }

    /// <summary>
    /// Constructor for initialization of receiver clients
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="serviceProvider"></param>
    public SchismServiceBusClient(IGrouping<string, ServiceBusHostConnection> connection, IServiceProvider serviceProvider, string clientId)
        : this(connection.Key, serviceProvider, clientId)
    {
        foreach (ServiceBusHostConnection sub in connection)
        {
            sub.ServiceProvider = serviceProvider;
            ServiceBusProcessor processor = string.IsNullOrWhiteSpace(sub.Subscription)
                ? _client.CreateProcessor(sub.QueueOrTopic)
                : _client.CreateProcessor(sub.QueueOrTopic, sub.Subscription);
            processor.ProcessMessageAsync += sub.ProcessMessage;
            processor.ProcessErrorAsync += sub.ProcessError;
            _processors.Add(processor);
        }
    }

    public async Task SendMessage(string queueOrTopic, object message)
    {
        ServiceBusSender sender = _senders.GetSender(queueOrTopic);
        ServiceBusMessage sbmsg = new(_serializer.Serialize(message));
        await sender.SendMessageAsync(sbmsg);
    }

    public async Task StartProcessing()
    {
        foreach (ServiceBusProcessor processor in _processors)
        {
            await processor.StartProcessingAsync();
        }
    }

    public async Task StopAsync()
    {
        foreach (ServiceBusProcessor processor in _processors)
        {
            await processor.StopProcessingAsync();
        }
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            foreach (ServiceBusProcessor processor in _processors)
            {
                await processor.DisposeAsync();
            }
            await _client.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    private class SenderCache(ServiceBusClient client)
    {
        private readonly ConcurrentDictionary<string, ServiceBusSender> _cache = [];
        public ServiceBusSender GetSender(string queueOrTopic)
        {
            if (_cache.TryGetValue(queueOrTopic, out ServiceBusSender? sender))
            {
                return sender;
            }
            sender = client.CreateSender(queueOrTopic);
            _cache.TryAdd(queueOrTopic, sender);
            return sender;
        }
    }
}