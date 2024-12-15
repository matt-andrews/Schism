using ProtoBuf;
using System.Diagnostics.CodeAnalysis;

namespace Schism.Hub.Abstractions.Models;

[ProtoContract]
[ExcludeFromCodeCoverage]
public record RefreshRequest();

[ProtoContract]
[ExcludeFromCodeCoverage]
public record RefreshResponse
{
    [ProtoMember(1)]
    public required Connection[] Data { get; set; }
}

[ProtoContract]
[ExcludeFromCodeCoverage]
public class Connection
{
    [ProtoMember(1)]
    public required string Version { get; set; }
    [ProtoMember(2)]
    public required string ClientId { get; set; }
    [ProtoMember(3)]
    public required string Namespace { get; set; }
    [ProtoMember(4)]
    public required string BaseUri { get; set; }
    [ProtoMember(5)]
    public required ConnectionPoint[] ConnectionPoints { get; set; }
    [ProtoMember(6)]
    public required Guid InstanceId { get; set; }
}

[ProtoContract]
[ExcludeFromCodeCoverage]
public record ConnectionPoint
{
    [ProtoMember(1)]
    public required string Name { get; set; }
    [ProtoMember(2)]
    public required string Path { get; set; }
    [ProtoMember(3)]
    public required string Type { get; set; }
    [ProtoMember(4)]
    public int Priority { get; set; }
    [ProtoMember(5)]
    public Dictionary<string, string> Props { get; set; } = [];
}