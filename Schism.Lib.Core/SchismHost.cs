using Microsoft.Extensions.Logging;
using Schism.Hub.Abstractions.Models;
using Schism.Lib.Core.Interfaces;

namespace Schism.Lib.Core;

internal class SchismHost(
    SchismOptions options,
    ConnectionPoint[] connectionPoints,
    string version,
    ILogger<SchismHost> logger,
    ISchismHubClient httpClient) : IMiddlewareDelegationFeature
{
    private readonly ISchismHubClient _httpClient = httpClient;
    private readonly ILogger<SchismHost> _logger = logger;
    private readonly ConnectionPoint[] _connectionPoints = connectionPoints;
    private readonly string _version = version;
    private readonly SchismOptions _options = options;
    private DateTimeOffset _expiry;

    public async Task<bool> Invoke()
    {
        if (DateTimeOffset.UtcNow > _expiry)
        {
            await SendRegistrationRequest();
            return true;
        }
        return false;
    }

    private async Task SendRegistrationRequest()
    {
        Connection connection = new()
        {
            BaseUri = _options.Host,
            ConnectionPoints = _connectionPoints,
            ClientId = _options.ClientId,
            Version = _version,
            Namespace = _options.Namespace,
            InstanceId = _options.InstanceId
        };
        RegistrationRequest req = new()
        {
            Connection = connection
        };
        try
        {
            await _httpClient.PostRegistrationRequest(req);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send registration request: {Message}", ex.Message);
        }
        _expiry = DateTimeOffset.UtcNow.AddSeconds(_options.Refresh);
    }
}