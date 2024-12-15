using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Schism.Lib.Core;
using System.Diagnostics.CodeAnalysis;

namespace Schism.Tests.Core;

[ExcludeFromCodeCoverage]
public static class TestHelpers
{
    private static Dictionary<string, string?> Config => new()
            {
                { "Schism:HubUri", "http://localhost:30100" },
                { "Schism:ClientId", "testid" },
                { "Schism:Host", "localhost" }
            };
    public static SchismOptions BuildOptions(Dictionary<string, string?>? options = null)
    {
        ConfigurationBuilder builder = new();
        if (options is not null)
        {
            foreach (KeyValuePair<string, string?> item in Config)
            {
                options.Add(item.Key, item.Value);
            }
            builder.AddInMemoryCollection(options);
        }
        else
        {
            builder.AddInMemoryCollection(Config);
        }
        return new SchismOptions(builder.Build());
    }
    public static ILogger<T> MockLogger<T>()
    {
        ILoggerFactory factory = Substitute.For<ILoggerFactory>();
        return factory.CreateLogger<T>();
    }
}