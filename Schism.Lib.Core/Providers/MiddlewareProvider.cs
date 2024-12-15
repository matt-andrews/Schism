using Schism.Lib.Core.Interfaces;
using static Schism.Lib.Core.Providers.IMiddlewareDelegationProvider;

namespace Schism.Lib.Core.Providers;

public interface IMiddlewareDelegationProvider
{
    Task Invoke();
    public interface IMiddlewareDelegationCollection
    {
        IMiddlewareDelegationCollection AddFeature(Func<IServiceProvider, IMiddlewareDelegationFeature> factory);
        IMiddlewareDelegationProvider Build(IServiceProvider provider);
    }
}
internal class MiddlewareDelegationProvider(List<IMiddlewareDelegationFeature> features) : IMiddlewareDelegationProvider
{
    public async Task Invoke()
    {
        foreach (IMiddlewareDelegationFeature feature in features)
        {
            await feature.Invoke();
        }
    }

    public class MiddlewareDelegationCollection : IMiddlewareDelegationCollection
    {
        private readonly List<Func<IServiceProvider, IMiddlewareDelegationFeature>> _factories = [];
        public IMiddlewareDelegationCollection AddFeature(Func<IServiceProvider, IMiddlewareDelegationFeature> factory)
        {
            _factories.Add(factory);
            return this;
        }
        public IMiddlewareDelegationProvider Build(IServiceProvider provider)
        {
            List<IMiddlewareDelegationFeature> result = [];
            foreach (Func<IServiceProvider, IMiddlewareDelegationFeature> factory in _factories)
            {
                result.Add(factory(provider));
            }
            return new MiddlewareDelegationProvider(result);
        }
    }
}