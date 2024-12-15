using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Schism.Lib.Core.Providers;
using System.Reflection;

namespace Schism.Lib.ServiceBus.Host;
internal class ServiceBusHostConnection
{
    public required string ClientId { get; set; }
    public required string ConnectionString { get; set; }
    public required string QueueOrTopic { get; set; }
    public required string Subscription { get; set; }
    public required MethodInfo Action { get; set; }
    public IServiceProvider? ServiceProvider { get; set; }
    public async Task ProcessMessage(ProcessMessageEventArgs args)
    {
        _ = await ProcessMessage(args.Message.Body.ToString());
    }

    public Task ProcessError(ProcessErrorEventArgs args)
    {
        if (ServiceProvider is null)
        {
            return Task.CompletedTask;
        }

        ILogger<ServiceBusHostConnection>? logger = ServiceProvider.GetService<ILogger<ServiceBusHostConnection>>();
        logger?.LogError(args.Exception, "Service bus Error: {Message}", args.Exception.Message);
        return Task.CompletedTask;
    }

    internal async Task<object?> ProcessMessage(string body)
    {
        if (ServiceProvider is null)
        {
            throw new NullReferenceException($"{nameof(ServiceProvider)} must be set during client initialization");
        }

        using IServiceScope scope = ServiceProvider.CreateScope();
        IMiddlewareDelegationProvider middleware = scope.ServiceProvider.GetRequiredService<IMiddlewareDelegationProvider>();
        Core.Interfaces.ISchismSerializer serializer = scope.ServiceProvider.GetRequiredService<ISerializationProvider>().GetSerializer();
        await middleware.Invoke();
        object? controller = scope.ServiceProvider.GetService(Action.DeclaringType!);

        ParameterInfo[] parameters = Action.GetParameters();
        if (parameters.Length == 0)
        {
            return Action.Invoke(controller, []);
        }
        else
        {
            object? obj = serializer.Deserialize(body, parameters[0].ParameterType);
            return Action.Invoke(controller, [obj]);
        }
    }
}