using ProtoBuf.Grpc.Configuration;
using Schism.Hub.Abstractions.Models;

namespace Schism.Hub.Abstractions.Contracts;

[Service]
public interface IRefreshService
{
    Task<RefreshResponse> Refresh(RefreshRequest request);
}