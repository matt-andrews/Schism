using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Schism.Lib.Core.Interfaces;
public interface ISchismSerializer
{
    string Serialize<TValue>(TValue value);
    T? Deserialize<T>(string json)
        where T : notnull;
    object? Deserialize(string json, Type returnType);
    ValueTask<TValue?> DeserializeAsync<TValue>(
            Stream utf8Json,
            CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public sealed class DefaultJsonSerializer : ISchismSerializer
{
    private readonly JsonSerializerOptions? _options;
    public DefaultJsonSerializer() { }
    public DefaultJsonSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }
    public TValue? Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json)
        where TValue : notnull
    {
        return JsonSerializer.Deserialize<TValue>(json, _options);
    }

    public object? Deserialize([StringSyntax(StringSyntaxAttribute.Json)] string json, Type returnType)
    {
        return JsonSerializer.Deserialize(json, returnType, _options);
    }

    public async ValueTask<TValue?> DeserializeAsync<TValue>(Stream utf8Json, CancellationToken cancellationToken = default)
    {
        return await JsonSerializer.DeserializeAsync<TValue>(utf8Json, options: _options, cancellationToken: cancellationToken);
    }

    public string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, _options);
    }
}