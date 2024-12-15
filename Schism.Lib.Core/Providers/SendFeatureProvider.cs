using Schism.Lib.Core.Interfaces;
using System.Collections;
using static Schism.Lib.Core.Providers.ISendFeatureProvider;

namespace Schism.Lib.Core.Providers;

public interface ISendFeatureProvider : IEnumerable<ISendFeature>
{
    public interface ISendFeatureCollection
    {
        ISendFeatureCollection AddFeature(Func<IServiceProvider, ISendFeature> factory);
        ISendFeatureProvider Build(IServiceProvider serviceProvider);
    }
}
internal class SendFeatureProvider : ISendFeatureProvider
{
    private readonly IReadOnlyList<ISendFeature> _sendFeatures;
    private SendFeatureProvider(IEnumerable<ISendFeature> features)
    {
        _sendFeatures = features.GroupBy(g => g.Key).Select(s => s.Last()).ToList();
    }
    public IEnumerator<ISendFeature> GetEnumerator()
    {
        return _sendFeatures.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public class SendFeatureCollection : ISendFeatureCollection
    {
        private readonly List<Func<IServiceProvider, ISendFeature>> _factories = [];
        public ISendFeatureCollection AddFeature(Func<IServiceProvider, ISendFeature> factory)
        {
            _factories.Add(factory);
            return this;
        }
        public ISendFeatureProvider Build(IServiceProvider serviceProvider)
        {
            return new SendFeatureProvider(_factories.Select(s => s.Invoke(serviceProvider)));
        }
    }
}