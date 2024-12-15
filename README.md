# Schism

> [!NOTE]
> This repo and readme is in its early stages and not ready to be consumed

//TODO

- [ ] Add workflow for Hub image push to repository
- [ ] Add workflow for building nuget packages
- [ ] Add docker/nuget instructions to readme
- [ ] Split readme into lib specifics
- [ ] Contribution?

## Summary

**Schism** is dotnet library intended to unify the communication between different services with simple and generic patterns.

> **schism** _noun_ /ˈs(k)izəm/ : a split or division between strongly opposed sections or parties, caused by differences in opinion or belief.

## Getting Started

### The Hub

The keystone component to **Schism** is the Hub. This is a web service that sits in your network of services acting as a service locator.

You can run the Hub with the following command

```docker
TODO: docker run
```

> [!IMPORTANT]
> You will also need a database for the hub. Currently PostgreSql is supported.

### Configuration

Your hosts are any applications that are accepting a request or message, such as an ASP.Core web application.

In your `Program.cs` file you can register the **Schism** builder:

```csharp
builder.Services
    .AddSchism(typeof(Program).Assembly, builder.Configuration)
    //Add clients/hosts here
    .Build();
```

For instance, our web application has controllers so we use the following:

```csharp
builder.Services
    .AddSchism(typeof(Program).Assembly, builder.Configuration)
    .WithHttpHost()
    .Build();
```

This will automatically register all your controller endpoints with the hub so that other applications can access them.

You will also need to setup some environment variables in your `appsettings.json`

```json
{
  "Schism": {
    //The URI pointing to the Hub
    "HubUri": "http://host.docker.internal:30100",
    //The URI that targets this application
    "Host": "http://host.docker.internal:30300"
  }
}
```

This is the bare minimum setup required for **Schism** to begin hosting.

### Client Requests

Once you have your host setup, we need to configure your client. There are a variety of methods to accomplish this, depending on your needs.

<details open>
<summary>Auto-mapped Concrete Classes</summary>
    
#### Auto-mapped Concrete Contracts
With contracts you can interact with your host using strongly typed interfaces and very little boilerplate. The downside to this method is it tightly couples this library with your code on both the host and the client.

This method requires an additional shared project, which includes interfaces for your controllers. These interfaces must inherit from `ISchismContract` and have the attribute `[SchismContract]`

In your shared client library create a contract interface:

```csharp
//The ClientId property should describe the ClientId of the host
//application. The default value when creating a host is the
//assembly name.
[SchismContract(ClientId = "Example.WebApp")]
public interface IMyController : ISchismContract
{
    Task<AddResponse> Add(AddRequest request);
}
public record AddRequest(int A, int B);
public record AddResponse
{
    [JsonPropertyName("result")]
    public int Result {get;set;}
}
```

In your host application called `Example.WebApp`, your controller would look like this:

```csharp

[ApiController]
[Route("[controller]/[action]")]
public class MyController : ControllerBase, IMyController
{
    [HttpGet]
    public Task<AddResponse> Add([FromQuery] AddRequest request)
    {
        return Task.FromResult(new AddResponse()
        {
            Result = request.A + request.B
        });
    }
}
```

Now from our client application we can access our host like this:

```csharp
//...
private readonly ISchismClientFactory _factory;
public async Task DoMath(int a, int b)
{
    IMyController client = _factory.GetClientFor<IMyController>();
    AddResponse result = await client.Add(new AddRequest(a, b));
    Console.WriteLine($"{a} + {b} = {result.Result}");
}
//await DoMath(12, 8);
//prints: 12 + 8 = 20
//...
```

</details>

<details>
<summary>Blind Concrete Contract</summary>

#### Blind Concrete Contracts

Concrete contracts don't require being implemented on the host! This has the downside of potential for breaking changes being implemented since you lose the strong typing of your contract implementations.

In your shared client library create a contract interface:

```csharp
//The ClientId property should describe the ClientId of the host
//application. The default value when creating a host is the
//assembly name.

//The Type property should describe the name of the controller
//this interface is a contract for
[SchismContract(ClientId = "Example.WebApp", Type = "MyController")]
public interface IMyController : ISchismContract
{
    Task<AddResponse> Add(AddRequest request);
}
public record AddRequest(int A, int B);
public record AddResponse
{
    [JsonPropertyName("result")]
    public int Result {get;set;}
}
```

In your host application called `Example.WebApp`, your controller would look like this:

```csharp

[ApiController]
[Route("[controller]/[action]")]
public class MyController : ControllerBase
{
    [HttpGet]
    public Task<AddResponse> Add([FromQuery] AddRequest request)
    {
        return Task.FromResult(new AddResponse()
        {
            Result = request.A + request.B
        });
    }
}
```

Now from our client application we can access our host like this:

```csharp
//...
private readonly ISchismClientFactory _factory;
public async Task DoMath(int a, int b)
{
    IMyController client = _factory.GetClientFor<IMyController>();
    AddResponse result = await client.Add(new AddRequest(a, b));
    Console.WriteLine($"{a} + {b} = {result.Result}");
}
//await DoMath(12, 8);
//prints: 12 + 8 = 20
//...
```

</details>
<details>
<summary>Flexible string-based requests</summary>

#### Flexible string-based requests

In case you want nothing to do with concrete contracts, you can create your requests simply by string locators. The downside to this method is its more verbose. The following example uses the above controller as an example

```csharp
//...
private readonly ISchismClientFactory _factory;
public async Task DoMath(int a, int b)
{
    ISchismClient client = _factory.GetClient("Example.WebApp"); //default client is assembly name
    SchismRequest request = client
        .GetRequest("MyController.Add") //default connection point is <Class>.<Method>
        .WithBody(new AddRequest(a, b));
    SchismResponse result = await client.SendRequestAsync(request);
    AddResponse response = await result.ContentAsJsonAsync<AddResponse>();
    Console.WriteLine($"{a} + {b} = {response.Result}");
}
//await DoMath(12, 8);
//prints: 12 + 8 = 20
//...
```

</details>

### Azure Service Bus

So far we've only covered HTTP, lets talk about Service bus. The setup for Service bus is very similar:

```csharp
builder.Services
    .AddSchism(typeof(Program).Assembly, builder.Configuration)
    //...
    .WithServiceBusHost()
    .WithServiceBusClient()
    //...
    .Build();
```

Hosting for the Service Bus requires a method to be declared to accept the request.

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class MyController : ControllerBase, IMyController
{
    //Connection can be omitted if a default service bus
    //connection is defined in `.WithServiceBusHost(string)`
    [SchismServiceBusQueue(Connection = "%MySbConnection%" Queue = "data-collection-queue")]
    public Task<DataCollectionResponse?> DataCollection(DataCollectionRequest request)
    {
        //Do stuff
    }
}
```

> [!TIP]
> Service bus hosts don't need to be controller actions, but it makes sense to keep _all_ entry points in controllers for consistency and for the ability to fallback!

> [!WARNING]
> You can have a return type on your service bus method to allow for fallbacks to HTTP, but the client will return `null` on a successful service bus request!

## Customization

**Schism** has several configuration options to meet your needs.

### Custom Serialization

You can configure both the default serializer, as well as register custom serializers for specific clients. First you must create a concrete type for `ISchismSerializer`, then you can register at startup:

```csharp
//SchismBuilder
.WithDefaultSerializer(new MyCustomSerializer())
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

## Future Plans

- [ ] Performance improvements
- [ ] More client/host support such as gRPC, MQTT, AMQP
- [ ] Better documentation
- [ ] Support for other languages