using Microsoft.Extensions.Logging;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Internal;
using Schism.Lib.Core.Providers;
using System.Reflection;

namespace Schism.Lib.Core;
public interface ISchismClient
{
    /// <summary>
    /// The identity of this client
    /// </summary>
    string ClientId { get; }

    /// <summary>
    /// Gets the contract <typeparamref name="TContract"/>
    /// </summary>
    /// <typeparam name="TContract"></typeparam>
    /// <param name="meta"></param>
    /// <returns></returns>
    TContract CreateFor<TContract>(SchismContractAttribute? meta = null)
        where TContract : ISchismContract;

    /// <summary>
    /// Creates a new <see cref="SchismRequest"/> for the given connection point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    SchismRequest CreateRequest(string point);

    /// <summary>
    /// Sends a request
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Throws when the given feature type is not registered in the <see cref="ISendFeatureProvider"/></exception>
    /// <exception cref="Exception">Throws when none of the connection types send successfully</exception>
    Task<SchismResponse> SendRequestAsync(SchismRequest req);
}

internal class SchismClient(
    ISendFeatureProvider featureProvider,
    Connection connection,
    ISerializationProvider serializationProvider,
    IInterfaceEmitter interfaceEmitter,
    ILogger logger) : ISchismClient
{
    private readonly IInterfaceEmitter _interfaceEmitter = interfaceEmitter;
    private readonly ISendFeatureProvider _featureProvider = featureProvider;
    private readonly ISerializationProvider _serializationProvider = serializationProvider;
    private Connection _connection = connection;
    private readonly ILogger _logger = logger;
    public string ClientId => _connection.ClientId;
    public SchismClient Initialize(Connection connection)
    {
        _connection = connection;
        return this;
    }

    public TContract CreateFor<TContract>(SchismContractAttribute? meta = null)
        where TContract : ISchismContract
    {
        meta ??= typeof(TContract).GetCustomAttribute<SchismContractAttribute>()
            ?? new SchismContractAttribute()
            {
                Type = typeof(TContract).Name,
            };

        TContract obj = _interfaceEmitter.CreateType<TContract>(i => EmitterDelegate<TContract>(i, meta));
        return obj;
    }

    private async Task<object?> EmitterDelegate<TContract>(CallerInfo caller, SchismContractAttribute meta)
        where TContract : ISchismContract
    {
        ISchismSerializer serializer = _serializationProvider.GetSerializer(this);
        string typeName = string.IsNullOrWhiteSpace(meta.Type) ? typeof(TContract).Name! : meta.Type;
        SchismRequest req = CreateRequest($"{typeName}.{caller.MethodName}");

        //TODO: need to account for emitted method signature with multiple parameters
        //Usecase would be only be get requests atm but still worth doing
        object? arg = caller.MoveNext();
        if (arg is not null)
        {
            req.WithBody(arg);
        }

        SchismResponse response = await SendRequestAsync(req);
        if (caller.ReturnType is null || caller.ReturnType == typeof(Task) || caller.ReturnType == typeof(ValueTask))
        {
            return null;
        }

        string? content = await response.ContentAsStringAsync();
        if (content is null)
        {
            return null;
        }
        try
        {
            return caller.ReturnType.BaseType == typeof(Task) || caller.ReturnType.BaseType == typeof(ValueTask)
                ? serializer.Deserialize(content, caller.ReturnType.GenericTypeArguments[0])
                : serializer.Deserialize(content, caller.ReturnType);
        }
        catch
        {
            try
            {
                return caller.ReturnType.BaseType == typeof(Task) || caller.ReturnType.BaseType == typeof(ValueTask)
                    ? Convert.ChangeType(content, caller.ReturnType.GenericTypeArguments[0])
                    : Convert.ChangeType(content, caller.ReturnType);
            }
            catch
            {
                return null;
            }
        }
    }

    public SchismRequest CreateRequest(string point)
    {
        ISchismSerializer serializer = _serializationProvider.GetSerializer(this);
        return new SchismRequest(
            ClientId,
            [.. _connection.ConnectionPoints.Where(f => f.Name == point).OrderBy(o => o.Priority)],
            _connection.BaseUri,
            serializer);
    }

    public async Task<SchismResponse> SendRequestAsync(SchismRequest req)
    {
        foreach (ConnectionPoint point in req.ConnectionPoints)
        {
            try
            {
                ISendFeature? feature = _featureProvider.FirstOrDefault(f => f.Key == point.Type) 
                    ?? throw new NotSupportedException($"Feature {point.Type} is not registered");
                _logger.LogDebug("Send Feature identified for invocation: {Feature}", feature.Key);
                return await feature.SendAsync(req, point);
            }
            catch (Exception ex)
            {
                //TODO: we should forward failed connection points to the hub to have some sort of priority shift?
                //This could reduce the number of subsequent and collective failed requests, possibly increasing 
                //client performance in situations with unstable connections
                _logger.LogError(ex, "Failed to send request with the given point {Point}, message: {Message}", point.Name, ex.Message);
            }
        }
        _logger.LogError("Failed to fallback send request with any of the given points");
        throw new Exception("Failed to fallback send request");
    }
}