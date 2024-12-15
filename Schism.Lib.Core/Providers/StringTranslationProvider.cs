using Schism.Lib.Core.Interfaces;
using static Schism.Lib.Core.Providers.IStringTranslationProvider;

namespace Schism.Lib.Core.Providers;

public interface IStringTranslationProvider
{
    Task<string> Translate(string str);
    public interface IStringTranslationCollection
    {
        IStringTranslationCollection AddFeature(Func<IServiceProvider, IStringTranslationFeature> factory);
        IStringTranslationProvider Build(IServiceProvider provider);
    }
}
internal class StringTranslationProvider(List<IStringTranslationFeature> _features) : IStringTranslationProvider
{
    public async Task<string> Translate(string str)
    {
        foreach (IStringTranslationFeature feat in _features)
        {
            if (feat.IsTranslatable(str))
            {
                return await feat.Translate(str);
            }
        }
        return str;
    }

    public class StringTranslationCollection : IStringTranslationCollection
    {
        private readonly List<Func<IServiceProvider, IStringTranslationFeature>> _factories = [];
        public IStringTranslationCollection AddFeature(Func<IServiceProvider, IStringTranslationFeature> factory)
        {
            _factories.Add(factory);
            return this;
        }
        public IStringTranslationProvider Build(IServiceProvider provider)
        {
            List<IStringTranslationFeature> features = [];
            foreach (Func<IServiceProvider, IStringTranslationFeature> factory in _factories)
            {
                features.Add(factory(provider));
            }
            return new StringTranslationProvider(features);
        }
    }
}