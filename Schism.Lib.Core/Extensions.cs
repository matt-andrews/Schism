using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Internal;
using System.Data;
using System.Reflection;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Schism.Tests.Core")]
[assembly: InternalsVisibleTo("Schism.Tests.Http")]
[assembly: InternalsVisibleTo("Schism.Tests.ServiceBus")]
namespace Schism.Lib.Core;
public static class Extensions
{
    /// <inheritdoc cref="AddSchism(IServiceCollection, Assembly, IConfiguration, SchismOptions)"/>
    /// <param name="services">The host builder service collection</param>
    /// <param name="loadingAssembly">The executing assembly</param>
    /// <param name="config">The host build configuration</param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder AddSchism(this IServiceCollection services, Assembly loadingAssembly, IConfiguration config)
    {
        SchismOptions options = new(config);
        return services.AddSchism(loadingAssembly, config, options);
    }

    /// <inheritdoc cref="AddSchism(IServiceCollection, Assembly, IConfiguration, SchismOptions)"/>
    /// <param name="services">The host builder service collection</param>
    /// <param name="loadingAssembly">The executing assembly</param>
    /// <param name="config">The host build configuration</param>
    /// <param name="options">Additional configuration options</param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder AddSchism(this IServiceCollection services, Assembly loadingAssembly, IConfiguration config, Action<SchismOptions> optionsBuilder)
    {
        SchismOptions options = new();
        optionsBuilder(options);
        return services.AddSchism(loadingAssembly, config, options);
    }

    /// <summary>
    /// Configures the <see cref="SchismBuilder"/> and adds some default services
    /// </summary>
    /// <param name="services">The host builder service collection</param>
    /// <param name="loadingAssembly">The executing assembly</param>
    /// <param name="config">The host build configuration</param>
    /// <param name="options">Additional configuration options</param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder AddSchism(this IServiceCollection services, Assembly loadingAssembly, IConfiguration config, SchismOptions options)
    {
        SchismBuilder builder = new()
        {
            Configuration = config,
            LoadingAssembly = loadingAssembly,
            Options = options,
            Services = services,
        };
        options.Namespace = loadingAssembly.GetName().Name ?? "";
        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            options.ClientId = options.Namespace;
        }
        services.AddSingleton(options);
        services.AddSingleton<IInterfaceEmitter, InterfaceEmitter>();
        services.AddSingleton<ISchismHubClient, SchismHubGrpcClient>();
        services.AddSingleton<ISchismClientFactory, SchismClientFactory>();
        builder.WithFeature(provider => (SchismClientFactory)provider.GetRequiredService<ISchismClientFactory>());
        return builder;
    }

    /// <summary>
    /// Add additional <see cref="ConnectionPoint"/> to the <see cref="SchismBuilder.ConnectionPoints"/> list.
    /// <code>Note:</code>
    /// This is only intended to be used by Schism libraries, and was not meant to be used by consumers.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="points">The <see cref="ConnectionPoint"/> to add</param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder WithConnectionPoints(this SchismBuilder builder, params ConnectionPoint[] points)
    {
        builder.ConnectionPoints.AddRange(points);
        return builder;
    }

    /// <summary>
    /// Configure the Hub connection to use an HTTP client instead of gRPC
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="clientBuilder">Optional HTTP client configuration builder</param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder WithHubHttpClient(this SchismBuilder builder, Action<IHttpClientBuilder>? clientBuilder = null)
    {
        builder.Services.Remove<ISchismHubClient>();
        IHttpClientBuilder httpClient = builder.Services
            .AddHttpClient<ISchismHubClient, SchismHubHttpClient>(client => client.BaseAddress = new Uri(builder.Options.HubUri));
        if (clientBuilder is not null)
        {
            clientBuilder(httpClient);
        }
        return builder;
    }

    /// <summary>
    /// Configure a new <see cref="ISendFeature"/>
    /// <code>Note:</code>
    /// This is only intended to be used by Schism libraries, and was not meant to be used by consumers.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="factory"></param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder WithFeature(this SchismBuilder builder, Func<IServiceProvider, ISendFeature> factory)
    {
        builder.SendFeatures.AddFeature(factory);
        return builder;
    }

    /// <summary>
    /// Configure a new <see cref="IMiddlewareDelegationFeature"/>
    /// <code>Note:</code>
    /// This is only intended to be used by Schism libraries, and was not meant to be used by consumers.
    /// <param name="builder"></param>
    /// <param name="factory"></param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder WithFeature(this SchismBuilder builder, Func<IServiceProvider, IMiddlewareDelegationFeature> factory)
    {
        builder.MiddlewareFeatures.AddFeature(factory);
        return builder;
    }

    /// <summary>
    /// Configure a new <see cref="IStringTranslationFeature"/>
    /// <param name="builder"></param>
    /// <param name="factory"></param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder WithFeature(this SchismBuilder builder, Func<IServiceProvider, IStringTranslationFeature> factory)
    {
        builder.TranslationFeatures.AddFeature(factory);
        return builder;
    }

    /// <summary>
    /// Builds the <see cref="SchismBuilder"/>, adds required services, and builds all Providers. 
    /// <code>Note:</code>
    /// This extension executes <see cref="SchismBuilder.BuildInternal()"/> for libraries
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection Build(this SchismBuilder builder)
    {
        //If we have connection points then we configure the host, else this is a client-only build
        //this breaks the assumption that a host will always exist, and any dependencies on the host should be 
        //initialized in this block to prevent runtime failure
        if (builder.ConnectionPoints.Count != 0)
        {
            builder.Services.AddSingleton(provider => new SchismHost(
                builder.Options,
                [.. builder.ConnectionPoints],
                builder.Version,
                provider.GetRequiredService<ILogger<SchismHost>>(),
                provider.GetRequiredService<ISchismHubClient>()));
            builder.WithFeature(provider => provider.GetRequiredService<SchismHost>());
        }
        builder.Services.AddSingleton(builder.MiddlewareFeatures.Build);
        builder.Services.AddSingleton(builder.TranslationFeatures.Build);
        builder.Services.AddSingleton(builder.SendFeatures.Build);
        builder.Services.AddSingleton(provider => builder.SerializationFeatures.Build(provider, builder.DefaultSerializer));
        builder.BuildInternal();
        return builder.Services;
    }

    /// <summary>
    /// Configurue the default serializer
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="customSerializer">An implementation of the custom serializer</param>
    /// <returns><see cref="SchismBuilder"/></returns>
    public static SchismBuilder WithDefaultSerializer(this SchismBuilder builder, ISchismSerializer customSerializer)
    {
        builder.DefaultSerializer = customSerializer;
        return builder;
    }

    /// <summary>
    /// Configures the <see cref="IApplicationBuilder"/> to use <see cref="SchismMiddleware"/>
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseSchism(this IApplicationBuilder app)
    {
        app.UseMiddleware<SchismMiddleware>();
        return app;
    }

    /// <summary>
    /// Initialize the <see cref="SchismHost"/> for registering to the hub early in the application pipeline
    /// <code>Warning:</code>
    /// This method should only be called by applications that are hosts. 
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static async Task EarlyInitializeSchism(this IApplicationBuilder app)
    {
        IServiceProvider services = app.ApplicationServices;
        SchismHost schism = services.GetRequiredService<SchismHost>();
        await schism.Invoke();
    }

    /// <summary>
    /// Remove the given service from the <see cref="IServiceCollection"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <exception cref="ReadOnlyException"></exception>
    public static IServiceCollection Remove<T>(this IServiceCollection services)
    {
        if (services.IsReadOnly)
        {
            throw new ReadOnlyException($"{nameof(services)} read only.");
        }
        ServiceDescriptor? clientProvider = services.FirstOrDefault(f => f.ServiceType == typeof(T));
        if (clientProvider is not null)
        {
            services.Remove(clientProvider);
        }
        return services;
    }
}