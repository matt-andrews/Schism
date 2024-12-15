using Microsoft.AspNetCore.Http;
using Schism.Lib.Core.Providers;
using System.Diagnostics.CodeAnalysis;

namespace Schism.Lib.Core;

[ExcludeFromCodeCoverage]
internal class SchismMiddleware(RequestDelegate _next)
{
    public async Task InvokeAsync(HttpContext context, IMiddlewareDelegationProvider middlewareProvider)
    {
        await middlewareProvider.Invoke();
        await _next(context);
    }
}