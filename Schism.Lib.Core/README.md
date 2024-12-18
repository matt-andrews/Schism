## Schism Core

The Schism Core library contains several helpful extensions for configuration

## Api
Extension|Params|Notes
-|-|-
`IServiceCollection.AddSchism`|`Assembly`, `IConfiguration`|This extension creates the builder for you. The assembly should be whichever assembly contains your actions
`IServiceCollection.AddSchism`|`Assembly`, `IConfiguration`, `Action<SchismOptions>`|Overrides the above with an options builder for setting options at runtime
`IServiceCollection.AddSchism`|`Assembly`, `IConfiguration`, `SchismOptions`|Overrides the above, allowing you to pass in a `SchismOptions` object for configuration
`SchismBuilder.WithHubHttpClient`|`Action<IHttpClientBuilder>?`|This extension will disable gRPC connection to the Hub and use HTTP instead
`SchismBuilder.WithTranslationFeature`|`Func<IServiceProvider, IStringTranslationFeature>`|Adds a new Translation Feature to the provider. You can read more about Translation Features below
`SchismBuilder.WithDefaultSerializer`|`ISchismSerializer`|You can use this extension to replace the default serializer
`SchismBuilder.WithSerializer`|`string`, `Func<IServiceProvider, ISchismSerializer>`| Use this extension for specifying a custom serializer for a specific client
`IApplicationBuilder.UseSchism`|`none`|This adds the Schism middleware to your application. This is **required**
`IApplicationBuilder.EarlyInitializeSchism`|`none`|This `Task` can be awaited in your startup after middleware has been declared, it ensures that host registration happens on startup preventing any race conditions

## Customization

**Schism** has several configuration options to meet your needs.

### Custom Serialization

You can configure both the default serializer, as well as register custom serializers for specific clients. First you must create a concrete type for `ISchismSerializer`, then you can register at startup:

```csharp
//SchismBuilder
.WithDefaultSerializer(new MyCustomSerializer())
//and/or
.WithSerializer("My.WebApi", provider => {
    return new MyCustomSerializer();
});
```

> [!WARNING]
> Custom serializers defined here **do not** affect how your ASP.Core web application deserializes HTTP requests!

### String Translation

Maybe you don't want to expose your service bus connection string to the hub, in that case you can use String Translation to get the value from a key vault or secure store:

```csharp
// Implmentation
internal class MyStringTranslator(IKeyVault _keyVault) : IStringTranslationFeature
{
    private const string _prefix = "MyStringTranslator=";
    public bool IsTranslatable(string str)
    {
        return str.StartsWith(_prefix);
    }
    //This method is called to resolve the string if the above
    //condition is true
    public async Task<string> Translate(string str)
    {
        str = str.Replace(_prefix, string.Empty);
        return await _keyVault.GetSecret(str);
    }
}

//Registration
//SchismBuilder
.WithFeature(provider => new MyStringTranslator(provider.GetRequiredService<IKeyVault>()))

//Usage
[SchismServiceBusQueue(Connection = "MyStringTranslator=MySbConnection" Queue = "data-collection-queue")]
public Task MyServiceBusMethod(Request request)
{
    //do stuff
}
```

> [!WARNING]
> The above needs to be implemented and registered on both the client and the host!