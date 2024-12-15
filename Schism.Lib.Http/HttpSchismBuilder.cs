using Microsoft.Extensions.DependencyInjection;
using Schism.Lib.Core;
using System.Diagnostics.CodeAnalysis;

namespace Schism.Lib.Http;

[ExcludeFromCodeCoverage]
public class HttpSchismBuilder : SchismBuilder
{
    internal IHttpClientBuilder HttpDefaultClientBuilder { get; set; } = default!;
    internal IHttpClientProvider.IHttpClientCollection HttpClientCollection { get; set; } = new HttpClientProvider.HttpClientCollection();

    public override void BuildInternal()
    {
        base.BuildInternal();
        Services.AddSingleton(HttpClientCollection.Build);
    }
}