using Schism.Lib.Core.Interfaces;
using static Schism.Lib.Core.Providers.ISerializationProvider;

namespace Schism.Lib.Core.Providers;

public interface ISerializationProvider
{
    ISchismSerializer GetSerializer();
    ISchismSerializer GetSerializer(string clientId);
    ISchismSerializer GetSerializer(ISchismClient client);
    public interface ISerializationCollection
    {
        ISerializationCollection AddFeature(string id, Func<IServiceProvider, ISchismSerializer> factory);
        ISerializationProvider Build(IServiceProvider provider, ISchismSerializer defaultSerializer);
    }
}

internal class SerializationProvider(Dictionary<string, ISchismSerializer> _serializers, ISchismSerializer _defaultSerializer) : ISerializationProvider
{
    public ISchismSerializer GetSerializer()
    {
        return _defaultSerializer;
    }

    public ISchismSerializer GetSerializer(ISchismClient client)
    {
        return GetSerializer(client.ClientId);
    }

    public ISchismSerializer GetSerializer(string clientId)
    {
        return _serializers.TryGetValue(clientId, out ISchismSerializer? serializer) ? serializer : _defaultSerializer;
    }

    public class SerializationCollection : ISerializationCollection
    {
        private readonly Dictionary<string, Func<IServiceProvider, ISchismSerializer>> _clientFactories = [];
        public ISerializationCollection AddFeature(string id, Func<IServiceProvider, ISchismSerializer> factory)
        {
            _clientFactories.Add(id, factory);
            return this;
        }

        public ISerializationProvider Build(IServiceProvider provider, ISchismSerializer defaultSerializer)
        {
            Dictionary<string, ISchismSerializer> result = [];
            foreach (KeyValuePair<string, Func<IServiceProvider, ISchismSerializer>> factory in _clientFactories)
            {
                result.Add(factory.Key, factory.Value(provider));
            }
            return new SerializationProvider(result, defaultSerializer);
        }
    }
}