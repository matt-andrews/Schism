using Schism.Lib.Core;
using System.Diagnostics.CodeAnalysis;

namespace Schism.Lib.ServiceBus;
[ExcludeFromCodeCoverage]
public class SchismServiceBusTopicAttribute : SchismAttribute
{
    public string Connection { get; set; } = "";
    public required string Topic { get; set; }
    public required string Subscription { get; set; }
    public override int Priority { get; set; } = 10;
}

[ExcludeFromCodeCoverage]
public class SchismServiceBusQueueAttribute : SchismAttribute
{
    public string Connection { get; set; } = "";
    public required string Queue { get; set; }
    public override int Priority { get; set; } = 10;
}