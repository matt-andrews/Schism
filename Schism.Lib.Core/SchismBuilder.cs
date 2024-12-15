using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core.Interfaces;
using Schism.Lib.Core.Providers;
using System.Reflection;
namespace Schism.Lib.Core;

public class SchismBuilder
{
    /// <summary>
    /// App configuration
    /// </summary>
    public IConfiguration Configuration { get; set; } = default!;
    /// <summary>
    /// Executing assembly
    /// </summary>
    public Assembly LoadingAssembly { get; set; } = default!;
    /// <summary>
    /// Schism configuration options
    /// </summary>
    public SchismOptions Options { get; set; } = default!;
    /// <summary>
    /// App service collection
    /// </summary>
    public IServiceCollection Services { get; set; } = default!;
    /// <summary>
    /// Host connection points
    /// </summary>
    internal List<ConnectionPoint> ConnectionPoints { get; private set; } = [];
    /// <summary>
    /// Send features
    /// </summary>
    internal ISendFeatureProvider.ISendFeatureCollection SendFeatures { get; private set; } = new SendFeatureProvider.SendFeatureCollection();
    /// <summary>
    /// String translation features
    /// </summary>
    internal IStringTranslationProvider.IStringTranslationCollection TranslationFeatures { get; private set; } = new StringTranslationProvider.StringTranslationCollection();
    /// <summary>
    /// Middleware delegation features
    /// </summary>
    internal IMiddlewareDelegationProvider.IMiddlewareDelegationCollection MiddlewareFeatures { get; private set; } = new MiddlewareDelegationProvider.MiddlewareDelegationCollection();
    /// <summary>
    /// Serialization features
    /// </summary>
    internal ISerializationProvider.ISerializationCollection SerializationFeatures { get; private set; } = new SerializationProvider.SerializationCollection();
    /// <summary>
    /// Default serializer
    /// </summary>
    internal ISchismSerializer DefaultSerializer { get; set; } = new DefaultJsonSerializer();
    /// <summary>
    /// Application version
    /// </summary>
    internal string Version => LoadingAssembly.GetName().Version?.ToString() ?? "0.0.0";
    /// <summary>
    /// Copy this object to the given <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns><typeparamref name="T"/></returns>
    public T CopyTo<T>()
        where T : SchismBuilder, new()
    {
        return new()
        {
            Configuration = Configuration,
            LoadingAssembly = LoadingAssembly,
            Options = Options,
            Services = Services,
            ConnectionPoints = ConnectionPoints,
            SendFeatures = SendFeatures,
            DefaultSerializer = DefaultSerializer,
            TranslationFeatures = TranslationFeatures,
            MiddlewareFeatures = MiddlewareFeatures,
            SerializationFeatures = SerializationFeatures
        };
    }
    /// <summary>
    /// Additional virtual Build step, executed by the <see cref="Extensions.Build(SchismBuilder)"/> method
    /// </summary>
    public virtual void BuildInternal() { }
}