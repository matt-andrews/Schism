using Microsoft.Extensions.DependencyInjection;
using Schism.Lib.Core;
using static Schism.Lib.Http.IHttpClientProvider;

namespace Schism.Lib.Http;

public interface IHttpClientProvider
{
    ISchismHttpClient GetHttpClient(SchismRequest req);
    public interface IHttpClientCollection
    {
        IHttpClientCollection AddFeature(string clientId, IServiceCollection services, Action<IHttpClientBuilder> configureClient);
        IHttpClientProvider Build(IServiceProvider provider);
    }
}

internal class HttpClientProvider(Dictionary<string, ISchismHttpClient> _clients, ISchismHttpClient defaultClient) : IHttpClientProvider
{
    public ISchismHttpClient GetHttpClient(SchismRequest req)
    {
        return _clients.TryGetValue(req.ClientId, out ISchismHttpClient? http) ? http : defaultClient;
    }

    public class HttpClientCollection : IHttpClientCollection
    {
        private readonly Dictionary<string, IHttpClientBuilder> _factories = [];
        public IHttpClientCollection AddFeature(string clientId, IServiceCollection services, Action<IHttpClientBuilder> configureClient)
        {
            IHttpClientBuilder builder = services.AddHttpClient(clientId);
            configureClient(builder);
            _factories.Add(clientId, builder);
            return this;
        }
        public IHttpClientProvider Build(IServiceProvider provider)
        {
            IHttpClientFactory httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            Dictionary<string, ISchismHttpClient> result = [];
            foreach (KeyValuePair<string, IHttpClientBuilder> factory in _factories)
            {
                result.Add(factory.Key, new SchismHttpClient(httpClientFactory.CreateClient(factory.Key)));
            }
            return new HttpClientProvider(result, new SchismHttpClient(httpClientFactory.CreateClient(HttpConsts.ConstSchismDefaultHttpClient)));
        }
    }
}