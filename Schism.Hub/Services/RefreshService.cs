using Schism.Hub.Abstractions.Contracts;
using Schism.Hub.Abstractions.Models;
using Schism.Hub.Data;

namespace Schism.Hub.Services;

public class RefreshService(IRepository _repository) : IRefreshService
{
    public async Task<RefreshResponse> Refresh(RefreshRequest request)
    {
        RegistrationTable[] response = await _repository.GetTablesByClient();
        return new RefreshResponse()
        {
            Data = response.Select(s =>
                new Connection()
                {
                    BaseUri = s.Uri,
                    ClientId = s.ClientId,
                    Version = s.Version,
                    ConnectionPoints = s.ConnectionPoints,
                    Namespace = s.Namespace,
                    InstanceId = Guid.Empty
                }).ToArray()
        };
    }
}