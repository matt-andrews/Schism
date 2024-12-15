using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core.Interfaces;

namespace Schism.Lib.Core;

public record SchismRequest(string ClientId, ConnectionPoint[] ConnectionPoints, string BaseUri, ISchismSerializer Serializer)
{
    public const string ConstBody = "REQUEST_BODY";
    private readonly Dictionary<string, object> _props = [];
    public void AddProp(Action<Dictionary<string, object>> props)
    {
        props(_props);
    }
    public T? GetPropAs<T>(string key)
    {
        return _props.TryGetValue(key, out object? prop) ? (T)prop : default;
    }
    public object? GetProp(string key)
    {
        return _props.TryGetValue(key, out object? prop) ? prop : default;
    }
}
public static class SchismRequestExtensions
{
    /// <summary>
    /// Add a body to be serialized as part of the request
    /// </summary>
    /// <param name="req"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static SchismRequest WithBody(this SchismRequest req, object obj)
    {
        req.AddProp(props =>
        {
            if (!props.TryAdd(SchismRequest.ConstBody, obj))
            {
                props[SchismRequest.ConstBody] = obj;
            }
        });
        return req;
    }
}