using Microsoft.Extensions.Hosting;

namespace Schism.Lib.ServiceBus.Host;
internal class SchismServiceBusHost(ISchismServiceBusFactory factory) : IHostedService, IAsyncDisposable
{
    private readonly ISchismServiceBusExecutor _factory = factory as ISchismServiceBusExecutor
        ?? throw new InvalidCastException($"{nameof(factory)} must implement {nameof(ISchismServiceBusExecutor)}");
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _factory.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _factory.StopAsync();
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await _factory.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
}