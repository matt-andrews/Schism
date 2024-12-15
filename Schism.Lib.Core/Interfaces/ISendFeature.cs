using Schism.Hub.Abstractions.Models;

namespace Schism.Lib.Core.Interfaces;
public interface ISendFeature
{
    string Key { get; }
    Task<SchismResponse> SendAsync(SchismRequest request, ConnectionPoint point);
}