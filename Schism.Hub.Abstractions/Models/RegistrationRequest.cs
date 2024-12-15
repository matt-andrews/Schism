using ProtoBuf;
using System.Diagnostics.CodeAnalysis;

namespace Schism.Hub.Abstractions.Models;

[ProtoContract]
[ExcludeFromCodeCoverage]
public record RegistrationRequest
{
    [ProtoMember(1)]
    public required Connection Connection { get; set; }
}

[ProtoContract]
[ExcludeFromCodeCoverage]
public record RegistrationResponse
{

}