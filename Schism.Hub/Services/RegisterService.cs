using Schism.Hub.Abstractions.Contracts;
using Schism.Hub.Abstractions.Models;
using Schism.Hub.Data;

namespace Schism.Hub.Services;

public class RegisterService(IRepository _repository) : IRegisterService
{
    public async Task<RegistrationResponse> Register(RegistrationRequest request)
    {
        RegistrationTable? existing = await _repository.GetByClientId(request.Connection.ClientId);
        if (existing is null)
        {
            await _repository.AddAsync(new RegistrationTable()
            {
                ClientId = request.Connection.ClientId,
                Namespace = request.Connection.Namespace,
                Uri = request.Connection.BaseUri,
                Version = request.Connection.Version,
                ConnectionPoints = request.Connection.ConnectionPoints,
                LastPing = DateTimeOffset.UtcNow
            });
        }
        else
        {
            existing.Namespace = request.Connection.Namespace;
            existing.Uri = request.Connection.BaseUri;
            existing.Version = request.Connection.Version;
            existing.ConnectionPoints = request.Connection.ConnectionPoints;
            existing.LastPing = DateTimeOffset.UtcNow;
        }
        await _repository.SaveChangesAsync();
        return new RegistrationResponse();
    }
}