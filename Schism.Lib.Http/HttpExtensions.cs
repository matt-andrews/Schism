using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Http.SendRequests;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Schism.Tests.Http")]
namespace Schism.Lib.Http;
public static class HttpExtensions
{
    /// <summary>
    /// Adds the Schism Http client for sending Http requests
    /// </summary>
    /// <param name="builder"></param>
    /// <returns><see cref="HttpSchismBuilder"/></returns>
    public static HttpSchismBuilder WithHttpClient(this SchismBuilder builder)
    {
        HttpSchismBuilder result = builder.CopyTo<HttpSchismBuilder>();
        //Add the default http client
        result.HttpDefaultClientBuilder = builder.Services.AddHttpClient(HttpConsts.ConstSchismDefaultHttpClient);

        builder.WithFeature(provider => new HttpGetSendFeature(provider.GetRequiredService<IHttpClientProvider>()));
        builder.WithFeature(provider => new HttpPutSendFeature(provider.GetRequiredService<IHttpClientProvider>()));
        builder.WithFeature(provider => new HttpPostSendFeature(provider.GetRequiredService<IHttpClientProvider>()));
        builder.WithFeature(provider => new HttpHeadSendFeature(provider.GetRequiredService<IHttpClientProvider>()));
        builder.WithFeature(provider => new HttpPatchSendFeature(provider.GetRequiredService<IHttpClientProvider>()));
        builder.WithFeature(provider => new HttpDeleteSendFeature(provider.GetRequiredService<IHttpClientProvider>()));
        builder.WithFeature(provider => new HttpOptionsSendFeature(provider.GetRequiredService<IHttpClientProvider>()));

        return result;
    }
    /// <summary>
    /// Configures the default Http client used in Http requests
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="httpBuilder"></param>
    /// <returns><see cref="HttpSchismBuilder"/></returns>
    public static HttpSchismBuilder WithDefaultClientBuilder(this HttpSchismBuilder builder, Action<IHttpClientBuilder> httpBuilder)
    {
        httpBuilder(builder.HttpDefaultClientBuilder);
        return builder;
    }
    /// <summary>
    /// Configure the Http client used for a specified Client Id
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="clientId"></param>
    /// <param name="httpBuilder"></param>
    /// <returns><see cref="HttpSchismBuilder"/></returns>
    public static HttpSchismBuilder WithClientBuilder(this HttpSchismBuilder builder, string clientId, Action<IHttpClientBuilder> httpBuilder)
    {
        builder.HttpClientCollection.AddFeature(clientId, builder.Services, httpBuilder);
        return builder;
    }
    /// <summary>
    /// Adds the Schism Http host for registering Http requests to the hub
    /// </summary>
    /// <param name="builder"></param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder WithHttpHost(this SchismBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(builder.Options.Host))
        {
            throw new NullReferenceException("Host applications require the SchismBuilder.Host property to be set");
        }
        ServiceProvider provider = builder.Services.BuildServiceProvider();
        IActionDescriptorCollectionProvider adcp = provider.GetRequiredService<IActionDescriptorCollectionProvider>();
        IEnumerable<ControllerActionDescriptor> descriptors = adcp.ActionDescriptors
                              .Items
                              .OfType<ControllerActionDescriptor>();

        List<ConnectionPoint> connections = [];
        foreach (ControllerActionDescriptor action in descriptors)
        {
            string controllerName = action.ControllerTypeInfo.Name;
            if (action.ControllerTypeInfo.ImplementedInterfaces.Contains(typeof(ISchismContract)))
            {
                Type iface = action.ControllerTypeInfo.ImplementedInterfaces
                    .First(f => f.GetInterface(nameof(ISchismContract)) is not null);
                controllerName = iface.Name;
            }
            string name = $"{controllerName}.{action.ActionName}";
            string path = action.AttributeRouteInfo.Template;

            foreach (string? method in action.EndpointMetadata
                .Where(w => w is HttpMethodMetadata)
                .Cast<HttpMethodMetadata>()
                .SelectMany(s => s.HttpMethods))
            {
                connections.Add(new ConnectionPoint()
                {
                    Name = name,
                    Path = path,
                    Type = "HTTP_" + method,
                    Priority = 100
                });
            }
        }
        builder.WithConnectionPoints([.. connections]);
        return builder;
    }
}