using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.ServiceBus.Client;
using Schism.Lib.ServiceBus.Host;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Schism.Tests.ServiceBus")]
namespace Schism.Lib.ServiceBus;
public static class ServiceBusExtensions
{
    /// <summary>
    /// Adds the Schism Azure Service Bus client for sending Service Bus messages
    /// </summary>
    /// <param name="builder"></param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder WithServiceBusClient(this SchismBuilder builder)
    {
        builder.Services.AddSingleton<ISchismServiceBusFactory>(provider => new SchismServiceBusFactory([], provider));
        builder.WithFeature(provider => new ServiceBusSendFeature(provider.GetRequiredService<ISchismServiceBusFactory>()));
        return builder;
    }

    /// <summary>
    /// Adds the Schism Azure Service Bus host for receiving Service Bus messages
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="defaultServiceBusConnection">The default connection string</param>
    /// <returns><see cref="SchismBuilder"/></returns>
    /// <exception cref="ArgumentException">Thrown when no default service bus connection is defined, 
    /// and an attribute connection string is not defined</exception>
    public static SchismBuilder WithServiceBusHost(this SchismBuilder builder, string? defaultServiceBusConnection = null)
    {
        MethodInfo[] actions = builder.LoadingAssembly.GetTypes()
            .SelectMany(s => s.GetMethods())
            .Where(w => w.GetCustomAttributes(typeof(SchismServiceBusTopicAttribute), false).Length > 0
                || w.GetCustomAttributes(typeof(SchismServiceBusQueueAttribute), false).Length > 0)
            .ToArray();
        return builder.WithServiceBusHost(actions, defaultServiceBusConnection);
    }

    /// <inheritdoc cref="WithServiceBusHost(SchismBuilder, string?)"/>
    /// <param name="builder"></param>
    /// <param name="actions">The method actions that receive messages</param>
    /// <param name="defaultServiceBusConnection">The default connection string</param>
    /// <returns><see cref="SchismBuilder"/></returns>
    /// <exception cref="ArgumentException">Thrown when no default service bus connection is defined, 
    /// and an attribute connection string is not defined</exception>
    public static SchismBuilder WithServiceBusHost(this SchismBuilder builder, MethodInfo[] actions, string? defaultServiceBusConnection = null)
    {
        if (string.IsNullOrWhiteSpace(builder.Options.Host))
        {
            throw new NullReferenceException("Host applications require the SchismBuilder.Host property to be set");
        }
        List<ServiceBusHostConnection> sbConnections = [];
        List<ConnectionPoint> connections = [];
        foreach (MethodInfo method in actions)
        {
            if (method.DeclaringType is null)
            {
                continue;
            }

            builder.Services.TryAddScoped(method.DeclaringType);
            string controllerName = method.DeclaringType.Name;

            if (method.DeclaringType.GetInterfaces().Contains(typeof(ISchismContract)))
            {
                Type iface = method.DeclaringType.GetInterfaces()
                    .First(f => f.GetInterface(nameof(ISchismContract)) is not null);
                controllerName = iface.Name;
            }

            SchismServiceBusTopicAttribute? topicAttr = method.GetCustomAttribute<SchismServiceBusTopicAttribute>();
            SchismServiceBusQueueAttribute? queueAttr = method.GetCustomAttribute<SchismServiceBusQueueAttribute>();
            TransientConnection conn;
            if (topicAttr is not null)
            {
                conn = new(topicAttr);
            }
            else if (queueAttr is not null)
            {
                conn = new(queueAttr);
            }
            else
            {
                continue;
            }

            //Get connection string or fallback to default connection string
            string connectionString = conn.Connection.GetStringMightBeEnv(builder.Configuration);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                if (string.IsNullOrWhiteSpace(defaultServiceBusConnection))
                {
                    throw new ArgumentException("If no default service bus connection string is defined then attribute connection string is required");
                }
                connectionString = defaultServiceBusConnection.GetStringMightBeEnv(builder.Configuration);
            }

            string subscription = conn.Subscription.GetStringMightBeEnv(builder.Configuration);
            string queueOrTopic = conn.QueueOrTopic.GetStringMightBeEnv(builder.Configuration);
            string defaultName = $"{controllerName}.{method.Name}";
            connections.Add(new ConnectionPoint()
            {
                Name = defaultName,
                Path = connectionString,
                Type = SbConsts.ServiceBusKey,
                Priority = conn.Priority,
                Props = new()
                {
                    { SbConsts.ServiceBusTopicOrQueueNameKey, queueOrTopic }
                }
            });
            sbConnections.Add(new ServiceBusHostConnection()
            {
                ClientId = builder.Options.ClientId,
                ConnectionString = connectionString,
                Action = method,
                Subscription = subscription,
                QueueOrTopic = queueOrTopic
            });
        }
        builder.WithConnectionPoints([.. connections]);

        builder.Services.Remove<ISchismServiceBusFactory>();
        builder.Services.AddSingleton<ISchismServiceBusFactory>(provider => new SchismServiceBusFactory(sbConnections, provider));
        builder.Services.AddHostedService<SchismServiceBusHost>();
        return builder;
    }
    private static string GetStringMightBeEnv(this string str, IConfiguration config)
    {
        return str.StartsWith('%') && str.EndsWith('%')
                ? config[str.Trim('%')] ?? str
                : str;
    }

    private record TransientConnection
    {
        public string QueueOrTopic { get; }
        public string Subscription { get; }
        public string Connection { get; }
        public int Priority { get; }
        public TransientConnection(SchismServiceBusTopicAttribute topicAttr)
        {
            QueueOrTopic = topicAttr.Topic;
            Subscription = topicAttr.Subscription;
            Connection = topicAttr.Connection;
            Priority = topicAttr.Priority;
        }
        public TransientConnection(SchismServiceBusQueueAttribute queueAttr)
        {
            QueueOrTopic = queueAttr.Queue;
            Subscription = "";
            Connection = queueAttr.Connection;
            Priority = queueAttr.Priority;
        }
    }
}