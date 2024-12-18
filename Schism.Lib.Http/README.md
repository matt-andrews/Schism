# Schism Http

## Getting Started

Add the Schism Http library
```
TODO: add package
```

In your Startup/Program.cs you'll need to configure the `SchismBuilder` for HTTP:
```csharp
builder.Services
    .AddSchism(typeof(Program).Assembly, builder.Configuration)
    //...
    .WithHttpClient()
    .WithHttpHost()
    //...
    .Build();
```

All hosting requires is setting up a controller, everything else is automatically built for you.

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

## Api

Extension|Params|Notes
-|-|-
`SchismBuilder.WithHttpClient`|`none`|Adds the Schism HTTP client infrastructure for sending HTTP requests. Supported HTTP methods are `GET`,`PUT`,`POST`,`HEAD`,`PATCH`,`DELETE`, and `OPTIONS`. This extension returns an `HttpSchismBuilder` which allows for some HTTP specific customization.
`HttpSchismBuilder.WithDefaultClientBuilder`|`Action<IHttpClientBuilder>`|This extension allows you to customize the default underlying HTTP client
`HttpSchismBuilder.WithClientBuilder`|`string`, `Action<IHttpClientBuilder>`|This extension allows you to customize the underlying HTTP client for a specific clientId
`SchismBuilder.WithHttpHost`|`none`|Adds the Schism HTTP host infrastructure

