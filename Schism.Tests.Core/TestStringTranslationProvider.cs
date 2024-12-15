using NSubstitute;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Providers;
using System.Diagnostics.CodeAnalysis;

namespace Schism.Tests.Core;
internal class TestStringTranslationProvider
{
    [Test]
    public async Task TestWithStringMatch()
    {
        string str = $"{nameof(MockTranslationFeature)}=MY_TEST_VAR";

        IStringTranslationProvider provider = new StringTranslationProvider.StringTranslationCollection()
            .AddFeature(_ => new MockTranslationFeature())
            .Build(Substitute.For<IServiceProvider>());

        Assert.That(await provider.Translate(str), Is.EqualTo("MY_TEST_VAR"));
    }
    [Test]
    public async Task TestWithStringNotMatch()
    {
        string str = $"NO_MATCH";

        IStringTranslationProvider provider = new StringTranslationProvider.StringTranslationCollection()
            .AddFeature(_ => new MockTranslationFeature())
            .Build(Substitute.For<IServiceProvider>());

        Assert.That(await provider.Translate(str), Is.EqualTo("NO_MATCH"));
    }
    [Test]
    public async Task TestWithString_Precidence()
    {
        string str = $"{nameof(MockTranslationFeature)}=MY_TEST_VAR";

        IStringTranslationProvider provider = new StringTranslationProvider.StringTranslationCollection()
            .AddFeature(_ => new MockTranslationFeature())
            //should throw exception if this feature is triggered
            .AddFeature(_ => new MockTranslationFeatureNeverMatch())
            .Build(Substitute.For<IServiceProvider>());

        Assert.That(await provider.Translate(str), Is.EqualTo("MY_TEST_VAR"));
    }

    [ExcludeFromCodeCoverage]
    private class MockTranslationFeature : IStringTranslationFeature
    {
        public bool IsTranslatable(string str)
        {
            return str.StartsWith($"{nameof(MockTranslationFeature)}=");
        }

        public Task<string> Translate(string str)
        {
            return Task.FromResult(str.Replace($"{nameof(MockTranslationFeature)}=", ""));
        }
    }
    [ExcludeFromCodeCoverage]
    private class MockTranslationFeatureNeverMatch : IStringTranslationFeature
    {
        public bool IsTranslatable(string str)
        {
            return true;
        }

        public Task<string> Translate(string str)
        {
            throw new NotImplementedException();
        }
    }
}