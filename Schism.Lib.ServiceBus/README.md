# Schism Azure Service Bus

## Getting Started

Add the Schism Service Bus library
```
TODO: add package
```

The setup for Azure Service bus is very similar to the HTTP examples:

```csharp
builder.Services
    .AddSchism(typeof(Program).Assembly, builder.Configuration)
    //...
    .WithServiceBusHost()
    .WithServiceBusClient()
    //...
    .Build();
```

Hosting for the Service Bus requires a method to be declared to accept the request. This method isn't required to be in a controller, but there can be benefits to doing so such as supporting fallbacks and overall consistency.

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

> [!WARNING]
> You can have a return type on your service bus method to allow for fallbacks to HTTP, but the client will return `null` on a successful service bus request!

## Api

The Service Bus library contains the following builder extensions:

Extension|Params|Notes
-|-|-
`SchismBuilder.WithServiceBusClient`|`none`|Adds the infrastructure to send service bus messages
`SchismBuilder.WithServiceBusHost`|`string?`|Adds the infrastructure to receive service bus messages. If a connection string is passed this will be the default service bus connection string
`SchismBuilder.WithServiceBusHost`|`MethodInfo[]`, `string?`|Overloads the above with the ability to pass in which methods are used to host.